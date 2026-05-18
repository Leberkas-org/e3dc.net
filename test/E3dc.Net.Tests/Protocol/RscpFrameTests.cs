using System.Buffers.Binary;
using E3dc.Protocol;
using FluentAssertions;

namespace E3dc.Tests.Protocol;

public class RscpFrameTests
{
    [Fact]
    public void Roundtrip_frame_with_single_item()
    {
        var item = new RscpDataItem(0x01000001, RscpDataType.None, Array.Empty<byte>());
        var frame = new RscpFrame(DateTimeOffset.UtcNow, [item]);
        var bytes = frame.ToBytes();
        var parsed = RscpFrame.Parse(bytes);

        parsed.Items.Should().HaveCount(1);
        parsed.Items[0].Tag.Should().Be(0x01000001);
    }

    [Fact]
    public void Frame_starts_with_magic_bytes_E3_DC()
    {
        var frame = new RscpFrame(DateTimeOffset.UtcNow, []);
        var bytes = frame.ToBytes();
        bytes[0].Should().Be(0xE3, "first wire byte");
        bytes[1].Should().Be(0xDC, "second wire byte");
        BinaryPrimitives.ReadUInt16LittleEndian(bytes).Should().Be(0xDCE3);
    }

    [Fact]
    public void Frame_control_has_version_1_at_bits_8_11_and_crc_at_bit_12()
    {
        var frame = new RscpFrame(DateTimeOffset.UtcNow, []);
        var bytes = frame.ToBytes();
        var ctrl = BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(2));
        ((ctrl >> 8) & 0x0F).Should().Be(0x01, "version 1 at bits 8-11");
        ((ctrl >> 12) & 0x01).Should().Be(1, "CRC bit at bit 12");
    }

    [Fact]
    public void Frame_has_correct_header_size()
    {
        RscpFrame.HeaderSize.Should().Be(18);
    }

    [Fact]
    public void Frame_preserves_timestamp()
    {
        var ts = new DateTimeOffset(2024, 6, 15, 12, 0, 0, TimeSpan.Zero);
        var frame = new RscpFrame(ts, []);
        var bytes = frame.ToBytes();
        var parsed = RscpFrame.Parse(bytes);
        parsed.Timestamp.ToUnixTimeSeconds().Should().Be(ts.ToUnixTimeSeconds());
    }

    [Fact]
    public void Frame_with_CRC_validates_on_parse()
    {
        var item = new RscpDataItem(0x01000001, RscpDataType.Int32, BitConverter.GetBytes(42));
        var frame = new RscpFrame(DateTimeOffset.UtcNow, [item]);
        var bytes = frame.ToBytes();
        bytes[20] ^= 0xFF;
        var act = () => RscpFrame.Parse(bytes);
        act.Should().Throw<InvalidDataException>().WithMessage("*CRC*");
    }

    [Fact]
    public void Roundtrip_frame_with_multiple_items()
    {
        var items = new[]
        {
            new RscpDataItem(0x01000001, RscpDataType.None, Array.Empty<byte>()),
            new RscpDataItem(0x01000002, RscpDataType.None, Array.Empty<byte>()),
            new RscpDataItem(0x01000003, RscpDataType.Int32, BitConverter.GetBytes(99)),
        };
        var frame = new RscpFrame(DateTimeOffset.UtcNow, items);
        var bytes = frame.ToBytes();
        var parsed = RscpFrame.Parse(bytes);

        parsed.Items.Should().HaveCount(3);
        BitConverter.ToInt32(parsed.Items[2].Value.Span).Should().Be(99);
    }
}
