using System.Security.Cryptography;
using System.Text.Json;

namespace WindowsXIVAuthenticator.Core.Services;

public sealed class AuthenticatorVault
{
    public int Version { get; set; } = 2;

    public string DisplayName { get; set; } = "";

    public ProtectedVaultKey ProtectedVaultKey { get; set; } =
        new();

    public EncryptedVaultPayload Secret { get; set; } =
        new();
}

public static class AuthenticatorVaultStore
{
private static readonly string VaultPath =
    Path.Combine(
        Environment.GetFolderPath(
            Environment.SpecialFolder.LocalApplicationData),
        "WindowsXIVAuthenticator",
        "vault.json");

    public static bool Exists()
    {
        return File.Exists(
            VaultPath);
    }

    public static AuthenticatorVault? Load()
    {
        if (!Exists())
            return null;

        var json =
            File.ReadAllText(
                VaultPath);

        var vault =
            JsonSerializer.Deserialize<AuthenticatorVault>(
                json);

        if (vault is null)
        {
            throw new CryptographicException(
                "The authenticator vault could not be read.");
        }

        if (vault.Version != 2)
        {
            throw new CryptographicException(
                $"Unsupported authenticator vault version: {vault.Version}");
        }

        return vault;
    }

    public static async Task SaveAsync(
        string secret,
        string displayName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            secret);

        ArgumentException.ThrowIfNullOrWhiteSpace(
            displayName);

        var vaultKey =
            VaultCryptoService.GenerateKey();

        try
        {
            var encryptedSecret =
                VaultCryptoService.Encrypt(
                    secret,
                    vaultKey);

            var protectedVaultKey =
                await VaultKeyProtector.ProtectAsync(
                    vaultKey);

            var vault =
                new AuthenticatorVault
                {
                    Version = 2,

                    DisplayName =
                        displayName,

                    ProtectedVaultKey =
                        protectedVaultKey,

                    Secret =
                        encryptedSecret
                };

            SaveVault(
                vault);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(
                vaultKey);
        }
    }

    public static async Task<string?> GetSecretAsync()
    {
        var vault =
            Load();

        if (vault is null)
            return null;

        var vaultKey =
            await VaultKeyProtector.UnprotectAsync(
                vault.ProtectedVaultKey);

        try
        {
            return VaultCryptoService.Decrypt(
                vault.Secret,
                vaultKey);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(
                vaultKey);
        }
    }

    public static void Delete()
    {
        if (Exists())
        {
            File.Delete(
                VaultPath);
        }
    }

    private static void SaveVault(
        AuthenticatorVault vault)
    {
        var directory =
            Path.GetDirectoryName(
                VaultPath);

        if (string.IsNullOrWhiteSpace(
                directory))
        {
            throw new InvalidOperationException(
                "The authenticator vault directory is invalid.");
        }

        Directory.CreateDirectory(
            directory);

        var json =
            JsonSerializer.Serialize(
                vault,
                new JsonSerializerOptions
                {
                    WriteIndented = true
                });

        var temporaryPath =
            VaultPath + ".tmp";

        File.WriteAllText(
            temporaryPath,
            json);

        try
        {
            File.Move(
                temporaryPath,
                VaultPath,
                true);
        }
        finally
        {
            if (File.Exists(
                    temporaryPath))
            {
                File.Delete(
                    temporaryPath);
            }
        }
    }
}