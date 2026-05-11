# Polling

RSCP has no server push — polling is required for live data. The library handles this automatically.

## Builder Configuration

```csharp
var client = new RscpClientBuilder()
    .Connect("192.168.1.100")
    .WithCredentials("user", "pass")
    .WithEncryptionKey("key")
    .WithPolling(TimeSpan.FromSeconds(2), [
        RscpTag.EMS_REQ_POWER_PV,
        RscpTag.EMS_REQ_POWER_BAT,
        RscpTag.EMS_REQ_POWER_GRID,
        RscpTag.EMS_REQ_POWER_HOME,
        RscpTag.EMS_REQ_BAT_SOC,
    ])
    .Build();
```

## Subscribing to Updates

```csharp
client.Subscribe<RscpDataResponse>(response =>
{
    var snapshot = response.ToEmsPowerSnapshot();
    if (snapshot is not null)
        Console.WriteLine($"PV={snapshot.PvWatts}W SOC={snapshot.Soc:F1}%");
});
```

## How It Works

Internally, `Source.Tick` emits a `ReadTagsCommand` at the configured interval. These are merged with on-demand commands via `MergePreferred`, giving on-demand requests priority.

```
Source.Tick (polling) ─┐
                       MergePreferred ─► Pipeline
On-demand commands ────┘ (preferred)
```
