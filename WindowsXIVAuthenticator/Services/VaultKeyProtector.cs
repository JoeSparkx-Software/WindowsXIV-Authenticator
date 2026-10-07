using System.Security.Cryptography;

namespace WindowsXIVAuthenticator.Services;

public sealed class ProtectedVaultKey
{
    public int Version { get; set; } = 2;

    public string CipherText { get; set; } = "";
}

public static class VaultKeyProtector
{
    private const string KeyName =
        "WindowsXIVAuthenticator.VaultKey.v2";

    private const int VaultKeySizeBytes = 32;

    private static readonly CngProvider Provider =
        CngProvider.MicrosoftPlatformCryptoProvider;

    public static Task<ProtectedVaultKey> ProtectAsync(
        byte[] vaultKey)
    {
        ValidateVaultKey(
            vaultKey);

        using var key =
            OpenOrCreateKey();

        using var rsa =
            new RSACng(
                key);

        var encryptedKey =
            rsa.Encrypt(
                vaultKey,
                RSAEncryptionPadding.OaepSHA256);

        var result =
            new ProtectedVaultKey
            {
                Version = 2,

                CipherText =
                    Convert.ToBase64String(
                        encryptedKey)
            };

        CryptographicOperations.ZeroMemory(
            encryptedKey);

        return Task.FromResult(
            result);
    }

    public static Task<byte[]> UnprotectAsync(
        ProtectedVaultKey protectedKey)
    {
        ArgumentNullException.ThrowIfNull(
            protectedKey);

        if (protectedKey.Version != 2)
        {
            throw new CryptographicException(
                $"Unsupported protected vault key version: {protectedKey.Version}");
        }

        if (string.IsNullOrWhiteSpace(
                protectedKey.CipherText))
        {
            throw new CryptographicException(
                "The protected vault key is missing.");
        }

        var encryptedKey =
            Convert.FromBase64String(
                protectedKey.CipherText);

        try
        {
            using var key =
                OpenExistingKey();

            using var rsa =
                new RSACng(
                    key);

            var vaultKey =
                rsa.Decrypt(
                    encryptedKey,
                    RSAEncryptionPadding.OaepSHA256);

            ValidateVaultKey(
                vaultKey);

            return Task.FromResult(
                vaultKey);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(
                encryptedKey);
        }
    }

    public static bool Exists()
    {
        return CngKey.Exists(
            KeyName,
            Provider);
    }

    private static CngKey OpenOrCreateKey()
    {
        if (CngKey.Exists(
                KeyName,
                Provider))
        {
            return OpenExistingKey();
        }

        var creationParameters =
            new CngKeyCreationParameters
            {
                Provider =
                    Provider,

                ExportPolicy =
                    CngExportPolicies.None,

                KeyUsage =
                    CngKeyUsages.Decryption,

                UIPolicy =
                    new CngUIPolicy(
                        CngUIProtectionLevels
                            .ForceHighProtection,

                        "Windows XIV Authenticator",

                        "Protects the local authenticator vault.",

                        "Set up Windows XIV Authenticator vault protection.")
            };

        return CngKey.Create(
            CngAlgorithm.Rsa,
            KeyName,
            creationParameters);
    }

    private static CngKey OpenExistingKey()
    {
        if (!CngKey.Exists(
                KeyName,
                Provider))
        {
            throw new CryptographicException(
                "The TPM-backed vault key does not exist.");
        }

        return CngKey.Open(
            KeyName,
            Provider);
    }

    private static void ValidateVaultKey(
        byte[] vaultKey)
    {
        ArgumentNullException.ThrowIfNull(
            vaultKey);

        if (vaultKey.Length !=
            VaultKeySizeBytes)
        {
            throw new CryptographicException(
                "The vault key must be exactly 256 bits.");
        }
    }
}