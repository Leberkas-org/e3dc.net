using E3dc.Messages;
using E3dc.Protocol;
using E3dc.Tags;

namespace E3dc.Descriptors;

public static class Ems
{
    // Power
    public static readonly TagDescriptor PowerPv = new(RscpTag.EMS_REQ_POWER_PV);
    public static readonly TagDescriptor PowerBat = new(RscpTag.EMS_REQ_POWER_BAT);
    public static readonly TagDescriptor PowerHome = new(RscpTag.EMS_REQ_POWER_HOME);
    public static readonly TagDescriptor PowerGrid = new(RscpTag.EMS_REQ_POWER_GRID);
    public static readonly TagDescriptor PowerAdd = new(RscpTag.EMS_REQ_POWER_ADD);

    // State
    public static readonly TagDescriptor BatSoc = new(RscpTag.EMS_REQ_BAT_SOC);
    public static readonly TagDescriptor Autarky = new(RscpTag.EMS_REQ_AUTARKY);
    public static readonly TagDescriptor SelfConsumption = new(RscpTag.EMS_REQ_SELF_CONSUMPTION);
    public static readonly TagDescriptor CouplingMode = new(RscpTag.EMS_REQ_COUPLING_MODE);
    public static readonly TagDescriptor Mode = new(RscpTag.EMS_REQ_MODE);

    // Control
    public static readonly TagDescriptor SetPower = new(RscpTag.EMS_REQ_SET_POWER, RscpDataType.Container);
    public static readonly TagDescriptor SetPowerMode = new(RscpTag.EMS_REQ_SET_POWER_MODE, RscpDataType.UChar8);
    public static readonly TagDescriptor SetPowerValue = new(RscpTag.EMS_REQ_SET_POWER_VALUE, RscpDataType.Int32);

    // Limits
    public static readonly TagDescriptor MaxChargePower = new(RscpTag.EMS_REQ_MAX_CHARGE_POWER);
    public static readonly TagDescriptor MaxDischargePower = new(RscpTag.EMS_REQ_MAX_DISCHARGE_POWER);
    public static readonly TagDescriptor BatChargeLimit = new(RscpTag.EMS_REQ_BAT_CHARGE_LIMIT);
    public static readonly TagDescriptor UserChargeLimit = new(RscpTag.EMS_REQ_USER_CHARGE_LIMIT);

    // Emergency Power
    public static readonly TagDescriptor EmergencyPowerStatus = new(RscpTag.EMS_REQ_EMERGENCY_POWER_STATUS);
    public static readonly TagDescriptor SetEmergencyPower = new(RscpTag.EMS_REQ_SET_EMERGENCY_POWER, RscpDataType.UChar8);
}
