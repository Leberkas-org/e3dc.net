# RSCP Protocol Overview

RSCP (Remote Storage Control Protocol) is a proprietary binary protocol developed by E3/DC GmbH (a HagerEnergy brand) for communicating with their home energy storage systems.

## Supported Systems

- E3/DC S10 E and S10 Pro
- E3/DC Quattroporte
- Other E3/DC systems with RSCP enabled

## RSCP vs Modbus

E3/DC systems also support Modbus TCP, but RSCP has significant advantages:

| Feature | RSCP | Modbus |
|---------|------|--------|
| Read data | Yes | Yes |
| Write data / control | Yes | No |
| Historical data | Yes | No |
| Encryption | Rijndael-256 CBC | None |
| Authentication | Username + Password | None |
| Protocol | Binary TLV | Register-based |

## Connection Model

RSCP uses a direct TCP connection to the E3/DC system on **port 5033**. The entire communication is encrypted with Rijndael-256 in CBC mode.

```
Your Application ──TCP:5033──► E3DC S10 Pro
                  (Rijndael-256 CBC encrypted)
```

## Communication Pattern

RSCP follows a strict request-response pattern:

1. Client sends a frame containing one or more request tags
2. Server responds with a frame containing the corresponding response tags
3. There are no server-initiated push messages — polling is required for live data

## Protocol Stack

```
┌─────────────────────────┐
│  Typed Records (.NET)   │
├─────────────────────────┤
│  RSCP Data Items (TLV)  │
├─────────────────────────┤
│  RSCP Frame + CRC32     │
├─────────────────────────┤
│  Rijndael-256 CBC       │
├─────────────────────────┤
│  TCP (Port 5033)        │
└─────────────────────────┘
```
