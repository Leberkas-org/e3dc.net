namespace E3dcConnector.Typed.Ep;

public sealed record EmergencyPowerSnapshot(bool IsReadyForSwitch, bool IsGridConnected, bool IsIslandGrid);
