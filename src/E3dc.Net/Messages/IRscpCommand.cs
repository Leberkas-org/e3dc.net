using E3dc.Protocol;

namespace E3dc.Messages;

public interface IRscpCommand
{
    RscpRequestOptions Options { get; }
}

public interface IRawItemsCommand : IRscpCommand
{
    IReadOnlyList<RscpDataItem> Items { get; }
}
