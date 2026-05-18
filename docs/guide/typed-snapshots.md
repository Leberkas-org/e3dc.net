# Typed Snapshots

The `E3dc.Net` package provides strongly-typed records parsed from raw RSCP responses. One `using E3dc;` gives you access to all snapshot types.

## Available Snapshots

### EmsPowerSnapshot

```csharp
public sealed record EmsPowerSnapshot(
    int PvWatts,           // Solar generation
    int BatteryWatts,      // Battery power (positive = charging)
    int GridWatts,         // Grid power (positive = import, negative = feed-in)
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

### InverterSnapshot

```csharp
public sealed record InverterSnapshot(
    float AcPowerL1, float AcPowerL2, float AcPowerL3,       // AC power per phase (W)
    float AcVoltageL1, float AcVoltageL2, float AcVoltageL3, // AC voltage per phase (V)
    float DcPower, float DcVoltage, float DcCurrent,          // DC side
    float Frequency                                            // Grid frequency (Hz)
);
```

### PowerMeterSnapshot

```csharp
public sealed record PowerMeterSnapshot(
    float PowerL1, float PowerL2, float PowerL3,       // Power per phase (W)
    float VoltageL1, float VoltageL2, float VoltageL3, // Voltage per phase (V)
    double EnergyL1, double EnergyL2, double EnergyL3  // Cumulative energy (Wh)
);
```

### DcdcSnapshot

```csharp
public sealed record DcdcSnapshot(
    float BatteryCurrent,  // DC-DC converter battery current (A)
    float BatteryVoltage,  // DC-DC converter battery voltage (V)
    float BatteryPower     // DC-DC converter battery power (W)
);
```

### EmergencyPowerSnapshot

```csharp
public sealed record EmergencyPowerSnapshot(
    bool IsReadyForSwitch,  // Can switch to island mode
    bool IsGridConnected,   // Connected to utility grid
    bool IsIslandGrid       // Running in island mode
);
```

### WallboxSnapshot

```csharp
public sealed record WallboxSnapshot(
    double EnergyAll, double EnergySolar,                  // Total / solar energy (Wh)
    int Status, int ErrorCode, int Mode,                   // Status codes
    float PowerL1, float PowerL2, float PowerL3            // Charging power per phase (W)
);
```

### DeviceInfo

```csharp
public sealed record DeviceInfo(
    string SerialNumber, string ProductionDate, string SwRelease,
    string IpAddress, string SubnetMask, string Gateway
);
```

## Parsing Responses

Use the extension methods on `RscpDataResponse`:

```csharp
using E3dc;
using E3dc.Descriptors;
using E3dc.Messages;

// Request all EMS + battery data
var request = RscpRequest.Create()
    .Read(Ems.All)
    .FromDevice(Bat.Device, 0, b => b.Read(Bat.All));

// Parse the response
if (response is RscpDataResponse data)
{
    var ems  = data.ToEmsPowerSnapshot();          // null if no EMS tags
    var bat  = data.ToBatterySnapshot();           // null if no BAT tags
    var pvi  = data.ToInverterSnapshot();          // null if no PVI tags
    var pm   = data.ToPowerMeterSnapshot();        // null if no PM tags
    var dcdc = data.ToDcdcSnapshot();              // null if no DCDC tags
    var ep   = data.ToEmergencyPowerSnapshot();    // null if no EP tags
    var wb   = data.ToWallboxSnapshot();           // null if no WB tags
    var info = data.ToDeviceInfo();                // null if no INFO tags
}
```

::: tip
Each `To*Snapshot()` method returns `null` if the response doesn't contain any matching tags. The snapshot assumes you requested all relevant tags — unrequested fields default to `0` / `false` / `""`.
:::
