# Getting Started

## Installation

Add the NuGet package to your project:

```bash
dotnet add package E3dcConnector
dotnet add package E3dcConnector.Typed
```

## First Query

Read the current PV power from your E3DC S10 in 10 lines:

```csharp
using E3dcConnector.Client;
using E3dcConnector.Messages;
using E3dcConnector.Messages.Descriptors;
using E3dcConnector.Messages.Responses;
using E3dcConnector.Typed;

var client = new RscpClientBuilder()
    .Connect("192.168.1.100")
    .WithCredentials("your-user", "your-password")
    .WithEncryptionKey("your-rscp-key")
    .Build();

await using (client)
{
    var response = await client.SendAsync(
        RscpRequest.Create()
            .Read(Ems.PowerPv, Ems.BatSoc));

    if (response is RscpDataResponse data)
    {
        var snapshot = data.ToEmsPowerSnapshot();
        Console.WriteLine($"PV: {snapshot?.PvWatts} W, SOC: {snapshot?.Soc:F1} %");
    }
}
```

## Connection Parameters

You need three pieces of information from your E3DC system:

| Parameter | Where to find |
|-----------|--------------|
| IP Address | E3DC display > Settings > Network |
| Username / Password | E3DC portal account credentials |
| RSCP Encryption Key | E3DC display > Settings > RSCP |

The RSCP encryption key is separate from your login password. It's configured on the device itself and is used for the Rijndael-256 encryption layer.
