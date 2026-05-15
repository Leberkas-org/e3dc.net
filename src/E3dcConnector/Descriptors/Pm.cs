using E3dcConnector.Messages;
using E3dcConnector.Tags;

namespace E3dcConnector.Descriptors;

public static class Pm
{
    public static readonly DeviceDescriptor Device = new(RscpTag.PM_REQ_DATA, RscpTag.PM_INDEX);

    // Indexed sub-tags (must be inside FromDevice)
    public static readonly IndexedTag PowerL1 = new(RscpTag.PM_REQ_POWER_L1);
    public static readonly IndexedTag PowerL2 = new(RscpTag.PM_REQ_POWER_L2);
    public static readonly IndexedTag PowerL3 = new(RscpTag.PM_REQ_POWER_L3);
    public static readonly IndexedTag VoltageL1 = new(RscpTag.PM_REQ_VOLTAGE_L1);
    public static readonly IndexedTag VoltageL2 = new(RscpTag.PM_REQ_VOLTAGE_L2);
    public static readonly IndexedTag VoltageL3 = new(RscpTag.PM_REQ_VOLTAGE_L3);
    public static readonly IndexedTag EnergyL1 = new(RscpTag.PM_REQ_ENERGY_L1);
    public static readonly IndexedTag EnergyL2 = new(RscpTag.PM_REQ_ENERGY_L2);
    public static readonly IndexedTag EnergyL3 = new(RscpTag.PM_REQ_ENERGY_L3);
}
