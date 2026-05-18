namespace E3dc.Messages;

public enum HistoryPeriod { Day, Week, Month, Year }

public sealed record HistoryQuery(
    DateTimeOffset Start,
    HistoryPeriod Period,
    RscpRequestOptions? Options = null) : IRscpCommand
{
    RscpRequestOptions IRscpCommand.Options => Options ?? RscpRequestOptions.Default;
}
