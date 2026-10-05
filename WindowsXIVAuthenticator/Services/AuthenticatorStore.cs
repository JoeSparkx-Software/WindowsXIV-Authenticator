namespace WindowsXIVAuthenticator.Services;

public sealed class StoredAuthenticator
{
    public string DisplayName { get; set; } = "";
    public string EncryptedSecret { get; set; } = "";
}

public static class AuthenticatorStore
{
    private static readonly byte[] Entropy =
        Encoding.UTF8.GetBytes("WindowsXIVAuthenticator:v1");

    private static readonly string AppDirectory =
        Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData),
            "WindowsXIVAuthenticator");

    private static readonly string StorePath =
        Path.Combine(AppDirectory, "authenticator.json");

    public static void Save(
        string secret,
        string displayName)
    {
        Directory.CreateDirectory(AppDirectory);

        var secretBytes =
            Encoding.UTF8.GetBytes(secret);

        var encryptedBytes =
            ProtectedData.Protect(
                secretBytes,
                Entropy,
                DataProtectionScope.CurrentUser);

        var stored = new StoredAuthenticator
        {
            DisplayName = displayName,
            EncryptedSecret =
                Convert.ToBase64String(encryptedBytes)
        };

        var json = JsonSerializer.Serialize(
            stored,
            new JsonSerializerOptions
            {
                WriteIndented = true
            });

        File.WriteAllText(StorePath, json);
    }

    public static StoredAuthenticator? Load()
    {
        if (!File.Exists(StorePath))
            return null;

        var json = File.ReadAllText(StorePath);

        return JsonSerializer.Deserialize<StoredAuthenticator>(
            json);
    }

    public static string? GetSecret()
    {
        var stored = Load();

        if (stored is null ||
            string.IsNullOrWhiteSpace(stored.EncryptedSecret))
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

        return Encoding.UTF8.GetString(decryptedBytes);
    }

    public static bool Exists()
    {
        return File.Exists(StorePath);
    }

    public static void Delete()
    {
        if (File.Exists(StorePath))
            File.Delete(StorePath);
    }
}