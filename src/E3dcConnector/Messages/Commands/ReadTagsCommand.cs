using E3dcConnector.Tags;

namespace E3dcConnector.Messages.Commands;

public sealed record ReadTagsCommand(
    RscpTag[] Tags,
    RscpRequestOptions? Options = null) : IRscpCommand
{
    RscpRequestOptions IRscpCommand.Options => Options ?? RscpRequestOptions.Default;
}
