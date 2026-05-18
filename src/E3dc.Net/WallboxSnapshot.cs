namespace E3dc;

public sealed record WallboxSnapshot(
    double EnergyAll, double EnergySolar,
    int Status, int ErrorCode, int Mode,
    float PowerL1, float PowerL2, float PowerL3);
