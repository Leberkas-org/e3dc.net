using E3dc.Protocol;
using FluentAssertions;

namespace E3dc.Tests.Protocol;

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
        var plaintext = new byte[64];
        for (int i = 0; i < plaintext.Length; i++) plaintext[i] = (byte)(i & 0xFF);

        var encrypted = crypt.Encrypt(plaintext);
        encrypted.Should().NotEqual(plaintext);
        encrypted.Length.Should().Be(64);

        var decrypted = crypt.Decrypt(encrypted);
        decrypted.Should().Equal(plaintext);
    }

    [Fact]
    public void Encrypt_pads_to_32_byte_boundary()
    {
        var crypt = new RscpCrypt("test");
        var plaintext = new byte[10];
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

        var enc1 = crypt1.Encrypt(block1);
        var enc2 = crypt1.Encrypt(block2);

        var enc1b = crypt2.Encrypt(block1);
        var enc2b = crypt2.Encrypt(block2);

        enc1.Should().Equal(enc1b);
        enc2.Should().Equal(enc2b);
    }
}
