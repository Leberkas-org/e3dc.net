using E3dcConnector.Messages;
using E3dcConnector.Tags;

namespace E3dcConnector.Descriptors;

public static class Um
{
    public static readonly TagDescriptor UpdateStatus = new(RscpTag.UM_REQ_UPDATE_STATUS);
    public static readonly TagDescriptor CheckForUpdates = new(RscpTag.UM_REQ_CHECK_FOR_UPDATES);
}
