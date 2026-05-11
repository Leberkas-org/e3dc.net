namespace E3dcConnector.Typed.Info;

public sealed record DeviceInfo(
    string SerialNumber, string ProductionDate, string SwRelease,
    string IpAddress, string SubnetMask, string Gateway);
