using System.Drawing;
using System.Windows.Media.Imaging;
using ZXing.Windows.Compatibility;

namespace WindowsXIVAuthenticator.Services;

public static class QrImportService
{
    public static string? DecodeQrFromImage(string filePath)
    {
        using var bitmap =
            (Bitmap)Image.FromFile(filePath);

        return DecodeBitmap(bitmap);
    }

    public static string? DecodeQrFromClipboard()
    {
        if (!Clipboard.ContainsImage())
            return null;

        var clipboardImage =
            Clipboard.GetImage();

        if (clipboardImage is null)
            return null;

        var encoder =
            new PngBitmapEncoder();

        encoder.Frames.Add(
            BitmapFrame.Create(clipboardImage));

        using var stream =
            new MemoryStream();

        encoder.Save(stream);

        stream.Position = 0;

        using var bitmap =
            new Bitmap(stream);

        return DecodeBitmap(bitmap);
    }

    private static string? DecodeBitmap(
        Bitmap bitmap)
    {
        var reader =
            new BarcodeReader();

        var result =
            reader.Decode(bitmap);

        return result?.Text;
    }
}