# E3DC Dashboard Sample — Design Spec

Single sample project replacing the 3 existing samples. Live dashboard showing real-time E3DC power data via SSE.

## Architecture

Minimal ASP.NET API + static HTML. Akka.Streams `RscpFlow` handles polling, channel reader feeds SSE endpoint. No BackgroundService.

## Project

```
samples/E3dcConnector.Dashboard/
  Program.cs                    # Minimal API + Akka flow materialization
  wwwroot/index.html            # Dashboard UI (vanilla HTML/CSS/JS)
  Dockerfile
  docker-compose.yml
  appsettings.json
  E3dcConnector.Dashboard.csproj
```

Delete: `samples/E3dcConnector.Sample/`, `samples/E3dcConnector.FlowSample/`, `samples/E3dcConnector.ActorSample/`

## Backend (Program.cs)

- Create `ActorSystem`, materialize `RscpFlow.Create(...)` with polling tags (EMS power + SOC + autarky + battery details)
- Config via `appsettings.json` with env var override: `E3DC__Host`, `E3DC__User`, `E3DC__Password`, `E3DC__RscpKey`
- `GET /` — serves `wwwroot/index.html` (static files)
- `GET /api/stream` — SSE endpoint: reads from `ChannelReader<IRscpMessage>`, parses to JSON, writes `data: {json}\n\n`

## SSE Data Model

```json
{
  "pvWatts": 3500,
  "batteryWatts": -1200,
  "gridWatts": 0,
  "homeWatts": 2300,
  "soc": 85.5,
  "autarky": 92.3,
  "batteryVoltage": 48.2,
  "batteryCurrent": -5.1,
  "chargeCycles": 312,
  "timestamp": "2026-05-15T20:10:00Z"
}
```

## Frontend (index.html)

- E3DC-green themed (#5CC244), dark background (#2E3538)
- Big number cards: PV, Battery, Grid, Home watts
- Battery gauge: SOC %, voltage, current, charge cycles
- Power flow arrows showing direction
- `EventSource('/api/stream')` for live updates, no framework

## Config

`appsettings.json`:
```json
{
  "E3DC": {
    "Host": "192.168.1.100",
    "User": "",
    "Password": "",
    "RscpKey": ""
  }
}
```

Overridable via env vars (`E3DC__Host` etc.) for Docker.

## Docker

```yaml
services:
  dashboard:
    build: .
    ports:
      - "5000:8080"
    environment:
      - E3DC__Host=192.168.1.100
      - E3DC__User=user
      - E3DC__Password=pass
      - E3DC__RscpKey=key
```
