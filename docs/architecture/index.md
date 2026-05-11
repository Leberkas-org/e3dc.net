# Architecture

The e3dc-connector library is structured in layers, each with clear responsibilities.

## System Context

The library sits between your application and the E3DC S10 Pro hardware, handling all protocol complexity.

```
┌─────────────┐     RSCP/TCP       ┌──────────────────┐      C# API       ┌──────────────┐
│  E3DC S10   │◄──────────────────►│  e3dc-connector  │◄──────────────────►│  Your App    │
│  Pro        │   Rijndael-256     │  (this library)  │   RscpClient       │  (consumer)  │
└─────────────┘   Port 5033        └──────────────────┘   async/await      └──────────────┘
```

## Akka.Streams Pipeline

Internally, commands flow through an Akka.Streams pipeline that handles serialization, TCP communication with Rijndael-256 encryption, and deserialization. `RestartFlow.WithBackoff` provides automatic reconnection with exponential backoff.

```
Source.Tick (polling) ─┐
                       MergePreferred ─► Encode ─► Execute ─► Decode ─► ChannelSink
On-demand commands ────┘ (preferred)     Stage      Stage      Stage     (responses)
                                                      │
                                                RscpConnection
                                              (TCP + Rijndael-256)
```

## Protocol Layers

The encoding stack from typed .NET records down to encrypted TCP bytes:

| Layer | Component | Responsibility |
|-------|-----------|----------------|
| **Typed** | `EmsPowerSnapshot`, `BatterySnapshot`, ... | Strongly-typed .NET records |
| **Messages** | `IRscpCommand`, `IRscpMessage` | Protocol-agnostic command/response |
| **Frame** | `RscpFrame`, `RscpDataItem` | Binary TLV encoding + CRC32 |
| **Crypto** | `RscpCrypt` | Rijndael-256 CBC encryption (BouncyCastle) |
| **Transport** | `RscpConnection` | TCP socket on port 5033 |

## Key Patterns

**Correlation-based request-reply:** Every command gets a `CorrelationId`. The `RscpClient` stores a `TaskCompletionSource` in a `ConcurrentDictionary`, which is completed when the matching response arrives.

**Channel bridging:** Commands flow through a bounded `Channel<IRscpCommand>` (capacity 256), responses through an unbounded `Channel<IRscpMessage>`. The Akka.Streams flow runs independently in the background.

**Automatic reconnection:** The entire inner flow is wrapped in `RestartFlow.WithBackoff` (1s-30s exponential backoff, 20% jitter). On restart, a new TCP connection is established and authentication is re-executed.

## Source

The LikeC4 architecture model is defined in:
- [`likec4/model.c4`](./likec4/model.c4) — elements and relationships
- [`likec4/views.c4`](./likec4/views.c4) — diagram views (system context, pipeline, layers)
