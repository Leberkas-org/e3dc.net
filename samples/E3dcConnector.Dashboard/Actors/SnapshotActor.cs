using Akka.Actor;
using Generated = E3dcConnector.Dashboard.Controllers.Generated;
using E3dcConnector.Typed.Bat;
using E3dcConnector.Typed.Dcdc;
using E3dcConnector.Typed.Ems;
using E3dcConnector.Typed.Ep;
using E3dcConnector.Typed.Info;
using E3dcConnector.Typed.Pm;
using E3dcConnector.Typed.Pvi;
using E3dcConnector.Typed.Wb;

namespace E3dcConnector.Dashboard.Actors;

public sealed class SnapshotActor : ReceiveActor
{
    private readonly int _maxHistory;
    private readonly Queue<Generated.DashboardSnapshot> _history;

    private EmsPowerSnapshot? _lastEms;
    private BatterySnapshot? _lastBat;
    private InverterSnapshot? _lastPvi;
    private PowerMeterSnapshot? _lastPm;
    private DcdcSnapshot? _lastDcdc;
    private EmergencyPowerSnapshot? _lastEp;
    private WallboxSnapshot? _lastWb;
    private DeviceInfo? _deviceInfo;
    private Generated.DashboardSnapshot? _latestSnapshot;
    private string _rawDump = "no data yet";
    private string? _lastError;

    public SnapshotActor(int maxHistory)
    {
        _maxHistory = maxHistory;
        _history = new Queue<Generated.DashboardSnapshot>(maxHistory);

        Receive<UpdateEms>(msg => Handle(msg));
        Receive<UpdateBat>(msg => Handle(msg));
        Receive<UpdatePvi>(msg => Handle(msg));
        Receive<UpdatePm>(msg => Handle(msg));
        Receive<UpdateDcdc>(msg => Handle(msg));
        Receive<UpdateEp>(msg => Handle(msg));
        Receive<UpdateWb>(msg => Handle(msg));
        Receive<UpdateDeviceInfo>(msg => Handle(msg));
        Receive<UpdateRawDump>(msg => Handle(msg));

        Receive<GetLatestSnapshot>(_ => Sender.Tell(new LatestSnapshotResult(_latestSnapshot)));
        Receive<GetHistory>(_ => Sender.Tell(new HistoryResult(_history.ToArray())));
        Receive<GetDeviceInfo>(_ => Sender.Tell(BuildDeviceInfoResult()));
        Receive<GetDiagnostics>(_ => Sender.Tell(BuildDiagnosticsResult()));
        Receive<GetRawDump>(_ => Sender.Tell(new RawDumpResult(_rawDump)));
    }

    private void Handle(UpdateEms msg)
    {
        try
        {
            _lastEms = msg.Snapshot;
            RebuildSnapshot();
        }
        catch (Exception ex) { _lastError = FormatError(ex); }
    }

    private void Handle(UpdateBat msg)
    {
        try
        {
            _lastBat = msg.Snapshot;
            RebuildSnapshot();
        }
        catch (Exception ex) { _lastError = FormatError(ex); }
    }

    private void Handle(UpdatePvi msg)
    {
        try
        {
            _lastPvi = msg.Snapshot;
            RebuildSnapshot();
        }
        catch (Exception ex) { _lastError = FormatError(ex); }
    }

    private void Handle(UpdatePm msg)
    {
        try
        {
            _lastPm = msg.Snapshot;
            RebuildSnapshot();
        }
        catch (Exception ex) { _lastError = FormatError(ex); }
    }

    private void Handle(UpdateDcdc msg) { try { _lastDcdc = msg.Snapshot; RebuildSnapshot(); } catch (Exception ex) { _lastError = FormatError(ex); } }
    private void Handle(UpdateEp msg)   { try { _lastEp   = msg.Snapshot; RebuildSnapshot(); } catch (Exception ex) { _lastError = FormatError(ex); } }
    private void Handle(UpdateWb msg)   { try { _lastWb   = msg.Snapshot; RebuildSnapshot(); } catch (Exception ex) { _lastError = FormatError(ex); } }

    private void Handle(UpdateDeviceInfo msg)
    {
        try
        {
            _deviceInfo = msg.Info;
        }
        catch (Exception ex) { _lastError = FormatError(ex); }
    }

    private void Handle(UpdateRawDump msg)
    {
        _rawDump = msg.Dump;
    }

    private void RebuildSnapshot()
    {
        if (_lastEms is null) return;

        var snapshot = new Generated.DashboardSnapshot
        {
            PvWatts         = _lastEms.PvWatts,
            BatteryWatts    = _lastEms.BatteryWatts,
            GridWatts       = _lastEms.GridWatts,
            HomeWatts       = _lastEms.HomeWatts,
            Soc             = _lastEms.Soc,
            Autarky         = _lastEms.Autarky,
            SelfConsumption = _lastEms.SelfConsumption,
            BatteryVoltage  = _lastBat?.Voltage ?? 0f,
            BatteryCurrent  = _lastBat?.Current ?? 0f,
            ChargeCycles    = _lastBat?.ChargeCycles ?? 0,
            PviAcPowerL1    = _lastPvi?.AcPowerL1,
            PviAcVoltageL1  = _lastPvi?.AcVoltageL1,
            PviDcPower      = _lastPvi?.DcPower,
            PviDcVoltage    = _lastPvi?.DcVoltage,
            PviDcCurrent    = _lastPvi?.DcCurrent,
            PviFrequency    = _lastPvi?.Frequency,
            PmPowerL1       = _lastPm?.PowerL1,
            PmPowerL2       = _lastPm?.PowerL2,
            PmPowerL3       = _lastPm?.PowerL3,
            PmVoltageL1     = _lastPm?.VoltageL1,
            PmVoltageL2     = _lastPm?.VoltageL2,
            PmVoltageL3     = _lastPm?.VoltageL3,
            PmEnergyL1      = _lastPm?.EnergyL1,
            PmEnergyL2      = _lastPm?.EnergyL2,
            PmEnergyL3      = _lastPm?.EnergyL3,
            DcdcBatteryCurrent = _lastDcdc?.BatteryCurrent,
            DcdcBatteryVoltage = _lastDcdc?.BatteryVoltage,
            DcdcBatteryPower   = _lastDcdc?.BatteryPower,
            EpIsReadyForSwitch = _lastEp?.IsReadyForSwitch,
            EpIsGridConnected  = _lastEp?.IsGridConnected,
            EpIsIslandGrid     = _lastEp?.IsIslandGrid,
            WbEnergyAll        = _lastWb?.EnergyAll,
            WbEnergySolar      = _lastWb?.EnergySolar,
            WbStatus           = _lastWb?.Status,
            WbMode             = _lastWb?.Mode,
            WbPowerL1          = _lastWb?.PowerL1,
            WbPowerL2          = _lastWb?.PowerL2,
            WbPowerL3          = _lastWb?.PowerL3,
            Timestamp       = DateTimeOffset.UtcNow,
        };

        _latestSnapshot = snapshot;

        _history.Enqueue(snapshot);
        while (_history.Count > _maxHistory)
            _history.Dequeue();
    }

    private DeviceInfoResult BuildDeviceInfoResult()
    {
        if (_deviceInfo is null) return new DeviceInfoResult(null);

        var dto = new Generated.DeviceInfoResponse
        {
            SerialNumber   = _deviceInfo.SerialNumber,
            ProductionDate = _deviceInfo.ProductionDate,
            SwRelease      = _deviceInfo.SwRelease,
            IpAddress      = _deviceInfo.IpAddress,
            SubnetMask     = _deviceInfo.SubnetMask,
            Gateway        = _deviceInfo.Gateway,
        };
        return new DeviceInfoResult(dto);
    }

    private DiagnosticsResult BuildDiagnosticsResult()
    {
        var info = new Generated.DiagnosticInfo
        {
            HasSnapshot   = _latestSnapshot is not null,
            HasEms        = _lastEms is not null,
            HasBat        = _lastBat is not null,
            HasPvi        = _lastPvi is not null,
            HasPm         = _lastPm is not null,
            HasDcdc       = _lastDcdc is not null,
            HasEp         = _lastEp is not null,
            HasWb         = _lastWb is not null,
            ConsumerCount = 0, // PollingActor will track this later
            LastError     = _lastError,
        };
        return new DiagnosticsResult(info);
    }

    private static string FormatError(Exception ex) =>
        $"{ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}";
}
