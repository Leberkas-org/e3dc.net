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
        var padded = new byte[paddedLength];
        data.CopyTo(padded);
        return padded;
    }
}
