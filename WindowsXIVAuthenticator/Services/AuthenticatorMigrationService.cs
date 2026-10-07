using System.Security.Cryptography;
using System.Text;

namespace WindowsXIVAuthenticator.Services;

public static class AuthenticatorMigrationService
{
    public static bool MigrationRequired()
    {
        return
            AuthenticatorStore.Exists()
            &&
            !AuthenticatorVaultStore.Exists();
    }

    public static async Task<bool> MigrateAsync()
    {
        if (!MigrationRequired())
            return false;

        var stored =
            AuthenticatorStore.Load();

        if (stored is null)
        {
            throw new CryptographicException(
                "The existing authenticator could not be read.");
        }

        var secret =
            AuthenticatorStore.GetSecret();

        if (string.IsNullOrWhiteSpace(secret))
        {
            throw new CryptographicException(
                "The existing authenticator secret could not be decrypted.");
        }

        await AuthenticatorVaultStore.SaveAsync(
            secret,
            stored.DisplayName);

        var migratedSecret =
            await AuthenticatorVaultStore.GetSecretAsync();

        if (string.IsNullOrWhiteSpace(
                migratedSecret))
        {
            AuthenticatorVaultStore.Delete();

            throw new CryptographicException(
                "The migrated authenticator secret could not be read back.");
        }

        var originalBytes =
            Encoding.UTF8.GetBytes(
                secret);

        var migratedBytes =
            Encoding.UTF8.GetBytes(
                migratedSecret);

        try
        {
            if (!CryptographicOperations.FixedTimeEquals(
                    originalBytes,
                    migratedBytes))
            {
                AuthenticatorVaultStore.Delete();

                throw new CryptographicException(
                    "The migrated authenticator did not match the original.");
            }
        }
        finally
        {
            CryptographicOperations.ZeroMemory(
                originalBytes);

            CryptographicOperations.ZeroMemory(
                migratedBytes);
        }

            AuthenticatorStore.Delete();
        return true;
    }
}