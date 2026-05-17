using Akka.Actor;
using E3dcConnector.Dashboard.Configuration;
using E3dcConnector.Descriptors;
using E3dcConnector.Messages;

namespace E3dcConnector.Dashboard.Actors;

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
            .Read(Ems.PowerPv, Ems.PowerBat, Ems.PowerGrid, Ems.PowerHome)
            .Read(Ems.BatSoc, Ems.Autarky, Ems.SelfConsumption) as IRscpCommand;

        _mediumRequest = RscpRequest.Create()
            .FromDevice(Bat.Device, _options.BatDeviceIndex, b => b
                .Read(Bat.Rsoc, Bat.ModuleVoltage, Bat.Current, Bat.ChargeCycles))
            .FromDevice(Pvi.Device, _options.PviDeviceIndex, b => b
                .Read(Pvi.AcPower, Pvi.AcVoltage, Pvi.AcFrequency,
                      Pvi.DcPower, Pvi.DcVoltage, Pvi.DcCurrent))
            .FromDevice(Pm.Device, _options.PmDeviceIndex, b => b
                .Read(Pm.PowerL1, Pm.PowerL2, Pm.PowerL3,
                      Pm.VoltageL1, Pm.VoltageL2, Pm.VoltageL3,
                      Pm.EnergyL1, Pm.EnergyL2, Pm.EnergyL3))
            .FromDevice(Dcdc.Device, 0, b => b
                .Read(Dcdc.IBat, Dcdc.UBat, Dcdc.PBat))
            .FromDevice(Wb.Device, 0, b => b
                .Read(Wb.EnergyAll, Wb.EnergySolar, Wb.Status, Wb.Mode,
                      Wb.PmPowerL1, Wb.PmPowerL2, Wb.PmPowerL3)) as IRscpCommand;

        var infoRequest = RscpRequest.Create()
            .Read(Info.SerialNumber, Info.SwRelease, Info.IpAddress) as IRscpCommand;

        var epRequest = RscpRequest.Create()
            .Read(Ep.IsReadyForSwitch, Ep.IsGridConnected, Ep.IsIslandGrid) as IRscpCommand;

        Timers.StartPeriodicTimer("fast", new FastTick(), TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(_options.FastPollingIntervalSeconds));

        Context.System.Scheduler.ScheduleTellOnce(TimeSpan.FromSeconds(2), _gateway, new SendPollingCommand(infoRequest!), Self);
        Context.System.Scheduler.ScheduleTellOnce(TimeSpan.FromSeconds(3), _gateway, new SendPollingCommand(epRequest!), Self);
    }

    public static Props Props(E3dcOptions options, IActorRef gateway)
        => Akka.Actor.Props.Create(() => new PollingActor(options, gateway));
}
