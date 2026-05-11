using E3dcConnector.Messages;

namespace E3dcConnector.Typed.Db;

public enum HistoryPeriod { Day, Week, Month, Year }

public sealed record HistoryQuery(
    DateTimeOffset Start,
    HistoryPeriod Period,
    RscpRequestOptions? Options = null) : IRscpCommand
{
    RscpRequestOptions IRscpCommand.Options => Options ?? RscpRequestOptions.Default;
}
