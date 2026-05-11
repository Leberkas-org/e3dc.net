# EP Tags (Emergency Power)

Namespace prefix: `0x0B`

## Status

| Tag | Hex | Type | Description |
|-----|-----|------|-------------|
| `EP_REQ_IS_READY_FOR_SWITCH` | `0x0B000003` | None | Request EP readiness |
| `EP_IS_READY_FOR_SWITCH` | `0x0B800003` | Bool | True if EP is ready |
| `EP_REQ_IS_GRID_CONNECTED` | `0x0B000004` | None | Request grid connection |
| `EP_IS_GRID_CONNECTED` | `0x0B800004` | Bool | True if grid is connected |
| `EP_REQ_IS_ISLAND_GRID` | `0x0B000005` | None | Request island mode |
| `EP_IS_ISLAND_GRID` | `0x0B800005` | Bool | True if running in island mode |

When `EP_IS_ISLAND_GRID` is true, the system is disconnected from the grid and running on battery + PV power only. This happens during a power outage when emergency power is enabled.
