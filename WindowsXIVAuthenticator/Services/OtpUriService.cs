namespace WindowsXIVAuthenticator.Services;

public static class OtpUriService
{
    public static string? ExtractSecret(string uriText)
    {
        if (string.IsNullOrWhiteSpace(uriText))
            return null;

        if (!Uri.TryCreate(uriText, UriKind.Absolute, out var uri))
            return null;

        if (!uri.Scheme.Equals("otpauth", StringComparison.OrdinalIgnoreCase))
            return null;

        var query = uri.Query.TrimStart('?');

        foreach (var item in query.Split('&'))
        {
            var parts = item.Split('=', 2);

            if (parts.Length != 2)
                continue;

            if (parts[0].Equals("secret", StringComparison.OrdinalIgnoreCase))
            {
                return Uri.UnescapeDataString(parts[1])
                    .Replace(" ", "")
                    .ToUpperInvariant();
            }
        }

        return null;
    }
}