# PM Tags (Power Meter)

Namespace prefix: `0x05`

Power meter data is available per phase (L1, L2, L3).

## Power

| Tag | Hex | Type | Description |
|-----|-----|------|-------------|
| `PM_REQ_POWER_L1` | `0x05000001` | None | Request L1 power |
| `PM_POWER_L1` | `0x05800001` | Float32 | Phase L1 power in watts |
| `PM_REQ_POWER_L2` | `0x05000002` | None | Request L2 power |
| `PM_POWER_L2` | `0x05800002` | Float32 | Phase L2 power in watts |
| `PM_REQ_POWER_L3` | `0x05000003` | None | Request L3 power |
| `PM_POWER_L3` | `0x05800003` | Float32 | Phase L3 power in watts |

## Voltage

| Tag | Hex | Type | Description |
|-----|-----|------|-------------|
| `PM_REQ_VOLTAGE_L1` | `0x05000011` | None | Request L1 voltage |
| `PM_VOLTAGE_L1` | `0x05800011` | Float32 | Phase L1 voltage in volts |
| `PM_REQ_VOLTAGE_L2` | `0x05000012` | None | Request L2 voltage |
| `PM_VOLTAGE_L2` | `0x05800012` | Float32 | Phase L2 voltage in volts |
| `PM_REQ_VOLTAGE_L3` | `0x05000013` | None | Request L3 voltage |
| `PM_VOLTAGE_L3` | `0x05800013` | Float32 | Phase L3 voltage in volts |

## Energy

| Tag | Hex | Type | Description |
|-----|-----|------|-------------|
| `PM_REQ_ENERGY_L1` | `0x05000006` | None | Request L1 energy |
| `PM_ENERGY_L1` | `0x05800006` | Double64 | Phase L1 energy in Wh |
| `PM_REQ_ENERGY_L2` | `0x05000007` | None | Request L2 energy |
| `PM_ENERGY_L2` | `0x05800007` | Double64 | Phase L2 energy in Wh |
| `PM_REQ_ENERGY_L3` | `0x05000008` | None | Request L3 energy |
| `PM_ENERGY_L3` | `0x05800008` | Double64 | Phase L3 energy in Wh |
