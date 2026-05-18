# Actor Integration

Use the `ConnectionActor` pattern for actor-based systems (digital twins, supervision).

## ConnectionActor Pattern

```csharp
public sealed class ConnectionActor : ReceiveActor
{
    private ISourceQueueWithComplete<IRscpCommand>? _commandQueue;
    private readonly HashSet<IActorRef> _subscribers = [];

    public ConnectionActor(...)
    {
        ReceiveAsync<Connect>(async _ =>
        {
            var materializer = Context.Materializer();
            var (queue, source) = Source.Queue<IRscpCommand>(64, OverflowStrategy.DropHead)
                .PreMaterialize(materializer);
            _commandQueue = queue;

            var flow = RscpFlow.Create(connectionFactory, pollingTags, settings);
            source.Via(flow)
                .To(Sink.ActorRef<IRscpMessage>(Self, StreamCompleted.Instance, ...))
                .Run(materializer);
        });

        Receive<IRscpMessage>(msg =>
        {
            foreach (var sub in _subscribers) sub.Tell(msg);
        });
    }
}
```

## Usage

```csharp
var system = ActorSystem.Create("e3dc");
var actor = system.ActorOf(ConnectionActor.Create(
    () => new RscpConnection("192.168.1.100", 5033, "user", "pass", "key"),
    [RscpTag.EMS_REQ_POWER_PV, RscpTag.EMS_REQ_BAT_SOC],
    new RscpFlowSettings { PollingInterval = TimeSpan.FromSeconds(2) }));

actor.Tell(ConnectionActor.Connect.Instance);
```

## Subscribing to Data

Other actors subscribe to receive all RSCP messages:

```csharp
actor.Tell(new ConnectionActor.Subscribe(Self));
```

The ConnectionActor watches subscribers and removes them on termination.

See the `E3dc.ActorSample` project for a complete working example.
