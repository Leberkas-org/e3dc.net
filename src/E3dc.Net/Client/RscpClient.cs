using System.Collections.Concurrent;
using System.Threading.Channels;
using Akka;
using Akka.Actor;
using Akka.Streams;
using Akka.Streams.Dsl;
using E3dc.Messages;
using E3dc.Messages.Responses;

namespace E3dc.Client;

public sealed class RscpClient : IAsyncDisposable
{
    private readonly ChannelWriter<IRscpCommand> _commands;
    private readonly ConcurrentDictionary<Type, List<Delegate>> _handlers = new();
    private readonly ConcurrentDictionary<string, TaskCompletionSource<IRscpMessage>> _pending = new();
    private readonly ActorSystem? _ownedActorSystem;
    private readonly CancellationTokenSource _cts = new();

    internal RscpClient(
        Func<Flow<IRscpCommand, IRscpMessage, NotUsed>> flowFactory,
        ActorSystem? actorSystem = null)
    {
        _ownedActorSystem = actorSystem is null ? ActorSystem.Create("rscp-client") : null;
        var materializer = (actorSystem ?? _ownedActorSystem!).Materializer();
        var flow = flowFactory();
        (_commands, var messages) = flow.Materialize(materializer);
        _ = Task.Run(() => DispatchMessages(messages, _cts.Token));
    }

    public async Task<IRscpResponse> SendAsync(IRscpCommand command, CancellationToken ct = default)
    {
        var correlationId = command.Options.CorrelationId;
        var tcs = new TaskCompletionSource<IRscpMessage>(TaskCreationOptions.RunContinuationsAsynchronously);
        _pending[correlationId] = tcs;

        try
        {
            await _commands.WriteAsync(command, ct);
            using var reg = ct.Register(() => tcs.TrySetCanceled());
            var message = await tcs.Task;
            return message as IRscpResponse
                ?? new RscpErrorResponse($"Unexpected response type: {message.GetType().Name}", null, correlationId);
        }
        finally
        {
            _pending.TryRemove(correlationId, out _);
        }
    }

    public IDisposable Subscribe<T>(Action<T> handler) where T : IRscpMessage
    {
        var list = _handlers.GetOrAdd(typeof(T), _ => new List<Delegate>());
        lock (list) { list.Add(handler); }
        return new Unsubscriber(() => { lock (list) { list.Remove(handler); } });
    }

    public void WriteCommand(IRscpCommand command)
    {
        _commands.TryWrite(command);
    }

    private async Task DispatchMessages(ChannelReader<IRscpMessage> reader, CancellationToken ct)
    {
        await foreach (var message in reader.ReadAllAsync(ct))
        {
            if (message is IRscpResponse correlated
                && _pending.TryRemove(correlated.CorrelationId, out var tcs))
            {
                tcs.TrySetResult(message);
                continue;
            }

            var messageType = message.GetType();
            foreach (var kvp in _handlers)
            {
                if (!kvp.Key.IsAssignableFrom(messageType)) continue;

                List<Delegate> snapshot;
                lock (kvp.Value) { snapshot = new List<Delegate>(kvp.Value); }

                foreach (var handler in snapshot)
                {
                    try
                    {
                        var result = handler.DynamicInvoke(message);
                        if (result is Task task) await task;
                    }
                    catch { }
                }
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        _cts.Cancel();
        _commands.TryComplete();
        if (_ownedActorSystem is not null)
            await _ownedActorSystem.Terminate();
        _cts.Dispose();
    }

    private sealed class Unsubscriber(Action onDispose) : IDisposable
    {
        public void Dispose() => onDispose();
    }
}
