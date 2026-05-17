using E3dcConnector.Messages;
using E3dcConnector.Tags;

namespace E3dcConnector.Descriptors;

public static class Sys
{
    public static readonly TagDescriptor Reboot = new(RscpTag.SYS_REQ_SYSTEM_REBOOT);
    public static readonly TagDescriptor RestartApp = new(RscpTag.SYS_REQ_RESTART_APPLICATION);
}
