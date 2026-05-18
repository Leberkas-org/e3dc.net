namespace E3dc;

public sealed record BatterySnapshot(
    float Rsoc,
    float Voltage,
    float Current,
    int ChargeCycles,
    int StatusCode,
    int ErrorCode);
