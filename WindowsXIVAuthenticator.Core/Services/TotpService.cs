using OtpNet;

namespace WindowsXIVAuthenticator.Core.Services;

public static class TotpService
{
    public static string GenerateCode(string base32Secret)
    {
        var secretBytes = Base32Encoding.ToBytes(base32Secret);

        var totp = new Totp(
            secretBytes,
            step: 30,
            mode: OtpHashMode.Sha1,
            totpSize: 6
        );

        return totp.ComputeTotp();
    }

    public static int GetRemainingSeconds()
    {
        const int interval = 30;

        var unixTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        return interval - (int)(unixTime % interval);
    }
}