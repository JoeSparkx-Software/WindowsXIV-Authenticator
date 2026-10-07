using System.Text.RegularExpressions;
using WindowsXIVAuthenticator.Core.Services;

if (args.Length != 1 ||
    !string.Equals(
        args[0],
        "code",
        StringComparison.OrdinalIgnoreCase))
{
    Console.Error.WriteLine(
        "Usage: xiv-auth code");

    return 2;
}

try
{
    if (!AuthenticatorVaultStore.Exists())
    {
        Console.Error.WriteLine(
            "No authenticator is configured.");

        return 3;
    }

    var verified =
        await WindowsHelloService.VerifyAsync(
            "Unlock Windows XIV Authenticator");

    if (!verified)
    {
        Console.Error.WriteLine(
            "Windows Hello verification was cancelled or failed.");

        return 4;
    }

    var secret =
        await AuthenticatorVaultStore.GetSecretAsync();

    if (string.IsNullOrWhiteSpace(secret))
    {
        Console.Error.WriteLine(
            "The authenticator secret could not be loaded.");

        return 5;
    }

    var otp =
        TotpService.GenerateCode(
            secret);

    if (!Regex.IsMatch(
            otp,
            @"^\d{6}$"))
    {
        Console.Error.WriteLine(
            "The generated OTP was invalid.");

        return 6;
    }

    Console.WriteLine(
        otp);

    return 0;
}
catch (Exception ex)
{
    Console.Error.WriteLine(
        $"xiv-auth failed: {ex.Message}");

    return 10;
}