using E3dc.Protocol;
using FluentAssertions;

namespace E3dc.Tests.Protocol;

public class RscpDataTypeTests
{
    [Theory]
    [InlineData(RscpDataType.None, 0x00)]
    [InlineData(RscpDataType.Bool, 0x01)]
    [InlineData(RscpDataType.Int32, 0x06)]
    [InlineData(RscpDataType.Float32, 0x0A)]
    [InlineData(RscpDataType.CString, 0x0D)]
    [InlineData(RscpDataType.Container, 0x0E)]
    [InlineData(RscpDataType.Timestamp, 0x0F)]
    [InlineData(RscpDataType.Error, 0xFF)]
    public void DataType_has_correct_byte_value(RscpDataType type, byte expected)
    {
        ((byte)type).Should().Be(expected);
    }

    [Theory]
    [InlineData(RscpDataType.Bool, 1)]
    [InlineData(RscpDataType.Char8, 1)]
    [InlineData(RscpDataType.UChar8, 1)]
    [InlineData(RscpDataType.Int16, 2)]
    [InlineData(RscpDataType.UInt16, 2)]
    [InlineData(RscpDataType.Int32, 4)]
    [InlineData(RscpDataType.UInt32, 4)]
    [InlineData(RscpDataType.Int64, 8)]
    [InlineData(RscpDataType.UInt64, 8)]
    [InlineData(RscpDataType.Float32, 4)]
    [InlineData(RscpDataType.Double64, 8)]
    [InlineData(RscpDataType.Timestamp, 12)]
    public void FixedSize_returns_correct_size(RscpDataType type, int expected)
    {
        RscpDataTypes.FixedSize(type).Should().Be(expected);
    }

    [Theory]
    [InlineData(RscpDataType.CString)]
    [InlineData(RscpDataType.Container)]
    [InlineData(RscpDataType.ByteArray)]
    [InlineData(RscpDataType.None)]
    public void FixedSize_returns_null_for_variable_types(RscpDataType type)
    {
        RscpDataTypes.FixedSize(type).Should().BeNull();
    }
}
