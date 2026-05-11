namespace E3dcConnector.Reactive;

public sealed record RscpFlowSettings
{
    public TimeSpan PollingInterval { get; init; } = TimeSpan.FromSeconds(2);
    public TimeSpan MinReconnectBackoff { get; init; } = TimeSpan.FromSeconds(1);
    public TimeSpan MaxReconnectBackoff { get; init; } = TimeSpan.FromSeconds(30);
    public double ReconnectRandomFactor { get; init; } = 0.2;
    public int MaxReconnectAttempts { get; init; } = -1;
    public TimeSpan SendTimeout { get; init; } = TimeSpan.FromSeconds(5);
}
