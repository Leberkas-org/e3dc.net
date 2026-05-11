# BAT Tags (Battery)

Namespace prefix: `0x03`

BAT tags use the same data request pattern as PVI: send `BAT_REQ_DATA` as a container with `BAT_INDEX` and desired sub-tags.

## State

| Tag | Hex | Type | Description |
|-----|-----|------|-------------|
| `BAT_RSOC` | `0x03800001` | Float32 | Relative state of charge (%) |
| `BAT_MODULE_VOLTAGE` | `0x03800002` | Float32 | Module voltage in volts |
| `BAT_CURRENT` | `0x03800003` | Float32 | Battery current in amps |
| `BAT_CHARGE_CYCLES` | `0x03800008` | Int32 | Number of charge cycles |
| `BAT_STATUS_CODE` | `0x0380000A` | Int32 | Battery status code |
| `BAT_ERROR_CODE` | `0x0380000B` | Int32 | Battery error code |

## Limits

| Tag | Hex | Type | Description |
|-----|-----|------|-------------|
| `BAT_MAX_BAT_VOLTAGE` | `0x03800004` | Float32 | Maximum battery voltage |
| `BAT_MAX_CHARGE_CURRENT` | `0x03800005` | Float32 | Max charge current in amps |
| `BAT_EOD_VOLTAGE` | `0x03800006` | Float32 | End-of-discharge voltage |
| `BAT_MAX_DISCHARGE_CURRENT` | `0x03800007` | Float32 | Max discharge current in amps |

## DCB (Battery Modules)

| Tag | Hex | Type | Description |
|-----|-----|------|-------------|
| `BAT_DCB_COUNT` | `0x0380000D` | Int32 | Number of DCB modules |
| `BAT_DCB_CELL_VOLTAGE` | `0x0380001B` | Float32 | Individual cell voltage |
| `BAT_DCB_CELL_TEMPERATURE` | `0x03800019` | Float32 | Individual cell temperature |
| `BAT_MAX_DCB_CELL_TEMPERATURE` | `0x03800016` | Float32 | Max cell temperature |
| `BAT_MIN_DCB_CELL_TEMPERATURE` | `0x03800017` | Float32 | Min cell temperature |
