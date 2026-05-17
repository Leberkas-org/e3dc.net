namespace E3dcConnector.Dashboard.Configuration;

public sealed class E3dcOptions
{
    public const string SectionName = "E3DC";

    public string Host { get; init; } = "192.168.1.100";
    public int Port { get; init; } = 5033;
    public string User { get; init; } = "";
    public string Password { get; init; } = "";
    public string RscpKey { get; init; } = "";
    public int FastPollingIntervalSeconds { get; init; } = 2;
    public int MediumPollingIntervalSeconds { get; init; } = 10;
    public int HistoryRetentionMinutes { get; init; } = 60;
    public int BatDeviceIndex { get; init; } = 0;
    public int PviDeviceIndex { get; init; } = 0;
    public int PmDeviceIndex { get; init; } = 6;
}
