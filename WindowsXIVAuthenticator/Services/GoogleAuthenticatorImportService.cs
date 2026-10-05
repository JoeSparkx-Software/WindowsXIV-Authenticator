using Google.Protobuf;
using OtpNet;

namespace WindowsXIVAuthenticator.Services;

public sealed class GoogleAuthenticatorEntry
{
    public string Name { get; init; } = "";
    public string Issuer { get; init; } = "";
    public string Secret { get; init; } = "";
    public int Algorithm { get; init; }
    public int Digits { get; init; }
    public int Type { get; init; }
    public long Counter { get; init; }

    public string DisplayName =>
        !string.IsNullOrWhiteSpace(Issuer)
            ? $"{Issuer} — {Name}"
            : Name;
}

public sealed class GoogleAuthenticatorImportResult
{
    public List<GoogleAuthenticatorEntry> Entries { get; } = [];

    public int Version { get; set; }
    public int BatchSize { get; set; }
    public int BatchIndex { get; set; }
    public int BatchId { get; set; }
}

public static class GoogleAuthenticatorImportService
{
    public static GoogleAuthenticatorImportResult Decode(string migrationUri)
    {
        if (string.IsNullOrWhiteSpace(migrationUri))
            throw new ArgumentException("Migration URI is empty.");

        if (!migrationUri.StartsWith(
                "otpauth-migration://",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                "This is not a Google Authenticator migration QR code.");
        }

        var data = ExtractQueryValue(migrationUri, "data");

        if (string.IsNullOrWhiteSpace(data))
            throw new InvalidOperationException(
                "The Google Authenticator QR does not contain migration data.");

        var decoded = Uri.UnescapeDataString(data);

        // Base64 payloads sometimes omit padding.
        decoded = decoded.Trim();

        var padding = decoded.Length % 4;

        if (padding != 0)
            decoded = decoded.PadRight(decoded.Length + (4 - padding), '=');

        var payloadBytes = Convert.FromBase64String(decoded);

        return ParsePayload(payloadBytes);
    }

    private static GoogleAuthenticatorImportResult ParsePayload(byte[] bytes)
    {
        var result = new GoogleAuthenticatorImportResult();
        var input = new CodedInputStream(bytes);

        uint tag;

        while ((tag = input.ReadTag()) != 0)
        {
            switch (tag)
            {
                // repeated OtpParameters otp_parameters = 1;
                case 10:
                {
                    var parameterBytes = input.ReadBytes().ToByteArray();
                    result.Entries.Add(ParseOtpParameters(parameterBytes));
                    break;
                }

                // int32 version = 2;
                case 16:
                    result.Version = input.ReadInt32();
                    break;

                // int32 batch_size = 3;
                case 24:
                    result.BatchSize = input.ReadInt32();
                    break;

                // int32 batch_index = 4;
                case 32:
                    result.BatchIndex = input.ReadInt32();
                    break;

                // int32 batch_id = 5;
                case 40:
                    result.BatchId = input.ReadInt32();
                    break;

                default:
                    input.SkipLastField();
                    break;
            }
        }

        return result;
    }

    private static GoogleAuthenticatorEntry ParseOtpParameters(byte[] bytes)
    {
        var input = new CodedInputStream(bytes);

        byte[] secret = [];
        string name = "";
        string issuer = "";
        int algorithm = 0;
        int digits = 0;
        int type = 0;
        long counter = 0;

        uint tag;

        while ((tag = input.ReadTag()) != 0)
        {
            switch (tag)
            {
                // bytes secret = 1;
                case 10:
                    secret = input.ReadBytes().ToByteArray();
                    break;

                // string name = 2;
                case 18:
                    name = input.ReadString();
                    break;

                // string issuer = 3;
                case 26:
                    issuer = input.ReadString();
                    break;

                // Algorithm algorithm = 4;
                case 32:
                    algorithm = input.ReadEnum();
                    break;

                // DigitCount digits = 5;
                case 40:
                    digits = input.ReadEnum();
                    break;

                // OtpType type = 6;
                case 48:
                    type = input.ReadEnum();
                    break;

                // int64 counter = 7;
                case 56:
                    counter = input.ReadInt64();
                    break;

                default:
                    // Includes newer fields such as uniqueId.
                    input.SkipLastField();
                    break;
            }
        }

        return new GoogleAuthenticatorEntry
        {
            Name = name,
            Issuer = issuer,
            Secret = Base32Encoding.ToString(secret),
            Algorithm = algorithm,
            Digits = digits,
            Type = type,
            Counter = counter
        };
    }

    private static string? ExtractQueryValue(string uri, string key)
    {
        var questionMark = uri.IndexOf('?');

        if (questionMark < 0 || questionMark >= uri.Length - 1)
            return null;

        var query = uri[(questionMark + 1)..];

        foreach (var item in query.Split('&'))
        {
            var parts = item.Split('=', 2);

            if (parts.Length == 2 &&
                parts[0].Equals(key, StringComparison.OrdinalIgnoreCase))
            {
                return parts[1];
            }
        }

        return null;
    }
}