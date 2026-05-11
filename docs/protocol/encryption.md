# Encryption

RSCP uses Rijndael-256 in CBC mode to encrypt all communication. This is **not** the same as AES-256.

## Rijndael-256 vs AES-256

| Property | Rijndael-256 (RSCP) | AES-256 |
|----------|:-------------------:|:-------:|
| Algorithm | Rijndael | Rijndael |
| Key size | 256 bits (32 bytes) | 256 bits (32 bytes) |
| Block size | **256 bits (32 bytes)** | **128 bits (16 bytes)** |
| Compatible | No | No |

AES is a subset of Rijndael with a fixed 128-bit block size. RSCP uses the full Rijndael with a 256-bit block size, which means standard AES libraries (including .NET's `System.Security.Cryptography.Aes`) cannot be used. The e3dc-connector library uses BouncyCastle's `RijndaelEngine` for this reason.

## Key Derivation

The encryption key is derived from a password string:

1. Convert the password to UTF-8 bytes
2. Create a 32-byte array filled with `0xFF`
3. Copy the password bytes into the array (truncating if longer than 32 bytes)

```csharp
var key = new byte[32];
Array.Fill(key, (byte)0xFF);
var passwordBytes = Encoding.UTF8.GetBytes(password);
Array.Copy(passwordBytes, key, Math.Min(passwordBytes.Length, 32));
```

## Initialization Vector (IV)

The initial IV is 32 bytes of `0xFF`:

```
FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF
FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF
```

## CBC Chaining

After each encrypt or decrypt operation, the last ciphertext block becomes the IV for the next operation. This means the IV state must be maintained across frames for the entire TCP session.

## Padding

Frames are zero-padded to a multiple of 32 bytes (the Rijndael-256 block size) before encryption. Since the frame header contains the data length, the receiver knows where the actual data ends and padding begins.

## Encryption Flow

```
Frame Bytes → Zero-Pad to 32B boundary → Rijndael-256 CBC Encrypt → TCP Write
TCP Read → Rijndael-256 CBC Decrypt → Strip Padding → Parse Frame
```
