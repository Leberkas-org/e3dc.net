using E3dc.Messages;
using E3dc.Tags;

namespace E3dc.Descriptors;

public static class Sys
{
    public static readonly TagDescriptor Reboot = new(RscpTag.SYS_REQ_SYSTEM_REBOOT);
    public static readonly TagDescriptor RestartApp = new(RscpTag.SYS_REQ_RESTART_APPLICATION);
}
