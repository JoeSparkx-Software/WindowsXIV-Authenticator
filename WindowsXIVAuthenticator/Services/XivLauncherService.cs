namespace WindowsXIVAuthenticator.Services;

public static class XivLauncherService
{
    private static readonly HttpClient HttpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(3)
    };

    private static string GetLauncherPath()
    {
        var path =
            AppSettingsService.GetLauncherPath();

        if (string.IsNullOrWhiteSpace(path))
        {
            throw new FileNotFoundException(
                "XIVLauncher could not be found. " +
                "Configure its location in Settings.");
        }

        return path;
    }

    public static async Task SendOtpAsync(string otp)
    {
        if (string.IsNullOrWhiteSpace(otp) || otp.Length != 6)
            throw new ArgumentException("OTP must be a 6-digit code.", nameof(otp));

        var url = $"http://127.0.0.1:4646/ffxivlauncher/{otp}";

        using var response = await HttpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
    }

    public static void StartLauncher()
    {
        var launcherPath =
            GetLauncherPath();

        Process.Start(
            new ProcessStartInfo
            {
                FileName = launcherPath,
                UseShellExecute = false
            });
    }

    public static async Task<bool> WaitForOtpListenerAsync(
        TimeSpan timeout)
    {
        var started = DateTime.UtcNow;

        while (DateTime.UtcNow - started < timeout)
        {
            try
            {
                using var client = new TcpClient();

                var connectTask =
                    client.ConnectAsync("127.0.0.1", 4646);

                var completedTask = await Task.WhenAny(
                    connectTask,
                    Task.Delay(500));

                if (completedTask == connectTask &&
                    client.Connected)
                {
                    return true;
                }
            }
            catch
            {
                // Listener not ready yet.
            }

            await Task.Delay(500);
        }

        return false;
    }

    public static async Task LaunchAndSendOtpAsync(string otp)
    {
        StartLauncher();

        var listenerReady =
            await WaitForOtpListenerAsync(
                TimeSpan.FromSeconds(15));

        if (!listenerReady)
        {
            throw new TimeoutException(
                "XIVLauncher started, but its OTP listener did not become available.");
        }

        await SendOtpAsync(otp);
    }
}