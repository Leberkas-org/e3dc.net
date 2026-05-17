using Akka.Actor;

namespace E3dcConnector.Dashboard.Configuration;

public sealed class ActorRegistry
{
    public required IActorRef Snapshot { get; init; }
    public required IActorRef Gateway { get; init; }
    public required IActorRef Polling { get; init; }
}
