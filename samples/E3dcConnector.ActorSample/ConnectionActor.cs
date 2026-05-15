using Akka.Actor;
using Akka.Event;
using Akka.Streams;
using Akka.Streams.Dsl;
using E3dcConnector.Client;
using E3dcConnector.Messages;
using E3dcConnector.Reactive;
using E3dcConnector.Reactive.Internal;

namespace E3dcConnector.ActorSample;

public sealed class ConnectionActor : ReceiveActor
{
    public sealed record Connect
    {
        public static readonly Connect Instance = new();
    }
    public sealed record Subscribe(IActorRef Subscriber);
    private sealed record StreamCompleted
    {
        public static readonly StreamCompleted Instance = new();
    }

    private readonly ILoggingAdapter _log = Context.GetLogger();
    private readonly Func<RscpConnection> _connectionFactory;
    private readonly TagDescriptor[] _pollingTags;
    private readonly RscpFlowSettings _settings;
    private readonly HashSet<IActorRef> _subscribers = [];
    private ISourceQueueWithComplete<IRscpCommand>? _commandQueue;

    public ConnectionActor(
        Func<RscpConnection> connectionFactory,
        TagDescriptor[] pollingTags,
        RscpFlowSettings settings)
    {
        _connectionFactory = connectionFactory;
        _pollingTags = pollingTags;
        _settings = settings;

        Receive<Subscribe>(msg =>
        {
            _subscribers.Add(msg.Subscriber);
            Context.Watch(msg.Subscriber);
        });

        Receive<Terminated>(msg => _subscribers.Remove(msg.ActorRef));

        ReceiveAsync<Connect>(async _ =>
        {
            var materializer = Context.Materializer();
            var (queue, source) = Source.Queue<IRscpCommand>(64, OverflowStrategy.DropHead)
                .PreMaterialize(materializer);
            _commandQueue = queue;

            var flow = RscpFlow.Create(_connectionFactory, _pollingTags, _settings);
            source.Via(flow)
                .To(Sink.ActorRef<IRscpMessage>(Self, StreamCompleted.Instance, _ => StreamCompleted.Instance))
                .Run(materializer);

            _log.Info("RSCP stream materialized");
        });

        Receive<IRscpCommand>(cmd =>
        {
            if (_commandQueue is null) { _log.Warning("Not connected"); return; }
            _commandQueue.OfferAsync(cmd);
        });

        Receive<IRscpMessage>(msg =>
        {
            foreach (var sub in _subscribers) sub.Tell(msg);
        });

        Receive<StreamCompleted>(_ => _log.Warning("Stream completed"));
    }

    protected override void PostStop() => _commandQueue?.Complete();

    public static Props Create(
        Func<RscpConnection> connectionFactory,
        TagDescriptor[] pollingTags,
        RscpFlowSettings? settings = null) =>
        Props.Create(() => new ConnectionActor(connectionFactory, pollingTags, settings ?? new()));
}
