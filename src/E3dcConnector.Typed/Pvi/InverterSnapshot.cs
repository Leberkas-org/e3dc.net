namespace E3dcConnector.Typed.Pvi;

public sealed record InverterSnapshot(
    float AcPowerL1, float AcPowerL2, float AcPowerL3,
    float AcVoltageL1, float AcVoltageL2, float AcVoltageL3,
    float DcPower, float DcVoltage, float DcCurrent,
    float Frequency);
