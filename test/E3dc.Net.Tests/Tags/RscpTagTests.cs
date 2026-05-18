using E3dc.Tags;
using FluentAssertions;

namespace E3dc.Tests.Tags;

public class RscpTagTests
{
    [Theory]
    [InlineData(RscpTag.RSCP_REQ_AUTHENTICATION, 0x00000001u)]
    [InlineData(RscpTag.RSCP_AUTHENTICATION, 0x00800001u)]
    [InlineData(RscpTag.EMS_REQ_POWER_PV, 0x01000001u)]
    [InlineData(RscpTag.EMS_POWER_PV, 0x01800001u)]
    [InlineData(RscpTag.BAT_RSOC, 0x03800001u)]
    [InlineData(RscpTag.INFO_SERIAL_NUMBER, 0x0A800001u)]
    public void Tag_has_correct_value(RscpTag tag, uint expected)
    {
        ((uint)tag).Should().Be(expected);
    }

    [Theory]
    [InlineData(RscpTag.EMS_REQ_POWER_PV, RscpTagNamespace.Ems)]
    [InlineData(RscpTag.BAT_RSOC, RscpTagNamespace.Bat)]
    [InlineData(RscpTag.PVI_AC_POWER, RscpTagNamespace.Pvi)]
    [InlineData(RscpTag.RSCP_AUTHENTICATION, RscpTagNamespace.Rscp)]
    public void GetNamespace_returns_correct_namespace(RscpTag tag, RscpTagNamespace expected)
    {
        tag.GetNamespace().Should().Be(expected);
    }

    [Theory]
    [InlineData(RscpTag.EMS_REQ_POWER_PV, true)]
    [InlineData(RscpTag.EMS_POWER_PV, false)]
    [InlineData(RscpTag.RSCP_REQ_AUTHENTICATION, true)]
    [InlineData(RscpTag.RSCP_AUTHENTICATION, false)]
    public void IsRequest_and_IsResponse_work(RscpTag tag, bool isRequest)
    {
        tag.IsRequest().Should().Be(isRequest);
        tag.IsResponse().Should().Be(!isRequest);
    }
}
