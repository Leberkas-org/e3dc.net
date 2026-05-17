using System.Buffers.Binary;
using E3dcConnector.Messages.Responses;
using E3dcConnector.Protocol;
using E3dcConnector.Tags;
using E3dcConnector.Typed;
using FluentAssertions;

namespace E3dcConnector.Tests.Typed;

public class RscpResponseExtensionTests
{
    private static RscpDataItem FloatItem(RscpTag tag, float value)
    {
        var buf = new byte[4];
        BinaryPrimitives.WriteSingleLittleEndian(buf, value);
        return new RscpDataItem((uint)tag, RscpDataType.Float32, buf);
    }

    private static RscpDataItem DoubleItem(RscpTag tag, double value)
    {
        var buf = new byte[8];
        BinaryPrimitives.WriteDoubleLittleEndian(buf, value);
        return new RscpDataItem((uint)tag, RscpDataType.Double64, buf);
    }

    private static RscpDataItem Int32Item(RscpTag tag, int value)
    {
        var buf = new byte[4];
        BinaryPrimitives.WriteInt32LittleEndian(buf, value);
        return new RscpDataItem((uint)tag, RscpDataType.Int32, buf);
    }

    private static RscpDataItem BoolItem(RscpTag tag, bool value)
        => new((uint)tag, RscpDataType.Bool, new[] { (byte)(value ? 1 : 0) });

    [Fact]
    public void ToInverterSnapshot_parses_flat_PVI_response()
    {
        var response = new RscpDataResponse([
            FloatItem(RscpTag.PVI_AC_POWER, 983f),
            FloatItem(RscpTag.PVI_AC_VOLTAGE, 232.4f),
            FloatItem(RscpTag.PVI_DC_POWER, 2920f),
            FloatItem(RscpTag.PVI_DC_VOLTAGE, 380.5f),
            FloatItem(RscpTag.PVI_DC_CURRENT, 7.68f),
            FloatItem(RscpTag.PVI_AC_FREQUENCY, 50.01f),
        ], "test");

        var snap = response.ToInverterSnapshot();
        snap.Should().NotBeNull();
        snap!.AcPowerL1.Should().BeApproximately(983f, 0.1f);
        snap.AcVoltageL1.Should().BeApproximately(232.4f, 0.1f);
        snap.DcPower.Should().BeApproximately(2920f, 0.1f);
        snap.DcVoltage.Should().BeApproximately(380.5f, 0.1f);
        snap.DcCurrent.Should().BeApproximately(7.68f, 0.1f);
        snap.Frequency.Should().BeApproximately(50.01f, 0.01f);
    }

    [Fact]
    public void ToInverterSnapshot_returns_null_when_no_PVI_tags()
    {
        var response = new RscpDataResponse([
            FloatItem(RscpTag.EMS_POWER_PV, 1000f),
        ], "test");
        response.ToInverterSnapshot().Should().BeNull();
    }

    [Fact]
    public void ToPowerMeterSnapshot_parses_flat_PM_response()
    {
        var response = new RscpDataResponse([
            FloatItem(RscpTag.PM_POWER_L1, -12f),
            FloatItem(RscpTag.PM_POWER_L2, 3f),
            FloatItem(RscpTag.PM_POWER_L3, 9f),
            FloatItem(RscpTag.PM_VOLTAGE_L1, 232.1f),
            FloatItem(RscpTag.PM_VOLTAGE_L2, 231.5f),
            FloatItem(RscpTag.PM_VOLTAGE_L3, 232.8f),
            DoubleItem(RscpTag.PM_ENERGY_L1, 1247.3),
            DoubleItem(RscpTag.PM_ENERGY_L2, 1189.7),
            DoubleItem(RscpTag.PM_ENERGY_L3, 1203.1),
        ], "test");

        var snap = response.ToPowerMeterSnapshot();
        snap.Should().NotBeNull();
        snap!.PowerL1.Should().BeApproximately(-12f, 0.1f);
        snap.PowerL2.Should().BeApproximately(3f, 0.1f);
        snap.PowerL3.Should().BeApproximately(9f, 0.1f);
        snap.VoltageL1.Should().BeApproximately(232.1f, 0.1f);
        snap.EnergyL1.Should().BeApproximately(1247.3, 0.1);
        snap.EnergyL2.Should().BeApproximately(1189.7, 0.1);
    }

    [Fact]
    public void ToPowerMeterSnapshot_returns_null_when_no_PM_tags()
    {
        var response = new RscpDataResponse([
            FloatItem(RscpTag.EMS_POWER_PV, 1000f),
        ], "test");
        response.ToPowerMeterSnapshot().Should().BeNull();
    }

    [Fact]
    public void ToInverterSnapshot_handles_container_wrapped_values()
    {
        // Real E3DC wraps each PVI value in a container: {PVI_INDEX, VALUE_TAG(Float32)}
        var acPower = RscpDataItem.CreateContainer((uint)RscpTag.PVI_AC_POWER, [
            new RscpDataItem((uint)RscpTag.PVI_INDEX, RscpDataType.UInt16, BitConverter.GetBytes((ushort)0)),
            FloatItem((RscpTag)0x02040005, 965f),
        ]);
        var dcPower = RscpDataItem.CreateContainer((uint)RscpTag.PVI_DC_POWER, [
            new RscpDataItem((uint)RscpTag.PVI_INDEX, RscpDataType.UInt16, BitConverter.GetBytes((ushort)0)),
            FloatItem((RscpTag)0x02040005, 1756f),
        ]);
        var freq = RscpDataItem.CreateContainer((uint)RscpTag.PVI_AC_FREQUENCY, [
            new RscpDataItem((uint)RscpTag.PVI_INDEX, RscpDataType.UInt16, BitConverter.GetBytes((ushort)0)),
            FloatItem((RscpTag)0x02040005, 50.05f),
        ]);

        var pviData = RscpDataItem.CreateContainer((uint)RscpTag.PVI_DATA, [
            new RscpDataItem((uint)RscpTag.PVI_INDEX, RscpDataType.UInt16, BitConverter.GetBytes((ushort)0)),
            acPower, dcPower, freq,
        ]);

        var response = new RscpDataResponse([pviData], "test");
        var snap = response.ToInverterSnapshot();
        snap.Should().NotBeNull();
        snap!.AcPowerL1.Should().BeApproximately(965f, 0.1f);
        snap.DcPower.Should().BeApproximately(1756f, 0.1f);
        snap.Frequency.Should().BeApproximately(50.05f, 0.01f);
    }

    [Fact]
    public void ToDcdcSnapshot_parses_flat_DCDC_response()
    {
        var response = new RscpDataResponse([
            FloatItem(RscpTag.DCDC_I_BAT, 12.5f),
            FloatItem(RscpTag.DCDC_U_BAT, 48.2f),
            FloatItem(RscpTag.DCDC_P_BAT, 603f),
        ], "test");

        var snap = response.ToDcdcSnapshot();
        snap.Should().NotBeNull();
        snap!.BatteryCurrent.Should().BeApproximately(12.5f, 0.1f);
        snap.BatteryVoltage.Should().BeApproximately(48.2f, 0.1f);
        snap.BatteryPower.Should().BeApproximately(603f, 0.1f);
    }

    [Fact]
    public void ToDcdcSnapshot_returns_null_when_no_DCDC_tags()
    {
        var response = new RscpDataResponse([
            FloatItem(RscpTag.EMS_POWER_PV, 1000f),
        ], "test");
        response.ToDcdcSnapshot().Should().BeNull();
    }

    [Fact]
    public void ToEmergencyPowerSnapshot_parses_EP_response()
    {
        var response = new RscpDataResponse([
            BoolItem(RscpTag.EP_IS_READY_FOR_SWITCH, true),
            BoolItem(RscpTag.EP_IS_GRID_CONNECTED, true),
            BoolItem(RscpTag.EP_IS_ISLAND_GRID, false),
        ], "test");

        var snap = response.ToEmergencyPowerSnapshot();
        snap.Should().NotBeNull();
        snap!.IsReadyForSwitch.Should().BeTrue();
        snap.IsGridConnected.Should().BeTrue();
        snap.IsIslandGrid.Should().BeFalse();
    }

    [Fact]
    public void ToEmergencyPowerSnapshot_returns_null_when_no_EP_tags()
    {
        var response = new RscpDataResponse([
            FloatItem(RscpTag.EMS_POWER_PV, 1000f),
        ], "test");
        response.ToEmergencyPowerSnapshot().Should().BeNull();
    }

    [Fact]
    public void ToWallboxSnapshot_parses_WB_response()
    {
        var response = new RscpDataResponse([
            DoubleItem(RscpTag.WB_ENERGY_ALL, 12345.6),
            DoubleItem(RscpTag.WB_ENERGY_SOLAR, 9876.5),
            Int32Item(RscpTag.WB_STATUS, 1),
            Int32Item(RscpTag.WB_ERROR_CODE, 0),
            Int32Item(RscpTag.WB_MODE, 4),
            FloatItem(RscpTag.WB_PM_POWER_L1, 3680f),
            FloatItem(RscpTag.WB_PM_POWER_L2, 3650f),
            FloatItem(RscpTag.WB_PM_POWER_L3, 3700f),
        ], "test");

        var snap = response.ToWallboxSnapshot();
        snap.Should().NotBeNull();
        snap!.EnergyAll.Should().BeApproximately(12345.6, 0.1);
        snap.EnergySolar.Should().BeApproximately(9876.5, 0.1);
        snap.Status.Should().Be(1);
        snap.ErrorCode.Should().Be(0);
        snap.Mode.Should().Be(4);
        snap.PowerL1.Should().BeApproximately(3680f, 0.1f);
        snap.PowerL2.Should().BeApproximately(3650f, 0.1f);
        snap.PowerL3.Should().BeApproximately(3700f, 0.1f);
    }

    [Fact]
    public void ToWallboxSnapshot_returns_null_when_no_WB_tags()
    {
        var response = new RscpDataResponse([
            FloatItem(RscpTag.EMS_POWER_PV, 1000f),
        ], "test");
        response.ToWallboxSnapshot().Should().BeNull();
    }
}
