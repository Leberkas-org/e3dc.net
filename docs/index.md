---
layout: home
hero:
  name: E3DC.NET
  text: RSCP Protocol Client for .NET
  tagline: Connect to your E3DC S10 home battery system. Monitor energy flows, query history, and control your system — all from .NET.
  image:
    src: /e3dc-logo.svg
    alt: E3DC.NET
  actions:
    - theme: brand
      text: Get Started
      link: /guide/getting-started
    - theme: alt
      text: Protocol Reference
      link: /protocol/overview
    - theme: alt
      text: View on GitHub
      link: https://github.com/Leberkas-org/e3dc.net
features:
  - icon: 🔐
    title: Full RSCP Protocol
    details: Complete binary protocol implementation — framing, CRC32, Rijndael-256 encryption. Talks directly to the E3DC over TCP on your local network.
  - icon: ⚡
    title: Real-Time Energy Data
    details: Solar production, battery state, grid import/export, home consumption — all updated in real-time via reactive Akka.Streams pipelines.
  - icon: 🎯
    title: Typed Snapshots
    details: Strongly-typed records for all RSCP namespaces — EMS, Battery, Inverter, Power Meter, DCDC, Emergency Power, Wallbox. No manual byte parsing.
  - icon: 🔄
    title: Three Usage Modes
    details: Imperative (async/await), reactive (channels), or actor-based — pick what fits your architecture.
  - icon: 📊
    title: Sample Dashboard
    details: Full 4-tab web dashboard included — live energy flow, historical charts, protocol explorer, and RSCP request builder.
  - icon: 🏗️
    title: Akka.Streams
    details: Reactive streaming with automatic reconnection, backpressure, tiered polling, and demand-driven activation.
---

## What is this?

E3DC.NET is a .NET library that communicates with [E3DC](https://www.e3dc.com/) S10 home battery systems using the proprietary **RSCP** (Remote Storage Control Protocol). At least the S10 is the one that is tested. It gives you direct, local access to your energy system — no cloud dependency.

## Quick Example

```csharp
using E3dc;

var request = RscpRequest.Create()
    .Read(Ems.PowerPv, Ems.PowerBat, Ems.PowerGrid, Ems.PowerHome)
    .Read(Ems.BatSoc);

// ... send via RscpFlow or RscpClient, then:
var ems = response.ToEmsPowerSnapshot();
Console.WriteLine($"Solar: {ems.PvWatts}W, Battery: {ems.Soc}%");
```

## Dashboard

The included sample dashboard is a full demonstrator of the RSCP protocol, which you can just use and enjoy, or use as inspiration and modify it to your liking. 

![Dashboard](./images/docs-01-dashboard-top.png)

![Request Builder](./images/docs-05-builder.png)

If you want to use it as it, a container image is available at:
``` shell
docker pull ghcr.io/leberkas-org/e3dc-dashboard:latest
```