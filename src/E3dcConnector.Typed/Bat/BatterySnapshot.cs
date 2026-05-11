namespace E3dcConnector.Typed.Bat;

public sealed record BatterySnapshot(
    float Rsoc,
    float Voltage,
    float Current,
    int ChargeCycles,
    int StatusCode,
    int ErrorCode);
