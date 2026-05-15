using Akka.Actor;
using E3dcConnector.ActorSample;
using E3dcConnector.Messages.Descriptors;
using E3dcConnector.Reactive;
using E3dcConnector.Reactive.Internal;

var system = ActorSystem.Create("e3dc-actors");

var connection = ConnectionActor.Create(
    () => new RscpConnection("192.168.1.100", 5033, "user", "password", "rscp_password"),
    [Ems.PowerPv, Ems.PowerBat, Ems.PowerGrid, Ems.PowerHome, Ems.BatSoc],
    new RscpFlowSettings { PollingInterval = TimeSpan.FromSeconds(2) });

var actor = system.ActorOf(connection, "e3dc-connection");
actor.Tell(ConnectionActor.Connect.Instance);

Console.WriteLine("Press Enter to exit...");
Console.ReadLine();
await system.Terminate();
