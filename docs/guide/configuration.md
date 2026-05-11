# Configuration

## RscpClientBuilder

The builder provides a fluent API for configuring the client.

```csharp
var client = new RscpClientBuilder()
    .Connect("192.168.1.100", 5033)
    .WithCredentials("user", "password")
    .WithEncryptionKey("rscp_key")
    .WithPolling(TimeSpan.FromSeconds(2), [
        RscpTag.EMS_REQ_POWER_PV,
        RscpTag.EMS_REQ_BAT_SOC,
    ])
    .WithReconnect(
        min: TimeSpan.FromSeconds(1),
        max: TimeSpan.FromSeconds(30))
    .Build();
```

## Methods

| Method | Description |
|--------|-------------|
| `Connect(host, port)` | E3DC IP address and port (default 5033) |
| `WithCredentials(user, password)` | E3DC portal login credentials |
| `WithEncryptionKey(key)` | RSCP encryption password (configured on device) |
| `WithPolling(interval, tags)` | Enable automatic polling with interval and tag list |
| `WithReconnect(min, max)` | Reconnection backoff bounds (default 1s-30s) |
| `Build(actorSystem?)` | Create the client (optional: provide existing ActorSystem) |

## RscpFlowSettings

For direct flow usage, settings are configured via a record:

```csharp
new RscpFlowSettings
{
    PollingInterval = TimeSpan.FromSeconds(2),
    MinReconnectBackoff = TimeSpan.FromSeconds(1),
    MaxReconnectBackoff = TimeSpan.FromSeconds(30),
    ReconnectRandomFactor = 0.2,
    MaxReconnectAttempts = -1, // unlimited
    SendTimeout = TimeSpan.FromSeconds(5),
}
```
