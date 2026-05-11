namespace E3dcConnector.Typed.Pm;

public sealed record PowerMeterSnapshot(
    float PowerL1, float PowerL2, float PowerL3,
    float VoltageL1, float VoltageL2, float VoltageL3,
    double EnergyL1, double EnergyL2, double EnergyL3);
