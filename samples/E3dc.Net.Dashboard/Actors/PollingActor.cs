using Akka.Actor;
using E3dc.Dashboard.Configuration;
using E3dc.Messages;
using D = E3dc.Descriptors;

namespace E3dc.Dashboard.Actors;

public sealed class PollingActor : ReceiveActor, IWithTimers
{
    private sealed record FastTick;
    private sealed record MediumTick;

    public ITimerScheduler Timers { get; set; } = null!;

    private readonly E3dcOptions _options;
    private readonly IActorRef _gateway;
    private IRscpCommand? _fastRequest;
    private IRscpCommand? _mediumRequest;
    private int _consumerCount;

    public PollingActor(E3dcOptions options, IActorRef gateway)
    {
        _options = options;
        _gateway = gateway;

        Receive<FastTick>(_ => _gateway.Tell(new SendPollingCommand(_fastRequest!)));
        Receive<MediumTick>(_ => _gateway.Tell(new SendPollingCommand(_mediumRequest!)));

        Receive<ConsumerConnected>(_ =>
        {
            _consumerCount++;
            if (_consumerCount == 1)
                Timers.StartPeriodicTimer("medium", new MediumTick(), TimeSpan.Zero, TimeSpan.FromSeconds(_options.MediumPollingIntervalSeconds));
        });

        Receive<ConsumerDisconnected>(_ =>
        {
            _consumerCount = Math.Max(0, _consumerCount - 1);
            if (_consumerCount == 0)
                Timers.Cancel("medium");
        });
    }

    protected override void PreStart()
    {
        _fastRequest = RscpRequest.Create()
            .Read(D.Ems.PowerPv, D.Ems.PowerBat, D.Ems.PowerGrid, D.Ems.PowerHome)
            .Read(D.Ems.BatSoc, D.Ems.Autarky, D.Ems.SelfConsumption) as IRscpCommand;

        _mediumRequest = RscpRequest.Create()
            .FromDevice(D.Bat.Device, _options.BatDeviceIndex, b => b
                .Read(D.Bat.Rsoc, D.Bat.ModuleVoltage, D.Bat.Current, D.Bat.ChargeCycles))
            .FromDevice(D.Pvi.Device, _options.PviDeviceIndex, b => b
                .Read(D.Pvi.AcPower, D.Pvi.AcVoltage, D.Pvi.AcFrequency,
                      D.Pvi.DcPower, D.Pvi.DcVoltage, D.Pvi.DcCurrent))
            .FromDevice(D.Pm.Device, _options.PmDeviceIndex, b => b
                .Read(D.Pm.PowerL1, D.Pm.PowerL2, D.Pm.PowerL3,
                      D.Pm.VoltageL1, D.Pm.VoltageL2, D.Pm.VoltageL3,
                      D.Pm.EnergyL1, D.Pm.EnergyL2, D.Pm.EnergyL3))
            .FromDevice(D.Dcdc.Device, _options.DcdcDeviceIndex, b => b
                .Read(D.Dcdc.IBat, D.Dcdc.UBat, D.Dcdc.PBat))
            .FromDevice(D.Wb.Device, _options.WbDeviceIndex, b => b
                .Read(D.Wb.EnergyAll, D.Wb.EnergySolar, D.Wb.Status, D.Wb.Mode,
                      D.Wb.PmPowerL1, D.Wb.PmPowerL2, D.Wb.PmPowerL3)) as IRscpCommand;

        var infoRequest = RscpRequest.Create()
            .Read(D.Info.SerialNumber, D.Info.ProductionDate, D.Info.SwRelease,
                  D.Info.IpAddress, D.Info.SubnetMask, D.Info.Gateway,
                  D.Info.Dns, D.Info.Time, D.Info.TimeZone) as IRscpCommand;

        var epRequest = RscpRequest.Create()
            .Read(D.Ep.IsReadyForSwitch, D.Ep.IsGridConnected, D.Ep.IsIslandGrid) as IRscpCommand;

        Timers.StartPeriodicTimer("fast", new FastTick(), TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(_options.FastPollingIntervalSeconds));

        var haRequest = RscpRequest.Create()
            .Read(D.Ha.DatapointList, D.Ha.ActuatorStates) as IRscpCommand;

        var umRequest = RscpRequest.Create()
            .Read(D.Um.UpdateStatus) as IRscpCommand;

        Context.System.Scheduler.ScheduleTellOnce(TimeSpan.FromSeconds(2), _gateway, new SendPollingCommand(infoRequest!), Self);
        Context.System.Scheduler.ScheduleTellOnce(TimeSpan.FromSeconds(3), _gateway, new SendPollingCommand(epRequest!), Self);
        Context.System.Scheduler.ScheduleTellOnce(TimeSpan.FromSeconds(4), _gateway, new SendPollingCommand(haRequest!), Self);
        Context.System.Scheduler.ScheduleTellOnce(TimeSpan.FromSeconds(5), _gateway, new SendPollingCommand(umRequest!), Self);
    }

    public static Props Props(E3dcOptions options, IActorRef gateway)
        => Akka.Actor.Props.Create(() => new PollingActor(options, gateway));
}
