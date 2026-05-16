using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Akka.Actor;
using Akka.Streams;
using E3dcConnector.Client;
using E3dcConnector.Descriptors;
using E3dcConnector.Messages;
using E3dcConnector.Messages.Responses;
using E3dcConnector.Protocol;
using E3dcConnector.Reactive;
using E3dcConnector.Reactive.Internal;
using E3dcConnector.Tags;
using E3dcConnector.Typed;
using E3dcConnector.Typed.Bat;
using E3dcConnector.Typed.Db;
using E3dcConnector.Typed.Ems;
using E3dcConnector.Typed.Info;
using E3dcConnector.Typed.Pm;
using E3dcConnector.Typed.Pvi;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// ── Configuration ──
var config = builder.Configuration.GetSection("E3DC");
var host = config["Host"] ?? "192.168.1.100";
var user = config["User"] ?? "";
var password = config["Password"] ?? "";
var rscpKey = config["RscpKey"] ?? "";
var fastInterval = TimeSpan.FromSeconds(int.TryParse(config["FastPollingIntervalSeconds"], out var f) ? f : 2);
var mediumInterval = TimeSpan.FromSeconds(int.TryParse(config["MediumPollingIntervalSeconds"], out var m) ? m : 10);
var historyMinutes = int.TryParse(config["HistoryRetentionMinutes"], out var h) ? h : 60;

// ── Akka + RSCP Flow (no built-in polling — we manage ticks ourselves) ──
var system = ActorSystem.Create("e3dc-dashboard");
var materializer = system.Materializer();

var flow = RscpFlow.Create(
    () => new RscpConnection(host, user: user, password: password, encryptionKey: rscpKey),
    pollingRequest: null,
    new RscpFlowSettings());

var (commands, messages) = flow.Materialize(materializer);

// ── Shared state ──
var jsonOptions = new JsonSerializerOptions
{
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
};
object? latestSnapshot = null;
string lastRawDump = "no data yet";
DeviceInfo? cachedDeviceInfo = null;

// Cached snapshots from different polling tiers (merged into latestSnapshot)
EmsPowerSnapshot? lastEms = null;
BatterySnapshot? lastBat = null;
InverterSnapshot? lastPvi = null;
PowerMeterSnapshot? lastPm = null;
var maxHistory = (int)(historyMinutes * 60 / fastInterval.TotalSeconds);
var history = new ConcurrentQueue<object>();

// ── Consumer count for demand-driven medium polling ──
var consumerCount = 0;
var mediumPollCts = new CancellationTokenSource();

// ── Polling requests ──
var fastRequest = RscpRequest.Create()
    .Read(Ems.PowerPv, Ems.PowerBat, Ems.PowerGrid, Ems.PowerHome)
    .Read(Ems.BatSoc, Ems.Autarky, Ems.SelfConsumption)
    as IRscpCommand;

var mediumRequest = RscpRequest.Create()
    .FromDevice(Bat.Device, 0, b => b
        .Read(Bat.Rsoc, Bat.ModuleVoltage, Bat.Current, Bat.ChargeCycles))
    .FromDevice(Pvi.Device, 0, b => b
        .Read(Pvi.AcPower, Pvi.AcVoltage, Pvi.AcFrequency,
              Pvi.DcPower, Pvi.DcVoltage, Pvi.DcCurrent))
    .FromDevice(Pm.Device, 0, b => b
        .Read(Pm.PowerL1, Pm.PowerL2, Pm.PowerL3,
              Pm.VoltageL1, Pm.VoltageL2, Pm.VoltageL3,
              Pm.EnergyL1, Pm.EnergyL2, Pm.EnergyL3))
    as IRscpCommand;

var infoRequest = RscpRequest.Create()
    .Read(Info.SerialNumber, Info.SwRelease, Info.IpAddress)
    as IRscpCommand;

// ── DumpItems for debug/explorer ──
string DumpItems(IReadOnlyList<RscpDataItem> items, int indent = 0)
{
    var sb = new StringBuilder();
    var pad = new string(' ', indent * 2);
    foreach (var item in items)
    {
        var tagName = Enum.IsDefined(typeof(RscpTag), item.Tag)
            ? ((RscpTag)item.Tag).ToString()
            : $"0x{item.Tag:X8}";
        var hex = BitConverter.ToString(item.Value.ToArray()).Replace("-", " ");
        sb.AppendLine($"{pad}{tagName}  Type={item.DataType}  Len={item.Value.Length}  Val=[{hex}]");
        if (item.DataType == RscpDataType.Container)
            sb.Append(DumpItems(item.ParseContainerChildren(), indent + 1));
    }
    return sb.ToString();
}

// ── Response consumer (single reader) ──
string? lastConsumerError = null;
_ = Task.Run(async () =>
{
    await foreach (var msg in messages.ReadAllAsync())
    {
        try
        {
        if (msg is not RscpDataResponse data) continue;

        lastRawDump = DumpItems(data.Items);

        var info = data.ToDeviceInfo();
        if (info is not null) cachedDeviceInfo = info;

        var ems = data.ToEmsPowerSnapshot();
        var bat = data.ToBatterySnapshot();
        var pvi = data.ToInverterSnapshot();
        var pm = data.ToPowerMeterSnapshot();

        if (ems is not null) lastEms = ems;
        if (bat is not null) lastBat = bat;
        if (pvi is not null) lastPvi = pvi;
        if (pm is not null) lastPm = pm;

        if (lastEms is null) continue;

        var snapshot = new
        {
            lastEms.PvWatts,
            lastEms.BatteryWatts,
            lastEms.GridWatts,
            lastEms.HomeWatts,
            lastEms.Soc,
            lastEms.Autarky,
            lastEms.SelfConsumption,
            BatteryVoltage = lastBat?.Voltage ?? 0,
            BatteryCurrent = lastBat?.Current ?? 0,
            ChargeCycles = lastBat?.ChargeCycles ?? 0,
            PviAcPowerL1 = lastPvi?.AcPowerL1,
            PviAcVoltageL1 = lastPvi?.AcVoltageL1,
            PviDcPower = lastPvi?.DcPower,
            PviDcVoltage = lastPvi?.DcVoltage,
            PviDcCurrent = lastPvi?.DcCurrent,
            PviFrequency = lastPvi?.Frequency,
            PmPowerL1 = lastPm?.PowerL1,
            PmPowerL2 = lastPm?.PowerL2,
            PmPowerL3 = lastPm?.PowerL3,
            PmVoltageL1 = lastPm?.VoltageL1,
            PmVoltageL2 = lastPm?.VoltageL2,
            PmVoltageL3 = lastPm?.VoltageL3,
            PmEnergyL1 = lastPm?.EnergyL1,
            PmEnergyL2 = lastPm?.EnergyL2,
            PmEnergyL3 = lastPm?.EnergyL3,
            Timestamp = DateTimeOffset.UtcNow,
        };

        latestSnapshot = snapshot;
        history.Enqueue(snapshot);
        while (history.Count > maxHistory) history.TryDequeue(out _);
        }
        catch (Exception ex)
        {
            lastConsumerError = $"{ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}";
        }
    }
});

// ── Fast polling (always on) ──
_ = Task.Run(async () =>
{
    await Task.Delay(500);
    while (true)
    {
        await commands.WriteAsync(fastRequest!);
        await Task.Delay(fastInterval);
    }
});

// ── One-shot Info request ──
_ = Task.Run(async () =>
{
    await Task.Delay(1000);
    await commands.WriteAsync(infoRequest!);
});

// ── Medium polling (demand-driven) ──
async Task RunMediumPolling(CancellationToken ct)
{
    while (!ct.IsCancellationRequested)
    {
        try
        {
            await commands.WriteAsync(mediumRequest!, ct);
            await Task.Delay(mediumInterval, ct);
        }
        catch (OperationCanceledException) { break; }
    }
}

void StartMediumPolling()
{
    mediumPollCts = new CancellationTokenSource();
    _ = Task.Run(() => RunMediumPolling(mediumPollCts.Token));
}

void StopMediumPolling()
{
    mediumPollCts.Cancel();
}

// ── HTTP Endpoints ──
app.UseStaticFiles();

app.MapGet("/api/stream", async (HttpContext ctx) =>
{
    ctx.Response.ContentType = "text/event-stream";
    ctx.Response.Headers.CacheControl = "no-cache";
    ctx.Response.Headers.Connection = "keep-alive";

    var count = Interlocked.Increment(ref consumerCount);
    if (count == 1) StartMediumPolling();

    try
    {
        while (!ctx.RequestAborted.IsCancellationRequested)
        {
            if (latestSnapshot is not null)
            {
                var json = JsonSerializer.Serialize(latestSnapshot, jsonOptions);
                await ctx.Response.WriteAsync($"data: {json}\n\n", ctx.RequestAborted);
                await ctx.Response.Body.FlushAsync(ctx.RequestAborted);
            }
            await Task.Delay(fastInterval, ctx.RequestAborted);
        }
    }
    catch (OperationCanceledException) { }
    finally
    {
        var remaining = Interlocked.Decrement(ref consumerCount);
        if (remaining == 0) StopMediumPolling();
    }
});

app.MapGet("/api/history", () => Results.Json(history.ToArray(), jsonOptions));

app.MapGet("/api/debug", () => Results.Text(lastRawDump, "text/plain"));

app.MapGet("/api/diag", () => Results.Text(
    $"latestSnapshot: {(latestSnapshot is not null ? "SET" : "NULL")}\n" +
    $"lastEms: {(lastEms is not null ? "SET" : "NULL")}\n" +
    $"lastBat: {(lastBat is not null ? "SET" : "NULL")}\n" +
    $"lastPvi: {(lastPvi is not null ? "SET" : "NULL")}\n" +
    $"lastPm: {(lastPm is not null ? "SET" : "NULL")}\n" +
    $"consumerCount: {consumerCount}\n" +
    $"lastError: {lastConsumerError ?? "none"}\n",
    "text/plain"));

app.MapGet("/api/info", () => cachedDeviceInfo is not null
    ? Results.Json(cachedDeviceInfo, jsonOptions)
    : Results.Json(new { status = "loading" }));

app.MapGet("/api/tags", () =>
{
    var tags = Enum.GetValues<RscpTag>()
        .GroupBy(t =>
        {
            var val = (uint)t;
            return (val >> 24) switch
            {
                0x00 => "RSCP",
                0x01 => "EMS",
                0x02 => "PVI",
                0x03 => "BAT",
                0x04 => "DCDC",
                0x05 => "PM",
                0x06 => "DB",
                0x09 => "HA",
                0x0A => "INFO",
                0x0B => "EP",
                0x0C => "SYS",
                0x0D => "UM",
                0x0E => "WB",
                _ => "OTHER"
            };
        })
        .ToDictionary(
            g => g.Key,
            g => g.Select(t => new { name = t.ToString(), hex = $"0x{(uint)t:X8}" }).ToArray());

    return Results.Json(tags);
});

app.MapPost("/api/send", async (HttpContext ctx) =>
{
    var body = await JsonSerializer.DeserializeAsync<SendRequest>(ctx.Request.Body,
        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    if (body?.Tags is null || body.Tags.Length == 0)
        return Results.BadRequest(new { error = "tags required" });

    var request = RscpRequest.Create();
    var hasDevice = body.DeviceNamespace is not null && body.DeviceIndex is not null;
    var tagCount = 0;

    if (hasDevice)
    {
        var device = body.DeviceNamespace!.ToUpperInvariant() switch
        {
            "BAT" => Bat.Device,
            "PVI" => Pvi.Device,
            "PM" => Pm.Device,
            "WB" => Wb.Device,
            _ => (DeviceDescriptor?)null
        };
        if (device is null)
            return Results.BadRequest(new { error = $"Unknown device namespace: {body.DeviceNamespace}" });

        request.FromDevice(device.Value, body.DeviceIndex!.Value, b =>
        {
            foreach (var tagStr in body.Tags)
            {
                if (Enum.TryParse<RscpTag>(tagStr, true, out var tag))
                {
                    b.Read(new IndexedTag(tag));
                    tagCount++;
                }
                else if (tagStr.StartsWith("0x", StringComparison.OrdinalIgnoreCase)
                         && uint.TryParse(tagStr[2..], System.Globalization.NumberStyles.HexNumber, null, out var hex))
                {
                    b.Read(new IndexedTag((RscpTag)hex));
                    tagCount++;
                }
            }
        });
    }
    else
    {
        foreach (var tagStr in body.Tags)
        {
            if (Enum.TryParse<RscpTag>(tagStr, true, out var tag))
            {
                request.Read(new TagDescriptor(tag));
                tagCount++;
            }
            else if (tagStr.StartsWith("0x", StringComparison.OrdinalIgnoreCase)
                     && uint.TryParse(tagStr[2..], System.Globalization.NumberStyles.HexNumber, null, out var hex))
            {
                request.Read(new TagDescriptor((RscpTag)hex));
                tagCount++;
            }
        }
    }

    if (tagCount == 0)
        return Results.BadRequest(new { error = "No valid tags found" });

    await commands.WriteAsync(request);
    await Task.Delay(500);

    return Results.Text(lastRawDump, "text/plain");
});

app.MapPost("/api/history-query", async (HttpContext ctx) =>
{
    var body = await JsonSerializer.DeserializeAsync<HistoryQueryRequest>(ctx.Request.Body,
        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    if (body is null)
        return Results.BadRequest(new { error = "Invalid request" });

    var period = body.Period?.ToUpperInvariant() switch
    {
        "WEEK" => HistoryPeriod.Week,
        "MONTH" => HistoryPeriod.Month,
        "YEAR" => HistoryPeriod.Year,
        _ => HistoryPeriod.Day,
    };

    var start = body.Start ?? DateTimeOffset.UtcNow.Date;
    var query = new HistoryQuery(start, period);
    await commands.WriteAsync(query);
    await Task.Delay(1000);

    return Results.Text(lastRawDump, "text/plain");
});

app.MapFallback(async ctx =>
{
    ctx.Response.ContentType = "text/html";
    await ctx.Response.SendFileAsync(Path.Combine(app.Environment.WebRootPath, "index.html"));
});

app.Run();

record SendRequest(string[]? Tags, string? DeviceNamespace, int? DeviceIndex);
record HistoryQueryRequest(DateTimeOffset? Start, string? Period);
