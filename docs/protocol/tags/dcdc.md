# DCDC Tags (DC-DC Converter)

Namespace prefix: `0x04`

The DCDC namespace provides metrics from the battery-side DC-DC converter. This is an indexed device — use `FromDevice(Dcdc.Device, index, ...)` to query.

## Data Container

| Tag | Hex | Type | Description |
|-----|-----|------|-------------|
| `DCDC_REQ_DATA` | `0x04040000` | Container | Request container (indexed) |
| `DCDC_DATA` | `0x04840000` | Container | Response container |
| `DCDC_INDEX` | `0x04040001` | UInt16 | Device index |

## Battery Metrics

| Tag | Hex | Type | Description |
|-----|-----|------|-------------|
| `DCDC_REQ_I_BAT` | `0x04000001` | None | Request battery current at converter |
| `DCDC_I_BAT` | `0x04800001` | Float32 | Battery current (A) |
| `DCDC_REQ_U_BAT` | `0x04000002` | None | Request battery voltage at converter |
| `DCDC_U_BAT` | `0x04800002` | Float32 | Battery voltage (V) |
| `DCDC_REQ_P_BAT` | `0x04000003` | None | Request battery power at converter |
| `DCDC_P_BAT` | `0x04800003` | Float32 | Battery power (W) |

These values represent the actual electrical measurements at the DC-DC converter level, which may differ slightly from the EMS-reported values due to conversion losses.

## Usage

```csharp
var request = RscpRequest.Create()
    .FromDevice(Dcdc.Device, 0, b => b
        .Read(Dcdc.IBat, Dcdc.UBat, Dcdc.PBat));
```

## Typed Snapshot

```csharp
var dcdc = response.ToDcdcSnapshot();
// DcdcSnapshot(float BatteryCurrent, float BatteryVoltage, float BatteryPower)
```
