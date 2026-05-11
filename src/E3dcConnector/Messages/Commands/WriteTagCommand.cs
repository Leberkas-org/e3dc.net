using E3dcConnector.Protocol;
using E3dcConnector.Tags;

namespace E3dcConnector.Messages.Commands;

public sealed record WriteTagCommand(
    RscpTag Tag,
    RscpDataType DataType,
    byte[] Value,
    RscpRequestOptions? Options = null) : IRscpCommand
{
    RscpRequestOptions IRscpCommand.Options => Options ?? RscpRequestOptions.Default;
}
