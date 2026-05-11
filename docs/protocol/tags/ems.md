# EMS Tags (Energy Management System)

Namespace prefix: `0x01`

## Power Values

| Tag | Hex | Type | Description |
|-----|-----|------|-------------|
| `EMS_REQ_POWER_PV` | `0x01000001` | None | Request PV power |
| `EMS_POWER_PV` | `0x01800001` | Int32 | PV generation in watts |
| `EMS_REQ_POWER_BAT` | `0x01000002` | None | Request battery power |
| `EMS_POWER_BAT` | `0x01800002` | Int32 | Battery power in watts (negative = charging) |
| `EMS_REQ_POWER_HOME` | `0x01000003` | None | Request home consumption |
| `EMS_POWER_HOME` | `0x01800003` | Int32 | Home consumption in watts |
| `EMS_REQ_POWER_GRID` | `0x01000004` | None | Request grid power |
| `EMS_POWER_GRID` | `0x01800004` | Int32 | Grid power in watts (negative = feed-in) |
| `EMS_REQ_POWER_ADD` | `0x01000005` | None | Request additional power |
| `EMS_POWER_ADD` | `0x01800005` | Int32 | Additional source power in watts |

## Battery State

| Tag | Hex | Type | Description |
|-----|-----|------|-------------|
| `EMS_REQ_BAT_SOC` | `0x01000008` | None | Request state of charge |
| `EMS_BAT_SOC` | `0x01800008` | Float32 | Battery SOC in percent (0-100) |
| `EMS_REQ_AUTARKY` | `0x01000006` | None | Request autarky rate |
| `EMS_AUTARKY` | `0x01800006` | Float32 | Autarky percentage |
| `EMS_REQ_SELF_CONSUMPTION` | `0x01000007` | None | Request self-consumption rate |
| `EMS_SELF_CONSUMPTION` | `0x01800007` | Float32 | Self-consumption percentage |

## Control

| Tag | Hex | Type | Description |
|-----|-----|------|-------------|
| `EMS_REQ_SET_POWER` | `0x01000030` | Container | Set power mode + value |
| `EMS_REQ_SET_POWER_MODE` | `0x01000031` | UChar8 | Power mode (0=normal, 1=idle, 2=discharge, 3=charge, 4=grid charge) |
| `EMS_REQ_SET_POWER_VALUE` | `0x01000032` | Int32 | Power value in watts |
| `EMS_SET_POWER` | `0x01800030` | Container | Confirmation |

## Emergency Power

| Tag | Hex | Type | Description |
|-----|-----|------|-------------|
| `EMS_REQ_EMERGENCY_POWER_STATUS` | `0x01000073` | None | Request EP status |
| `EMS_EMERGENCY_POWER_STATUS` | `0x01800073` | UChar8 | EP status |
| `EMS_REQ_SET_EMERGENCY_POWER` | `0x01000074` | UChar8 | Enable/disable EP |
| `EMS_SET_EMERGENCY_POWER` | `0x01800074` | UChar8 | Confirmation |

## Limits

| Tag | Hex | Type | Description |
|-----|-----|------|-------------|
| `EMS_REQ_MAX_CHARGE_POWER` | `0x01000101` | None | Request max charge power |
| `EMS_MAX_CHARGE_POWER` | `0x01800101` | Int32 | Max charge power in watts |
| `EMS_REQ_MAX_DISCHARGE_POWER` | `0x01000102` | None | Request max discharge power |
| `EMS_MAX_DISCHARGE_POWER` | `0x01800102` | Int32 | Max discharge power in watts |
