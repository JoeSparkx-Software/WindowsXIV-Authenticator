using System.Drawing;
using ZXing.Windows.Compatibility;

namespace WindowsXIVAuthenticator.Services;

public static class QrImportService
{
    public static string? DecodeQrFromImage(string filePath)
    {
        using var bitmap = (Bitmap)Image.FromFile(filePath);

        var reader = new BarcodeReader();

        var result = reader.Decode(bitmap);

        return result?.Text;
    }
}