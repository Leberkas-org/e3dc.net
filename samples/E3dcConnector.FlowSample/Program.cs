using Akka.Actor;
using Akka.Streams;
using E3dcConnector.Client;
using E3dcConnector.Messages.Responses;
using E3dcConnector.Reactive;
using E3dcConnector.Reactive.Internal;
using E3dcConnector.Tags;
using E3dcConnector.Typed;

var system = ActorSystem.Create("e3dc-flow");
var materializer = system.Materializer();

var flow = RscpFlow.Create(
    () => new RscpConnection("192.168.1.100", 5033, "user", "password", "rscp_password"),
    pollingTags: [
        RscpTag.EMS_REQ_POWER_PV,
        RscpTag.EMS_REQ_POWER_BAT,
        RscpTag.EMS_REQ_POWER_GRID,
        RscpTag.EMS_REQ_POWER_HOME,
        RscpTag.EMS_REQ_BAT_SOC,
    ],
    new RscpFlowSettings { PollingInterval = TimeSpan.FromSeconds(2) });

var (commands, messages) = flow.Materialize(materializer);

await foreach (var msg in messages.ReadAllAsync())
{
    if (msg is RscpDataResponse data)
    {
        var snapshot = data.ToEmsPowerSnapshot();
        if (snapshot is not null)
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] PV={snapshot.PvWatts}W  BAT={snapshot.BatteryWatts}W  GRID={snapshot.GridWatts}W  SOC={snapshot.Soc:F1}%");
    }
}

await system.Terminate();
