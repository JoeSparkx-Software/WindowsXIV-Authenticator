using System.Reflection;
using System.Text.Json.Serialization;

namespace WindowsXIVAuthenticator.Services;

public static class UpdateService
{
    private static readonly HttpClient HttpClient =
        new();

    static UpdateService()
    {
        HttpClient.DefaultRequestHeaders.UserAgent.ParseAdd(
            "WindowsXIVAuthenticator/1.0");
    }

    public static async Task<UpdateCheckResult?> CheckForUpdateAsync()
    {
        try
        {
            using var response =
                await HttpClient.GetAsync(
                    AppConstants.GitHubLatestReleaseApiUrl);

            if (!response.IsSuccessStatusCode)
                return null;

            var json =
                await response.Content.ReadAsStringAsync();

            var release =
                JsonSerializer.Deserialize<GitHubRelease>(
                    json);

            if (string.IsNullOrWhiteSpace(
                    release?.TagName))
            {
                return null;
            }

            var latestVersionText =
                release.TagName.Trim();

            if (latestVersionText.StartsWith(
                    "v",
                    StringComparison.OrdinalIgnoreCase))
            {
                latestVersionText =
                    latestVersionText[1..];
            }

            if (!Version.TryParse(
                    latestVersionText,
                    out var latestVersion))
            {
                return null;
            }

            var assemblyVersion =
                Assembly.GetExecutingAssembly()
                    .GetName()
                    .Version;

            if (assemblyVersion is null)
                return null;

            var currentVersion =
                new Version(
                    assemblyVersion.Major,
                    assemblyVersion.Minor,
                    Math.Max(
                        assemblyVersion.Build,
                        0));

            var comparableLatest =
                new Version(
                    latestVersion.Major,
                    latestVersion.Minor,
                    Math.Max(
                        latestVersion.Build,
                        0));

            return new UpdateCheckResult(
                currentVersion,
                comparableLatest,
                comparableLatest > currentVersion);
        }
        catch
        {
            // Update checks must never interfere
            // with normal authenticator operation.
            return null;
        }
    }

    private sealed class GitHubRelease
    {
        [JsonPropertyName("tag_name")]
        public string? TagName { get; init; }
    }
}

public sealed record UpdateCheckResult(
    Version CurrentVersion,
    Version LatestVersion,
    bool UpdateAvailable);