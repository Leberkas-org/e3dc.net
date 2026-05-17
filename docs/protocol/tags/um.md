# UM Tags (Update Manager)

Namespace prefix: `0x0D`

The UM namespace provides firmware update status and version checking.

## Tags

| Tag | Hex | Type | Description |
|-----|-----|------|-------------|
| `UM_REQ_UPDATE_STATUS` | `0x0D000001` | None | Request current firmware update status |
| `UM_UPDATE_STATUS` | `0x0D800001` | — | Update status response |
| `UM_REQ_CHECK_FOR_UPDATES` | `0x0D000003` | None | Trigger a check for available updates |
| `UM_CHECK_FOR_UPDATES` | `0x0D800003` | — | Update check result |

## Usage

```csharp
var request = RscpRequest.Create()
    .Read(Um.UpdateStatus);
```

`UM_REQ_CHECK_FOR_UPDATES` is a write command that triggers the E3DC to check for firmware updates. The response indicates whether updates are available.
