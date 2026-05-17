using Akka.Actor;
using E3dcConnector.Dashboard.Actors;
using E3dcConnector.Tags;
using Microsoft.AspNetCore.Mvc;
using Generated = E3dcConnector.Dashboard.Controllers.Generated;

namespace E3dcConnector.Dashboard.Controllers;

public class RscpController(IActorRef gatewayActor) : Generated.RscpControllerBase
{
    private static readonly TimeSpan AskTimeout = TimeSpan.FromSeconds(10);

    public override Task<ActionResult<IDictionary<string, ICollection<Generated.TagEntry>>>> GetTags(CancellationToken ct)
    {
        var tags = Enum.GetValues<RscpTag>()
            .GroupBy(t =>
            {
                var val = (uint)t;
                return (val >> 24) switch
                {
                    0x00 => "RSCP", 0x01 => "EMS", 0x02 => "PVI", 0x03 => "BAT",
                    0x04 => "DCDC", 0x05 => "PM",  0x06 => "DB",  0x09 => "HA",
                    0x0A => "INFO", 0x0B => "EP",   0x0C => "SYS", 0x0D => "UM",
                    0x0E => "WB",   _ => "OTHER"
                };
            })
            .ToDictionary(
                g => g.Key,
                g => (ICollection<Generated.TagEntry>)g.Select(t => new Generated.TagEntry
                {
                    Name = t.ToString(),
                    Hex = $"0x{(uint)t:X8}"
                }).ToList());

        return Task.FromResult<ActionResult<IDictionary<string, ICollection<Generated.TagEntry>>>>(Ok(tags));
    }

    public override async Task<ActionResult<Generated.SendResponse>> SendRscpRequest(Generated.SendRequest body, CancellationToken ct)
    {
        var result = await gatewayActor.Ask<SendTagsResponse>(new SendTagsRequest(body), AskTimeout);
        if (result.Error is not null)
            return BadRequest(new Generated.ErrorResponse { Error = result.Error });
        return Ok(result.Response!);
    }

    public override async Task<ActionResult<Generated.HistoryQueryResponse>> QueryHistory(Generated.HistoryQueryRequest body, CancellationToken ct)
    {
        var result = await gatewayActor.Ask<HistoryQueryResult>(new HistoryQueryMessage(body), AskTimeout);
        if (result.Error is not null)
            return BadRequest(new Generated.ErrorResponse { Error = result.Error });
        return Ok(result.Response!);
    }
}
