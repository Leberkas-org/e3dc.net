using E3dc.Messages;
using E3dc.Tags;

namespace E3dc.Descriptors;

public static class Wb
{
    public static readonly DeviceDescriptor Device = new(RscpTag.WB_REQ_DATA, RscpTag.WB_INDEX);

    // Indexed sub-tags (must be inside FromDevice)
    public static readonly IndexedTag EnergyAll = new(RscpTag.WB_REQ_ENERGY_ALL);
    public static readonly IndexedTag EnergySolar = new(RscpTag.WB_REQ_ENERGY_SOLAR);
    public static readonly IndexedTag Status = new(RscpTag.WB_REQ_STATUS);
    public static readonly IndexedTag ErrorCode = new(RscpTag.WB_REQ_ERROR_CODE);
    public static readonly IndexedTag Mode = new(RscpTag.WB_REQ_MODE);
    public static readonly IndexedTag PmPowerL1 = new(RscpTag.WB_REQ_PM_POWER_L1);
    public static readonly IndexedTag PmPowerL2 = new(RscpTag.WB_REQ_PM_POWER_L2);
    public static readonly IndexedTag PmPowerL3 = new(RscpTag.WB_REQ_PM_POWER_L3);
}
