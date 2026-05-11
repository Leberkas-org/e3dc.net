namespace E3dcConnector.Messages.Responses;

public sealed record RscpErrorResponse(
    string Message,
    Exception? Exception = null,
    string? CorrelationId = null) : IRscpResponse
{
    string IRscpResponse.CorrelationId => CorrelationId ?? "";
}
