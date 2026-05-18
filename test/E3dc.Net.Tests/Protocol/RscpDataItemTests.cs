using E3dc.Protocol;
using FluentAssertions;

namespace E3dc.Tests.Protocol;

public class RscpDataItemTests
{
    [Fact]
    public void Roundtrip_Int32_value()
    {
        var item = new RscpDataItem(0x01800001, RscpDataType.Int32, BitConverter.GetBytes(42));
        var bytes = item.ToBytes();
        var parsed = RscpDataItem.Parse(bytes, out var consumed);

        parsed.Tag.Should().Be(0x01800001);
        parsed.DataType.Should().Be(RscpDataType.Int32);
        BitConverter.ToInt32(parsed.Value.Span).Should().Be(42);
        consumed.Should().Be(7 + 4);
    }

    [Fact]
    public void Roundtrip_CString_value()
    {
        var value = System.Text.Encoding.UTF8.GetBytes("hello");
        var item = new RscpDataItem(0x00000002, RscpDataType.CString, value);
        var bytes = item.ToBytes();
        var parsed = RscpDataItem.Parse(bytes, out var consumed);

        parsed.Tag.Should().Be(0x00000002);
        System.Text.Encoding.UTF8.GetString(parsed.Value.Span).Should().Be("hello");
        consumed.Should().Be(7 + 5);
    }

    [Fact]
    public void Roundtrip_Container_with_nested_items()
    {
        var child1 = new RscpDataItem(0x00000002, RscpDataType.CString,
            System.Text.Encoding.UTF8.GetBytes("user"));
        var child2 = new RscpDataItem(0x00000003, RscpDataType.CString,
            System.Text.Encoding.UTF8.GetBytes("pass"));
        var container = RscpDataItem.CreateContainer(0x00000001, [child1, child2]);

        var bytes = container.ToBytes();
        var parsed = RscpDataItem.Parse(bytes, out _);

        parsed.DataType.Should().Be(RscpDataType.Container);
        var children = parsed.ParseContainerChildren();
        children.Should().HaveCount(2);
        System.Text.Encoding.UTF8.GetString(children[0].Value.Span).Should().Be("user");
        System.Text.Encoding.UTF8.GetString(children[1].Value.Span).Should().Be("pass");
    }

    [Fact]
    public void Roundtrip_Timestamp_value()
    {
        var ts = new DateTimeOffset(2024, 6, 15, 12, 30, 45, TimeSpan.Zero);
        var item = RscpDataItem.FromTimestamp(0x0000000E, ts);
        var bytes = item.ToBytes();
        var parsed = RscpDataItem.Parse(bytes, out _);

        parsed.DataType.Should().Be(RscpDataType.Timestamp);
        var roundtripped = parsed.ToTimestamp();
        roundtripped.ToUnixTimeSeconds().Should().Be(ts.ToUnixTimeSeconds());
    }

    [Fact]
    public void Header_is_7_bytes()
    {
        var item = new RscpDataItem(0x01000001, RscpDataType.None, Array.Empty<byte>());
        item.ToBytes().Length.Should().Be(7);
    }

    [Fact]
    public void Parse_multiple_items_sequentially()
    {
        var item1 = new RscpDataItem(0x01800001, RscpDataType.Int32, BitConverter.GetBytes(100));
        var item2 = new RscpDataItem(0x01800002, RscpDataType.Int32, BitConverter.GetBytes(200));
        var combined = item1.ToBytes().Concat(item2.ToBytes()).ToArray();

        var first = RscpDataItem.Parse(combined, out var consumed1);
        var second = RscpDataItem.Parse(combined.AsSpan(consumed1), out _);

        BitConverter.ToInt32(first.Value.Span).Should().Be(100);
        BitConverter.ToInt32(second.Value.Span).Should().Be(200);
    }
}
