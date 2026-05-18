using System.Buffers.Binary;
using System.Net.Sockets;
using System.Text;
using E3dc.Protocol;
using E3dc.Tags;

namespace E3dc.Reactive.Internal;

public sealed class RscpConnection : IDisposable
{
    private readonly string _host;
    private readonly int _port;
    private readonly string _user;
    private readonly string _password;
    private readonly RscpCrypt _crypt;

    private TcpClient? _tcp;
    private NetworkStream? _stream;
    private bool _authenticated;

    public RscpConnection(string host, int port = 5033, string user = "", string password = "", string encryptionKey = "")
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
        {
            var tags = string.Join(", ", response.Items.Select(i =>
            {
                var hex = BitConverter.ToString(i.Value.ToArray()).Replace("-", " ");
                return $"Tag=0x{i.Tag:X8} Type={i.DataType} Len={i.Value.Length} Val=[{hex}]";
            }));
            throw new InvalidOperationException(
                $"RSCP authentication failed: level={level}. Response items: [{tags}]");
        }
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

        var firstBlock = new byte[blockSize];
        await ReadExactAsync(_stream!, firstBlock, ct);
        var decryptedFirst = _crypt.Decrypt(firstBlock);
        buffer.AddRange(decryptedFirst);

        var magic = BinaryPrimitives.ReadUInt16LittleEndian(decryptedFirst);
        if (magic != RscpFrame.Magic)
            throw new InvalidDataException($"Invalid RSCP magic: 0x{magic:X4}");

        var ctrl = BinaryPrimitives.ReadUInt16LittleEndian(decryptedFirst.AsSpan(2));
        var hasCrc = ((ctrl >> 12) & 1) == 1;
        var dataLength = BinaryPrimitives.ReadUInt16LittleEndian(decryptedFirst.AsSpan(16));
        var totalFrameSize = RscpFrame.HeaderSize + dataLength + (hasCrc ? 4 : 0);
        var totalEncryptedSize = (totalFrameSize + blockSize - 1) / blockSize * blockSize;

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
