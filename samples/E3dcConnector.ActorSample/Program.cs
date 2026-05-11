using Akka.Actor;
using E3dcConnector.ActorSample;
using E3dcConnector.Reactive;
using E3dcConnector.Reactive.Internal;
using E3dcConnector.Tags;

var system = ActorSystem.Create("e3dc-actors");

var connection = ConnectionActor.Create(
    () => new RscpConnection("192.168.1.100", 5033, "user", "password", "rscp_password"),
    [
        RscpTag.EMS_REQ_POWER_PV,
        RscpTag.EMS_REQ_POWER_BAT,
        RscpTag.EMS_REQ_POWER_GRID,
        RscpTag.EMS_REQ_POWER_HOME,
        RscpTag.EMS_REQ_BAT_SOC,
    ],
    new RscpFlowSettings { PollingInterval = TimeSpan.FromSeconds(2) });

var actor = system.ActorOf(connection, "e3dc-connection");
actor.Tell(ConnectionActor.Connect.Instance);

Console.WriteLine("Press Enter to exit...");
Console.ReadLine();
await system.Terminate();
