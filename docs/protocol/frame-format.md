# Frame Format

Every RSCP message is wrapped in a frame with a fixed 18-byte header, followed by data items and an optional CRC32 checksum.

## Frame Header (18 bytes)

| Offset | Size | Field | Description |
|--------|------|-------|-------------|
| 0 | 2 | Magic | `0xE3DC` (little-endian) |
| 2 | 2 | Control | Version + flags (see below) |
| 4 | 8 | Seconds | Unix timestamp, `int64` LE |
| 12 | 4 | Nanoseconds | Sub-second precision, `int32` LE |
| 16 | 2 | Data Length | Length of all data items, `uint16` LE |

### Control Field (2 bytes)

```
Bits [3:0]  — Protocol version (always 0x01)
Bit  [4]    — CRC enabled (1 = CRC32 appended after data)
Bits [15:5] — Reserved (0)
```

When the CRC bit is set, a 4-byte IEEE CRC32 is appended after all data items.

## Data Item Format (7-byte header + value)

Each data item within a frame uses Tag-Length-Value encoding:

| Offset | Size | Field | Description |
|--------|------|-------|-------------|
| 0 | 4 | Tag | Tag identifier, `uint32` LE |
| 4 | 1 | DataType | Type of the value (see [Data Types](./data-types)) |
| 5 | 2 | Length | Length of the value in bytes, `uint16` LE |
| 7 | N | Value | The actual data (type-dependent) |

Data items with `DataType = Container (0x0E)` contain nested data items in their value field.

## Example: Requesting PV Power

A frame requesting `EMS_REQ_POWER_PV` (tag `0x01000001`):

```
E3 DC       — Magic
11 00       — Control: version 1, CRC enabled
XX XX XX XX XX XX XX XX  — Timestamp seconds
XX XX XX XX              — Timestamp nanoseconds
07 00       — Data length: 7 bytes (one empty tag)
01 00 00 01 — Tag: EMS_REQ_POWER_PV
00          — DataType: None
00 00       — Length: 0
XX XX XX XX — CRC32
```

The response would contain `EMS_POWER_PV` (tag `0x01800001`) with a 4-byte `Int32` value representing watts.
