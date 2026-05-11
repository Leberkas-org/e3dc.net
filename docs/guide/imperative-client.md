# Imperative Client

The `RscpClient` provides async/await request-response communication.

## Reading Data

```csharp
var response = await client.SendAsync(new ReadTagsCommand([
    RscpTag.EMS_REQ_POWER_PV,
    RscpTag.EMS_REQ_POWER_BAT,
    RscpTag.EMS_REQ_POWER_GRID,
    RscpTag.EMS_REQ_POWER_HOME,
    RscpTag.EMS_REQ_BAT_SOC,
    RscpTag.EMS_REQ_AUTARKY,
    RscpTag.EMS_REQ_SELF_CONSUMPTION,
]));

if (response is RscpDataResponse data)
{
    var snapshot = data.ToEmsPowerSnapshot();
    // Use snapshot.PvWatts, snapshot.Soc, etc.
}
```

## Writing Data

```csharp
var response = await client.SendAsync(new WriteTagCommand(
    RscpTag.EMS_REQ_SET_POWER_MODE,
    RscpDataType.UChar8,
    new byte[] { 1 } // Idle mode
));
```

## Error Handling

```csharp
switch (response)
{
    case RscpDataResponse data:
        // Success — parse the data
        break;
    case RscpErrorResponse error:
        Console.Error.WriteLine($"Error: {error.Message}");
        break;
}
```

## Cancellation

All operations support `CancellationToken`:

```csharp
using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
var response = await client.SendAsync(command, cts.Token);
```

## Disposal

`RscpClient` implements `IAsyncDisposable`. Always dispose when done:

```csharp
await using var client = new RscpClientBuilder()
    .Connect("192.168.1.100")
    .WithCredentials("user", "pass")
    .WithEncryptionKey("key")
    .Build();
```
