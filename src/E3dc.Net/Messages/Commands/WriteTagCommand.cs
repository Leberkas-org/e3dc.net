using E3dc.Protocol;
using E3dc.Tags;

namespace E3dc.Messages.Commands;

public sealed record WriteTagCommand(
    RscpTag Tag,
    RscpDataType DataType,
    byte[] Value,
    RscpRequestOptions? Options = null) : IRscpCommand
{
    RscpRequestOptions IRscpCommand.Options => Options ?? RscpRequestOptions.Default;
}
