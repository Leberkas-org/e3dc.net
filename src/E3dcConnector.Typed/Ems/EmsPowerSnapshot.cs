namespace E3dcConnector.Typed.Ems;

public sealed record EmsPowerSnapshot(
    int PvWatts,
    int BatteryWatts,
    int GridWatts,
    int HomeWatts,
    int AdditionalWatts,
    float Soc,
    float Autarky,
    float SelfConsumption);
