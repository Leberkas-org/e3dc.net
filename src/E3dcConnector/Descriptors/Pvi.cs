using E3dcConnector.Messages;
using E3dcConnector.Tags;

namespace E3dcConnector.Descriptors;

public static class Pvi
{
    public static readonly DeviceDescriptor Device = new(RscpTag.PVI_REQ_DATA, RscpTag.PVI_INDEX);

    // Indexed sub-tags — use REQ_ variants inside FromDevice
    public static readonly IndexedTag OnGrid = new(RscpTag.PVI_REQ_ON_GRID);
    public static readonly IndexedTag State = new(RscpTag.PVI_REQ_STATE);
    public static readonly IndexedTag AcPower = new(RscpTag.PVI_REQ_AC_POWER);
    public static readonly IndexedTag AcVoltage = new(RscpTag.PVI_REQ_AC_VOLTAGE);
    public static readonly IndexedTag AcCurrent = new(RscpTag.PVI_REQ_AC_CURRENT);
    public static readonly IndexedTag AcFrequency = new(RscpTag.PVI_REQ_AC_FREQUENCY);
    public static readonly IndexedTag DcPower = new(RscpTag.PVI_REQ_DC_POWER);
    public static readonly IndexedTag DcVoltage = new(RscpTag.PVI_REQ_DC_VOLTAGE);
    public static readonly IndexedTag DcCurrent = new(RscpTag.PVI_REQ_DC_CURRENT);
}
