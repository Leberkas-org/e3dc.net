# INFO Tags (Device Information)

Namespace prefix: `0x0A`

## Device Identity

| Tag | Hex | Type | Description |
|-----|-----|------|-------------|
| `INFO_REQ_SERIAL_NUMBER` | `0x0A000001` | None | Request serial number |
| `INFO_SERIAL_NUMBER` | `0x0A800001` | CString | Device serial number |
| `INFO_REQ_PRODUCTION_DATE` | `0x0A000002` | None | Request production date |
| `INFO_PRODUCTION_DATE` | `0x0A800002` | CString | Production date string |
| `INFO_REQ_SW_RELEASE` | `0x0A000019` | None | Request software version |
| `INFO_SW_RELEASE` | `0x0A800019` | CString | Software release string |

## Network

| Tag | Hex | Type | Description |
|-----|-----|------|-------------|
| `INFO_REQ_IP_ADDRESS` | `0x0A000008` | None | Request IP address |
| `INFO_IP_ADDRESS` | `0x0A800008` | CString | Device IP address |
| `INFO_REQ_SUBNET_MASK` | `0x0A000009` | None | Request subnet mask |
| `INFO_SUBNET_MASK` | `0x0A800009` | CString | Subnet mask |
| `INFO_REQ_GATEWAY` | `0x0A00000B` | None | Request gateway |
| `INFO_GATEWAY` | `0x0A80000B` | CString | Default gateway |
| `INFO_REQ_DNS` | `0x0A00000C` | None | Request DNS server |
| `INFO_DNS` | `0x0A80000C` | CString | DNS server address |

## Time

| Tag | Hex | Type | Description |
|-----|-----|------|-------------|
| `INFO_REQ_TIME` | `0x0A00000E` | None | Request system time |
| `INFO_TIME` | `0x0A80000E` | Timestamp | Current system time |
| `INFO_REQ_TIME_ZONE` | `0x0A000010` | None | Request timezone |
| `INFO_TIME_ZONE` | `0x0A800010` | CString | Timezone string |
