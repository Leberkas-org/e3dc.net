# DB Tags (Database / History)

Namespace prefix: `0x06`

Historical energy data can be queried by time period.

## History Requests

| Tag | Hex | Type | Description |
|-----|-----|------|-------------|
| `DB_REQ_HISTORY_DATA_DAY` | `0x06000100` | Container | Request daily history |
| `DB_REQ_HISTORY_DATA_WEEK` | `0x06000200` | Container | Request weekly history |
| `DB_REQ_HISTORY_DATA_MONTH` | `0x06000300` | Container | Request monthly history |
| `DB_REQ_HISTORY_DATA_YEAR` | `0x06000400` | Container | Request yearly history |

## History Responses

| Tag | Hex | Type | Description |
|-----|-----|------|-------------|
| `DB_HISTORY_DATA_DAY` | `0x06800100` | Container | Daily history data |
| `DB_HISTORY_DATA_WEEK` | `0x06800200` | Container | Weekly history data |
| `DB_HISTORY_DATA_MONTH` | `0x06800300` | Container | Monthly history data |
| `DB_HISTORY_DATA_YEAR` | `0x06800400` | Container | Yearly history data |

## Data Fields

| Tag | Hex | Type | Description |
|-----|-----|------|-------------|
| `DB_SUM_CONTAINER` | `0x06800010` | Container | Summary data |
| `DB_BAT_POWER_IN` | `0x06800002` | Float32 | Battery charge energy (Wh) |
| `DB_BAT_POWER_OUT` | `0x06800003` | Float32 | Battery discharge energy (Wh) |
| `DB_GRID_POWER_IN` | `0x06800005` | Float32 | Grid import energy (Wh) |
| `DB_GRID_POWER_OUT` | `0x06800006` | Float32 | Grid export energy (Wh) |
| `DB_CONSUMPTION` | `0x06800007` | Float32 | Total consumption (Wh) |
