using System.Buffers.Binary;

namespace E3dcConnector.Protocol;

public readonly struct RscpDataItem
{
    public const int HeaderSize = 7;

    public uint Tag { get; }
    public RscpDataType DataType { get; }
    public ReadOnlyMemory<byte> Value { get; }

    public RscpDataItem(uint tag, RscpDataType dataType, ReadOnlyMemory<byte> value)
    {
        Tag = tag;
        DataType = dataType;
        Value = value;
    }

    public static RscpDataItem CreateContainer(uint tag, IReadOnlyList<RscpDataItem> children)
    {
        var totalSize = children.Sum(c => HeaderSize + c.Value.Length);
        var buffer = new byte[totalSize];
        var offset = 0;
        foreach (var child in children)
        {
            var childBytes = child.ToBytes();
            childBytes.CopyTo(buffer.AsSpan(offset));
            offset += childBytes.Length;
        }
        return new RscpDataItem(tag, RscpDataType.Container, buffer);
    }

    public static RscpDataItem FromTimestamp(uint tag, DateTimeOffset ts)
    {
        var buffer = new byte[12];
        BinaryPrimitives.WriteInt64LittleEndian(buffer, ts.ToUnixTimeSeconds());
        var fractionalTicks = ts.UtcTicks % TimeSpan.TicksPerSecond;
        var nanos = (int)(fractionalTicks * 100);
        BinaryPrimitives.WriteInt32LittleEndian(buffer.AsSpan(8), nanos);
        return new RscpDataItem(tag, RscpDataType.Timestamp, buffer);
    }

    public DateTimeOffset ToTimestamp()
    {
        var seconds = BinaryPrimitives.ReadInt64LittleEndian(Value.Span);
        var nanos = BinaryPrimitives.ReadInt32LittleEndian(Value.Span[8..]);
        return DateTimeOffset.FromUnixTimeSeconds(seconds).AddTicks(nanos / 100);
    }

    public List<RscpDataItem> ParseContainerChildren()
    {
        var items = new List<RscpDataItem>();
        var span = Value.Span;
        var offset = 0;
        while (offset < span.Length)
        {
            var child = Parse(span[offset..], out var consumed);
            items.Add(child);
            offset += consumed;
        }
        return items;
    }

    public byte[] ToBytes()
    {
        var buffer = new byte[HeaderSize + Value.Length];
        BinaryPrimitives.WriteUInt32LittleEndian(buffer, Tag);
        buffer[4] = (byte)DataType;
        BinaryPrimitives.WriteUInt16LittleEndian(buffer.AsSpan(5), (ushort)Value.Length);
        Value.Span.CopyTo(buffer.AsSpan(HeaderSize));
        return buffer;
    }

    public static RscpDataItem Parse(ReadOnlySpan<byte> data, out int consumed)
    {
        var tag = BinaryPrimitives.ReadUInt32LittleEndian(data);
        var dataType = (RscpDataType)data[4];
        var length = BinaryPrimitives.ReadUInt16LittleEndian(data[5..]);
        var value = data.Slice(HeaderSize, length).ToArray();
        consumed = HeaderSize + length;
        return new RscpDataItem(tag, dataType, value);
    }
}
