using E3dcConnector.Client;
using E3dcConnector.Messages.Commands;
using E3dcConnector.Messages.Responses;
using E3dcConnector.Tags;
using E3dcConnector.Typed;

var host = args.Length > 0 ? args[0] : "192.168.1.100";
var user = args.Length > 1 ? args[1] : "user";
var password = args.Length > 2 ? args[2] : "password";
var encKey = args.Length > 3 ? args[3] : "rscp_password";

var client = new RscpClientBuilder()
    .Connect(host, 5033)
    .WithCredentials(user, password)
    .WithEncryptionKey(encKey)
    .Build();

await using (client)
{
    var response = await client.SendAsync(new ReadTagsCommand([
        RscpTag.EMS_REQ_POWER_PV,
        RscpTag.EMS_REQ_POWER_BAT,
        RscpTag.EMS_REQ_POWER_GRID,
        RscpTag.EMS_REQ_POWER_HOME,
        RscpTag.EMS_REQ_BAT_SOC,
    ]));

    if (response is RscpDataResponse data)
    {
        var snapshot = data.ToEmsPowerSnapshot();
        if (snapshot is not null)
        {
            Console.WriteLine($"PV:      {snapshot.PvWatts} W");
            Console.WriteLine($"Battery: {snapshot.BatteryWatts} W");
            Console.WriteLine($"Grid:    {snapshot.GridWatts} W");
            Console.WriteLine($"Home:    {snapshot.HomeWatts} W");
            Console.WriteLine($"SOC:     {snapshot.Soc:F1} %");
        }
    }
    else if (response is RscpErrorResponse error)
    {
        Console.Error.WriteLine($"Error: {error.Message}");
    }
}
