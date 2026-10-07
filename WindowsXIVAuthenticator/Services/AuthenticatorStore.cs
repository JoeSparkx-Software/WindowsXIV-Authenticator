using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace WindowsXIVAuthenticator.Services;

public sealed class LegacyStoredAuthenticator
{
    public string DisplayName { get; set; } = "";

    public string EncryptedSecret { get; set; } = "";
}

public static class AuthenticatorStore
{
    private static readonly byte[] Entropy =
        Encoding.UTF8.GetBytes(
            "WindowsXIVAuthenticator:v1");

    private static readonly string AppDirectory =
        Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData),
            "WindowsXIVAuthenticator");

    private static readonly string StorePath =
        Path.Combine(
            AppDirectory,
            "authenticator.json");

    public static bool Exists()
    {
        return File.Exists(
            StorePath);
    }

    public static LegacyStoredAuthenticator? Load()
    {
        if (!Exists())
            return null;

        var json =
            File.ReadAllText(
                StorePath);

        return JsonSerializer.Deserialize<LegacyStoredAuthenticator>(
            json);
    }

    public static string? GetSecret()
    {
        var stored =
            Load();

        if (stored is null ||
            string.IsNullOrWhiteSpace(
                stored.EncryptedSecret))
        {
            return null;
        }

        var encryptedBytes =
            Convert.FromBase64String(
                stored.EncryptedSecret);

        var decryptedBytes =
            ProtectedData.Unprotect(
                encryptedBytes,
                Entropy,
                DataProtectionScope.CurrentUser);

        try
        {
            return Encoding.UTF8.GetString(
                decryptedBytes);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(
                decryptedBytes);
        }
    }

    public static void Delete()
    {
        if (Exists())
        {
            File.Delete(
                StorePath);
        }
    }
}