# Streaming

For continuous data flow, use `RscpFlow` directly with Akka.Streams channels.

## Channel Materialization

```csharp
var system = ActorSystem.Create("e3dc");
var materializer = system.Materializer();

var flow = RscpFlow.Create(
    () => new RscpConnection("192.168.1.100", 5033, "user", "pass", "key"),
    pollingTags: [RscpTag.EMS_REQ_POWER_PV, RscpTag.EMS_REQ_BAT_SOC],
    new RscpFlowSettings { PollingInterval = TimeSpan.FromSeconds(2) });

var (commands, messages) = flow.Materialize(materializer);
```

## Async Enumeration

```csharp
await foreach (var msg in messages.ReadAllAsync())
{
    if (msg is RscpDataResponse data)
    {
        var snapshot = data.ToEmsPowerSnapshot();
        if (snapshot is not null)
            Console.WriteLine($"PV={snapshot.PvWatts}W SOC={snapshot.Soc:F1}%");
    }
}
```

## Sending On-Demand Commands

While polling runs automatically, you can also send one-off commands:

```csharp
await commands.WriteAsync(
    RscpRequest.Create()
        .Read(Info.SerialNumber, Info.SwRelease));
```

## Composing with Other Stages

The flow is a standard `Flow<IRscpCommand, IRscpMessage, NotUsed>` — compose it with any Akka.Streams stage:

```csharp
var filtered = flow
    .Where(msg => msg is RscpDataResponse)
    .Select(msg => ((RscpDataResponse)msg).ToEmsPowerSnapshot())
    .Where(snapshot => snapshot is not null);
```
