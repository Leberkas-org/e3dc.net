# Typed Snapshots

The `E3dc.Typed` package provides strongly-typed records parsed from raw RSCP responses.

## Available Snapshots

### EmsPowerSnapshot

```csharp
public sealed record EmsPowerSnapshot(
    int PvWatts,           // Solar generation
    int BatteryWatts,      // Battery power (negative = charging)
    int GridWatts,         // Grid power (negative = feed-in)
    int HomeWatts,         // Home consumption
    int AdditionalWatts,   // Additional sources
    float Soc,             // State of charge (0-100%)
    float Autarky,         // Autarky rate (%)
    float SelfConsumption  // Self-consumption rate (%)
);
```

### BatterySnapshot

```csharp
public sealed record BatterySnapshot(
    float Rsoc,        // Relative state of charge (%)
    float Voltage,     // Module voltage (V)
    float Current,     // Current (A)
    int ChargeCycles,  // Number of cycles
    int StatusCode,    // Battery status
    int ErrorCode      // Error code (0 = no error)
);
```

### DeviceInfo

```csharp
public sealed record DeviceInfo(
    string SerialNumber,
    string ProductionDate,
    string SwRelease,
    string IpAddress,
    string SubnetMask,
    string Gateway
);
```

Also available: `InverterSnapshot`, `PowerMeterSnapshot`, `WallboxSnapshot`.

## Parsing Responses

Use the extension methods on `RscpDataResponse`:

```csharp
if (response is RscpDataResponse data)
{
    var ems = data.ToEmsPowerSnapshot();     // returns null if no EMS tags
    var bat = data.ToBatterySnapshot();       // returns null if no BAT tags
    var info = data.ToDeviceInfo();           // returns null if no INFO tags
}
```
