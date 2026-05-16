using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
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

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var config = builder.Configuration.GetSection("E3DC");
var host = config["Host"] ?? "192.168.1.100";
var user = config["User"] ?? "";
var password = config["Password"] ?? "";
var rscpKey = config["RscpKey"] ?? "";

var system = ActorSystem.Create("e3dc-dashboard");
var materializer = system.Materializer();

var pollingRequest = RscpRequest.Create()
    .Read(Ems.PowerPv, Ems.PowerBat, Ems.PowerGrid, Ems.PowerHome)
    .Read(Ems.BatSoc, Ems.Autarky, Ems.SelfConsumption)
    .FromDevice(Bat.Device, 0, b => b
        .Read(Bat.Rsoc, Bat.ModuleVoltage, Bat.Current, Bat.ChargeCycles));

var flow = RscpFlow.Create(
    () => new RscpConnection(host, user: user, password: password, encryptionKey: rscpKey),
    pollingRequest,
    new RscpFlowSettings { PollingInterval = TimeSpan.FromSeconds(2) });

var (_, messages) = flow.Materialize(materializer);

var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
object? latestData = null;
string lastRawDump = "no data yet";

const int MaxHistory = 1800;
var history = new ConcurrentQueue<object>();

string DumpItems(IReadOnlyList<RscpDataItem> items, int indent = 0)
{
    var sb = new StringBuilder();
    var pad = new string(' ', indent * 2);
    foreach (var item in items)
    {
        var tagName = Enum.IsDefined(typeof(RscpTag), item.Tag) ? ((RscpTag)item.Tag).ToString() : $"0x{item.Tag:X8}";
        var hex = BitConverter.ToString(item.Value.ToArray()).Replace("-", " ");
        sb.AppendLine($"{pad}{tagName}  Type={item.DataType}  Len={item.Value.Length}  Val=[{hex}]");
        if (item.DataType == RscpDataType.Container)
            sb.Append(DumpItems(item.ParseContainerChildren(), indent + 1));
    }
    return sb.ToString();
}

_ = Task.Run(async () =>
{
    await foreach (var msg in messages.ReadAllAsync())
    {
        if (msg is not RscpDataResponse data) continue;

        lastRawDump = DumpItems(data.Items);

        var ems = data.ToEmsPowerSnapshot();
        var bat = data.ToBatterySnapshot();
        if (ems is null) continue;

        var snapshot = new
        {
            ems.PvWatts,
            ems.BatteryWatts,
            ems.GridWatts,
            ems.HomeWatts,
            ems.Soc,
            ems.Autarky,
            ems.SelfConsumption,
            BatteryVoltage = bat?.Voltage ?? 0,
            BatteryCurrent = bat?.Current ?? 0,
            ChargeCycles = bat?.ChargeCycles ?? 0,
            Timestamp = DateTimeOffset.UtcNow,
        };

        latestData = snapshot;
        history.Enqueue(snapshot);
        while (history.Count > MaxHistory) history.TryDequeue(out _);
    }
});

app.UseStaticFiles();

app.MapGet("/api/stream", async (HttpContext ctx) =>
{
    ctx.Response.ContentType = "text/event-stream";
    ctx.Response.Headers.CacheControl = "no-cache";
    ctx.Response.Headers.Connection = "keep-alive";

    while (!ctx.RequestAborted.IsCancellationRequested)
    {
        if (latestData is not null)
        {
            var json = JsonSerializer.Serialize(latestData, jsonOptions);
            await ctx.Response.WriteAsync($"data: {json}\n\n", ctx.RequestAborted);
            await ctx.Response.Body.FlushAsync(ctx.RequestAborted);
        }
        await Task.Delay(2000, ctx.RequestAborted);
    }
});

app.MapGet("/api/history", () => Results.Json(history.ToArray(), jsonOptions));

app.MapGet("/api/debug", () => Results.Text(lastRawDump, "text/plain"));

app.MapFallback(async ctx =>
{
    ctx.Response.ContentType = "text/html";
    await ctx.Response.SendFileAsync(Path.Combine(app.Environment.WebRootPath, "index.html"));
});

app.Run();
