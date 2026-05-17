using System.Text.Json;
using System.Text.Json.Serialization;
using Akka.Actor;
using Akka.Streams;
using E3dcConnector.Dashboard.Actors;
using E3dcConnector.Dashboard.Configuration;

var builder = WebApplication.CreateBuilder(args);

// ── Configuration ──
builder.Services.Configure<E3dcOptions>(builder.Configuration.GetSection(E3dcOptions.SectionName));
var e3dcOptions = builder.Configuration.GetSection(E3dcOptions.SectionName).Get<E3dcOptions>() ?? new E3dcOptions();

// ── Akka ActorSystem ──
var actorSystem = ActorSystem.Create("e3dc-dashboard");
var materializer = actorSystem.Materializer();

// ── Create actors ──
var maxHistory = (int)(e3dcOptions.HistoryRetentionMinutes * 60.0 / e3dcOptions.FastPollingIntervalSeconds);
var snapshotActor = actorSystem.ActorOf(Props.Create<SnapshotActor>(maxHistory), "snapshot");
var gatewayActor = actorSystem.ActorOf(RscpGatewayActor.Props(e3dcOptions, snapshotActor, materializer), "gateway");
var pollingActor = actorSystem.ActorOf(PollingActor.Props(e3dcOptions, gatewayActor), "polling");

// ── Register services ──
builder.Services.AddSingleton(new ActorRegistry
{
    Snapshot = snapshotActor,
    Gateway = gatewayActor,
    Polling = pollingActor,
});
builder.Services.AddControllers().AddNewtonsoftJson();

var app = builder.Build();

// ── Static files + fallback ──
app.UseStaticFiles();

// ── SSE stream endpoint ──
var jsonOptions = new JsonSerializerOptions
{
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
};

app.MapGet("/api/stream", async (HttpContext ctx, ActorRegistry actors) =>
{
    ctx.Response.ContentType = "text/event-stream";
    ctx.Response.Headers.CacheControl = "no-cache";
    ctx.Response.Headers.Connection = "keep-alive";

    actors.Polling.Tell(new ConsumerConnected());
    try
    {
        var interval = TimeSpan.FromSeconds(e3dcOptions.FastPollingIntervalSeconds);
        while (!ctx.RequestAborted.IsCancellationRequested)
        {
            var result = await actors.Snapshot.Ask<LatestSnapshotResult>(new GetLatestSnapshot(), TimeSpan.FromSeconds(3));
            if (result.Snapshot is not null)
            {
                var json = JsonSerializer.Serialize(result.Snapshot, jsonOptions);
                await ctx.Response.WriteAsync($"data: {json}\n\n", ctx.RequestAborted);
                await ctx.Response.Body.FlushAsync(ctx.RequestAborted);
            }
            await Task.Delay(interval, ctx.RequestAborted);
        }
    }
    catch (OperationCanceledException) { }
    finally
    {
        actors.Polling.Tell(new ConsumerDisconnected());
    }
});

// ── MVC controllers ──
app.MapControllers();

// ── SPA fallback ──
app.MapFallback(async ctx =>
{
    ctx.Response.ContentType = "text/html";
    await ctx.Response.SendFileAsync(Path.Combine(app.Environment.WebRootPath, "index.html"));
});

app.Run();
