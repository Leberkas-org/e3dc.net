using E3dcConnector.Messages;

namespace E3dcConnector.Typed.Ems;

public enum EmsMode : byte { Normal = 0, Idle = 1, Discharge = 2, Charge = 3, GridCharge = 4 }

public sealed record SetPowerMode(EmsMode Mode, int ValueWatts, RscpRequestOptions? Options = null) : IRscpCommand
{
    RscpRequestOptions IRscpCommand.Options => Options ?? RscpRequestOptions.Default;
}

public sealed record SetChargeLimit(int LimitWatts, RscpRequestOptions? Options = null) : IRscpCommand
{
    RscpRequestOptions IRscpCommand.Options => Options ?? RscpRequestOptions.Default;
}

public sealed record SetEmergencyPower(bool Enable, RscpRequestOptions? Options = null) : IRscpCommand
{
    RscpRequestOptions IRscpCommand.Options => Options ?? RscpRequestOptions.Default;
}
