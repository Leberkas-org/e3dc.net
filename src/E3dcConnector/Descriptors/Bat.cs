using E3dcConnector.Messages;
using E3dcConnector.Tags;

namespace E3dcConnector.Descriptors;

public static class Bat
{
    public static readonly DeviceDescriptor Device = new(RscpTag.BAT_REQ_DATA, RscpTag.BAT_INDEX);

    // Indexed sub-tags (must be inside FromDevice)
    public static readonly IndexedTag Rsoc = new(RscpTag.BAT_RSOC);
    public static readonly IndexedTag ModuleVoltage = new(RscpTag.BAT_MODULE_VOLTAGE);
    public static readonly IndexedTag Current = new(RscpTag.BAT_CURRENT);
    public static readonly IndexedTag ChargeCycles = new(RscpTag.BAT_CHARGE_CYCLES);
    public static readonly IndexedTag StatusCode = new(RscpTag.BAT_STATUS_CODE);
    public static readonly IndexedTag ErrorCode = new(RscpTag.BAT_ERROR_CODE);
    public static readonly IndexedTag DcbCount = new(RscpTag.BAT_DCB_COUNT);
}
