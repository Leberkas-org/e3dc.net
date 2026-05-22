# E3DC.NET

[![NuGet](https://img.shields.io/nuget/v/E3dc?label=E3dc&logo=nuget)](https://www.nuget.org/packages/E3dc)
[![Docker](https://img.shields.io/badge/ghcr.io-e3dc--dashboard-blue?logo=docker)](https://ghcr.io/leberkas-org/e3dc-dashboard)
[![License](https://img.shields.io/github/license/Leberkas-org/E3dc)](LICENSE)
[![Build](https://img.shields.io/github/actions/workflow/status/Leberkas-org/E3dc/docker.yml?label=build&logo=github)](https://github.com/Leberkas-org/E3dc/actions)

A .NET 10 library for communicating with E3DC S10 home battery systems via the proprietary RSCP (Remote Storage Control Protocol) over TCP. Includes a full-featured sample dashboard application.

![E3dc](https://raw.githubusercontent.com/Leberkas-org/e3dc.net/refs/heads/master/icon.png)

## Overview

- Binary RSCP protocol implementation — framing, CRC32, Rijndael-256 encryption
- [Akka.Streams](https://getakka.net/)-based reactive data pipeline with automatic reconnection
- Typed snapshot API covering all major RSCP namespaces: EMS, BAT, PVI, PM, DCDC, EP, WB, DB, INFO
- Sample web dashboard demonstrating real-time monitoring, history queries, live tag exploration, and protocol-level request building

## Features

- **Protocol layer** — full binary framing, CRC32 validation, Rijndael-256 AES-compatible encryption
- **Fluent request builder** — type-safe tag descriptors for every RSCP namespace
- **Typed snapshots** — `EmsPowerSnapshot`, `BatterySnapshot`, `InverterSnapshot`, `PowerMeterSnapshot`, `DcdcSnapshot`, `EpSnapshot`, `WallboxSnapshot`, and more
- **Reactive pipeline** — Akka.Streams source with automatic TCP reconnection and backpressure
- **Tiered polling** — fast (2 s), medium (10 s), and startup tiers with demand-driven activation

## Dashboard Sample

A four-tab web dashboard demonstrating end-to-end usage of the connector.

### Dashboard

Real-time energy flow schematic, scrollable power history chart, live KPI tiles, and detailed status panels for battery, inverter, power meter, DCDC converter, emergency power, and wallbox.

![Dashboard top](https://raw.githubusercontent.com/Leberkas-org/e3dc.net/refs/heads/master/docs/images/docs-01-dashboard-top.png)

### History

Query E3DC on-device stored data across day, week, month, and year resolutions. Results are displayed as bar charts.

![History](https://raw.githubusercontent.com/Leberkas-org/e3dc.net/refs/heads/master/docs/images/docs-03-history.png)

### Explorer

Live tag tree showing every polled RSCP tag with its current value — useful for discovering what your device exposes.

![Explorer](https://raw.githubusercontent.com/Leberkas-org/e3dc.net/refs/heads/master/docs/images/docs-04-explorer.png)

### Request Builder

Three-panel RSCP protocol workbench: tag browser, request composer, and raw response viewer. Lets you craft arbitrary RSCP requests and inspect the parsed response.

![Request Builder](https://raw.githubusercontent.com/Leberkas-org/e3dc.net/refs/heads/master/docs/images/docs-05-builder.png)

## Quick Start

```bash
# Clone
git clone https://github.com/Leberkas-org/e3dc.net.git
cd e3dc.net

# Configure — fill in your E3DC credentials
# Edit samples/E3dc.Net.Dashboard/appsettings.json

# Run
dotnet run --project samples/E3dc.Net.Dashboard

# Or with Docker
cd samples && docker compose up -d --build
```

## Project Structure

```
src/
  E3dc.Net/              Core RSCP protocol library (framing, encryption, request builder, typed snapshots)
samples/
  E3dc.Net.Dashboard/    Full web dashboard application
docs/                         VitePress documentation site
test/
  E3dc.Net.Tests/        Unit tests
```

## Configuration

Edit `samples/E3dc.Net.Dashboard/appsettings.json`:

```json
{
  "E3DC": {
    "Host": "192.168.1.100",
    "User": "",
    "Password": "",
    "RscpKey": "",
    "FastPollingIntervalSeconds": 2,
    "MediumPollingIntervalSeconds": 10,
    "HistoryRetentionMinutes": 60,
    "BatDeviceIndex": 0,
    "PviDeviceIndex": 0,
    "PmDeviceIndex": 6,
    "DcdcDeviceIndex": 0,
    "WbDeviceIndex": 0
  }
}
```

| Field | Description |
|---|---|
| `Host` | IP address of the E3DC S10 on your local network |
| `User` / `Password` | E3DC portal credentials |
| `RscpKey` | RSCP encryption key set in the E3DC device settings |
| `*DeviceIndex` | Zero-based device indices (check your installation; PM default is 6) |

## Documentation

📖 **[e3dc.leberkas.org](https://e3dc.leberkas.org/)**

## License

[MIT](LICENSE)
