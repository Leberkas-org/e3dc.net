using System.Threading.Channels;
using Akka;
using Akka.Streams;
using Akka.Streams.Dsl;
using E3dc.Messages;

namespace E3dc.Client;

public static class RscpClientExtensions
{
    public static (ChannelWriter<IRscpCommand> Commands, ChannelReader<IRscpMessage> Responses)
        Materialize(
            this Flow<IRscpCommand, IRscpMessage, NotUsed> flow,
            IMaterializer materializer)
    {
        var cmdChannel = Channel.CreateBounded<IRscpCommand>(256);
        var rspChannel = Channel.CreateUnbounded<IRscpMessage>();

        ChannelSource.FromReader(cmdChannel.Reader)
            .Via(flow)
            .To(ChannelSink.FromWriter(rspChannel.Writer, isOwner: true))
            .Run(materializer);

        return (cmdChannel.Writer, rspChannel.Reader);
    }
}
