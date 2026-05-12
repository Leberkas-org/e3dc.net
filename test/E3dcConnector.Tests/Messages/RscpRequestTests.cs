using E3dcConnector.Messages;
using E3dcConnector.Messages.Descriptors;
using E3dcConnector.Protocol;
using E3dcConnector.Tags;
using FluentAssertions;

namespace E3dcConnector.Tests.Messages;

public class RscpRequestTests
{
    [Fact]
    public void Read_single_tag_creates_one_item_with_None_type()
    {
        var request = RscpRequest.Create()
            .Read(Ems.PowerPv);

        var items = request.BuildItems();
        items.Should().HaveCount(1);
        items[0].Tag.Should().Be((uint)RscpTag.EMS_REQ_POWER_PV);
        items[0].DataType.Should().Be(RscpDataType.None);
        items[0].Value.Length.Should().Be(0);
    }

    [Fact]
    public void Read_multiple_tags_via_params()
    {
        var request = RscpRequest.Create()
            .Read(Ems.PowerPv, Ems.PowerBat, Ems.BatSoc);

        var items = request.BuildItems();
        items.Should().HaveCount(3);
        items[0].Tag.Should().Be((uint)RscpTag.EMS_REQ_POWER_PV);
        items[1].Tag.Should().Be((uint)RscpTag.EMS_REQ_POWER_BAT);
        items[2].Tag.Should().Be((uint)RscpTag.EMS_REQ_BAT_SOC);
    }

    [Fact]
    public void Write_int_creates_Int32_item()
    {
        var request = RscpRequest.Create()
            .Write(Ems.SetPowerValue, 1500);

        var items = request.BuildItems();
        items.Should().HaveCount(1);
        items[0].Tag.Should().Be((uint)RscpTag.EMS_REQ_SET_POWER_VALUE);
        items[0].DataType.Should().Be(RscpDataType.Int32);
        BitConverter.ToInt32(items[0].Value.Span).Should().Be(1500);
    }

    [Fact]
    public void Write_byte_creates_UChar8_item()
    {
        var request = RscpRequest.Create()
            .Write(Ems.SetPowerMode, (byte)3);

        var items = request.BuildItems();
        items[0].DataType.Should().Be(RscpDataType.UChar8);
        items[0].Value.Span[0].Should().Be(3);
    }

    [Fact]
    public void Write_string_creates_CString_item()
    {
        var descriptor = new TagDescriptor(RscpTag.RSCP_AUTHENTICATION_USER);
        var request = RscpRequest.Create()
            .Write(descriptor, "testuser");

        var items = request.BuildItems();
        items[0].DataType.Should().Be(RscpDataType.CString);
        System.Text.Encoding.UTF8.GetString(items[0].Value.Span).Should().Be("testuser");
    }

    [Fact]
    public void Write_float_creates_Float32_item()
    {
        var descriptor = new TagDescriptor(RscpTag.EMS_REQ_SET_POWER_VALUE, RscpDataType.Float32);
        var request = RscpRequest.Create()
            .Write(descriptor, 42.5f);

        var items = request.BuildItems();
        items[0].DataType.Should().Be(RscpDataType.Float32);
        BitConverter.ToSingle(items[0].Value.Span).Should().BeApproximately(42.5f, 0.01f);
    }

    [Fact]
    public void FromDevice_creates_container_with_index_and_sub_tags()
    {
        var request = RscpRequest.Create()
            .FromDevice(Pvi.Device, 0, b => b
                .Read(Pvi.AcPower)
                .Read(Pvi.DcPower));

        var items = request.BuildItems();
        items.Should().HaveCount(1);
        items[0].Tag.Should().Be((uint)RscpTag.PVI_REQ_DATA);
        items[0].DataType.Should().Be(RscpDataType.Container);

        var children = items[0].ParseContainerChildren();
        children.Should().HaveCount(3);
        children[0].Tag.Should().Be((uint)RscpTag.PVI_INDEX);
        BitConverter.ToUInt16(children[0].Value.Span).Should().Be(0);
        children[1].Tag.Should().Be((uint)RscpTag.PVI_AC_POWER);
        children[2].Tag.Should().Be((uint)RscpTag.PVI_DC_POWER);
    }

    [Fact]
    public void FromDevice_with_nonzero_index()
    {
        var request = RscpRequest.Create()
            .FromDevice(Bat.Device, 1, b => b
                .Read(Bat.Rsoc, Bat.ChargeCycles));

        var items = request.BuildItems();
        var children = items[0].ParseContainerChildren();
        children[0].Tag.Should().Be((uint)RscpTag.BAT_INDEX);
        BitConverter.ToUInt16(children[0].Value.Span).Should().Be(1);
        children.Should().HaveCount(3);
    }

    [Fact]
    public void Chaining_reads_and_writes_preserves_order()
    {
        var request = RscpRequest.Create()
            .Read(Ems.PowerPv)
            .Read(Ems.PowerBat)
            .Write(Ems.SetPowerMode, (byte)1)
            .Read(Ems.BatSoc);

        var items = request.BuildItems();
        items.Should().HaveCount(4);
        items[0].Tag.Should().Be((uint)RscpTag.EMS_REQ_POWER_PV);
        items[2].Tag.Should().Be((uint)RscpTag.EMS_REQ_SET_POWER_MODE);
        items[3].Tag.Should().Be((uint)RscpTag.EMS_REQ_BAT_SOC);
    }

    [Fact]
    public void Request_implements_IRscpCommand_with_correlation_id()
    {
        var request = RscpRequest.Create()
            .Read(Ems.PowerPv);

        var cmd = (IRscpCommand)request;
        cmd.Options.CorrelationId.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Nested_containers()
    {
        var request = RscpRequest.Create()
            .Container(Ems.SetPower, outer => outer
                .Write(Ems.SetPowerMode, (byte)2)
                .Write(Ems.SetPowerValue, 3000));

        var items = request.BuildItems();
        var children = items[0].ParseContainerChildren();
        children.Should().HaveCount(2);
        children[0].Value.Span[0].Should().Be(2);
        BitConverter.ToInt32(children[1].Value.Span).Should().Be(3000);
    }
}
