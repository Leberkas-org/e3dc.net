using Akka.Actor;
using E3dcConnector.Dashboard.Actors;
using Microsoft.AspNetCore.Mvc;
using Generated = E3dcConnector.Dashboard.Controllers.Generated;

namespace E3dcConnector.Dashboard.Controllers;

public class DiagnosticsController(IActorRef snapshotActor) : Generated.DiagnosticsControllerBase
{
    private static readonly TimeSpan AskTimeout = TimeSpan.FromSeconds(3);

    public override async Task<ActionResult<string>> GetDebugDump(CancellationToken ct)
    {
        var result = await snapshotActor.Ask<RawDumpResult>(new GetRawDump(), AskTimeout);
        return Content(result.Dump, "text/plain");
    }

    public override async Task<ActionResult<Generated.DiagnosticInfo>> GetDiagnostics(CancellationToken ct)
    {
        var result = await snapshotActor.Ask<DiagnosticsResult>(new GetDiagnostics(), AskTimeout);
        return Ok(result.Info);
    }
}
