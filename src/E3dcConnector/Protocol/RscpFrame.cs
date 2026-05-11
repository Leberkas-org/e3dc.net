using System.Buffers.Binary;
using System.IO.Hashing;

namespace E3dcConnector.Protocol;

public sealed class RscpFrame
{
    public const int HeaderSize = 18;
    public const ushort Magic = 0xE3DC;
    private const ushort VersionAndCrc = 0x11;

    public DateTimeOffset Timestamp { get; }
    public IReadOnlyList<RscpDataItem> Items { get; }

    public RscpFrame(DateTimeOffset timestamp, IReadOnlyList<RscpDataItem> items)
    {
        Timestamp = timestamp;
        Items = items;
    }

    public byte[] ToBytes()
    {
        var dataBytes = new List<byte>();
        foreach (var item in Items)
            dataBytes.AddRange(item.ToBytes());

        var dataLength = (ushort)dataBytes.Count;
        var buffer = new byte[HeaderSize + dataLength + 4];

        BinaryPrimitives.WriteUInt16LittleEndian(buffer, Magic);
        BinaryPrimitives.WriteUInt16LittleEndian(buffer.AsSpan(2), VersionAndCrc);
        BinaryPrimitives.WriteInt64LittleEndian(buffer.AsSpan(4), Timestamp.ToUnixTimeSeconds());
        var fractionalTicks = Timestamp.UtcTicks % TimeSpan.TicksPerSecond;
        var nanos = (int)(fractionalTicks * 100);
        BinaryPrimitives.WriteInt32LittleEndian(buffer.AsSpan(12), nanos);
        BinaryPrimitives.WriteUInt16LittleEndian(buffer.AsSpan(16), dataLength);
        dataBytes.CopyTo(buffer, HeaderSize);

        var crc = Crc32.HashToUInt32(buffer.AsSpan(0, HeaderSize + dataLength));
        BinaryPrimitives.WriteUInt32LittleEndian(buffer.AsSpan(HeaderSize + dataLength), crc);

        return buffer;
    }

    public static RscpFrame Parse(ReadOnlySpan<byte> data)
    {
        var magic = BinaryPrimitives.ReadUInt16LittleEndian(data);
        if (magic != Magic)
            throw new InvalidDataException($"Invalid RSCP magic: 0x{magic:X4}");

        var ctrl = BinaryPrimitives.ReadUInt16LittleEndian(data[2..]);
        var hasCrc = ((ctrl >> 4) & 1) == 1;

        var seconds = BinaryPrimitives.ReadInt64LittleEndian(data[4..]);
        var nanos = BinaryPrimitives.ReadInt32LittleEndian(data[12..]);
        var timestamp = DateTimeOffset.FromUnixTimeSeconds(seconds).AddTicks(nanos / 100);

        var dataLength = BinaryPrimitives.ReadUInt16LittleEndian(data[16..]);

        if (hasCrc)
        {
            var expectedCrc = BinaryPrimitives.ReadUInt32LittleEndian(data[(HeaderSize + dataLength)..]);
            var actualCrc = Crc32.HashToUInt32(data[..(HeaderSize + dataLength)]);
            if (expectedCrc != actualCrc)
                throw new InvalidDataException($"CRC mismatch: expected 0x{expectedCrc:X8}, got 0x{actualCrc:X8}");
        }

        var items = new List<RscpDataItem>();
        var offset = HeaderSize;
        var end = HeaderSize + dataLength;
        while (offset < end)
        {
            var item = RscpDataItem.Parse(data[offset..], out var consumed);
            items.Add(item);
            offset += consumed;
        }

        return new RscpFrame(timestamp, items);
    }
}
