# E3DC RSCP Akka.Streams Connector — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build a standalone Akka.Streams client for E3DC S10 Pro via the RSCP protocol, with VitePress documentation and LikeC4 architecture diagrams.

**Architecture:** Mirrors the Hendl framework patterns (at C:\diit\wobble): IProtocolBuilder → Flow, channel materialization, correlation-based RscpClient, RestartFlow reconnection. Protocol layer handles Rijndael-256 CBC encryption, binary frame encode/decode, and tag-based request/response. Typed layer provides record snapshots per namespace.

**Tech Stack:** net10.0, Akka.Streams 1.5.*, BouncyCastle (Rijndael-256), xUnit + FluentAssertions, VitePress, LikeC4

---

## Task 1: Solution Scaffold

**Files:**
- Create: `E3dcConnector.slnx`
- Create: `src/E3dcConnector/E3dcConnector.csproj`
- Create: `src/E3dcConnector.Typed/E3dcConnector.Typed.csproj`
- Create: `test/E3dcConnector.Tests/E3dcConnector.Tests.csproj`
- Create: `samples/E3dcConnector.Sample/E3dcConnector.Sample.csproj`
- Create: `samples/E3dcConnector.FlowSample/E3dcConnector.FlowSample.csproj`
- Create: `samples/E3dcConnector.ActorSample/E3dcConnector.ActorSample.csproj`
- Create: `.gitignore`

- [ ] **Step 1: Create the solution and core project**

```xml
<!-- E3dcConnector.slnx -->
<Solution>
  <Folder Name="/src/">
    <Project Path="src/E3dcConnector/E3dcConnector.csproj" />
    <Project Path="src/E3dcConnector.Typed/E3dcConnector.Typed.csproj" />
  </Folder>
  <Folder Name="/test/">
    <Project Path="test/E3dcConnector.Tests/E3dcConnector.Tests.csproj" />
  </Folder>
  <Folder Name="/samples/">
    <Project Path="samples/E3dcConnector.Sample/E3dcConnector.Sample.csproj" />
    <Project Path="samples/E3dcConnector.FlowSample/E3dcConnector.FlowSample.csproj" />
    <Project Path="samples/E3dcConnector.ActorSample/E3dcConnector.ActorSample.csproj" />
  </Folder>
</Solution>
```

```xml
<!-- src/E3dcConnector/E3dcConnector.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <LangVersion>latest</LangVersion>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <RootNamespace>E3dcConnector</RootNamespace>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Akka.Streams" Version="1.5.*" />
    <PackageReference Include="BouncyCastle.Cryptography" Version="2.*" />
  </ItemGroup>
</Project>
```

```xml
<!-- src/E3dcConnector.Typed/E3dcConnector.Typed.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <LangVersion>latest</LangVersion>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <RootNamespace>E3dcConnector.Typed</RootNamespace>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\E3dcConnector\E3dcConnector.csproj" />
  </ItemGroup>
</Project>
```

```xml
<!-- test/E3dcConnector.Tests/E3dcConnector.Tests.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <LangVersion>latest</LangVersion>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <IsPackable>false</IsPackable>
    <IsTestProject>true</IsTestProject>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.*" />
    <PackageReference Include="xunit" Version="2.*" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.*" />
    <PackageReference Include="FluentAssertions" Version="8.*" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\..\src\E3dcConnector\E3dcConnector.csproj" />
    <ProjectReference Include="..\..\src\E3dcConnector.Typed\E3dcConnector.Typed.csproj" />
  </ItemGroup>
</Project>
```

```xml
<!-- samples/E3dcConnector.Sample/E3dcConnector.Sample.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <LangVersion>latest</LangVersion>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\..\src\E3dcConnector\E3dcConnector.csproj" />
    <ProjectReference Include="..\..\src\E3dcConnector.Typed\E3dcConnector.Typed.csproj" />
  </ItemGroup>
</Project>
```

```xml
<!-- samples/E3dcConnector.FlowSample/E3dcConnector.FlowSample.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <LangVersion>latest</LangVersion>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\..\src\E3dcConnector\E3dcConnector.csproj" />
    <ProjectReference Include="..\..\src\E3dcConnector.Typed\E3dcConnector.Typed.csproj" />
  </ItemGroup>
</Project>
```

```xml
<!-- samples/E3dcConnector.ActorSample/E3dcConnector.ActorSample.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <LangVersion>latest</LangVersion>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Akka" Version="1.5.*" />
    <ProjectReference Include="..\..\src\E3dcConnector\E3dcConnector.csproj" />
    <ProjectReference Include="..\..\src\E3dcConnector.Typed\E3dcConnector.Typed.csproj" />
  </ItemGroup>
</Project>
```

- [ ] **Step 2: Create .gitignore**

Standard .NET gitignore: `bin/`, `obj/`, `*.user`, `.vs/`, `node_modules/`.

- [ ] **Step 3: Initialize git and verify build**

Run: `git init && dotnet restore && dotnet build`
Expected: Build succeeds with 0 errors (projects have no source files yet, but structure is valid).

- [ ] **Step 4: Commit**

```
git add -A
git commit -m "chore: scaffold solution with core, typed, test, and sample projects"
```

---

## Task 2: RscpDataType Enum

**Files:**
- Create: `src/E3dcConnector/Protocol/RscpDataType.cs`
- Create: `test/E3dcConnector.Tests/Protocol/RscpDataTypeTests.cs`

- [ ] **Step 1: Write the failing test**

```csharp
// test/E3dcConnector.Tests/Protocol/RscpDataTypeTests.cs
using E3dcConnector.Protocol;
using FluentAssertions;

namespace E3dcConnector.Tests.Protocol;

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
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test test/E3dcConnector.Tests --filter "RscpDataTypeTests" -v quiet`
Expected: FAIL — types don't exist yet.

- [ ] **Step 3: Implement RscpDataType**

```csharp
// src/E3dcConnector/Protocol/RscpDataType.cs
namespace E3dcConnector.Protocol;

public enum RscpDataType : byte
{
    None      = 0x00,
    Bool      = 0x01,
    Char8     = 0x02,
    UChar8    = 0x03,
    Int16     = 0x04,
    UInt16    = 0x05,
    Int32     = 0x06,
    UInt32    = 0x07,
    Int64     = 0x08,
    UInt64    = 0x09,
    Float32   = 0x0A,
    Double64  = 0x0B,
    Bitfield  = 0x0C,
    CString   = 0x0D,
    Container = 0x0E,
    Timestamp = 0x0F,
    ByteArray = 0x10,
    Error     = 0xFF,
}

public static class RscpDataTypes
{
    public static int? FixedSize(RscpDataType type) => type switch
    {
        RscpDataType.Bool     => 1,
        RscpDataType.Char8    => 1,
        RscpDataType.UChar8   => 1,
        RscpDataType.Int16    => 2,
        RscpDataType.UInt16   => 2,
        RscpDataType.Int32    => 4,
        RscpDataType.UInt32   => 4,
        RscpDataType.Int64    => 8,
        RscpDataType.UInt64   => 8,
        RscpDataType.Float32  => 4,
        RscpDataType.Double64 => 8,
        RscpDataType.Bitfield => 1,
        RscpDataType.Timestamp => 12,
        _ => null,
    };
}
```

- [ ] **Step 4: Run tests**

Run: `dotnet test test/E3dcConnector.Tests --filter "RscpDataTypeTests" -v quiet`
Expected: All pass.

- [ ] **Step 5: Commit**

```
git add src/E3dcConnector/Protocol/RscpDataType.cs test/E3dcConnector.Tests/Protocol/RscpDataTypeTests.cs
git commit -m "feat: add RscpDataType enum with fixed size lookup"
```

---

## Task 3: RscpDataItem (TLV Encoding/Decoding)

**Files:**
- Create: `src/E3dcConnector/Protocol/RscpDataItem.cs`
- Create: `test/E3dcConnector.Tests/Protocol/RscpDataItemTests.cs`

- [ ] **Step 1: Write the failing tests**

```csharp
// test/E3dcConnector.Tests/Protocol/RscpDataItemTests.cs
using System.Buffers.Binary;
using E3dcConnector.Protocol;
using FluentAssertions;

namespace E3dcConnector.Tests.Protocol;

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
        BitConverter.ToInt32(parsed.Value).Should().Be(42);
        consumed.Should().Be(7 + 4); // header(7) + int32(4)
    }

    [Fact]
    public void Roundtrip_CString_value()
    {
        var value = System.Text.Encoding.UTF8.GetBytes("hello");
        var item = new RscpDataItem(0x00000002, RscpDataType.CString, value);

        var bytes = item.ToBytes();
        var parsed = RscpDataItem.Parse(bytes, out var consumed);

        parsed.Tag.Should().Be(0x00000002);
        parsed.DataType.Should().Be(RscpDataType.CString);
        System.Text.Encoding.UTF8.GetString(parsed.Value).Should().Be("hello");
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
        System.Text.Encoding.UTF8.GetString(children[0].Value).Should().Be("user");
        System.Text.Encoding.UTF8.GetString(children[1].Value).Should().Be("pass");
    }

    [Fact]
    public void Roundtrip_Timestamp_value()
    {
        var ts = new DateTimeOffset(2024, 6, 15, 12, 30, 45, 123, TimeSpan.Zero)
            .AddTicks(4567); // sub-ms precision
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
        // Tag(4) + DataType(1) + Length(2) = 7
        var item = new RscpDataItem(0x01000001, RscpDataType.None, []);
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

        BitConverter.ToInt32(first.Value).Should().Be(100);
        BitConverter.ToInt32(second.Value).Should().Be(200);
    }
}
```

- [ ] **Step 2: Run tests to verify they fail**

Run: `dotnet test test/E3dcConnector.Tests --filter "RscpDataItemTests" -v quiet`
Expected: FAIL.

- [ ] **Step 3: Implement RscpDataItem**

```csharp
// src/E3dcConnector/Protocol/RscpDataItem.cs
using System.Buffers.Binary;
using System.Text;

namespace E3dcConnector.Protocol;

public readonly struct RscpDataItem
{
    public const int HeaderSize = 7; // Tag(4) + DataType(1) + Length(2)

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
        BinaryPrimitives.WriteInt32LittleEndian(buffer.AsSpan(8),
            (int)(ts.ToUnixTimeMilliseconds() % 1000 * 1_000_000
                + ts.Ticks % TimeSpan.TicksPerMillisecond * 100));
        return new RscpDataItem(tag, RscpDataType.Timestamp, buffer);
    }

    public DateTimeOffset ToTimestamp()
    {
        var seconds = BinaryPrimitives.ReadInt64LittleEndian(Value.Span);
        var nanos = BinaryPrimitives.ReadInt32LittleEndian(Value.Span[8..]);
        return DateTimeOffset.FromUnixTimeSeconds(seconds)
            .AddTicks(nanos / 100);
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
```

- [ ] **Step 4: Run tests**

Run: `dotnet test test/E3dcConnector.Tests --filter "RscpDataItemTests" -v quiet`
Expected: All pass.

- [ ] **Step 5: Commit**

```
git add src/E3dcConnector/Protocol/RscpDataItem.cs test/E3dcConnector.Tests/Protocol/RscpDataItemTests.cs
git commit -m "feat: add RscpDataItem TLV encoding with container and timestamp support"
```

---

## Task 4: RscpFrame (Frame Encode/Decode + CRC32)

**Files:**
- Create: `src/E3dcConnector/Protocol/RscpFrame.cs`
- Create: `test/E3dcConnector.Tests/Protocol/RscpFrameTests.cs`

- [ ] **Step 1: Write the failing tests**

```csharp
// test/E3dcConnector.Tests/Protocol/RscpFrameTests.cs
using System.Buffers.Binary;
using E3dcConnector.Protocol;
using FluentAssertions;

namespace E3dcConnector.Tests.Protocol;

public class RscpFrameTests
{
    [Fact]
    public void Roundtrip_frame_with_single_item()
    {
        var item = new RscpDataItem(0x01000001, RscpDataType.None, []);
        var frame = new RscpFrame(DateTimeOffset.UtcNow, [item]);

        var bytes = frame.ToBytes();
        var parsed = RscpFrame.Parse(bytes);

        parsed.Items.Should().HaveCount(1);
        parsed.Items[0].Tag.Should().Be(0x01000001);
    }

    [Fact]
    public void Frame_starts_with_magic_0xE3DC()
    {
        var frame = new RscpFrame(DateTimeOffset.UtcNow, []);
        var bytes = frame.ToBytes();

        BinaryPrimitives.ReadUInt16LittleEndian(bytes).Should().Be(0xE3DC);
    }

    [Fact]
    public void Frame_control_has_version_1_and_crc_bit()
    {
        var frame = new RscpFrame(DateTimeOffset.UtcNow, []);
        var bytes = frame.ToBytes();
        var ctrl = BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(2));

        (ctrl & 0x0F).Should().Be(0x01, "version should be 1");
        ((ctrl >> 4) & 0x01).Should().Be(1, "CRC bit should be set");
    }

    [Fact]
    public void Frame_has_correct_header_size()
    {
        // Magic(2) + Ctrl(2) + TimestampSec(8) + TimestampNano(4) + DataLen(2) = 18
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

        // corrupt one data byte
        bytes[20] ^= 0xFF;

        var act = () => RscpFrame.Parse(bytes);
        act.Should().Throw<InvalidDataException>().WithMessage("*CRC*");
    }

    [Fact]
    public void Roundtrip_frame_with_multiple_items()
    {
        var items = new[]
        {
            new RscpDataItem(0x01000001, RscpDataType.None, []),
            new RscpDataItem(0x01000002, RscpDataType.None, []),
            new RscpDataItem(0x01000003, RscpDataType.Int32, BitConverter.GetBytes(99)),
        };
        var frame = new RscpFrame(DateTimeOffset.UtcNow, items);

        var bytes = frame.ToBytes();
        var parsed = RscpFrame.Parse(bytes);

        parsed.Items.Should().HaveCount(3);
        BitConverter.ToInt32(parsed.Items[2].Value.Span).Should().Be(99);
    }
}
```

- [ ] **Step 2: Run tests to verify they fail**

Run: `dotnet test test/E3dcConnector.Tests --filter "RscpFrameTests" -v quiet`
Expected: FAIL.

- [ ] **Step 3: Implement RscpFrame**

```csharp
// src/E3dcConnector/Protocol/RscpFrame.cs
using System.Buffers.Binary;
using System.IO.Hashing;

namespace E3dcConnector.Protocol;

public sealed class RscpFrame
{
    public const int HeaderSize = 18;
    public const ushort Magic = 0xE3DC;
    private const ushort VersionAndCrc = 0x11; // version 1 in low nibble, CRC bit at bit 4

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
        var buffer = new byte[HeaderSize + dataLength + 4]; // +4 for CRC

        BinaryPrimitives.WriteUInt16LittleEndian(buffer, Magic);
        BinaryPrimitives.WriteUInt16LittleEndian(buffer.AsSpan(2), VersionAndCrc);
        BinaryPrimitives.WriteInt64LittleEndian(buffer.AsSpan(4), Timestamp.ToUnixTimeSeconds());
        BinaryPrimitives.WriteInt32LittleEndian(buffer.AsSpan(12),
            (int)(Timestamp.ToUnixTimeMilliseconds() % 1000 * 1_000_000
                + Timestamp.Ticks % TimeSpan.TicksPerMillisecond * 100));
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
            var expectedCrc = BinaryPrimitives.ReadUInt32LittleEndian(
                data[(HeaderSize + dataLength)..]);
            var actualCrc = Crc32.HashToUInt32(data[..(HeaderSize + dataLength)]);
            if (expectedCrc != actualCrc)
                throw new InvalidDataException(
                    $"CRC mismatch: expected 0x{expectedCrc:X8}, got 0x{actualCrc:X8}");
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
```

- [ ] **Step 4: Run tests**

Run: `dotnet test test/E3dcConnector.Tests --filter "RscpFrameTests" -v quiet`
Expected: All pass.

- [ ] **Step 5: Commit**

```
git add src/E3dcConnector/Protocol/RscpFrame.cs test/E3dcConnector.Tests/Protocol/RscpFrameTests.cs
git commit -m "feat: add RscpFrame encode/decode with CRC32 validation"
```

---

## Task 5: RscpCrypt (Rijndael-256 CBC)

**Files:**
- Create: `src/E3dcConnector/Protocol/RscpCrypt.cs`
- Create: `test/E3dcConnector.Tests/Protocol/RscpCryptTests.cs`

- [ ] **Step 1: Write the failing tests**

```csharp
// test/E3dcConnector.Tests/Protocol/RscpCryptTests.cs
using E3dcConnector.Protocol;
using FluentAssertions;

namespace E3dcConnector.Tests.Protocol;

public class RscpCryptTests
{
    [Fact]
    public void Key_is_password_padded_with_0xFF_to_32_bytes()
    {
        var key = RscpCrypt.DeriveKey("abc");
        key.Length.Should().Be(32);
        key[0].Should().Be((byte)'a');
        key[1].Should().Be((byte)'b');
        key[2].Should().Be((byte)'c');
        key[3..].ToArray().Should().AllBeEquivalentTo(0xFF);
    }

    [Fact]
    public void Key_truncates_at_32_bytes()
    {
        var longPassword = new string('x', 40);
        var key = RscpCrypt.DeriveKey(longPassword);
        key.Length.Should().Be(32);
    }

    [Fact]
    public void Encrypt_then_decrypt_roundtrips()
    {
        var crypt = new RscpCrypt("testpassword");
        var plaintext = new byte[64]; // 2 blocks of 32
        for (int i = 0; i < plaintext.Length; i++) plaintext[i] = (byte)(i & 0xFF);

        var encrypted = crypt.Encrypt(plaintext);
        encrypted.Should().NotEqual(plaintext);
        encrypted.Length.Should().Be(64); // already aligned

        var decrypted = crypt.Decrypt(encrypted);
        decrypted.Should().Equal(plaintext);
    }

    [Fact]
    public void Encrypt_pads_to_32_byte_boundary()
    {
        var crypt = new RscpCrypt("test");
        var plaintext = new byte[10]; // not aligned
        var encrypted = crypt.Encrypt(plaintext);
        (encrypted.Length % 32).Should().Be(0);
    }

    [Fact]
    public void Separate_encrypt_decrypt_instances_with_same_key_roundtrip()
    {
        var encryptor = new RscpCrypt("shared");
        var decryptor = new RscpCrypt("shared");

        var plaintext = new byte[64];
        new Random(42).NextBytes(plaintext);

        var encrypted = encryptor.Encrypt(plaintext);
        var decrypted = decryptor.Decrypt(encrypted);

        decrypted.Should().Equal(plaintext);
    }

    [Fact]
    public void IV_chains_across_multiple_encryptions()
    {
        var crypt1 = new RscpCrypt("test");
        var crypt2 = new RscpCrypt("test");

        var block1 = new byte[32];
        var block2 = new byte[32];
        block1[0] = 0xAA;
        block2[0] = 0xBB;

        // crypt1 encrypts both blocks sequentially
        var enc1 = crypt1.Encrypt(block1);
        var enc2 = crypt1.Encrypt(block2);

        // crypt2 must also encrypt sequentially to get same results
        var enc1b = crypt2.Encrypt(block1);
        var enc2b = crypt2.Encrypt(block2);

        enc1.Should().Equal(enc1b);
        enc2.Should().Equal(enc2b);
    }
}
```

- [ ] **Step 2: Run tests to verify they fail**

Run: `dotnet test test/E3dcConnector.Tests --filter "RscpCryptTests" -v quiet`
Expected: FAIL.

- [ ] **Step 3: Implement RscpCrypt**

```csharp
// src/E3dcConnector/Protocol/RscpCrypt.cs
using System.Text;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;

namespace E3dcConnector.Protocol;

public sealed class RscpCrypt
{
    private const int BlockSize = 32;
    private readonly byte[] _key;
    private byte[] _encryptIv;
    private byte[] _decryptIv;

    public RscpCrypt(string password)
    {
        _key = DeriveKey(password);
        _encryptIv = CreateInitialIv();
        _decryptIv = CreateInitialIv();
    }

    public static byte[] DeriveKey(string password)
    {
        var key = new byte[BlockSize];
        Array.Fill(key, (byte)0xFF);
        var passwordBytes = Encoding.UTF8.GetBytes(password);
        var copyLen = Math.Min(passwordBytes.Length, BlockSize);
        Array.Copy(passwordBytes, key, copyLen);
        return key;
    }

    private static byte[] CreateInitialIv()
    {
        var iv = new byte[BlockSize];
        Array.Fill(iv, (byte)0xFF);
        return iv;
    }

    public byte[] Encrypt(ReadOnlySpan<byte> plaintext)
    {
        var padded = PadToBlockSize(plaintext);
        var cipher = CreateCipher(forEncryption: true, _encryptIv);
        var output = new byte[padded.Length];

        for (var offset = 0; offset < padded.Length; offset += BlockSize)
            cipher.ProcessBlock(padded, offset, output, offset);

        Array.Copy(output, output.Length - BlockSize, _encryptIv, 0, BlockSize);
        return output;
    }

    public byte[] Decrypt(ReadOnlySpan<byte> ciphertext)
    {
        var input = ciphertext.ToArray();
        var cipher = CreateCipher(forEncryption: false, _decryptIv);
        var output = new byte[input.Length];

        for (var offset = 0; offset < input.Length; offset += BlockSize)
            cipher.ProcessBlock(input, offset, output, offset);

        Array.Copy(input, input.Length - BlockSize, _decryptIv, 0, BlockSize);
        return output;
    }

    public void ResetIv()
    {
        _encryptIv = CreateInitialIv();
        _decryptIv = CreateInitialIv();
    }

    private CbcBlockCipher CreateCipher(bool forEncryption, byte[] iv)
    {
        var engine = new RijndaelEngine(256);
        var cipher = new CbcBlockCipher(engine);
        cipher.Init(forEncryption, new ParametersWithIV(new KeyParameter(_key), iv));
        return cipher;
    }

    private static byte[] PadToBlockSize(ReadOnlySpan<byte> data)
    {
        var paddedLength = (data.Length + BlockSize - 1) / BlockSize * BlockSize;
        if (paddedLength == 0) paddedLength = BlockSize;
        var padded = new byte[paddedLength]; // zero-padded
        data.CopyTo(padded);
        return padded;
    }
}
```

- [ ] **Step 4: Run tests**

Run: `dotnet test test/E3dcConnector.Tests --filter "RscpCryptTests" -v quiet`
Expected: All pass.

- [ ] **Step 5: Commit**

```
git add src/E3dcConnector/Protocol/RscpCrypt.cs test/E3dcConnector.Tests/Protocol/RscpCryptTests.cs
git commit -m "feat: add RscpCrypt with Rijndael-256 CBC and chaining IV"
```

---

## Task 6: RscpTag Enum + Namespace Helpers

**Files:**
- Create: `src/E3dcConnector/Tags/RscpTag.cs`
- Create: `src/E3dcConnector/Tags/RscpNamespace.cs`
- Create: `test/E3dcConnector.Tests/Tags/RscpTagTests.cs`

- [ ] **Step 1: Write the failing tests**

```csharp
// test/E3dcConnector.Tests/Tags/RscpTagTests.cs
using E3dcConnector.Tags;
using FluentAssertions;

namespace E3dcConnector.Tests.Tags;

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
```

- [ ] **Step 2: Run tests to verify they fail**

Run: `dotnet test test/E3dcConnector.Tests --filter "RscpTagTests" -v quiet`
Expected: FAIL.

- [ ] **Step 3: Implement RscpTag and RscpNamespace**

```csharp
// src/E3dcConnector/Tags/RscpNamespace.cs
namespace E3dcConnector.Tags;

public enum RscpTagNamespace : byte
{
    Rscp = 0x00,
    Ems  = 0x01,
    Pvi  = 0x02,
    Bat  = 0x03,
    Dcdc = 0x04,
    Pm   = 0x05,
    Db   = 0x06,
    Ha   = 0x09,
    Info = 0x0A,
    Ep   = 0x0B,
    Sys  = 0x0C,
    Um   = 0x0D,
    Wb   = 0x0E,
    Se   = 0x11,
}

public static class RscpTagExtensions
{
    public static RscpTagNamespace GetNamespace(this RscpTag tag)
        => (RscpTagNamespace)(((uint)tag >> 24) & 0xFF);

    public static bool IsRequest(this RscpTag tag)
        => (((uint)tag >> 20) & 0x08) == 0;

    public static bool IsResponse(this RscpTag tag)
        => (((uint)tag >> 20) & 0x08) != 0;
}
```

```csharp
// src/E3dcConnector/Tags/RscpTag.cs
namespace E3dcConnector.Tags;

public enum RscpTag : uint
{
    // RSCP (0x00)
    RSCP_REQ_AUTHENTICATION       = 0x00000001,
    RSCP_AUTHENTICATION_USER      = 0x00000002,
    RSCP_AUTHENTICATION_PASSWORD  = 0x00000003,
    RSCP_AUTHENTICATION           = 0x00800001,
    RSCP_REQ_USER_LEVEL           = 0x00000004,
    RSCP_USER_LEVEL               = 0x00800004,
    RSCP_GENERAL_ERROR            = 0x00FFFFFF,

    // EMS (0x01)
    EMS_REQ_POWER_PV              = 0x01000001,
    EMS_POWER_PV                  = 0x01800001,
    EMS_REQ_POWER_BAT             = 0x01000002,
    EMS_POWER_BAT                 = 0x01800002,
    EMS_REQ_POWER_HOME            = 0x01000003,
    EMS_POWER_HOME                = 0x01800003,
    EMS_REQ_POWER_GRID            = 0x01000004,
    EMS_POWER_GRID                = 0x01800004,
    EMS_REQ_POWER_ADD             = 0x01000005,
    EMS_POWER_ADD                 = 0x01800005,
    EMS_REQ_AUTARKY               = 0x01000006,
    EMS_AUTARKY                   = 0x01800006,
    EMS_REQ_SELF_CONSUMPTION      = 0x01000007,
    EMS_SELF_CONSUMPTION          = 0x01800007,
    EMS_REQ_BAT_SOC               = 0x01000008,
    EMS_BAT_SOC                   = 0x01800008,
    EMS_REQ_COUPLING_MODE         = 0x01000009,
    EMS_COUPLING_MODE             = 0x01800009,
    EMS_REQ_MODE                  = 0x01000011,
    EMS_MODE                      = 0x01800011,
    EMS_REQ_SET_POWER             = 0x01000030,
    EMS_REQ_SET_POWER_MODE        = 0x01000031,
    EMS_REQ_SET_POWER_VALUE       = 0x01000032,
    EMS_SET_POWER                 = 0x01800030,
    EMS_REQ_BAT_CHARGE_LIMIT      = 0x01000042,
    EMS_BAT_CHARGE_LIMIT          = 0x01800042,
    EMS_REQ_USER_CHARGE_LIMIT     = 0x01000044,
    EMS_USER_CHARGE_LIMIT         = 0x01800044,
    EMS_REQ_EMERGENCY_POWER_STATUS = 0x01000073,
    EMS_EMERGENCY_POWER_STATUS    = 0x01800073,
    EMS_REQ_SET_EMERGENCY_POWER   = 0x01000074,
    EMS_SET_EMERGENCY_POWER       = 0x01800074,
    EMS_REQ_MAX_CHARGE_POWER      = 0x01000101,
    EMS_MAX_CHARGE_POWER          = 0x01800101,
    EMS_REQ_MAX_DISCHARGE_POWER   = 0x01000102,
    EMS_MAX_DISCHARGE_POWER       = 0x01800102,

    // PVI (0x02)
    PVI_REQ_DATA                  = 0x02040000,
    PVI_DATA                      = 0x02840000,
    PVI_INDEX                     = 0x02040001,
    PVI_REQ_ON_GRID               = 0x02000001,
    PVI_ON_GRID                   = 0x02800001,
    PVI_REQ_STATE                 = 0x02000002,
    PVI_STATE                     = 0x02800002,
    PVI_AC_POWER                  = 0x028AC001,
    PVI_AC_VOLTAGE                = 0x028AC002,
    PVI_AC_CURRENT                = 0x028AC003,
    PVI_AC_FREQUENCY              = 0x028AC00A,
    PVI_AC_ENERGY_ALL             = 0x028AC006,
    PVI_AC_ENERGY_DAY             = 0x028AC008,
    PVI_DC_POWER                  = 0x028DC001,
    PVI_DC_VOLTAGE                = 0x028DC002,
    PVI_DC_CURRENT                = 0x028DC003,
    PVI_GENERAL_ERROR             = 0x02FFFFFF,

    // BAT (0x03)
    BAT_REQ_DATA                  = 0x03040000,
    BAT_DATA                      = 0x03840000,
    BAT_INDEX                     = 0x03040001,
    BAT_RSOC                      = 0x03800001,
    BAT_MODULE_VOLTAGE            = 0x03800002,
    BAT_CURRENT                   = 0x03800003,
    BAT_MAX_BAT_VOLTAGE           = 0x03800004,
    BAT_MAX_CHARGE_CURRENT        = 0x03800005,
    BAT_EOD_VOLTAGE               = 0x03800006,
    BAT_MAX_DISCHARGE_CURRENT     = 0x03800007,
    BAT_CHARGE_CYCLES             = 0x03800008,
    BAT_STATUS_CODE               = 0x0380000A,
    BAT_ERROR_CODE                = 0x0380000B,
    BAT_DCB_COUNT                 = 0x0380000D,
    BAT_MAX_DCB_CELL_TEMPERATURE  = 0x03800016,
    BAT_MIN_DCB_CELL_TEMPERATURE  = 0x03800017,
    BAT_DCB_CELL_TEMPERATURE      = 0x03800019,
    BAT_DCB_CELL_VOLTAGE          = 0x0380001B,
    BAT_GENERAL_ERROR             = 0x03FFFFFF,

    // DCDC (0x04)
    DCDC_REQ_DATA                 = 0x04040000,
    DCDC_DATA                     = 0x04840000,
    DCDC_INDEX                    = 0x04040001,
    DCDC_REQ_I_BAT                = 0x04000001,
    DCDC_I_BAT                    = 0x04800001,
    DCDC_REQ_U_BAT                = 0x04000002,
    DCDC_U_BAT                    = 0x04800002,
    DCDC_REQ_P_BAT                = 0x04000003,
    DCDC_P_BAT                    = 0x04800003,
    DCDC_REQ_I_DCL                = 0x04000004,
    DCDC_I_DCL                    = 0x04800004,
    DCDC_REQ_U_DCL                = 0x04000005,
    DCDC_U_DCL                    = 0x04800005,
    DCDC_REQ_STATUS               = 0x04000010,
    DCDC_STATUS                   = 0x04800010,
    DCDC_STATE                    = 0x04800011,
    DCDC_GENERAL_ERROR            = 0x04FFFFFF,

    // PM (0x05)
    PM_REQ_DATA                   = 0x05040000,
    PM_DATA                       = 0x05840000,
    PM_INDEX                      = 0x05040001,
    PM_REQ_POWER_L1               = 0x05000001,
    PM_POWER_L1                   = 0x05800001,
    PM_REQ_POWER_L2               = 0x05000002,
    PM_POWER_L2                   = 0x05800002,
    PM_REQ_POWER_L3               = 0x05000003,
    PM_POWER_L3                   = 0x05800003,
    PM_REQ_VOLTAGE_L1             = 0x05000011,
    PM_VOLTAGE_L1                 = 0x05800011,
    PM_REQ_VOLTAGE_L2             = 0x05000012,
    PM_VOLTAGE_L2                 = 0x05800012,
    PM_REQ_VOLTAGE_L3             = 0x05000013,
    PM_VOLTAGE_L3                 = 0x05800013,
    PM_REQ_ENERGY_L1              = 0x05000006,
    PM_ENERGY_L1                  = 0x05800006,
    PM_REQ_ENERGY_L2              = 0x05000007,
    PM_ENERGY_L2                  = 0x05800007,
    PM_REQ_ENERGY_L3              = 0x05000008,
    PM_ENERGY_L3                  = 0x05800008,
    PM_REQ_DEVICE_ID              = 0x05000009,
    PM_DEVICE_ID                  = 0x05800009,
    PM_REQ_ERROR_CODE             = 0x0500000A,
    PM_ERROR_CODE                 = 0x0580000A,
    PM_REQ_TYPE                   = 0x05000014,
    PM_TYPE                       = 0x05800014,
    PM_GENERAL_ERROR              = 0x05FFFFFF,

    // DB (0x06)
    DB_REQ_HISTORY_DATA_DAY       = 0x06000100,
    DB_REQ_HISTORY_DATA_WEEK      = 0x06000200,
    DB_REQ_HISTORY_DATA_MONTH     = 0x06000300,
    DB_REQ_HISTORY_DATA_YEAR      = 0x06000400,
    DB_HISTORY_DATA_DAY           = 0x06800100,
    DB_HISTORY_DATA_WEEK          = 0x06800200,
    DB_HISTORY_DATA_MONTH         = 0x06800300,
    DB_HISTORY_DATA_YEAR          = 0x06800400,
    DB_SUM_CONTAINER              = 0x06800010,
    DB_VALUE_CONTAINER            = 0x06800020,
    DB_GRAPH_INDEX                = 0x06800001,
    DB_BAT_POWER_IN               = 0x06800002,
    DB_BAT_POWER_OUT              = 0x06800003,
    DB_GRID_POWER_IN              = 0x06800005,
    DB_GRID_POWER_OUT             = 0x06800006,
    DB_CONSUMPTION                = 0x06800007,

    // HA (0x09)
    HA_REQ_DATAPOINT_LIST         = 0x09000001,
    HA_DATAPOINT_LIST             = 0x09800001,
    HA_DATAPOINT                  = 0x09800002,
    HA_DATAPOINT_INDEX            = 0x09800003,
    HA_DATAPOINT_STATE            = 0x09800011,
    HA_REQ_ACTUATOR_STATES        = 0x09000010,
    HA_ACTUATOR_STATES            = 0x09800010,

    // INFO (0x0A)
    INFO_REQ_SERIAL_NUMBER        = 0x0A000001,
    INFO_SERIAL_NUMBER            = 0x0A800001,
    INFO_REQ_PRODUCTION_DATE      = 0x0A000002,
    INFO_PRODUCTION_DATE          = 0x0A800002,
    INFO_REQ_SW_RELEASE           = 0x0A000019,
    INFO_SW_RELEASE               = 0x0A800019,
    INFO_REQ_IP_ADDRESS           = 0x0A000008,
    INFO_IP_ADDRESS               = 0x0A800008,
    INFO_REQ_SUBNET_MASK          = 0x0A000009,
    INFO_SUBNET_MASK              = 0x0A800009,
    INFO_REQ_GATEWAY              = 0x0A00000B,
    INFO_GATEWAY                  = 0x0A80000B,
    INFO_REQ_DNS                  = 0x0A00000C,
    INFO_DNS                      = 0x0A80000C,
    INFO_REQ_TIME                 = 0x0A00000E,
    INFO_TIME                     = 0x0A80000E,
    INFO_REQ_TIME_ZONE            = 0x0A000010,
    INFO_TIME_ZONE                = 0x0A800010,

    // EP (0x0B)
    EP_REQ_IS_READY_FOR_SWITCH    = 0x0B000003,
    EP_IS_READY_FOR_SWITCH        = 0x0B800003,
    EP_REQ_IS_GRID_CONNECTED      = 0x0B000004,
    EP_IS_GRID_CONNECTED          = 0x0B800004,
    EP_REQ_IS_ISLAND_GRID         = 0x0B000005,
    EP_IS_ISLAND_GRID             = 0x0B800005,
    EP_GENERAL_ERROR              = 0x0BFFFFFF,

    // SYS (0x0C)
    SYS_REQ_SYSTEM_REBOOT         = 0x0C000001,
    SYS_SYSTEM_REBOOT             = 0x0C800001,
    SYS_REQ_RESTART_APPLICATION    = 0x0C000003,
    SYS_RESTART_APPLICATION        = 0x0C800003,
    SYS_GENERAL_ERROR              = 0x0CFFFFFF,

    // UM (0x0D)
    UM_REQ_UPDATE_STATUS           = 0x0D000001,
    UM_UPDATE_STATUS               = 0x0D800001,
    UM_REQ_CHECK_FOR_UPDATES       = 0x0D000003,
    UM_CHECK_FOR_UPDATES           = 0x0D800003,
    UM_GENERAL_ERROR               = 0x0DFFFFFF,

    // WB (0x0E)
    WB_REQ_DATA                    = 0x0E040000,
    WB_DATA                        = 0x0E840000,
    WB_INDEX                       = 0x0E040001,
    WB_REQ_ENERGY_ALL              = 0x0E000001,
    WB_ENERGY_ALL                  = 0x0E800001,
    WB_REQ_ENERGY_SOLAR            = 0x0E000002,
    WB_ENERGY_SOLAR                = 0x0E800002,
    WB_REQ_STATUS                  = 0x0E000004,
    WB_STATUS                      = 0x0E800004,
    WB_REQ_ERROR_CODE              = 0x0E000005,
    WB_ERROR_CODE                  = 0x0E800005,
    WB_REQ_MODE                    = 0x0E000006,
    WB_MODE                        = 0x0E800006,
    WB_REQ_PM_POWER_L1             = 0x0E00000C,
    WB_PM_POWER_L1                 = 0x0E80000C,
    WB_REQ_PM_POWER_L2             = 0x0E00000D,
    WB_PM_POWER_L2                 = 0x0E80000D,
    WB_REQ_PM_POWER_L3             = 0x0E00000E,
    WB_PM_POWER_L3                 = 0x0E80000E,
    WB_GENERAL_ERROR               = 0x0EFFFFFF,
}
```

- [ ] **Step 4: Run tests**

Run: `dotnet test test/E3dcConnector.Tests --filter "RscpTagTests" -v quiet`
Expected: All pass.

- [ ] **Step 5: Commit**

```
git add src/E3dcConnector/Tags/ test/E3dcConnector.Tests/Tags/
git commit -m "feat: add RscpTag enum with all namespaces and extension helpers"
```

---

## Task 7: Message Interfaces + Commands + Responses

**Files:**
- Create: `src/E3dcConnector/Messages/IRscpCommand.cs`
- Create: `src/E3dcConnector/Messages/IRscpMessage.cs`
- Create: `src/E3dcConnector/Messages/RscpRequestOptions.cs`
- Create: `src/E3dcConnector/Messages/Commands/ReadTagsCommand.cs`
- Create: `src/E3dcConnector/Messages/Commands/WriteTagCommand.cs`
- Create: `src/E3dcConnector/Messages/Responses/RscpDataResponse.cs`
- Create: `src/E3dcConnector/Messages/Responses/RscpErrorResponse.cs`

- [ ] **Step 1: Create message interfaces**

```csharp
// src/E3dcConnector/Messages/RscpRequestOptions.cs
namespace E3dcConnector.Messages;

public sealed record RscpRequestOptions
{
    public string CorrelationId { get; init; } = Guid.NewGuid().ToString();
    public TimeSpan? Timeout { get; init; }
    public static RscpRequestOptions Default => new();
}
```

```csharp
// src/E3dcConnector/Messages/IRscpCommand.cs
namespace E3dcConnector.Messages;

public interface IRscpCommand
{
    RscpRequestOptions Options { get; }
}
```

```csharp
// src/E3dcConnector/Messages/IRscpMessage.cs
namespace E3dcConnector.Messages;

public interface IRscpMessage;

public interface IRscpResponse : IRscpMessage
{
    string CorrelationId { get; }
}
```

- [ ] **Step 2: Create command records**

```csharp
// src/E3dcConnector/Messages/Commands/ReadTagsCommand.cs
using E3dcConnector.Tags;

namespace E3dcConnector.Messages.Commands;

public sealed record ReadTagsCommand(
    RscpTag[] Tags,
    RscpRequestOptions? Options = null) : IRscpCommand
{
    RscpRequestOptions IRscpCommand.Options => Options ?? RscpRequestOptions.Default;
}
```

```csharp
// src/E3dcConnector/Messages/Commands/WriteTagCommand.cs
using E3dcConnector.Protocol;
using E3dcConnector.Tags;

namespace E3dcConnector.Messages.Commands;

public sealed record WriteTagCommand(
    RscpTag Tag,
    RscpDataType DataType,
    byte[] Value,
    RscpRequestOptions? Options = null) : IRscpCommand
{
    RscpRequestOptions IRscpCommand.Options => Options ?? RscpRequestOptions.Default;
}
```

- [ ] **Step 3: Create response records**

```csharp
// src/E3dcConnector/Messages/Responses/RscpDataResponse.cs
using E3dcConnector.Protocol;
using E3dcConnector.Tags;

namespace E3dcConnector.Messages.Responses;

public sealed record RscpDataResponse(
    IReadOnlyList<RscpDataItem> Items,
    string CorrelationId) : IRscpResponse;
```

```csharp
// src/E3dcConnector/Messages/Responses/RscpErrorResponse.cs
namespace E3dcConnector.Messages.Responses;

public sealed record RscpErrorResponse(
    string Message,
    Exception? Exception = null,
    string? CorrelationId = null) : IRscpResponse
{
    string IRscpResponse.CorrelationId => CorrelationId ?? "";
}
```

- [ ] **Step 4: Verify build**

Run: `dotnet build src/E3dcConnector`
Expected: Build succeeds.

- [ ] **Step 5: Commit**

```
git add src/E3dcConnector/Messages/
git commit -m "feat: add message interfaces, commands, and response types"
```

---

## Task 8: RscpConnection (TCP + Auth + Crypto)

**Files:**
- Create: `src/E3dcConnector/Reactive/Internal/RscpConnection.cs`
- Create: `test/E3dcConnector.Tests/Reactive/RscpConnectionTests.cs`

- [ ] **Step 1: Write the failing tests**

```csharp
// test/E3dcConnector.Tests/Reactive/RscpConnectionTests.cs
using E3dcConnector.Protocol;
using E3dcConnector.Reactive.Internal;
using E3dcConnector.Tags;
using FluentAssertions;

namespace E3dcConnector.Tests.Reactive;

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
```

- [ ] **Step 2: Run tests to verify they fail**

Run: `dotnet test test/E3dcConnector.Tests --filter "RscpConnectionTests" -v quiet`
Expected: FAIL.

- [ ] **Step 3: Implement RscpConnection**

```csharp
// src/E3dcConnector/Reactive/Internal/RscpConnection.cs
using System.Buffers.Binary;
using System.Net.Sockets;
using System.Text;
using E3dcConnector.Protocol;
using E3dcConnector.Tags;

namespace E3dcConnector.Reactive.Internal;

internal sealed class RscpConnection : IDisposable
{
    private readonly string _host;
    private readonly int _port;
    private readonly string _user;
    private readonly string _password;
    private readonly RscpCrypt _crypt;

    private TcpClient? _tcp;
    private NetworkStream? _stream;
    private bool _authenticated;

    public RscpConnection(string host, int port, string user, string password, string encryptionKey)
    {
        _host = host;
        _port = port;
        _user = user;
        _password = password;
        _crypt = new RscpCrypt(encryptionKey);
    }

    public async Task ConnectAsync(CancellationToken ct = default)
    {
        _tcp = new TcpClient();
        await _tcp.ConnectAsync(_host, _port, ct);
        _stream = _tcp.GetStream();
    }

    public async Task<int> AuthenticateAsync(CancellationToken ct = default)
    {
        var authFrame = BuildAuthFrame(_user, _password);
        await SendFrameAsync(authFrame, ct);
        var response = await ReceiveFrameAsync(ct);
        var level = ParseAuthLevel(response);
        if (level == 0)
            throw new InvalidOperationException("RSCP authentication failed: AUTH_LEVEL_NO_AUTH");
        _authenticated = true;
        return level;
    }

    public bool IsAuthenticated => _authenticated;

    public async Task SendFrameAsync(RscpFrame frame, CancellationToken ct = default)
    {
        var plaintext = frame.ToBytes();
        var encrypted = _crypt.Encrypt(plaintext);
        await _stream!.WriteAsync(encrypted, ct);
    }

    public async Task<RscpFrame> ReceiveFrameAsync(CancellationToken ct = default)
    {
        const int blockSize = 32;
        var buffer = new List<byte>();

        // Read first block to get header
        var firstBlock = new byte[blockSize];
        await ReadExactAsync(_stream!, firstBlock, ct);
        var decryptedFirst = _crypt.Decrypt(firstBlock);
        buffer.AddRange(decryptedFirst);

        // Parse data length from header to know total frame size
        var magic = BinaryPrimitives.ReadUInt16LittleEndian(decryptedFirst);
        if (magic != RscpFrame.Magic)
            throw new InvalidDataException($"Invalid RSCP magic: 0x{magic:X4}");

        var ctrl = BinaryPrimitives.ReadUInt16LittleEndian(decryptedFirst.AsSpan(2));
        var hasCrc = ((ctrl >> 4) & 1) == 1;
        var dataLength = BinaryPrimitives.ReadUInt16LittleEndian(decryptedFirst.AsSpan(16));
        var totalFrameSize = RscpFrame.HeaderSize + dataLength + (hasCrc ? 4 : 0);
        var totalEncryptedSize = (totalFrameSize + blockSize - 1) / blockSize * blockSize;

        // Read remaining blocks
        var remaining = totalEncryptedSize - blockSize;
        if (remaining > 0)
        {
            var rest = new byte[remaining];
            await ReadExactAsync(_stream!, rest, ct);
            var decryptedRest = _crypt.Decrypt(rest);
            buffer.AddRange(decryptedRest);
        }

        return RscpFrame.Parse(buffer.ToArray().AsSpan(0, totalFrameSize));
    }

    internal static RscpFrame BuildAuthFrame(string user, string password)
    {
        var userItem = new RscpDataItem(
            (uint)RscpTag.RSCP_AUTHENTICATION_USER,
            RscpDataType.CString,
            Encoding.UTF8.GetBytes(user));
        var passItem = new RscpDataItem(
            (uint)RscpTag.RSCP_AUTHENTICATION_PASSWORD,
            RscpDataType.CString,
            Encoding.UTF8.GetBytes(password));
        var container = RscpDataItem.CreateContainer(
            (uint)RscpTag.RSCP_REQ_AUTHENTICATION,
            [userItem, passItem]);

        return new RscpFrame(DateTimeOffset.UtcNow, [container]);
    }

    internal static int ParseAuthLevel(RscpFrame frame)
    {
        foreach (var item in frame.Items)
        {
            if (item.Tag == (uint)RscpTag.RSCP_AUTHENTICATION)
            {
                return item.DataType switch
                {
                    RscpDataType.UChar8 => item.Value.Span[0],
                    RscpDataType.Int32 => BinaryPrimitives.ReadInt32LittleEndian(item.Value.Span),
                    _ => 0,
                };
            }
        }
        return 0;
    }

    private static async Task ReadExactAsync(NetworkStream stream, byte[] buffer, CancellationToken ct)
    {
        var offset = 0;
        while (offset < buffer.Length)
        {
            var read = await stream.ReadAsync(buffer.AsMemory(offset), ct);
            if (read == 0) throw new IOException("Connection closed while reading RSCP frame");
            offset += read;
        }
    }

    public void Dispose()
    {
        _authenticated = false;
        _stream?.Dispose();
        _tcp?.Dispose();
    }
}
```

- [ ] **Step 4: Run tests**

Run: `dotnet test test/E3dcConnector.Tests --filter "RscpConnectionTests" -v quiet`
Expected: All pass (we test the static helpers, not the TCP connection).

- [ ] **Step 5: Commit**

```
git add src/E3dcConnector/Reactive/Internal/RscpConnection.cs test/E3dcConnector.Tests/Reactive/RscpConnectionTests.cs
git commit -m "feat: add RscpConnection with TCP, Rijndael encryption, and auth handshake"
```

---

## Task 9: RscpFlow (Akka.Streams Pipeline)

**Files:**
- Create: `src/E3dcConnector/Reactive/RscpFlowSettings.cs`
- Create: `src/E3dcConnector/Reactive/RscpFlow.cs`

- [ ] **Step 1: Create flow settings**

```csharp
// src/E3dcConnector/Reactive/RscpFlowSettings.cs
namespace E3dcConnector.Reactive;

public sealed record RscpFlowSettings
{
    public TimeSpan PollingInterval { get; init; } = TimeSpan.FromSeconds(2);
    public TimeSpan MinReconnectBackoff { get; init; } = TimeSpan.FromSeconds(1);
    public TimeSpan MaxReconnectBackoff { get; init; } = TimeSpan.FromSeconds(30);
    public double ReconnectRandomFactor { get; init; } = 0.2;
    public int MaxReconnectAttempts { get; init; } = -1;
    public TimeSpan SendTimeout { get; init; } = TimeSpan.FromSeconds(5);
}
```

- [ ] **Step 2: Create the flow**

```csharp
// src/E3dcConnector/Reactive/RscpFlow.cs
using Akka;
using Akka.Streams;
using Akka.Streams.Dsl;
using E3dcConnector.Messages;
using E3dcConnector.Messages.Commands;
using E3dcConnector.Messages.Responses;
using E3dcConnector.Protocol;
using E3dcConnector.Reactive.Internal;
using E3dcConnector.Tags;

namespace E3dcConnector.Reactive;

public static class RscpFlow
{
    public static Flow<IRscpCommand, IRscpMessage, NotUsed> Create(
        Func<RscpConnection> connectionFactory,
        RscpTag[]? pollingTags = null,
        RscpFlowSettings? settings = null)
    {
        settings ??= new RscpFlowSettings();
        var capturedSettings = settings;
        var capturedPollingTags = pollingTags;

        return RestartFlow.WithBackoff(
            () => CreateInnerFlow(connectionFactory, capturedPollingTags, capturedSettings),
            RestartSettings.Create(
                capturedSettings.MinReconnectBackoff,
                capturedSettings.MaxReconnectBackoff,
                capturedSettings.ReconnectRandomFactor)
            .WithMaxRestarts(capturedSettings.MaxReconnectAttempts, capturedSettings.MinReconnectBackoff));
    }

    private static Flow<IRscpCommand, IRscpMessage, NotUsed> CreateInnerFlow(
        Func<RscpConnection> connectionFactory,
        RscpTag[]? pollingTags,
        RscpFlowSettings settings)
    {
        var connectionReady = Task.Run(async () =>
        {
            var conn = connectionFactory();
            await conn.ConnectAsync();
            await conn.AuthenticateAsync();
            return conn;
        });

        var commandFlow = Flow.Create<IRscpCommand>()
            .SelectAsync(1, async cmd =>
            {
                var conn = await connectionReady;
                return await ProcessCommand(cmd, conn);
            });

        if (pollingTags is { Length: > 0 })
        {
            var pollSource = Source.Tick(
                    TimeSpan.Zero,
                    settings.PollingInterval,
                    new ReadTagsCommand(pollingTags) as IRscpCommand);

            return Flow.FromGraph(GraphDsl.Create(b =>
            {
                var poll = b.Add(pollSource);
                var commands = b.Add(commandFlow);
                var merge = b.Add(new MergePreferred<IRscpCommand>(1));

                b.From(poll.Outlet).To(merge.Preferred);
                b.From(merge.Out).To(commands.Inlet);

                return new FlowShape<IRscpCommand, IRscpMessage>(merge.In(0), commands.Outlet);
            }));
        }

        return commandFlow;
    }

    private static async Task<IRscpMessage> ProcessCommand(IRscpCommand cmd, RscpConnection conn)
    {
        try
        {
            var items = cmd switch
            {
                ReadTagsCommand read => read.Tags
                    .Select(t => new RscpDataItem((uint)t, RscpDataType.None, []))
                    .ToList(),
                WriteTagCommand write => [new RscpDataItem((uint)write.Tag, write.DataType, write.Value)],
                _ => throw new NotSupportedException($"Unknown command: {cmd.GetType().Name}"),
            };

            var requestFrame = new RscpFrame(DateTimeOffset.UtcNow, items);
            await conn.SendFrameAsync(requestFrame);
            var responseFrame = await conn.ReceiveFrameAsync();

            return new RscpDataResponse(responseFrame.Items, cmd.Options.CorrelationId);
        }
        catch (IOException ex)
        {
            throw; // propagate to RestartFlow
        }
        catch (InvalidDataException ex) when (ex.Message.Contains("magic") || ex.Message.Contains("CRC"))
        {
            throw; // propagate to RestartFlow
        }
        catch (Exception ex)
        {
            return new RscpErrorResponse(ex.Message, ex, cmd.Options.CorrelationId);
        }
    }
}
```

- [ ] **Step 3: Verify build**

Run: `dotnet build src/E3dcConnector`
Expected: Build succeeds.

- [ ] **Step 4: Commit**

```
git add src/E3dcConnector/Reactive/
git commit -m "feat: add RscpFlow with RestartFlow, polling, and command processing"
```

---

## Task 10: RscpClient + Materialization

**Files:**
- Create: `src/E3dcConnector/Client/RscpClient.cs`
- Create: `src/E3dcConnector/Client/RscpClientExtensions.cs`
- Create: `src/E3dcConnector/Client/RscpClientBuilder.cs`

- [ ] **Step 1: Create RscpClientExtensions (materialization)**

```csharp
// src/E3dcConnector/Client/RscpClientExtensions.cs
using System.Threading.Channels;
using Akka;
using Akka.Streams;
using Akka.Streams.Dsl;
using E3dcConnector.Messages;

namespace E3dcConnector.Client;

public static class RscpClientExtensions
{
    public static (ChannelWriter<IRscpCommand> Commands, ChannelReader<IRscpMessage> Responses)
        Materialize(
            this Flow<IRscpCommand, IRscpMessage, NotUsed> flow,
            IMaterializer materializer)
    {
        var cmdChannel = Channel.CreateBounded<IRscpCommand>(256);
        var rspChannel = Channel.CreateUnbounded<IRscpMessage>();

        ChannelSource.FromReader(cmdChannel.Reader)
            .Via(flow)
            .To(ChannelSink.FromWriter(rspChannel.Writer, isOwner: true))
            .Run(materializer);

        return (cmdChannel.Writer, rspChannel.Reader);
    }
}
```

- [ ] **Step 2: Create RscpClient**

```csharp
// src/E3dcConnector/Client/RscpClient.cs
using System.Collections.Concurrent;
using System.Threading.Channels;
using Akka.Actor;
using Akka.Streams;
using E3dcConnector.Messages;
using E3dcConnector.Messages.Responses;
using E3dcConnector.Reactive;

namespace E3dcConnector.Client;

public sealed class RscpClient : IAsyncDisposable
{
    private readonly ChannelWriter<IRscpCommand> _commands;
    private readonly ConcurrentDictionary<Type, List<Delegate>> _handlers = new();
    private readonly ConcurrentDictionary<string, TaskCompletionSource<IRscpMessage>> _pending = new();
    private readonly ActorSystem? _ownedActorSystem;
    private readonly CancellationTokenSource _cts = new();

    internal RscpClient(
        Func<Akka.Streams.Dsl.Flow<IRscpCommand, IRscpMessage, Akka.NotUsed>> flowFactory,
        ActorSystem? actorSystem = null)
    {
        _ownedActorSystem = actorSystem is null ? ActorSystem.Create("rscp-client") : null;
        var materializer = (actorSystem ?? _ownedActorSystem!).Materializer();
        var flow = flowFactory();
        (_commands, var messages) = flow.Materialize(materializer);
        _ = Task.Run(() => DispatchMessages(messages, _cts.Token));
    }

    public async Task<IRscpResponse> SendAsync(IRscpCommand command, CancellationToken ct = default)
    {
        var correlationId = command.Options.CorrelationId;
        var tcs = new TaskCompletionSource<IRscpMessage>(TaskCreationOptions.RunContinuationsAsynchronously);
        _pending[correlationId] = tcs;

        try
        {
            await _commands.WriteAsync(command, ct);
            using var reg = ct.Register(() => tcs.TrySetCanceled());
            var message = await tcs.Task;
            return message as IRscpResponse
                ?? new RscpErrorResponse($"Unexpected response type: {message.GetType().Name}", null, correlationId);
        }
        finally
        {
            _pending.TryRemove(correlationId, out _);
        }
    }

    public IDisposable Subscribe<T>(Action<T> handler) where T : IRscpMessage
    {
        var list = _handlers.GetOrAdd(typeof(T), _ => new List<Delegate>());
        lock (list) { list.Add(handler); }
        return new Unsubscriber(() => { lock (list) { list.Remove(handler); } });
    }

    public void WriteCommand(IRscpCommand command)
    {
        _commands.TryWrite(command);
    }

    private async Task DispatchMessages(ChannelReader<IRscpMessage> reader, CancellationToken ct)
    {
        await foreach (var message in reader.ReadAllAsync(ct))
        {
            if (message is IRscpResponse correlated
                && _pending.TryRemove(correlated.CorrelationId, out var tcs))
            {
                tcs.TrySetResult(message);
                continue;
            }

            var messageType = message.GetType();
            foreach (var kvp in _handlers)
            {
                if (!kvp.Key.IsAssignableFrom(messageType)) continue;

                List<Delegate> snapshot;
                lock (kvp.Value) { snapshot = new List<Delegate>(kvp.Value); }

                foreach (var handler in snapshot)
                {
                    try
                    {
                        var result = handler.DynamicInvoke(message);
                        if (result is Task task) await task;
                    }
                    catch { }
                }
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        _cts.Cancel();
        _commands.TryComplete();
        if (_ownedActorSystem is not null)
            await _ownedActorSystem.Terminate();
        _cts.Dispose();
    }

    private sealed class Unsubscriber(Action onDispose) : IDisposable
    {
        public void Dispose() => onDispose();
    }
}
```

- [ ] **Step 3: Create RscpClientBuilder**

```csharp
// src/E3dcConnector/Client/RscpClientBuilder.cs
using Akka.Actor;
using E3dcConnector.Reactive;
using E3dcConnector.Reactive.Internal;
using E3dcConnector.Tags;

namespace E3dcConnector.Client;

public sealed class RscpClientBuilder
{
    private string _host = "localhost";
    private int _port = 5033;
    private string _user = "";
    private string _password = "";
    private string _encryptionKey = "";
    private RscpTag[]? _pollingTags;
    private RscpFlowSettings _settings = new();

    public RscpClientBuilder Connect(string host, int port = 5033)
    {
        _host = host;
        _port = port;
        return this;
    }

    public RscpClientBuilder WithCredentials(string user, string password)
    {
        _user = user;
        _password = password;
        return this;
    }

    public RscpClientBuilder WithEncryptionKey(string key)
    {
        _encryptionKey = key;
        return this;
    }

    public RscpClientBuilder WithPolling(TimeSpan interval, RscpTag[] tags)
    {
        _pollingTags = tags;
        _settings = _settings with { PollingInterval = interval };
        return this;
    }

    public RscpClientBuilder WithReconnect(TimeSpan min, TimeSpan max)
    {
        _settings = _settings with { MinReconnectBackoff = min, MaxReconnectBackoff = max };
        return this;
    }

    public RscpClient Build(ActorSystem? actorSystem = null)
    {
        var host = _host;
        var port = _port;
        var user = _user;
        var password = _password;
        var encKey = _encryptionKey;
        var pollingTags = _pollingTags;
        var settings = _settings;

        return new RscpClient(
            () => RscpFlow.Create(
                () => new RscpConnection(host, port, user, password, encKey),
                pollingTags,
                settings),
            actorSystem);
    }
}
```

- [ ] **Step 4: Verify build**

Run: `dotnet build src/E3dcConnector`
Expected: Build succeeds.

- [ ] **Step 5: Commit**

```
git add src/E3dcConnector/Client/
git commit -m "feat: add RscpClient with channel materialization and builder API"
```

---

## Task 11: Typed Snapshots + Commands

**Files:**
- Create: `src/E3dcConnector.Typed/Ems/EmsPowerSnapshot.cs`
- Create: `src/E3dcConnector.Typed/Ems/EmsCommands.cs`
- Create: `src/E3dcConnector.Typed/Bat/BatterySnapshot.cs`
- Create: `src/E3dcConnector.Typed/Pvi/InverterSnapshot.cs`
- Create: `src/E3dcConnector.Typed/Pm/PowerMeterSnapshot.cs`
- Create: `src/E3dcConnector.Typed/Wb/WallboxSnapshot.cs`
- Create: `src/E3dcConnector.Typed/Info/DeviceInfo.cs`
- Create: `src/E3dcConnector.Typed/Db/HistoryQuery.cs`
- Create: `src/E3dcConnector.Typed/RscpResponseExtensions.cs`
- Create: `test/E3dcConnector.Tests/Typed/RscpResponseExtensionsTests.cs`

- [ ] **Step 1: Create typed records**

```csharp
// src/E3dcConnector.Typed/Ems/EmsPowerSnapshot.cs
namespace E3dcConnector.Typed.Ems;

public sealed record EmsPowerSnapshot(
    int PvWatts,
    int BatteryWatts,
    int GridWatts,
    int HomeWatts,
    int AdditionalWatts,
    float Soc,
    float Autarky,
    float SelfConsumption);
```

```csharp
// src/E3dcConnector.Typed/Ems/EmsCommands.cs
using E3dcConnector.Messages;
using E3dcConnector.Protocol;
using E3dcConnector.Tags;

namespace E3dcConnector.Typed.Ems;

public enum EmsMode : byte { Normal = 0, Idle = 1, Discharge = 2, Charge = 3, GridCharge = 4 }

public sealed record SetPowerMode(EmsMode Mode, int ValueWatts, RscpRequestOptions? Options = null) : IRscpCommand
{
    RscpRequestOptions IRscpCommand.Options => Options ?? RscpRequestOptions.Default;
}

public sealed record SetChargeLimit(int LimitWatts, RscpRequestOptions? Options = null) : IRscpCommand
{
    RscpRequestOptions IRscpCommand.Options => Options ?? RscpRequestOptions.Default;
}

public sealed record SetEmergencyPower(bool Enable, RscpRequestOptions? Options = null) : IRscpCommand
{
    RscpRequestOptions IRscpCommand.Options => Options ?? RscpRequestOptions.Default;
}
```

```csharp
// src/E3dcConnector.Typed/Bat/BatterySnapshot.cs
namespace E3dcConnector.Typed.Bat;

public sealed record BatterySnapshot(
    float Rsoc,
    float Voltage,
    float Current,
    int ChargeCycles,
    int StatusCode,
    int ErrorCode);
```

```csharp
// src/E3dcConnector.Typed/Pvi/InverterSnapshot.cs
namespace E3dcConnector.Typed.Pvi;

public sealed record InverterSnapshot(
    float AcPowerL1, float AcPowerL2, float AcPowerL3,
    float AcVoltageL1, float AcVoltageL2, float AcVoltageL3,
    float DcPower, float DcVoltage, float DcCurrent,
    float Frequency);
```

```csharp
// src/E3dcConnector.Typed/Pm/PowerMeterSnapshot.cs
namespace E3dcConnector.Typed.Pm;

public sealed record PowerMeterSnapshot(
    float PowerL1, float PowerL2, float PowerL3,
    float VoltageL1, float VoltageL2, float VoltageL3,
    double EnergyL1, double EnergyL2, double EnergyL3);
```

```csharp
// src/E3dcConnector.Typed/Wb/WallboxSnapshot.cs
namespace E3dcConnector.Typed.Wb;

public sealed record WallboxSnapshot(
    double EnergyAll, double EnergySolar,
    int Status, int ErrorCode, int Mode,
    float PowerL1, float PowerL2, float PowerL3);
```

```csharp
// src/E3dcConnector.Typed/Info/DeviceInfo.cs
namespace E3dcConnector.Typed.Info;

public sealed record DeviceInfo(
    string SerialNumber, string ProductionDate, string SwRelease,
    string IpAddress, string SubnetMask, string Gateway);
```

```csharp
// src/E3dcConnector.Typed/Db/HistoryQuery.cs
using E3dcConnector.Messages;

namespace E3dcConnector.Typed.Db;

public enum HistoryPeriod { Day, Week, Month, Year }

public sealed record HistoryQuery(
    DateTimeOffset Start,
    HistoryPeriod Period,
    RscpRequestOptions? Options = null) : IRscpCommand
{
    RscpRequestOptions IRscpCommand.Options => Options ?? RscpRequestOptions.Default;
}
```

- [ ] **Step 2: Create response extension for parsing typed data**

```csharp
// src/E3dcConnector.Typed/RscpResponseExtensions.cs
using System.Buffers.Binary;
using System.Text;
using E3dcConnector.Messages.Responses;
using E3dcConnector.Protocol;
using E3dcConnector.Tags;
using E3dcConnector.Typed.Ems;
using E3dcConnector.Typed.Bat;
using E3dcConnector.Typed.Info;

namespace E3dcConnector.Typed;

public static class RscpResponseExtensions
{
    public static EmsPowerSnapshot? ToEmsPowerSnapshot(this RscpDataResponse response)
    {
        int pv = 0, bat = 0, grid = 0, home = 0, add = 0;
        float soc = 0, autarky = 0, selfCons = 0;
        var found = false;

        foreach (var item in response.Items)
        {
            switch ((RscpTag)item.Tag)
            {
                case RscpTag.EMS_POWER_PV:      pv = ReadInt32(item); found = true; break;
                case RscpTag.EMS_POWER_BAT:     bat = ReadInt32(item); found = true; break;
                case RscpTag.EMS_POWER_GRID:    grid = ReadInt32(item); found = true; break;
                case RscpTag.EMS_POWER_HOME:    home = ReadInt32(item); found = true; break;
                case RscpTag.EMS_POWER_ADD:     add = ReadInt32(item); found = true; break;
                case RscpTag.EMS_BAT_SOC:       soc = ReadFloat(item); found = true; break;
                case RscpTag.EMS_AUTARKY:       autarky = ReadFloat(item); found = true; break;
                case RscpTag.EMS_SELF_CONSUMPTION: selfCons = ReadFloat(item); found = true; break;
            }
        }

        return found ? new EmsPowerSnapshot(pv, bat, grid, home, add, soc, autarky, selfCons) : null;
    }

    public static BatterySnapshot? ToBatterySnapshot(this RscpDataResponse response)
    {
        float rsoc = 0, voltage = 0, current = 0;
        int cycles = 0, status = 0, error = 0;
        var found = false;

        foreach (var item in response.Items)
        {
            switch ((RscpTag)item.Tag)
            {
                case RscpTag.BAT_RSOC:           rsoc = ReadFloat(item); found = true; break;
                case RscpTag.BAT_MODULE_VOLTAGE:  voltage = ReadFloat(item); found = true; break;
                case RscpTag.BAT_CURRENT:         current = ReadFloat(item); found = true; break;
                case RscpTag.BAT_CHARGE_CYCLES:   cycles = ReadInt32(item); found = true; break;
                case RscpTag.BAT_STATUS_CODE:     status = ReadInt32(item); found = true; break;
                case RscpTag.BAT_ERROR_CODE:      error = ReadInt32(item); found = true; break;
            }
        }

        return found ? new BatterySnapshot(rsoc, voltage, current, cycles, status, error) : null;
    }

    public static DeviceInfo? ToDeviceInfo(this RscpDataResponse response)
    {
        string serial = "", prod = "", sw = "", ip = "", mask = "", gw = "";
        var found = false;

        foreach (var item in response.Items)
        {
            switch ((RscpTag)item.Tag)
            {
                case RscpTag.INFO_SERIAL_NUMBER:   serial = ReadString(item); found = true; break;
                case RscpTag.INFO_PRODUCTION_DATE: prod = ReadString(item); found = true; break;
                case RscpTag.INFO_SW_RELEASE:      sw = ReadString(item); found = true; break;
                case RscpTag.INFO_IP_ADDRESS:      ip = ReadString(item); found = true; break;
                case RscpTag.INFO_SUBNET_MASK:     mask = ReadString(item); found = true; break;
                case RscpTag.INFO_GATEWAY:         gw = ReadString(item); found = true; break;
            }
        }

        return found ? new DeviceInfo(serial, prod, sw, ip, mask, gw) : null;
    }

    private static int ReadInt32(RscpDataItem item)
        => BinaryPrimitives.ReadInt32LittleEndian(item.Value.Span);

    private static float ReadFloat(RscpDataItem item)
        => BinaryPrimitives.ReadSingleLittleEndian(item.Value.Span);

    private static string ReadString(RscpDataItem item)
        => Encoding.UTF8.GetString(item.Value.Span);
}
```

- [ ] **Step 3: Write tests for response parsing**

```csharp
// test/E3dcConnector.Tests/Typed/RscpResponseExtensionsTests.cs
using E3dcConnector.Messages.Responses;
using E3dcConnector.Protocol;
using E3dcConnector.Tags;
using E3dcConnector.Typed;
using FluentAssertions;

namespace E3dcConnector.Tests.Typed;

public class RscpResponseExtensionsTests
{
    [Fact]
    public void ToEmsPowerSnapshot_parses_ems_items()
    {
        var items = new[]
        {
            MakeInt32Item(RscpTag.EMS_POWER_PV, 3500),
            MakeInt32Item(RscpTag.EMS_POWER_BAT, -1200),
            MakeInt32Item(RscpTag.EMS_POWER_GRID, 0),
            MakeInt32Item(RscpTag.EMS_POWER_HOME, 2300),
            MakeInt32Item(RscpTag.EMS_POWER_ADD, 0),
            MakeFloatItem(RscpTag.EMS_BAT_SOC, 85.5f),
            MakeFloatItem(RscpTag.EMS_AUTARKY, 92.3f),
            MakeFloatItem(RscpTag.EMS_SELF_CONSUMPTION, 78.1f),
        };
        var response = new RscpDataResponse(items, "test");

        var snapshot = response.ToEmsPowerSnapshot();

        snapshot.Should().NotBeNull();
        snapshot!.PvWatts.Should().Be(3500);
        snapshot.BatteryWatts.Should().Be(-1200);
        snapshot.Soc.Should().BeApproximately(85.5f, 0.01f);
    }

    [Fact]
    public void ToEmsPowerSnapshot_returns_null_when_no_ems_tags()
    {
        var response = new RscpDataResponse([], "test");
        response.ToEmsPowerSnapshot().Should().BeNull();
    }

    [Fact]
    public void ToBatterySnapshot_parses_bat_items()
    {
        var items = new[]
        {
            MakeFloatItem(RscpTag.BAT_RSOC, 90.0f),
            MakeFloatItem(RscpTag.BAT_MODULE_VOLTAGE, 48.2f),
            MakeFloatItem(RscpTag.BAT_CURRENT, -5.1f),
            MakeInt32Item(RscpTag.BAT_CHARGE_CYCLES, 312),
            MakeInt32Item(RscpTag.BAT_STATUS_CODE, 0),
            MakeInt32Item(RscpTag.BAT_ERROR_CODE, 0),
        };
        var response = new RscpDataResponse(items, "test");

        var snapshot = response.ToBatterySnapshot();

        snapshot.Should().NotBeNull();
        snapshot!.Rsoc.Should().BeApproximately(90.0f, 0.01f);
        snapshot.ChargeCycles.Should().Be(312);
    }

    private static RscpDataItem MakeInt32Item(RscpTag tag, int value)
        => new((uint)tag, RscpDataType.Int32, BitConverter.GetBytes(value));

    private static RscpDataItem MakeFloatItem(RscpTag tag, float value)
        => new((uint)tag, RscpDataType.Float32, BitConverter.GetBytes(value));
}
```

- [ ] **Step 4: Run tests**

Run: `dotnet test test/E3dcConnector.Tests --filter "RscpResponseExtensionsTests" -v quiet`
Expected: All pass.

- [ ] **Step 5: Commit**

```
git add src/E3dcConnector.Typed/ test/E3dcConnector.Tests/Typed/
git commit -m "feat: add typed snapshots, commands, and response parsing extensions"
```

---

## Task 12: Sample Applications

**Files:**
- Create: `samples/E3dcConnector.Sample/Program.cs`
- Create: `samples/E3dcConnector.FlowSample/Program.cs`
- Create: `samples/E3dcConnector.ActorSample/Program.cs`
- Create: `samples/E3dcConnector.ActorSample/ConnectionActor.cs`

- [ ] **Step 1: Imperative client sample**

```csharp
// samples/E3dcConnector.Sample/Program.cs
using E3dcConnector.Client;
using E3dcConnector.Messages.Commands;
using E3dcConnector.Messages.Responses;
using E3dcConnector.Tags;
using E3dcConnector.Typed;

var client = new RscpClientBuilder()
    .Connect(args.Length > 0 ? args[0] : "192.168.1.100", 5033)
    .WithCredentials("user", "password")
    .WithEncryptionKey("rscp_password")
    .Build();

await using (client)
{
    var response = await client.SendAsync(new ReadTagsCommand([
        RscpTag.EMS_REQ_POWER_PV,
        RscpTag.EMS_REQ_POWER_BAT,
        RscpTag.EMS_REQ_POWER_GRID,
        RscpTag.EMS_REQ_POWER_HOME,
        RscpTag.EMS_REQ_BAT_SOC,
    ]));

    if (response is RscpDataResponse data)
    {
        var snapshot = data.ToEmsPowerSnapshot();
        if (snapshot is not null)
        {
            Console.WriteLine($"PV:      {snapshot.PvWatts} W");
            Console.WriteLine($"Battery: {snapshot.BatteryWatts} W");
            Console.WriteLine($"Grid:    {snapshot.GridWatts} W");
            Console.WriteLine($"Home:    {snapshot.HomeWatts} W");
            Console.WriteLine($"SOC:     {snapshot.Soc:F1} %");
        }
    }
    else if (response is RscpErrorResponse error)
    {
        Console.Error.WriteLine($"Error: {error.Message}");
    }
}
```

- [ ] **Step 2: Flow sample (raw channels)**

```csharp
// samples/E3dcConnector.FlowSample/Program.cs
using Akka.Actor;
using Akka.Streams;
using E3dcConnector.Client;
using E3dcConnector.Messages;
using E3dcConnector.Messages.Responses;
using E3dcConnector.Reactive;
using E3dcConnector.Reactive.Internal;
using E3dcConnector.Tags;
using E3dcConnector.Typed;

var system = ActorSystem.Create("e3dc-flow");
var materializer = system.Materializer();

var flow = RscpFlow.Create(
    () => new RscpConnection("192.168.1.100", 5033, "user", "password", "rscp_password"),
    pollingTags: [
        RscpTag.EMS_REQ_POWER_PV,
        RscpTag.EMS_REQ_POWER_BAT,
        RscpTag.EMS_REQ_POWER_GRID,
        RscpTag.EMS_REQ_POWER_HOME,
        RscpTag.EMS_REQ_BAT_SOC,
    ],
    new RscpFlowSettings { PollingInterval = TimeSpan.FromSeconds(2) });

var (commands, messages) = flow.Materialize(materializer);

await foreach (var msg in messages.ReadAllAsync())
{
    if (msg is RscpDataResponse data)
    {
        var snapshot = data.ToEmsPowerSnapshot();
        if (snapshot is not null)
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] PV={snapshot.PvWatts}W  BAT={snapshot.BatteryWatts}W  GRID={snapshot.GridWatts}W  SOC={snapshot.Soc:F1}%");
    }
}

await system.Terminate();
```

- [ ] **Step 3: Actor sample**

```csharp
// samples/E3dcConnector.ActorSample/ConnectionActor.cs
using Akka.Actor;
using Akka.Event;
using Akka.Streams;
using Akka.Streams.Dsl;
using E3dcConnector.Client;
using E3dcConnector.Messages;
using E3dcConnector.Reactive;
using E3dcConnector.Reactive.Internal;
using E3dcConnector.Tags;

namespace E3dcConnector.ActorSample;

public sealed class ConnectionActor : ReceiveActor
{
    public sealed record Connect { public static readonly Connect Instance = new(); }
    public sealed record Subscribe(IActorRef Subscriber);
    private sealed record StreamCompleted { public static readonly StreamCompleted Instance = new(); }

    private readonly ILoggingAdapter _log = Context.GetLogger();
    private readonly Func<RscpConnection> _connectionFactory;
    private readonly RscpTag[] _pollingTags;
    private readonly RscpFlowSettings _settings;
    private readonly HashSet<IActorRef> _subscribers = [];
    private ISourceQueueWithComplete<IRscpCommand>? _commandQueue;

    public ConnectionActor(
        Func<RscpConnection> connectionFactory,
        RscpTag[] pollingTags,
        RscpFlowSettings settings)
    {
        _connectionFactory = connectionFactory;
        _pollingTags = pollingTags;
        _settings = settings;

        Receive<Subscribe>(msg =>
        {
            _subscribers.Add(msg.Subscriber);
            Context.Watch(msg.Subscriber);
        });

        Receive<Terminated>(msg => _subscribers.Remove(msg.ActorRef));

        ReceiveAsync<Connect>(async _ =>
        {
            var materializer = Context.Materializer();
            var (queue, source) = Source.Queue<IRscpCommand>(64, OverflowStrategy.DropHead)
                .PreMaterialize(materializer);
            _commandQueue = queue;

            var flow = RscpFlow.Create(_connectionFactory, _pollingTags, _settings);
            source.Via(flow)
                .To(Sink.ActorRef<IRscpMessage>(Self, StreamCompleted.Instance, _ => StreamCompleted.Instance))
                .Run(materializer);

            _log.Info("RSCP stream materialized");
        });

        Receive<IRscpCommand>(cmd =>
        {
            if (_commandQueue is null) { _log.Warning("Not connected"); return; }
            _commandQueue.OfferAsync(cmd);
        });

        Receive<IRscpMessage>(msg =>
        {
            foreach (var sub in _subscribers) sub.Tell(msg);
        });

        Receive<StreamCompleted>(_ => _log.Warning("Stream completed"));
    }

    protected override void PostStop() => _commandQueue?.Complete();

    public static Props Create(
        Func<RscpConnection> connectionFactory,
        RscpTag[] pollingTags,
        RscpFlowSettings? settings = null) =>
        Props.Create(() => new ConnectionActor(connectionFactory, pollingTags, settings ?? new()));
}
```

```csharp
// samples/E3dcConnector.ActorSample/Program.cs
using Akka.Actor;
using E3dcConnector.ActorSample;
using E3dcConnector.Messages.Responses;
using E3dcConnector.Reactive;
using E3dcConnector.Reactive.Internal;
using E3dcConnector.Tags;
using E3dcConnector.Typed;

var system = ActorSystem.Create("e3dc-actors");

var connection = ConnectionActor.Create(
    () => new RscpConnection("192.168.1.100", 5033, "user", "password", "rscp_password"),
    [
        RscpTag.EMS_REQ_POWER_PV,
        RscpTag.EMS_REQ_POWER_BAT,
        RscpTag.EMS_REQ_POWER_GRID,
        RscpTag.EMS_REQ_POWER_HOME,
        RscpTag.EMS_REQ_BAT_SOC,
    ],
    new RscpFlowSettings { PollingInterval = TimeSpan.FromSeconds(2) });

var actor = system.ActorOf(connection, "e3dc-connection");
actor.Tell(ConnectionActor.Connect.Instance);

Console.WriteLine("Press Enter to exit...");
Console.ReadLine();
await system.Terminate();
```

- [ ] **Step 4: Verify build**

Run: `dotnet build`
Expected: All projects build successfully.

- [ ] **Step 5: Commit**

```
git add samples/
git commit -m "feat: add imperative, flow, and actor sample applications"
```

---

## Task 13: VitePress Documentation Setup

**Files:**
- Create: `docs/package.json`
- Create: `docs/.vitepress/config.ts`
- Create: `docs/index.md`

- [ ] **Step 1: Create package.json**

```json
{
  "name": "e3dc-connector-docs",
  "private": true,
  "scripts": {
    "docs:dev": "vitepress dev",
    "docs:build": "vitepress build",
    "docs:preview": "vitepress preview"
  },
  "devDependencies": {
    "vitepress": "^1.6.0"
  }
}
```

- [ ] **Step 2: Create VitePress config**

```typescript
// docs/.vitepress/config.ts
import { defineConfig } from 'vitepress'

export default defineConfig({
  title: 'E3DC Connector',
  description: 'Akka.Streams RSCP client for E3DC S10 Pro',
  themeConfig: {
    nav: [
      { text: 'Guide', link: '/guide/getting-started' },
      { text: 'Protocol', link: '/protocol/overview' },
      { text: 'Architecture', link: '/architecture/' },
    ],
    sidebar: {
      '/protocol/': [
        {
          text: 'RSCP Protocol',
          items: [
            { text: 'Overview', link: '/protocol/overview' },
            { text: 'Frame Format', link: '/protocol/frame-format' },
            { text: 'Data Types', link: '/protocol/data-types' },
            { text: 'Encryption', link: '/protocol/encryption' },
            { text: 'Authentication', link: '/protocol/authentication' },
          ]
        },
        {
          text: 'Tag Reference',
          items: [
            { text: 'Overview', link: '/protocol/tags/' },
            { text: 'EMS (Energy)', link: '/protocol/tags/ems' },
            { text: 'PVI (Inverter)', link: '/protocol/tags/pvi' },
            { text: 'BAT (Battery)', link: '/protocol/tags/bat' },
            { text: 'PM (Power Meter)', link: '/protocol/tags/pm' },
            { text: 'DB (History)', link: '/protocol/tags/db' },
            { text: 'WB (Wallbox)', link: '/protocol/tags/wb' },
            { text: 'INFO (Device)', link: '/protocol/tags/info' },
            { text: 'EP (Emergency)', link: '/protocol/tags/ep' },
          ]
        }
      ],
      '/guide/': [
        {
          text: 'Guide',
          items: [
            { text: 'Getting Started', link: '/guide/getting-started' },
            { text: 'Imperative Client', link: '/guide/imperative-client' },
            { text: 'Streaming', link: '/guide/streaming' },
            { text: 'Actor Integration', link: '/guide/actor-integration' },
            { text: 'Polling', link: '/guide/polling' },
            { text: 'Typed Snapshots', link: '/guide/typed-snapshots' },
            { text: 'Configuration', link: '/guide/configuration' },
          ]
        }
      ],
      '/architecture/': [
        {
          text: 'Architecture',
          items: [
            { text: 'Overview', link: '/architecture/' },
          ]
        }
      ]
    },
    search: { provider: 'local' },
    socialLinks: [
      { icon: 'github', link: 'https://github.com/example/e3dc-connector' }
    ]
  }
})
```

- [ ] **Step 3: Create landing page**

```markdown
<!-- docs/index.md -->
---
layout: home
hero:
  name: E3DC Connector
  text: Akka.Streams RSCP Client
  tagline: .NET library for communicating with E3DC S10 Pro home battery systems via the RSCP protocol
  actions:
    - theme: brand
      text: Get Started
      link: /guide/getting-started
    - theme: alt
      text: Protocol Reference
      link: /protocol/overview
features:
  - title: Full RSCP Protocol
    details: Complete implementation of the E3DC Remote Storage Control Protocol with Rijndael-256 encryption
  - title: Akka.Streams
    details: Reactive streaming pipeline with automatic reconnection, backpressure, and polling subscriptions
  - title: Three Usage Modes
    details: Imperative (async/await), reactive (channels), or actor-based (digital twin) — pick what fits
  - title: Typed Snapshots
    details: Strongly-typed records for EMS, Battery, Inverter, Power Meter, Wallbox, and more
---
```

- [ ] **Step 4: Install and verify**

Run: `cd docs && npm install && npx vitepress build`
Expected: VitePress builds (with warnings about missing pages — we'll add them next).

- [ ] **Step 5: Commit**

```
git add docs/package.json docs/.vitepress/ docs/index.md
git commit -m "feat: set up VitePress documentation site with nav and sidebar"
```

---

## Task 14: Protocol Reference Documentation

**Files:**
- Create: `docs/protocol/overview.md`
- Create: `docs/protocol/frame-format.md`
- Create: `docs/protocol/data-types.md`
- Create: `docs/protocol/encryption.md`
- Create: `docs/protocol/authentication.md`
- Create: `docs/protocol/tags/index.md`
- Create: `docs/protocol/tags/ems.md`
- Create: `docs/protocol/tags/pvi.md`
- Create: `docs/protocol/tags/bat.md`
- Create: `docs/protocol/tags/pm.md`
- Create: `docs/protocol/tags/db.md`
- Create: `docs/protocol/tags/wb.md`
- Create: `docs/protocol/tags/info.md`
- Create: `docs/protocol/tags/ep.md`

- [ ] **Step 1: Write protocol overview**

Content for `docs/protocol/overview.md`: Introduce RSCP — what it is, what E3DC systems support it, how it compares to Modbus (read+write vs read-only), the TCP connection model, and a high-level diagram of the communication flow.

- [ ] **Step 2: Write frame format page**

Content for `docs/protocol/frame-format.md`: Byte-level diagram of the frame header (Magic 0xE3DC, Control bitfield with version+CRC flag, Timestamp as uint64 seconds + uint32 nanos, DataLength uint16), data item TLV format (Tag uint32, DataType uint8, Length uint16, Value), CRC32 IEEE calculation. Include a hex dump example of a real frame.

- [ ] **Step 3: Write data types page**

Content for `docs/protocol/data-types.md`: Table of all 17 data types with hex value, name, size, .NET type mapping, and encoding details. Special sections for Container (recursive nesting), Timestamp (12-byte format), and Error (0xFF).

- [ ] **Step 4: Write encryption page**

Content for `docs/protocol/encryption.md`: Explain Rijndael-256 vs AES-256 (block size 32 vs 16), key derivation (UTF-8 password padded to 32 bytes with 0xFF), initial IV (32 bytes of 0xFF), CBC mode with IV chaining across frames, zero-padding to block boundary.

- [ ] **Step 5: Write authentication page**

Content for `docs/protocol/authentication.md`: TCP connect to port 5033, send RSCP_REQ_AUTHENTICATION container with user+password CString items, receive RSCP_AUTHENTICATION response with auth level (UChar8 or Int32), AUTH_LEVEL_NO_AUTH = 0 means failure.

- [ ] **Step 6: Write tag reference pages**

Create tag reference index (`docs/protocol/tags/index.md`) with namespace table. Create one page per namespace (ems.md, pvi.md, bat.md, pm.md, db.md, wb.md, info.md, ep.md) — each with a table of tags: hex value, name, data type, request/response pair, description.

- [ ] **Step 7: Verify docs build**

Run: `cd docs && npx vitepress build`
Expected: Build succeeds with no missing page warnings for protocol section.

- [ ] **Step 8: Commit**

```
git add docs/protocol/
git commit -m "docs: add comprehensive RSCP protocol reference"
```

---

## Task 15: Library Guide Documentation

**Files:**
- Create: `docs/guide/getting-started.md`
- Create: `docs/guide/imperative-client.md`
- Create: `docs/guide/streaming.md`
- Create: `docs/guide/actor-integration.md`
- Create: `docs/guide/polling.md`
- Create: `docs/guide/typed-snapshots.md`
- Create: `docs/guide/configuration.md`

- [ ] **Step 1: Write getting-started**

Content: NuGet package reference, minimal example — create builder, connect, read PV power, print result. 10 lines of code to first data.

- [ ] **Step 2: Write imperative-client guide**

Content: RscpClient with SendAsync, ReadTagsCommand, WriteTagCommand, typed response parsing, error handling, disposal.

- [ ] **Step 3: Write streaming guide**

Content: RscpFlow.Create, Materialize to channels, async enumeration, integration with other Akka.Streams stages.

- [ ] **Step 4: Write actor-integration guide**

Content: ConnectionActor pattern, Source.Queue + Sink.ActorRef, subscriber pattern, handling stream completion.

- [ ] **Step 5: Write polling guide**

Content: WithPolling on builder, configuring interval and tags, receiving periodic snapshots via Subscribe.

- [ ] **Step 6: Write typed-snapshots guide**

Content: EmsPowerSnapshot, BatterySnapshot, etc. — what fields each contains, how to use ToEmsPowerSnapshot() extensions.

- [ ] **Step 7: Write configuration guide**

Content: RscpClientBuilder full API reference — Connect, WithCredentials, WithEncryptionKey, WithPolling, WithReconnect, Build.

- [ ] **Step 8: Verify docs build**

Run: `cd docs && npx vitepress build`
Expected: Clean build.

- [ ] **Step 9: Commit**

```
git add docs/guide/
git commit -m "docs: add library usage guide"
```

---

## Task 16: LikeC4 Architecture Diagrams

**Files:**
- Create: `docs/architecture/index.md`
- Create: `docs/architecture/likec4/model.c4`
- Create: `docs/architecture/likec4/views.c4`

- [ ] **Step 1: Create LikeC4 model**

```likec4
// docs/architecture/likec4/model.c4
specification {
  element actor
  element system
  element component
  element layer
}

model {
  actor user = 'Your Application' {
    description 'Consumer of the e3dc-connector library'
  }

  system e3dc = 'E3DC S10 Pro' {
    description 'Home battery system with RSCP protocol'
  }

  system connector = 'e3dc-connector' {
    description '.NET Akka.Streams RSCP client library'

    component client = 'RscpClient' {
      description 'Correlation-based request/reply with channels'
    }

    component flow = 'RscpFlow' {
      description 'Akka.Streams pipeline with RestartFlow'

      component merge = 'Merge' {
        description 'Merges polling + on-demand commands'
      }

      component encode = 'EncodeStage' {
        description 'Commands → RSCP frame bytes'
      }

      component execute = 'ExecuteStage' {
        description 'TCP send/receive + Rijndael-256'
      }

      component decode = 'DecodeStage' {
        description 'RSCP frame bytes → typed responses'
      }
    }

    component connection = 'RscpConnection' {
      description 'TCP socket + encryption state + auth'
    }

    component protocol = 'Protocol Layer' {
      description 'RscpFrame, RscpDataItem, RscpCrypt'
    }

    component typed = 'Typed Layer' {
      description 'EmsPowerSnapshot, BatterySnapshot, ...'
    }
  }

  user -> connector.client 'SendAsync / Subscribe'
  connector.client -> connector.flow 'ChannelWriter<IRscpCommand>'
  connector.flow -> connector.client 'ChannelReader<IRscpMessage>'
  connector.flow.merge -> connector.flow.encode
  connector.flow.encode -> connector.flow.execute
  connector.flow.execute -> connector.flow.decode
  connector.flow.execute -> connector.connection 'SendFrameAsync / ReceiveFrameAsync'
  connector.connection -> connector.protocol 'Serialize / Deserialize'
  connector.connection -> e3dc 'TCP:5033 + Rijndael-256 CBC'
  connector.flow.decode -> connector.typed 'Parse response items'
}
```

- [ ] **Step 2: Create LikeC4 views**

```likec4
// docs/architecture/likec4/views.c4
views {
  view systemContext of connector {
    title 'System Context'
    include user, e3dc, connector
  }

  view pipeline of connector.flow {
    title 'Akka.Streams Pipeline'
    include
      connector.flow,
      connector.flow.merge,
      connector.flow.encode,
      connector.flow.execute,
      connector.flow.decode,
      connector.connection,
      connector.client
  }

  view layers of connector {
    title 'Protocol Layers'
    include
      connector.typed,
      connector.flow,
      connector.protocol,
      connector.connection,
      e3dc
  }
}
```

- [ ] **Step 3: Write architecture index page**

```markdown
<!-- docs/architecture/index.md -->
# Architecture

The e3dc-connector library is structured in layers, each with clear responsibilities.

## System Context

The library sits between your application and the E3DC S10 Pro hardware, handling all protocol complexity.

![System Context](./system-context.svg)

## Akka.Streams Pipeline

Internally, commands flow through an Akka.Streams pipeline that handles serialization, TCP communication with Rijndael-256 encryption, and deserialization. `RestartFlow.WithBackoff` provides automatic reconnection.

![Pipeline](./pipeline.svg)

## Protocol Layers

The encoding stack from typed .NET records down to encrypted TCP bytes:

| Layer | Component | Responsibility |
|-------|-----------|----------------|
| **Typed** | `EmsPowerSnapshot`, `BatterySnapshot`, ... | Strongly-typed .NET records |
| **Messages** | `IRscpCommand`, `IRscpMessage` | Protocol-agnostic command/response |
| **Frame** | `RscpFrame`, `RscpDataItem` | Binary TLV encoding + CRC32 |
| **Crypto** | `RscpCrypt` | Rijndael-256 CBC encryption |
| **Transport** | `RscpConnection` | TCP socket on port 5033 |

![Protocol Layers](./layers.svg)

## Source

The LikeC4 model is in [`likec4/model.c4`](./likec4/model.c4) and [`likec4/views.c4`](./likec4/views.c4).
```

- [ ] **Step 4: Export SVGs**

Run: `npx likec4 export png -o docs/architecture/ docs/architecture/likec4/`
(or SVG if supported — adjust the architecture/index.md image references accordingly)

- [ ] **Step 5: Verify docs build**

Run: `cd docs && npx vitepress build`
Expected: Clean build with architecture page rendering diagrams.

- [ ] **Step 6: Commit**

```
git add docs/architecture/
git commit -m "docs: add LikeC4 architecture diagrams with system context, pipeline, and layer views"
```

---

## Task 17: Final Verification

- [ ] **Step 1: Run all tests**

Run: `dotnet test --verbosity quiet`
Expected: All tests pass.

- [ ] **Step 2: Build everything**

Run: `dotnet build`
Expected: 0 errors, 0 warnings.

- [ ] **Step 3: Build docs**

Run: `cd docs && npx vitepress build`
Expected: Clean build.

- [ ] **Step 4: Preview docs locally**

Run: `cd docs && npx vitepress preview`
Verify: Landing page, protocol reference, guide, and architecture pages all render correctly with navigation.

- [ ] **Step 5: Final commit**

If any cleanup was needed, commit it:
```
git add -A
git commit -m "chore: final cleanup and verification"
```
