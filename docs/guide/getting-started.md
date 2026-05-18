# Getting Started

## Installation

Add the NuGet package to your project:

```bash
dotnet add package E3dc
dotnet add package E3dc.Typed
```

## First Query

Read the current PV power from your E3DC S10 in 10 lines:

```csharp
using E3dc.Client;
using E3dc.Messages;
using E3dc.Messages.Descriptors;
using E3dc.Messages.Responses;
using E3dc.Typed;

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

## Device Setup

Before connecting, you need to enable RSCP on your E3DC device and gather three credentials.

### 1. Set the RSCP Encryption Key

This must be done on the device touchscreen (not the portal):

1. On the E3DC display, go to **Main Page > Personalize > User Profile**
2. Find **RSCP Password** and set a key (min. 6 characters)
3. Use only simple alphanumeric ASCII characters — some models don't support special characters like `.` or `@`

This key is used for Rijndael-256 encryption and is **not** the same as your portal password.

### 2. Find Your Device IP Address

On the E3DC display, go to **Main Page > Settings > Network**. Note the IP address (e.g., `192.168.1.100`).

Tip: Assign a static IP or DHCP reservation in your router so the address doesn't change.

### 3. Portal Credentials

Your **username and password** are the same ones you use to log into the [E3DC portal](https://s10.e3dc.com/). The device validates your credentials against the portal, so internet access is required on the E3DC device.

### Network Requirements

Your application must run on the **same subnet** as the E3DC device. RSCP connections from other subnets are blocked, even through VPN. If you need cross-subnet access, set up a network proxy on the local subnet.

## Connection Parameters

| Parameter | Where to find | Maps to |
|-----------|--------------|---------|
| IP Address | Device display > Settings > Network | `.Connect("192.168.1.100")` |
| Username | E3DC portal login | `.WithCredentials("user", "pass")` |
| Password | E3DC portal login | `.WithCredentials("user", "pass")` |
| RSCP Key | Device display > Personalize > RSCP Password | `.WithEncryptionKey("key")` |

Port 5033 is fixed and used by default — you don't need to configure it.
