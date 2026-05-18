using E3dc.Tags;

namespace E3dc.Messages.Commands;

public sealed record ReadTagsCommand(
    RscpTag[] Tags,
    RscpRequestOptions? Options = null) : IRscpCommand
{
    RscpRequestOptions IRscpCommand.Options => Options ?? RscpRequestOptions.Default;
}
