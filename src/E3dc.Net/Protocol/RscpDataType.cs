namespace E3dc.Protocol;

public enum RscpDataType : byte
{
    None      = 0x00,
    Bool      = 0x01,
    Char8     = 0x02,
    UChar8    = 0x03,
    Int16     = 0x04,
    UInt16    = 0x05,
    Int32     = 0x06,
    UInt32    = 0x07,
    Int64     = 0x08,
    UInt64    = 0x09,
    Float32   = 0x0A,
    Double64  = 0x0B,
    Bitfield  = 0x0C,
    CString   = 0x0D,
    Container = 0x0E,
    Timestamp = 0x0F,
    ByteArray = 0x10,
    Error     = 0xFF,
}

public static class RscpDataTypes
{
    public static int? FixedSize(RscpDataType type) => type switch
    {
        RscpDataType.Bool      => 1,
        RscpDataType.Char8     => 1,
        RscpDataType.UChar8    => 1,
        RscpDataType.Int16     => 2,
        RscpDataType.UInt16    => 2,
        RscpDataType.Int32     => 4,
        RscpDataType.UInt32    => 4,
        RscpDataType.Int64     => 8,
        RscpDataType.UInt64    => 8,
        RscpDataType.Float32   => 4,
        RscpDataType.Double64  => 8,
        RscpDataType.Bitfield  => 1,
        RscpDataType.Timestamp => 12,
        _ => null,
    };
}
