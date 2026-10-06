namespace WindowsXIVAuthenticator.Services;

public sealed class AppSettings
{
    public string? XivLauncherPath { get; set; }
    public bool RequireWindowsHello { get; set; }
}

public static class AppSettingsService
{
    private static readonly string AppDirectory =
        Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData),
            "WindowsXIVAuthenticator");

    private static readonly string SettingsPath =
        Path.Combine(AppDirectory, "settings.json");

    public static string DefaultLauncherPath =>
        Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData),
            "XIVLauncher",
            "XIVLauncher.exe");

    public static AppSettings Load()
    {
        try
        {
            if (!File.Exists(SettingsPath))
                return new AppSettings();

            var json =
                File.ReadAllText(SettingsPath);

            return JsonSerializer.Deserialize<AppSettings>(json)
                ?? new AppSettings();
        }
        catch
        {
            return new AppSettings();
        }
    }

    public static void Save(AppSettings settings)
    {
        Directory.CreateDirectory(AppDirectory);

        var json =
            JsonSerializer.Serialize(
                settings,
                new JsonSerializerOptions
                {
                    WriteIndented = true
                });

        File.WriteAllText(
            SettingsPath,
            json);
    }

    public static string? GetLauncherPath()
    {
        var settings = Load();

        if (!string.IsNullOrWhiteSpace(
                settings.XivLauncherPath) &&
            File.Exists(settings.XivLauncherPath))
        {
            return settings.XivLauncherPath;
        }

        if (File.Exists(DefaultLauncherPath))
            return DefaultLauncherPath;

        return null;
    }

    public static void SetLauncherPath(
        string path)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException(
                "XIVLauncher.exe could not be found.",
                path);
        }

        var settings = Load();

        settings.XivLauncherPath = path;

        Save(settings);
    }

    public static void ResetLauncherPath()
    {
        var settings = Load();

        settings.XivLauncherPath = null;

        Save(settings);
    }
    public static bool GetRequireWindowsHello()
    {
        return Load().RequireWindowsHello;
    }

    public static void SetRequireWindowsHello(bool enabled)
    {
        var settings = Load();

        settings.RequireWindowsHello = enabled;

        Save(settings);
    }
}