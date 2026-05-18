using E3dc.Messages;
using E3dc.Tags;

namespace E3dc.Descriptors;

public static class Dcdc
{
    public static readonly DeviceDescriptor Device = new(RscpTag.DCDC_REQ_DATA, RscpTag.DCDC_INDEX);

    // Indexed sub-tags (must be inside FromDevice)
    public static readonly IndexedTag IBat = new(RscpTag.DCDC_REQ_I_BAT);
    public static readonly IndexedTag UBat = new(RscpTag.DCDC_REQ_U_BAT);
    public static readonly IndexedTag PBat = new(RscpTag.DCDC_REQ_P_BAT);
}
