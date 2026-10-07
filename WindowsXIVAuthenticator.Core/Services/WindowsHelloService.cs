using Windows.Security.Credentials.UI;

namespace WindowsXIVAuthenticator.Core.Services;

public static class WindowsHelloService
{
    public static async Task<bool> IsAvailableAsync()
    {
        var availability = await UserConsentVerifier.CheckAvailabilityAsync();

        return availability == UserConsentVerifierAvailability.Available;
    }

    public static async Task<bool> VerifyAsync(string message = "Verify your identity to continue")
    {
        var availability = await UserConsentVerifier.CheckAvailabilityAsync();

        if (availability != UserConsentVerifierAvailability.Available)
            return false;

        var result = await UserConsentVerifier.RequestVerificationAsync(message);

        return result == UserConsentVerificationResult.Verified;
    }
}