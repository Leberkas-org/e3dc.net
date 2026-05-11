# E3DC RSCP Akka.Streams Client — Design Spec

Standalone Akka.Streams client for E3DC S10 Pro via RSCP (Remote Storage Control Protocol). Follows Hendl patterns (HendlClient, channels, RestartFlow, actor integration) but is self-contained.

## RSCP Protocol Summary

- Binary over TCP, port 5033
- Encryption: Rijndael-256 CBC (32-byte blocks, NOT standard AES). Key = password padded with `0xFF` to 32 bytes, IV starts all `0xFF`, CBC state chains across frames
- Frame: magic `0xE3DC` (uint16 LE) + ctrl (uint16, version+CRC flag) + timestamp (uint64 seconds + uint32 nanos) + dataLength (uint16) + data items + CRC32 (IEEE, optional per ctrl bit)
- Data item: Tag (uint32) + DataType (uint8) + Length (uint16) + Value (variable). Container type (0x0E) enables nesting
- Auth: send `RSCP_REQ_AUTHENTICATION` container with user+password strings, receive auth level
- Request/response tag convention: requests `0xXX0xxxxx`, responses `0xXX8xxxxx`, params `0xXX4xxxxx`

## Target

- net10.0, Akka.Streams 1.5.*, BouncyCastle for Rijndael-256
- Standalone project (no Hendl dependency), same architecture

## Project Structure

```
E3dcConnector.sln
src/
  E3dcConnector/                    # Core protocol + streaming
    Protocol/
      RscpFrame.cs                  # Frame encode/decode
      RscpDataItem.cs               # Tag-Type-Length-Value encoding
      RscpDataType.cs               # DataType enum
      RscpCrypt.cs                  # Rijndael-256 CBC (BouncyCastle)
    Tags/
      RscpTag.cs                    # Tag enum (all namespaces)
      RscpNamespace.cs              # Namespace helpers
    Messages/
      IRscpCommand.cs               # Command interface
      IRscpMessage.cs               # Response interface
      RscpRequestOptions.cs         # Correlation ID, timeout
      Commands/
        ReadTagsCommand.cs          # Request one or more tags
        WriteTagCommand.cs          # Write a value to a tag
      Responses/
        RscpDataResponse.cs         # Tag values returned
        RscpErrorResponse.cs        # Error with code
    Reactive/
      RscpFlow.cs                   # RestartFlow-wrapped pipeline
      RscpFlowSettings.cs           # Intervals, backoff, timeouts
      Stages/
        EncodeStage.cs              # Command -> frame bytes
        ExecuteStage.cs             # TCP send/receive + encryption
        DecodeStage.cs              # Frame bytes -> typed responses
      Internal/
        RscpConnection.cs           # TCP + Rijndael session
        PollingScheduler.cs         # Source.Tick for periodic reads
    Client/
      RscpClient.cs                 # Channels + ConcurrentDictionary correlation
      RscpClientExtensions.cs       # Materialize to channels / actor sink
  E3dcConnector.Typed/              # Typed wrappers per namespace
    Ems/
      EmsPowerSnapshot.cs
      EmsCommands.cs
    Bat/
      BatterySnapshot.cs
    Pvi/
      InverterSnapshot.cs
    Pm/
      PowerMeterSnapshot.cs
    Db/
      HistoryQuery.cs
    Wb/
      WallboxSnapshot.cs
    Info/
      DeviceInfo.cs
samples/
  E3dcConnector.Sample/             # Imperative client
  E3dcConnector.FlowSample/         # Raw channel streaming
  E3dcConnector.ActorSample/        # Actor digital twin
docs/
  .vitepress/
    config.ts                       # VitePress config (nav, sidebar, theme)
  index.md                          # Landing page
  protocol/
    overview.md                     # RSCP protocol introduction
    frame-format.md                 # Wire format with diagrams
    data-types.md                   # DataType enum + encoding rules
    tags/
      index.md                      # Tag namespace overview
      ems.md                        # EMS tags reference
      pvi.md                        # PVI tags reference
      bat.md                        # BAT tags reference
      pm.md                         # PM tags reference
      db.md                         # DB (history) tags reference
      wb.md                         # WB (wallbox) tags reference
      info.md                       # INFO tags reference
      ep.md                         # EP tags reference
    encryption.md                   # Rijndael-256 CBC details
    authentication.md               # Auth flow
  guide/
    getting-started.md              # Install, connect, first query
    imperative-client.md            # RscpClient usage
    streaming.md                    # Akka.Streams flow usage
    actor-integration.md            # Actor digital twin pattern
    polling.md                      # Polling subscriptions
    typed-snapshots.md              # Typed layer usage
    configuration.md                # Builder API reference
  architecture/
    index.md                        # Architecture overview (embeds LikeC4)
    likec4/
      model.c4                      # LikeC4 model (elements + relationships)
      views.c4                      # LikeC4 views (context, pipeline, layers)
  package.json                      # vitepress + likec4 dev dependencies
```

## Core Components

### RscpFrame

Serialize/deserialize the RSCP wire format:

```
Offset  Size   Field
0       2      Magic (0xE3DC, little-endian)
2       2      Control (4-bit version, 1-bit CRC enable, reserved)
4       8      Timestamp seconds (uint64)
12      4      Timestamp nanoseconds (uint32)
16      2      Data length (uint16)
18      N      Data items (RscpDataItem[])
18+N    4      CRC32 IEEE (if CRC bit set in control)
```

### RscpDataItem

Recursive TLV encoding:

```
Offset  Size   Field
0       4      Tag (uint32)
4       1      DataType (uint8)
5       2      Length (uint16)
7       N      Value (type-dependent, Container = nested RscpDataItem[])
```

### RscpDataType

```csharp
public enum RscpDataType : byte
{
    None       = 0x00,
    Bool       = 0x01,
    Char8      = 0x02,
    UChar8     = 0x03,
    Int16      = 0x04,
    UInt16     = 0x05,
    Int32      = 0x06,
    UInt32     = 0x07,
    Int64      = 0x08,
    UInt64     = 0x09,
    Float32    = 0x0A,
    Double64   = 0x0B,
    Bitfield   = 0x0C,
    CString    = 0x0D,
    Container  = 0x0E,
    Timestamp  = 0x0F,
    ByteArray  = 0x10,
    Error      = 0xFF,
}
```

### RscpCrypt

- BouncyCastle `RijndaelEngine` with 256-bit block size + `CbcBlockCipher` + zero-padding to 32-byte boundary
- Key: UTF-8 bytes of password, padded to 32 bytes with `0xFF`
- IV: 32 bytes of `0xFF`, then CBC chains across successive encrypt/decrypt calls (IV state persisted on `RscpConnection`)
- Encrypt after frame serialization, decrypt before frame parsing

### RscpConnection

Manages TCP socket + crypto state:

1. `ConnectAsync(host, port)` — open TCP socket
2. `AuthenticateAsync(user, password)` — send auth container, validate auth level
3. `SendFrameAsync(RscpFrame)` — serialize, encrypt, write to socket
4. `ReceiveFrameAsync()` — read from socket, decrypt, deserialize. Handle partial reads (frame reassembly)
5. Tracks encrypt/decrypt IV state across frames

### RscpFlow (Akka.Streams Pipeline)

```
Source.Tick (polling)  ─┐
                        Merge<IRscpCommand> → SelectAsync(1, ProcessCommand)
On-demand commands ─────┘                           ↓
                                              EncodeStage (→ RscpFrame)
                                                    ↓
                                              ExecuteStage (TCP send/receive)
                                                    ↓
                                              DecodeStage (→ IRscpMessage)
                                                    ↓
                                              Output (typed responses)
```

Wrapped in `RestartFlow.WithBackoff`:
- Min backoff: 1s
- Max backoff: 30s
- Random factor: 0.2
- On restart: new TCP connection + re-authenticate

### PollingScheduler

`Source.Tick(initialDelay, interval)` emitting `ReadTagsCommand` for the configured tag set. Merged with the on-demand command source before entering the pipeline.

### RscpClient

Mirrors HendlClient:

- `SendAsync(IRscpCommand, CancellationToken)` — write command to channel, await correlated response via `ConcurrentDictionary<string, TaskCompletionSource<IRscpMessage>>`
- `Subscribe<T>(Action<T> handler)` — register for typed snapshot notifications from polling
- Background dispatch loop reads from response channel:
  - Correlated response → complete matching TCS
  - Subscription message → broadcast to handlers

### Materialization

```csharp
public static (ChannelWriter<IRscpCommand>, ChannelReader<IRscpMessage>)
    Materialize(this Flow<IRscpCommand, IRscpMessage, NotUsed> flow, IMaterializer materializer)
```

Same pattern as Hendl: bounded command channel (256), unbounded response channel, flow runs in background.

## Typed Layer (E3dcConnector.Typed)

Records per namespace, decoded from raw tag responses:

```csharp
public sealed record EmsPowerSnapshot(
    int PvWatts,
    int BatteryWatts,
    int GridWatts,
    int HomeWatts,
    int AdditionalWatts,
    float Soc,
    float Autarky,
    float SelfConsumption);

public sealed record BatterySnapshot(
    float Rsoc,
    float Voltage,
    float Current,
    int ChargeCycles,
    int StatusCode,
    int ErrorCode);

public sealed record InverterSnapshot(
    float AcPowerL1, float AcPowerL2, float AcPowerL3,
    float AcVoltageL1, float AcVoltageL2, float AcVoltageL3,
    float DcPower, float DcVoltage, float DcCurrent,
    float Frequency);

public sealed record PowerMeterSnapshot(
    float PowerL1, float PowerL2, float PowerL3,
    float VoltageL1, float VoltageL2, float VoltageL3,
    double EnergyL1, double EnergyL2, double EnergyL3);

public sealed record WallboxSnapshot(
    double EnergyAll, double EnergySolar,
    int Status, int ErrorCode, int Mode,
    float PowerL1, float PowerL2, float PowerL3);

public sealed record DeviceInfo(
    string SerialNumber, string ProductionDate, string SwRelease,
    string IpAddress, string SubnetMask, string Gateway);
```

Command records for write operations:

```csharp
public sealed record SetPowerMode(EmsMode Mode, int ValueWatts) : IRscpCommand;
public sealed record SetChargeLimit(int LimitWatts) : IRscpCommand;
public sealed record SetEmergencyPower(bool Enable) : IRscpCommand;
```

`DecodeStage` maps groups of tag responses into these typed records using the same aggregation pattern as Hendl's `HandleAggregator`.

## Tag Coverage

All namespaces:

| Namespace | Prefix | Scope |
|-----------|--------|-------|
| RSCP | 0x00 | Auth, protocol |
| EMS | 0x01 | Power, SOC, modes, limits, emergency power |
| PVI | 0x02 | Inverter AC/DC power, voltage, current |
| BAT | 0x03 | Battery SOC, voltage, cycles, DCB cells |
| DCDC | 0x04 | DC-DC converter current, voltage, status |
| PM | 0x05 | Power meter per-phase power, voltage, energy |
| DB | 0x06 | Historical data (day/week/month/year) |
| HA | 0x09 | Home automation datapoints, actuators |
| INFO | 0x0A | Device info, network, time |
| EP | 0x0B | Emergency power readiness, grid/island state |
| SYS | 0x0C | System reboot, restart |
| UM | 0x0D | Update status |
| WB | 0x0E | Wallbox energy, status, mode, power |

## Error Handling

- Connection errors → `RestartFlow.WithBackoff` reconnects + re-authenticates
- CRC mismatch / invalid magic / decrypt failure → exception in `ExecuteStage`, triggers restart
- RSCP error tags (`0xXXFFFFFF`, DataType `0xFF`) → `RscpErrorResponse` routed via correlation ID
- Send timeout → TCS cancelled after configurable deadline

## Configuration API

```csharp
var client = new RscpClientBuilder()
    .Connect("192.168.1.100", 5033)
    .WithCredentials("user", "password")
    .WithEncryptionKey("myRscpPassword")
    .WithPolling(TimeSpan.FromSeconds(2), new[]
    {
        RscpTag.EMS_REQ_POWER_PV,
        RscpTag.EMS_REQ_POWER_BAT,
        RscpTag.EMS_REQ_POWER_GRID,
        RscpTag.EMS_REQ_POWER_HOME,
        RscpTag.EMS_REQ_BAT_SOC,
    })
    .WithReconnect(min: TimeSpan.FromSeconds(1), max: TimeSpan.FromSeconds(30))
    .Build(actorSystem);
```

## Testing

- Unit: frame roundtrip, Rijndael-256 encrypt/decrypt with known vectors, tag serialization, CRC32
- Integration: real E3DC connection, gated by env var (skipped in CI)

## VitePress Documentation

Docs site in `docs/` with two main sections:

### Protocol Reference

Comprehensive RSCP protocol documentation (the protocol is barely documented publicly — this fills that gap):

- **Frame Format** — binary layout with byte-level diagrams, magic bytes, control field bitflags, timestamp encoding, CRC32
- **Data Types** — all 17 types with encoding rules, sizes, .NET type mapping
- **Tag Reference** — per-namespace pages (EMS, PVI, BAT, PM, DB, WB, INFO, EP, etc.) with tag hex values, expected data types, request/response pairs, and descriptions
- **Encryption** — Rijndael-256 vs AES-256 differences, key derivation (password → 0xFF-padded 32 bytes), IV initialization and CBC chaining across frames
- **Authentication** — connection lifecycle, auth container structure, auth levels

### Library Guide

Usage documentation for the .NET library:

- **Getting Started** — NuGet install, minimal connect + read example
- **Imperative Client** — `RscpClient` with `SendAsync`, typed responses
- **Streaming** — raw channel materialization, async enumeration
- **Actor Integration** — `ConnectionActor` pattern, digital twin
- **Polling** — configuring periodic tag subscriptions
- **Typed Snapshots** — `EmsPowerSnapshot`, `BatterySnapshot`, etc.
- **Configuration** — builder API reference

### Setup

- `package.json` in `docs/` with `vitepress` as dev dependency
- VitePress config: sidebar navigation matching the section structure, search enabled
- Scripts: `docs:dev`, `docs:build`, `docs:preview`

## LikeC4 Architecture Visualization

Three views defined in `docs/architecture/likec4/`:

### 1. System Context

```
┌─────────────┐     RSCP/TCP      ┌──────────────────┐     C# API      ┌──────────────┐
│  E3DC S10   │◄────────────────►│  e3dc-connector  │◄───────────────►│  Your App    │
│  Pro        │   Rijndael-256    │  (this library)  │  HendlClient    │  (consumer)  │
└─────────────┘                   └──────────────────┘  pattern         └──────────────┘
```

Elements: E3DC hardware, e3dc-connector library, consumer application. Shows the protocol boundary.

### 2. Akka.Streams Pipeline

Internal flow architecture:

```
Source.Tick (polling) ─┐
                       Merge ─► EncodeStage ─► ExecuteStage ─► DecodeStage ─► ChannelSink
On-demand commands ────┘        (serialize)    (TCP+crypto)    (deserialize)   (responses)
                                                    │
                                              RscpConnection
                                              (TCP + Rijndael)
```

Elements: each stream stage as a component, RscpConnection as infrastructure, RestartFlow as boundary.

### 3. Protocol Layers

Stack view showing the encoding/decoding layers:

```
┌─────────────────────────────────┐
│  Typed Records                  │  EmsPowerSnapshot, BatterySnapshot, ...
├─────────────────────────────────┤
│  RSCP Data Items                │  Tag + DataType + Length + Value
├─────────────────────────────────┤
│  RSCP Frame                     │  Magic + Ctrl + Timestamp + Data + CRC
├─────────────────────────────────┤
│  Rijndael-256 CBC               │  32-byte blocks, chained IV
├─────────────────────────────────┤
│  TCP                            │  Port 5033
└─────────────────────────────────┘
```

### Integration

LikeC4 diagrams are rendered as static SVGs or embedded via `likec4` CLI export, referenced from the VitePress architecture page. The `.c4` source files live alongside the docs for easy updates.
