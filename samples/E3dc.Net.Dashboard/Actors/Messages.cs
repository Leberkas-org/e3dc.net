using E3dc;
using E3dc.Messages;
using Generated = E3dc.Dashboard.Controllers.Generated;

namespace E3dc.Dashboard.Actors;

// ── SnapshotActor messages ──

public sealed record UpdateEms(EmsPowerSnapshot Snapshot);
public sealed record UpdateBat(BatterySnapshot Snapshot);
public sealed record UpdatePvi(InverterSnapshot Snapshot);
public sealed record UpdatePm(PowerMeterSnapshot Snapshot);
public sealed record UpdateDcdc(DcdcSnapshot Snapshot);
public sealed record UpdateEp(EmergencyPowerSnapshot Snapshot);
public sealed record UpdateWb(WallboxSnapshot Snapshot);
public sealed record UpdateDeviceInfo(DeviceInfo Info);
public sealed record UpdateRawDump(string Dump);
public sealed record UpdateRawItems(List<Generated.RscpItem> Items);

public sealed record GetLatestSnapshot;
public sealed record GetHistory;
public sealed record GetDeviceInfo;
public sealed record GetDiagnostics;
public sealed record GetRawDump;
public sealed record GetRawItems;

public sealed record LatestSnapshotResult(Generated.DashboardSnapshot? Snapshot);
public sealed record HistoryResult(Generated.DashboardSnapshot[] Snapshots);
public sealed record DeviceInfoResult(Generated.DeviceInfoResponse? Info);
public sealed record DiagnosticsResult(Generated.DiagnosticInfo Info);
public sealed record RawDumpResult(string Dump);
public sealed record RawItemsResult(List<Generated.RscpItem> Items);

// ── PollingActor messages ──

public sealed record ConsumerConnected;
public sealed record ConsumerDisconnected;

// ── RscpGatewayActor messages ──

public sealed record SendTagsRequest(Generated.SendRequest Request);
public sealed record SendTagsResponse(Generated.SendResponse? Response, string? Error);

public sealed record HistoryQueryMessage(Generated.HistoryQueryRequest Request);
public sealed record HistoryQueryResult(Generated.HistoryQueryResponse? Response, string? Error);

public sealed record SendPollingCommand(IRscpCommand Command);
