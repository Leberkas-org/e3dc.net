namespace E3dcConnector.Tags;

public enum RscpTagNamespace : byte
{
    Rscp = 0x00,
    Ems  = 0x01,
    Pvi  = 0x02,
    Bat  = 0x03,
    Dcdc = 0x04,
    Pm   = 0x05,
    Db   = 0x06,
    Ha   = 0x09,
    Info = 0x0A,
    Ep   = 0x0B,
    Sys  = 0x0C,
    Um   = 0x0D,
    Wb   = 0x0E,
    Se   = 0x11,
}

public static class RscpTagExtensions
{
    public static RscpTagNamespace GetNamespace(this RscpTag tag)
        => (RscpTagNamespace)(((uint)tag >> 24) & 0xFF);

    public static bool IsRequest(this RscpTag tag)
        => (((uint)tag >> 20) & 0x08) == 0;

    public static bool IsResponse(this RscpTag tag)
        => (((uint)tag >> 20) & 0x08) != 0;
}
