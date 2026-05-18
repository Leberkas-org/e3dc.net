using Akka.Actor;
using E3dc.Dashboard.Actors;
using E3dc.Dashboard.Configuration;
using Microsoft.AspNetCore.Mvc;
using Generated = E3dc.Dashboard.Controllers.Generated;

namespace E3dc.Dashboard.Controllers;

public class DiagnosticsController(ActorRegistry actors) : Generated.DiagnosticsControllerBase
{
    private static readonly TimeSpan AskTimeout = TimeSpan.FromSeconds(3);

    public override async Task<ActionResult<string>> GetDebugDump(CancellationToken ct)
    {
        var result = await actors.Snapshot.Ask<RawDumpResult>(new GetRawDump(), AskTimeout);
        return Content(result.Dump, "text/plain");
    }

    public override async Task<ActionResult<Generated.DiagnosticInfo>> GetDiagnostics(CancellationToken ct)
    {
        var result = await actors.Snapshot.Ask<DiagnosticsResult>(new GetDiagnostics(), AskTimeout);
        return Ok(result.Info);
    }

    [HttpGet("api/raw-items")]
    public async Task<ActionResult> GetRawItems(CancellationToken ct)
    {
        var result = await actors.Snapshot.Ask<RawItemsResult>(new GetRawItems(), AskTimeout);
        return Ok(result.Items);
    }
}
