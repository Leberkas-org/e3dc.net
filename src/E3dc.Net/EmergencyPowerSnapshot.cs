namespace E3dc;

public sealed record EmergencyPowerSnapshot(bool IsReadyForSwitch, bool IsGridConnected, bool IsIslandGrid);
