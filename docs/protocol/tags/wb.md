# WB Tags (Wallbox)

Namespace prefix: `0x0E`

Wallbox data uses the same indexed data request pattern as PVI and BAT.

## Energy

| Tag | Hex | Type | Description |
|-----|-----|------|-------------|
| `WB_REQ_ENERGY_ALL` | `0x0E000001` | None | Request total energy |
| `WB_ENERGY_ALL` | `0x0E800001` | Double64 | Total charged energy (Wh) |
| `WB_REQ_ENERGY_SOLAR` | `0x0E000002` | None | Request solar energy |
| `WB_ENERGY_SOLAR` | `0x0E800002` | Double64 | Solar-charged energy (Wh) |

## Status

| Tag | Hex | Type | Description |
|-----|-----|------|-------------|
| `WB_REQ_STATUS` | `0x0E000004` | None | Request wallbox status |
| `WB_STATUS` | `0x0E800004` | Int32 | Wallbox status code |
| `WB_REQ_ERROR_CODE` | `0x0E000005` | None | Request error code |
| `WB_ERROR_CODE` | `0x0E800005` | Int32 | Error code |
| `WB_REQ_MODE` | `0x0E000006` | None | Request charging mode |
| `WB_MODE` | `0x0E800006` | UChar8 | Charging mode |

## Power Meter

| Tag | Hex | Type | Description |
|-----|-----|------|-------------|
| `WB_REQ_PM_POWER_L1` | `0x0E00000C` | None | Request L1 power |
| `WB_PM_POWER_L1` | `0x0E80000C` | Float32 | Phase L1 power in watts |
| `WB_REQ_PM_POWER_L2` | `0x0E00000D` | None | Request L2 power |
| `WB_PM_POWER_L2` | `0x0E80000D` | Float32 | Phase L2 power in watts |
| `WB_REQ_PM_POWER_L3` | `0x0E00000E` | None | Request L3 power |
| `WB_PM_POWER_L3` | `0x0E80000E` | Float32 | Phase L3 power in watts |
