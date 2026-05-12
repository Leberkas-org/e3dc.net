# Architecture

The e3dc-connector library is structured in layers, each with clear responsibilities.

## System Context

The library sits between your application and the E3DC S10 Pro hardware, handling all protocol complexity.

```mermaid
graph LR
    App["Your Application"]
    Lib["e3dc-connector"]
    E3DC["E3DC S10 Pro"]

    App -- "RscpRequest.Create()
    .Read(Ems.PowerPv)" --> Lib
    Lib -- "TCP:5033
    Rijndael-256 CBC" --> E3DC

    style Lib fill:#5CC244,color:#fff,stroke:#3d9a2c
    style E3DC fill:#2E3538,color:#fff,stroke:#555
    style App fill:#4a90d9,color:#fff,stroke:#357abd
```

## Akka.Streams Pipeline

Internally, commands flow through an Akka.Streams pipeline that handles serialization, TCP communication with Rijndael-256 encryption, and deserialization. `RestartFlow.WithBackoff` provides automatic reconnection with exponential backoff.

```mermaid
graph LR
    subgraph RestartFlow["RestartFlow.WithBackoff"]
        Tick["Source.Tick\n(polling)"]
        Cmd["On-demand\ncommands"]
        Merge["MergePreferred"]
        Enc["EncodeStage"]
        Exec["ExecuteStage"]
        Dec["DecodeStage"]

        Tick --> Merge
        Cmd --> Merge
        Merge --> Enc --> Exec --> Dec
    end

    Conn["RscpConnection\nTCP + Rijndael-256"]
    Sink["ChannelSink\n(responses)"]

    Exec <--> Conn
    Dec --> Sink

    style RestartFlow fill:#f0faf0,stroke:#5CC244
    style Conn fill:#2E3538,color:#fff,stroke:#555
    style Exec fill:#5CC244,color:#fff,stroke:#3d9a2c
```

## Protocol Layers

The encoding stack from typed .NET records down to encrypted TCP bytes:

```mermaid
graph TB
    T["Typed Layer\nEmsPowerSnapshot, BatterySnapshot, ..."]
    M["Message Layer\nRscpRequest builder, TagDescriptor, IndexedTag"]
    F["Frame Layer\nRscpFrame, RscpDataItem, CRC32"]
    C["Crypto Layer\nRijndael-256 CBC (BouncyCastle)"]
    N["Transport\nTCP Port 5033"]

    T --> M --> F --> C --> N

    style T fill:#D4FC37,color:#2E3538,stroke:#aaa
    style M fill:#8ed674,color:#2E3538,stroke:#5CC244
    style F fill:#5CC244,color:#fff,stroke:#3d9a2c
    style C fill:#3d9a2c,color:#fff,stroke:#2a7a1e
    style N fill:#2E3538,color:#fff,stroke:#555
```

| Layer | Component | Responsibility |
|-------|-----------|----------------|
| **Typed** | `EmsPowerSnapshot`, `BatterySnapshot`, ... | Strongly-typed .NET records |
| **Messages** | `RscpRequest` builder, `TagDescriptor`, `IndexedTag` | Fluent request composition with compile-time safety |
| **Frame** | `RscpFrame`, `RscpDataItem` | Binary TLV encoding + CRC32 |
| **Crypto** | `RscpCrypt` | Rijndael-256 CBC encryption (BouncyCastle) |
| **Transport** | `RscpConnection` | TCP socket on port 5033 |

## Key Patterns

**Compile-time safety:** Tag descriptors use the type system to prevent misuse. `TagDescriptor` for top-level tags (EMS, INFO, EP), `IndexedTag` for device sub-tags (PVI, BAT, PM, WB) that must be inside `FromDevice()`. Passing an `IndexedTag` to `.Read()` is a compile error.

**Fluent builder:** `RscpRequest.Create()` chains `.Read()`, `.Write()`, `.FromDevice()`, and `.Container()` to compose arbitrarily complex RSCP frames.

**Correlation-based request-reply:** Every command gets a `CorrelationId`. The `RscpClient` stores a `TaskCompletionSource` in a `ConcurrentDictionary`, which is completed when the matching response arrives.

**Channel bridging:** Commands flow through a bounded `Channel<IRscpCommand>` (capacity 256), responses through an unbounded `Channel<IRscpMessage>`. The Akka.Streams flow runs independently in the background.

**Automatic reconnection:** The entire inner flow is wrapped in `RestartFlow.WithBackoff` (1s-30s exponential backoff, 20% jitter). On restart, a new TCP connection is established and authentication is re-executed.

## LikeC4 Source

The architecture is also modeled in LikeC4 in `docs/architecture/likec4/`:
- `model.c4` — elements and relationships
- `views.c4` — diagram views
