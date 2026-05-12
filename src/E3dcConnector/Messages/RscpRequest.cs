using System.Buffers.Binary;
using System.Text;
using E3dcConnector.Protocol;
using E3dcConnector.Tags;

namespace E3dcConnector.Messages;

public sealed class RscpRequest : IRscpCommand
{
    private readonly List<RscpDataItem> _items = [];

    public RscpRequestOptions Options { get; init; } = RscpRequestOptions.Default;
    RscpRequestOptions IRscpCommand.Options => Options;

    private RscpRequest() { }

    public static RscpRequest Create() => new();

    public RscpRequest Read(TagDescriptor tag)
    {
        _items.Add(new RscpDataItem((uint)tag.Tag, RscpDataType.None, Array.Empty<byte>()));
        return this;
    }

    public RscpRequest Read(params TagDescriptor[] tags)
    {
        foreach (var tag in tags) Read(tag);
        return this;
    }

    public RscpRequest Write(TagDescriptor tag, byte value)
    {
        _items.Add(new RscpDataItem((uint)tag.Tag, RscpDataType.UChar8, new byte[] { value }));
        return this;
    }

    public RscpRequest Write(TagDescriptor tag, bool value)
    {
        _items.Add(new RscpDataItem((uint)tag.Tag, RscpDataType.Bool, new byte[] { (byte)(value ? 1 : 0) }));
        return this;
    }

    public RscpRequest Write(TagDescriptor tag, short value)
    {
        var buf = new byte[2];
        BinaryPrimitives.WriteInt16LittleEndian(buf, value);
        _items.Add(new RscpDataItem((uint)tag.Tag, RscpDataType.Int16, buf));
        return this;
    }

    public RscpRequest Write(TagDescriptor tag, int value)
    {
        _items.Add(new RscpDataItem((uint)tag.Tag, RscpDataType.Int32, BitConverter.GetBytes(value)));
        return this;
    }

    public RscpRequest Write(TagDescriptor tag, uint value)
    {
        _items.Add(new RscpDataItem((uint)tag.Tag, RscpDataType.UInt32, BitConverter.GetBytes(value)));
        return this;
    }

    public RscpRequest Write(TagDescriptor tag, float value)
    {
        _items.Add(new RscpDataItem((uint)tag.Tag, RscpDataType.Float32, BitConverter.GetBytes(value)));
        return this;
    }

    public RscpRequest Write(TagDescriptor tag, double value)
    {
        var buf = new byte[8];
        BinaryPrimitives.WriteDoubleLittleEndian(buf, value);
        _items.Add(new RscpDataItem((uint)tag.Tag, RscpDataType.Double64, buf));
        return this;
    }

    public RscpRequest Write(TagDescriptor tag, string value)
    {
        _items.Add(new RscpDataItem((uint)tag.Tag, RscpDataType.CString, Encoding.UTF8.GetBytes(value)));
        return this;
    }

    public RscpRequest Container(TagDescriptor tag, Action<RscpRequest> configure)
    {
        var inner = new RscpRequest();
        configure(inner);
        _items.Add(RscpDataItem.CreateContainer((uint)tag.Tag, inner.BuildItems()));
        return this;
    }

    public RscpRequest FromDevice(DeviceDescriptor device, int index, Action<DeviceRequestBuilder> configure)
    {
        var inner = new DeviceRequestBuilder();
        configure(inner);

        var indexItem = new RscpDataItem((uint)device.IndexTag, RscpDataType.UInt16, BitConverter.GetBytes((ushort)index));
        var children = new List<RscpDataItem> { indexItem };
        children.AddRange(inner.BuildItems());

        _items.Add(RscpDataItem.CreateContainer((uint)device.ContainerTag, children));
        return this;
    }

    internal IReadOnlyList<RscpDataItem> BuildItems() => _items;
}
