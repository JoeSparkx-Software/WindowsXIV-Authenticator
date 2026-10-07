using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace WindowsXIVAuthenticator.Services;

public sealed class EncryptedVaultPayload
{
    public int Version { get; set; } = 2;

    public string Nonce { get; set; } = "";

    public string CipherText { get; set; } = "";

    public string Tag { get; set; } = "";
}

public static class VaultCryptoService
{
    private const int KeySizeBytes = 32;
    private const int NonceSizeBytes = 12;
    private const int TagSizeBytes = 16;

    public static byte[] GenerateKey()
    {
        return RandomNumberGenerator.GetBytes(
            KeySizeBytes);
    }

    public static EncryptedVaultPayload Encrypt(
        string plaintext,
        byte[] key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            plaintext);

        ValidateKey(key);

        var plaintextBytes =
            Encoding.UTF8.GetBytes(
                plaintext);

        var nonce =
            RandomNumberGenerator.GetBytes(
                NonceSizeBytes);

        var cipherText =
            new byte[plaintextBytes.Length];

        var tag =
            new byte[TagSizeBytes];

        using var aes =
            new AesGcm(
                key,
                TagSizeBytes);

        aes.Encrypt(
            nonce,
            plaintextBytes,
            cipherText,
            tag);

        CryptographicOperations.ZeroMemory(
            plaintextBytes);

        return new EncryptedVaultPayload
        {
            Version = 2,

            Nonce =
                Convert.ToBase64String(
                    nonce),

            CipherText =
                Convert.ToBase64String(
                    cipherText),

            Tag =
                Convert.ToBase64String(
                    tag)
        };
    }

    public static string Decrypt(
        EncryptedVaultPayload payload,
        byte[] key)
    {
        ArgumentNullException.ThrowIfNull(
            payload);

        ValidateKey(key);

        if (payload.Version != 2)
        {
            throw new CryptographicException(
                $"Unsupported vault version: {payload.Version}");
        }

        var nonce =
            Convert.FromBase64String(
                payload.Nonce);

        var cipherText =
            Convert.FromBase64String(
                payload.CipherText);

        var tag =
            Convert.FromBase64String(
                payload.Tag);

        if (nonce.Length != NonceSizeBytes)
        {
            throw new CryptographicException(
                "The vault nonce is invalid.");
        }

        if (tag.Length != TagSizeBytes)
        {
            throw new CryptographicException(
                "The vault authentication tag is invalid.");
        }

        var plaintextBytes =
            new byte[cipherText.Length];

        using var aes =
            new AesGcm(
                key,
                TagSizeBytes);

        aes.Decrypt(
            nonce,
            cipherText,
            tag,
            plaintextBytes);

        try
        {
            return Encoding.UTF8.GetString(
                plaintextBytes);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(
                plaintextBytes);
        }
    }

    public static string Serialize(
        EncryptedVaultPayload payload)
    {
        return JsonSerializer.Serialize(
            payload,
            new JsonSerializerOptions
            {
                WriteIndented = true
            });
    }

    public static EncryptedVaultPayload Deserialize(
        string json)
    {
        var payload =
            JsonSerializer.Deserialize<EncryptedVaultPayload>(
                json);

        if (payload is null)
        {
            throw new CryptographicException(
                "The encrypted vault could not be read.");
        }

        return payload;
    }

    private static void ValidateKey(
        byte[] key)
    {
        ArgumentNullException.ThrowIfNull(
            key);

        if (key.Length != KeySizeBytes)
        {
            throw new CryptographicException(
                "The vault key must be exactly 256 bits.");
        }
    }
}