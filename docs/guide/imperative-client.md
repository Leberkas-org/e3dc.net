# Imperative Client

The `RscpClient` provides async/await request-response communication using the fluent `RscpRequest` builder.

## Reading Data

```csharp
using E3dc;
using E3dc.Descriptors;
using E3dc.Messages;

// Read all EMS tags at once
var response = await client.SendAsync(
    RscpRequest.Create().Read(Ems.All));

// Or pick specific tags
var response = await client.SendAsync(
    RscpRequest.Create()
        .Read(Ems.PowerPv, Ems.PowerBat, Ems.PowerGrid, Ems.PowerHome)
        .Read(Ems.BatSoc, Ems.Autarky, Ems.SelfConsumption));

if (response is RscpDataResponse data)
{
    var snapshot = data.ToEmsPowerSnapshot();
    // Use snapshot.PvWatts, snapshot.Soc, etc.
}
```

## Writing Data

```csharp
var response = await client.SendAsync(
    RscpRequest.Create()
        .Write(Ems.SetPowerMode, (byte)1));  // Idle mode
```

For grouped writes that need a container:

```csharp
var response = await client.SendAsync(
    RscpRequest.Create()
        .Container(Ems.SetPower, b => b
            .Write(Ems.SetPowerMode, (byte)3)     // Charge mode
            .Write(Ems.SetPowerValue, 2000)));     // 2000 watts
```

## Indexed Devices (PVI, BAT, PM, WB)

Devices with multiple instances (inverters, batteries, etc.) use `FromDevice()`:

```csharp
// Read all inverter tags from device 0
var response = await client.SendAsync(
    RscpRequest.Create()
        .FromDevice(Pvi.Device, index: 0, b => b.Read(Pvi.All)));

// Or pick specific tags
var response = await client.SendAsync(
    RscpRequest.Create()
        .FromDevice(Pvi.Device, index: 0, b => b
            .Read(Pvi.AcPower, Pvi.AcVoltage, Pvi.DcPower)));
```

Trying to use an `IndexedTag` at the top level is a **compile error**:

```csharp
// This won't compile — Pvi.AcPower is IndexedTag, not TagDescriptor
RscpRequest.Create().Read(Pvi.AcPower);  // CS1503
```

## Composing Mixed Requests

Read top-level EMS data and indexed device data in one request:

```csharp
var response = await client.SendAsync(
    RscpRequest.Create()
        .Read(Ems.All)
        .FromDevice(Pvi.Device, 0, b => b.Read(Pvi.All))
        .FromDevice(Bat.Device, 0, b => b.Read(Bat.All)));
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
var response = await client.SendAsync(request, cts.Token);
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
