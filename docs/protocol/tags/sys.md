# SYS Tags (System Control)

Namespace prefix: `0x0C`

The SYS namespace provides system management commands. **Use with caution** — these commands affect the E3DC hardware directly.

## Commands

| Tag | Hex | Type | Description |
|-----|-----|------|-------------|
| `SYS_REQ_SYSTEM_REBOOT` | `0x0C000001` | None | Reboot the entire E3DC system |
| `SYS_SYSTEM_REBOOT` | `0x0C800001` | — | Reboot confirmation response |
| `SYS_REQ_RESTART_APPLICATION` | `0x0C000003` | None | Restart the E3DC application only |
| `SYS_RESTART_APPLICATION` | `0x0C800003` | — | Restart confirmation response |

## Usage

```csharp
// Restart application (less invasive)
var request = RscpRequest.Create()
    .Read(Sys.RestartApp);

// Full system reboot (more invasive)
var request = RscpRequest.Create()
    .Read(Sys.Reboot);
```

::: warning
`SYS_REQ_SYSTEM_REBOOT` performs a full hardware reboot. The system will be offline for several minutes. Only use this when necessary.

`SYS_REQ_RESTART_APPLICATION` restarts the E3DC software without a full reboot. The system recovers faster.
:::
