using E3dc.Protocol;

namespace E3dc.Messages.Responses;

public sealed record RscpDataResponse(
    IReadOnlyList<RscpDataItem> Items,
    string CorrelationId) : IRscpResponse;
