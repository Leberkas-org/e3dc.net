using E3dcConnector.Protocol;

namespace E3dcConnector.Messages;

public interface IRscpCommand
{
    RscpRequestOptions Options { get; }
}

public interface IRawItemsCommand : IRscpCommand
{
    IReadOnlyList<RscpDataItem> Items { get; }
}
