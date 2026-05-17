using Akka.Actor;
using E3dcConnector.Dashboard.Actors;
using Microsoft.AspNetCore.Mvc;
using Generated = E3dcConnector.Dashboard.Controllers.Generated;

namespace E3dcConnector.Dashboard.Controllers;

public class DashboardController(IActorRef snapshotActor) : Generated.DashboardControllerBase
{
    private static readonly TimeSpan AskTimeout = TimeSpan.FromSeconds(3);

    public override async Task<ActionResult<ICollection<Generated.DashboardSnapshot>>> GetHistory(CancellationToken ct)
    {
        var result = await snapshotActor.Ask<HistoryResult>(new GetHistory(), AskTimeout);
        return Ok((ICollection<Generated.DashboardSnapshot>)result.Snapshots);
    }

    public override async Task<ActionResult<Generated.DeviceInfoResponse>> GetDeviceInfo(CancellationToken ct)
    {
        var result = await snapshotActor.Ask<DeviceInfoResult>(new GetDeviceInfo(), AskTimeout);
        if (result.Info is null)
            return Ok(new Generated.DeviceInfoResponse { SerialNumber = "", SwRelease = "", IpAddress = "" });
        return Ok(result.Info);
    }
}
