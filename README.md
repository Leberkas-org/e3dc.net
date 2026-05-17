# E3DC RSCP Connector

A .NET 10 library for communicating with E3DC S10 home battery systems via the proprietary RSCP (Remote Storage Control Protocol) over TCP. Includes a full-featured sample dashboard application.

## Overview

- Binary RSCP protocol implementation — framing, CRC32, Rijndael-256 encryption
- [Akka.Streams](https://getakka.net/)-based reactive data pipeline with automatic reconnection
- Typed snapshot API covering all major RSCP namespaces: EMS, BAT, PVI, PM, DCDC, EP, WB, DB, INFO
- Sample Blazor dashboard demonstrating real-time monitoring, history queries, live tag exploration, and protocol-level request building

## Features

- **Protocol layer** — full binary framing, CRC32 validation, Rijndael-256 AES-compatible encryption
- **Fluent request builder** — type-safe tag descriptors for every RSCP namespace
- **Typed snapshots** — `EmsPowerSnapshot`, `BatterySnapshot`, `InverterSnapshot`, `PowerMeterSnapshot`, `DcdcSnapshot`, `EpSnapshot`, `WallboxSnapshot`, and more
- **Reactive pipeline** — Akka.Streams source with automatic TCP reconnection and backpressure
- **Tiered polling** — fast (2 s), medium (10 s), and startup tiers with demand-driven activation

## Dashboard Sample

A four-tab Blazor dashboard demonstrating end-to-end usage of the connector.

### Dashboard

Real-time energy flow schematic, scrollable power history chart, live KPI tiles, and detailed status panels for battery, inverter, power meter, DCDC converter, emergency power, and wallbox.

![Dashboard top](docs/images/docs-01-dashboard-top.png)

### History

Query E3DC on-device stored data across day, week, month, and year resolutions. Results are displayed as bar charts.

![History](docs/images/docs-03-history.png)

### Explorer

Live tag tree showing every polled RSCP tag with its current value — useful for discovering what your device exposes.

![Explorer](docs/images/docs-04-explorer.png)

### Request Builder

Three-panel RSCP protocol workbench: tag browser, request composer, and raw response viewer. Lets you craft arbitrary RSCP requests and inspect the parsed response.

![Request Builder](docs/images/docs-05-builder.png)

## Quick Start

```bash
# Clone
git clone https://github.com/your-org/e3dc-connector.git
cd e3dc-connector

# Configure — fill in your E3DC credentials
# Edit samples/E3dcConnector.Dashboard/appsettings.json

# Run
dotnet run --project samples/E3dcConnector.Dashboard

# Or with Docker
cd samples && docker compose up -d --build
```

## Project Structure

```
src/
  E3dcConnector/              Core RSCP protocol library (framing, encryption, request builder)
  E3dcConnector.Typed/        Typed snapshots and response parsing
samples/
  E3dcConnector.Dashboard/    Full Blazor dashboard application
docs/                         VitePress documentation site
test/
  E3dcConnector.Tests/        Unit tests
```

## Configuration

Edit `samples/E3dcConnector.Dashboard/appsettings.json`:

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

Full documentation is in `/docs/` — a [VitePress](https://vitepress.dev/) site. Run it locally with:

```bash
cd docs && npm install && npm run dev
```

## License

TODO
