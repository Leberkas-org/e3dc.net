using E3dc.Protocol;
using E3dc.Reactive.Internal;
using E3dc.Tags;
using FluentAssertions;

namespace E3dc.Tests.Reactive;

public class RscpConnectionTests
{
    [Fact]
    public void BuildAuthFrame_creates_valid_container()
    {
        var frame = RscpConnection.BuildAuthFrame("testuser", "testpass");

        frame.Items.Should().HaveCount(1);
        frame.Items[0].DataType.Should().Be(RscpDataType.Container);
        frame.Items[0].Tag.Should().Be((uint)RscpTag.RSCP_REQ_AUTHENTICATION);

        var children = frame.Items[0].ParseContainerChildren();
        children.Should().HaveCount(2);
        children[0].Tag.Should().Be((uint)RscpTag.RSCP_AUTHENTICATION_USER);
        children[1].Tag.Should().Be((uint)RscpTag.RSCP_AUTHENTICATION_PASSWORD);
        System.Text.Encoding.UTF8.GetString(children[0].Value.Span).Should().Be("testuser");
        System.Text.Encoding.UTF8.GetString(children[1].Value.Span).Should().Be("testpass");
    }

    [Fact]
    public void ParseAuthResponse_extracts_auth_level()
    {
        var authItem = new RscpDataItem(
            (uint)RscpTag.RSCP_AUTHENTICATION,
            RscpDataType.UChar8,
            new byte[] { 10 });
        var frame = new RscpFrame(DateTimeOffset.UtcNow, [authItem]);

        var level = RscpConnection.ParseAuthLevel(frame);
        level.Should().Be(10);
    }

    [Fact]
    public void ParseAuthResponse_returns_0_for_no_auth_tag()
    {
        var frame = new RscpFrame(DateTimeOffset.UtcNow, []);
        var level = RscpConnection.ParseAuthLevel(frame);
        level.Should().Be(0);
    }
}
