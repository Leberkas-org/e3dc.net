using E3dcConnector.Messages.Responses;
using E3dcConnector.Protocol;
using E3dcConnector.Tags;
using E3dcConnector.Typed;
using FluentAssertions;

namespace E3dcConnector.Tests.Typed;

public class RscpResponseExtensionsTests
{
    [Fact]
    public void ToEmsPowerSnapshot_parses_ems_items()
    {
        var items = new[]
        {
            MakeInt32Item(RscpTag.EMS_POWER_PV, 3500),
            MakeInt32Item(RscpTag.EMS_POWER_BAT, -1200),
            MakeInt32Item(RscpTag.EMS_POWER_GRID, 0),
            MakeInt32Item(RscpTag.EMS_POWER_HOME, 2300),
            MakeInt32Item(RscpTag.EMS_POWER_ADD, 0),
            MakeFloatItem(RscpTag.EMS_BAT_SOC, 85.5f),
            MakeFloatItem(RscpTag.EMS_AUTARKY, 92.3f),
            MakeFloatItem(RscpTag.EMS_SELF_CONSUMPTION, 78.1f),
        };
        var response = new RscpDataResponse(items, "test");

        var snapshot = response.ToEmsPowerSnapshot();

        snapshot.Should().NotBeNull();
        snapshot!.PvWatts.Should().Be(3500);
        snapshot.BatteryWatts.Should().Be(-1200);
        snapshot.Soc.Should().BeApproximately(85.5f, 0.01f);
    }

    [Fact]
    public void ToEmsPowerSnapshot_returns_null_when_no_ems_tags()
    {
        var response = new RscpDataResponse([], "test");
        response.ToEmsPowerSnapshot().Should().BeNull();
    }

    [Fact]
    public void ToBatterySnapshot_parses_bat_items()
    {
        var items = new[]
        {
            MakeFloatItem(RscpTag.BAT_RSOC, 90.0f),
            MakeFloatItem(RscpTag.BAT_MODULE_VOLTAGE, 48.2f),
            MakeFloatItem(RscpTag.BAT_CURRENT, -5.1f),
            MakeInt32Item(RscpTag.BAT_CHARGE_CYCLES, 312),
            MakeInt32Item(RscpTag.BAT_STATUS_CODE, 0),
            MakeInt32Item(RscpTag.BAT_ERROR_CODE, 0),
        };
        var response = new RscpDataResponse(items, "test");

        var snapshot = response.ToBatterySnapshot();

        snapshot.Should().NotBeNull();
        snapshot!.Rsoc.Should().BeApproximately(90.0f, 0.01f);
        snapshot.ChargeCycles.Should().Be(312);
    }

    private static RscpDataItem MakeInt32Item(RscpTag tag, int value)
        => new((uint)tag, RscpDataType.Int32, BitConverter.GetBytes(value));

    private static RscpDataItem MakeFloatItem(RscpTag tag, float value)
        => new((uint)tag, RscpDataType.Float32, BitConverter.GetBytes(value));
}
