using System.Buffers.Binary;
using System.Text;
using E3dcConnector.Protocol;

namespace E3dcConnector.Messages;

public sealed class DeviceRequestBuilder
{
    private readonly List<RscpDataItem> _items = [];

    internal DeviceRequestBuilder() { }

    public DeviceRequestBuilder Read(IndexedTag tag)
    {
        _items.Add(new RscpDataItem((uint)tag.Tag, RscpDataType.None, Array.Empty<byte>()));
        return this;
    }

    public DeviceRequestBuilder Read(params IndexedTag[] tags)
    {
        foreach (var tag in tags) Read(tag);
        return this;
    }

    public DeviceRequestBuilder Write(IndexedTag tag, byte value)
    {
        _items.Add(new RscpDataItem((uint)tag.Tag, RscpDataType.UChar8, new byte[] { value }));
        return this;
    }

    public DeviceRequestBuilder Write(IndexedTag tag, int value)
    {
        _items.Add(new RscpDataItem((uint)tag.Tag, RscpDataType.Int32, BitConverter.GetBytes(value)));
        return this;
    }

    public DeviceRequestBuilder Write(IndexedTag tag, float value)
    {
        _items.Add(new RscpDataItem((uint)tag.Tag, RscpDataType.Float32, BitConverter.GetBytes(value)));
        return this;
    }

    public DeviceRequestBuilder Write(IndexedTag tag, string value)
    {
        _items.Add(new RscpDataItem((uint)tag.Tag, RscpDataType.CString, Encoding.UTF8.GetBytes(value)));
        return this;
    }

    internal IReadOnlyList<RscpDataItem> BuildItems() => _items;
}
