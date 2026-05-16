using Akka;
using Akka.Streams;
using Akka.Streams.Dsl;
using E3dcConnector.Messages;
using E3dcConnector.Messages.Commands;
using E3dcConnector.Messages.Responses;
using E3dcConnector.Protocol;
using E3dcConnector.Reactive.Internal;

namespace E3dcConnector.Reactive;

public static class RscpFlow
{
    public static Flow<IRscpCommand, IRscpMessage, NotUsed> Create(
        Func<RscpConnection> connectionFactory,
        RscpRequest? pollingRequest = null,
        RscpFlowSettings? settings = null)
    {
        settings ??= new RscpFlowSettings();
        var capturedSettings = settings;

        return RestartFlow.WithBackoff(
            () => CreateInnerFlow(connectionFactory, pollingRequest, capturedSettings),
            RestartSettings.Create(
                capturedSettings.MinReconnectBackoff,
                capturedSettings.MaxReconnectBackoff,
                capturedSettings.ReconnectRandomFactor)
            .WithMaxRestarts(capturedSettings.MaxReconnectAttempts, capturedSettings.MinReconnectBackoff));
    }

    public static Flow<IRscpCommand, IRscpMessage, NotUsed> Create(
        Func<RscpConnection> connectionFactory,
        TagDescriptor[] pollingTags,
        RscpFlowSettings? settings = null)
        => Create(connectionFactory, RscpRequest.Create().Read(pollingTags), settings);

    private static Flow<IRscpCommand, IRscpMessage, NotUsed> CreateInnerFlow(
        Func<RscpConnection> connectionFactory,
        RscpRequest? pollingRequest,
        RscpFlowSettings settings)
    {
        var connectionReady = Task.Run(async () =>
        {
            var conn = connectionFactory();
            await conn.ConnectAsync();
            await conn.AuthenticateAsync();
            return conn;
        });

        var commandFlow = Flow.Create<IRscpCommand>()
            .SelectAsync(1, async cmd =>
            {
                var conn = await connectionReady;
                return await ProcessCommand(cmd, conn);
            });

        if (pollingRequest is not null && pollingRequest.BuildItems().Count > 0)
        {
            var pollSource = Source.Tick(TimeSpan.Zero, settings.PollingInterval, pollingRequest as IRscpCommand);

            return Flow.FromGraph(GraphDsl.Create(b =>
            {
                var poll = b.Add(pollSource);
                var commands = b.Add(commandFlow);
                var merge = b.Add(new MergePreferred<IRscpCommand>(1));

                b.From(poll.Outlet).To(merge.Preferred);
                b.From(merge.Out).To(commands.Inlet);

                return new FlowShape<IRscpCommand, IRscpMessage>(merge.In(0), commands.Outlet);
            }));
        }

        return commandFlow;
    }

    private static async Task<IRscpMessage> ProcessCommand(IRscpCommand cmd, RscpConnection conn)
    {
        try
        {
            var items = cmd switch
            {
                RscpRequest request => request.BuildItems(),
                ReadTagsCommand read => read.Tags
                    .Select(t => new RscpDataItem((uint)t, RscpDataType.None, Array.Empty<byte>()))
                    .ToList() as IReadOnlyList<RscpDataItem>,
                WriteTagCommand write => new List<RscpDataItem>
                {
                    new((uint)write.Tag, write.DataType, write.Value)
                },
                _ => throw new NotSupportedException($"Unknown command: {cmd.GetType().Name}"),
            };

            var requestFrame = new RscpFrame(DateTimeOffset.UtcNow, items);
            await conn.SendFrameAsync(requestFrame);
            var responseFrame = await conn.ReceiveFrameAsync();

            return new RscpDataResponse(responseFrame.Items, cmd.Options.CorrelationId);
        }
        catch (IOException)
        {
            throw;
        }
        catch (InvalidDataException ex) when (ex.Message.Contains("magic") || ex.Message.Contains("CRC"))
        {
            throw;
        }
        catch (Exception ex)
        {
            return new RscpErrorResponse(ex.Message, ex, cmd.Options.CorrelationId);
        }
    }
}
