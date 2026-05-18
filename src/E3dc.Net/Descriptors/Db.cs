using E3dc.Messages;
using E3dc.Protocol;
using E3dc.Tags;

namespace E3dc.Descriptors;

public static class Db
{
    public static readonly TagDescriptor HistoryDay = new(RscpTag.DB_REQ_HISTORY_DATA_DAY, RscpDataType.Container);
    public static readonly TagDescriptor HistoryWeek = new(RscpTag.DB_REQ_HISTORY_DATA_WEEK, RscpDataType.Container);
    public static readonly TagDescriptor HistoryMonth = new(RscpTag.DB_REQ_HISTORY_DATA_MONTH, RscpDataType.Container);
    public static readonly TagDescriptor HistoryYear = new(RscpTag.DB_REQ_HISTORY_DATA_YEAR, RscpDataType.Container);
}
