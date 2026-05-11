namespace E3dcConnector.Messages;

public sealed record RscpRequestOptions
{
    public string CorrelationId { get; init; } = Guid.NewGuid().ToString();
    public TimeSpan? Timeout { get; init; }
    public static RscpRequestOptions Default => new();
}
