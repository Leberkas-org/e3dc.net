# Data Types

Each RSCP data item carries a 1-byte type identifier that determines how the value field is interpreted.

## Type Table

| Hex | Name | Size (bytes) | .NET Type | Notes |
|-----|------|:------------:|-----------|-------|
| `0x00` | None | 0 | — | Request tag, no value |
| `0x01` | Bool | 1 | `bool` | 0 = false, non-zero = true |
| `0x02` | Char8 | 1 | `sbyte` | Signed 8-bit |
| `0x03` | UChar8 | 1 | `byte` | Unsigned 8-bit |
| `0x04` | Int16 | 2 | `short` | Little-endian |
| `0x05` | UInt16 | 2 | `ushort` | Little-endian |
| `0x06` | Int32 | 4 | `int` | Little-endian |
| `0x07` | UInt32 | 4 | `uint` | Little-endian |
| `0x08` | Int64 | 8 | `long` | Little-endian |
| `0x09` | UInt64 | 8 | `ulong` | Little-endian |
| `0x0A` | Float32 | 4 | `float` | IEEE 754 |
| `0x0B` | Double64 | 8 | `double` | IEEE 754 |
| `0x0C` | Bitfield | 1 | `byte` | Bit flags |
| `0x0D` | CString | Variable | `string` | UTF-8 encoded |
| `0x0E` | Container | Variable | `RscpDataItem[]` | Nested data items |
| `0x0F` | Timestamp | 12 | `DateTimeOffset` | See below |
| `0x10` | ByteArray | Variable | `byte[]` | Raw binary |
| `0xFF` | Error | Variable | — | Error response |

## Special Types

### Container (`0x0E`)

A container's value field holds one or more nested data items, each with their own Tag-Type-Length-Value header. Containers are used for grouping related tags (e.g., authentication request contains user + password).

### Timestamp (`0x0F`)

12 bytes total:
- Bytes 0-7: Unix seconds since epoch (`int64` LE)
- Bytes 8-11: Nanoseconds within the second (`int32` LE)

### Error (`0xFF`)

Returned when a tag request fails. Known error codes:

| Value | Meaning |
|-------|---------|
| `0x01` | NOT_HANDLED |
| `0x02` | ACCESS_DENIED |
| `0x03` | FORMAT |
| `0x04` | AGAIN |
| `0x05` | OUT_OF_BOUNDS |
| `0x06` | NOT_AVAILABLE |
| `0x07` | UNKNOWN_TAG |
| `0x08` | ALREADY_IN_USE |
