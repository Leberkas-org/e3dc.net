using E3dc.Protocol;
using E3dc.Tags;

namespace E3dc.Messages;

/// <summary>Top-level tag — can be used directly in RscpRequest.Read() / Write().</summary>
public readonly record struct TagDescriptor(RscpTag Tag, RscpDataType DataType = RscpDataType.None);

/// <summary>Sub-tag of an indexed device — can ONLY be used inside FromDevice(). Compile error at top level.</summary>
public readonly record struct IndexedTag(RscpTag Tag);

/// <summary>Identifies an indexed device (PVI, BAT, PM, WB). Used with FromDevice().</summary>
public readonly record struct DeviceDescriptor(RscpTag ContainerTag, RscpTag IndexTag);
