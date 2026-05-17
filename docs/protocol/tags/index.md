# Tag Reference

RSCP tags use a 32-bit identifier where the high byte indicates the namespace. Request tags have `0x0` in the response nibble, response tags have `0x8`.

## Namespaces

| Prefix | Name | Description |
|--------|------|-------------|
| `0x00` | [RSCP](/protocol/authentication) | Authentication, protocol management |
| `0x01` | [EMS](./ems) | Energy Management System — power, SOC, modes |
| `0x02` | [PVI](./pvi) | Photovoltaic Inverter — AC/DC power |
| `0x03` | [BAT](./bat) | Battery — SOC, voltage, cycles |
| `0x04` | [DCDC](./dcdc) | DC-DC converter — battery-side metrics |
| `0x05` | [PM](./pm) | Power Meter — per-phase measurements |
| `0x06` | [DB](./db) | Database — historical data |
| `0x09` | [HA](./ha) | Home Automation — datapoints and actuators |
| `0x0A` | [INFO](./info) | Device information |
| `0x0B` | [EP](./ep) | Emergency Power |
| `0x0C` | [SYS](./sys) | System — reboot, restart |
| `0x0D` | [UM](./um) | Update Manager — firmware status |
| `0x0E` | [WB](./wb) | Wallbox |

## Tag Naming Convention

- `XXX_REQ_YYY` — Request tag (send to read data)
- `XXX_YYY` — Response tag (received with data)
- `XXX_REQ_SET_YYY` — Write request (send to change a value)
- `XXX_SET_YYY` — Write confirmation response

## Hex Pattern

```
0xNNTTTTTT
  ││└─────── Tag ID within namespace
  │└──────── 0=request, 4=param, 8=response
  └───────── Namespace
```
