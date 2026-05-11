using E3dcConnector.Protocol;

namespace E3dcConnector.Messages.Responses;

public sealed record RscpDataResponse(
    IReadOnlyList<RscpDataItem> Items,
    string CorrelationId) : IRscpResponse;
