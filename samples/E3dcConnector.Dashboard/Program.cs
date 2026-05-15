using System.Text.Json;
using Akka.Actor;
using Akka.Streams;
using E3dcConnector.Client;
using E3dcConnector.Descriptors;
using E3dcConnector.Messages;
using E3dcConnector.Messages.Responses;
using E3dcConnector.Reactive;
using E3dcConnector.Reactive.Internal;
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

var flow = RscpFlow.Create(
    () => new RscpConnection(host, user: user, password: password, encryptionKey: rscpKey),
    pollingTags: [
        Ems.PowerPv, Ems.PowerBat, Ems.PowerGrid, Ems.PowerHome,
        Ems.BatSoc, Ems.Autarky, Ems.SelfConsumption,
    ],
    new RscpFlowSettings { PollingInterval = TimeSpan.FromSeconds(2) });

var (_, messages) = flow.Materialize(materializer);

var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
object? latestData = null;

_ = Task.Run(async () =>
{
    await foreach (var msg in messages.ReadAllAsync())
    {
        if (msg is not RscpDataResponse data) continue;
        var ems = data.ToEmsPowerSnapshot();
        var bat = data.ToBatterySnapshot();
        if (ems is null) continue;

        latestData = new
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

app.MapFallback(async ctx =>
{
    ctx.Response.ContentType = "text/html";
    await ctx.Response.SendFileAsync(Path.Combine(app.Environment.WebRootPath, "index.html"));
});

app.Run();
