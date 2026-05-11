# PVI Tags (Photovoltaic Inverter)

Namespace prefix: `0x02`

PVI tags use a data request pattern: send `PVI_REQ_DATA` as a container with `PVI_INDEX` and the desired sub-tags.

## AC Side

| Tag | Hex | Type | Description |
|-----|-----|------|-------------|
| `PVI_AC_POWER` | `0x028AC001` | Float32 | AC power per phase in watts |
| `PVI_AC_VOLTAGE` | `0x028AC002` | Float32 | AC voltage per phase in volts |
| `PVI_AC_CURRENT` | `0x028AC003` | Float32 | AC current per phase in amps |
| `PVI_AC_FREQUENCY` | `0x028AC00A` | Float32 | Grid frequency in Hz |
| `PVI_AC_ENERGY_ALL` | `0x028AC006` | Float32 | Total AC energy in Wh |
| `PVI_AC_ENERGY_DAY` | `0x028AC008` | Float32 | Today's AC energy in Wh |

## DC Side

| Tag | Hex | Type | Description |
|-----|-----|------|-------------|
| `PVI_DC_POWER` | `0x028DC001` | Float32 | DC power per string in watts |
| `PVI_DC_VOLTAGE` | `0x028DC002` | Float32 | DC voltage per string in volts |
| `PVI_DC_CURRENT` | `0x028DC003` | Float32 | DC current per string in amps |

## Status

| Tag | Hex | Type | Description |
|-----|-----|------|-------------|
| `PVI_REQ_ON_GRID` | `0x02000001` | None | Request grid connection status |
| `PVI_ON_GRID` | `0x02800001` | Bool | True if connected to grid |
| `PVI_REQ_STATE` | `0x02000002` | None | Request inverter state |
| `PVI_STATE` | `0x02800002` | UInt32 | Inverter state flags |
