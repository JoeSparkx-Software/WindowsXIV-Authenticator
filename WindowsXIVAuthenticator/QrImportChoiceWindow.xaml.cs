namespace WindowsXIVAuthenticator;

public partial class QrImportChoiceWindow : Window
{
    public bool ImportFromClipboard { get; private set; }

    public QrImportChoiceWindow()
    {
        InitializeComponent();
    }

    private void Clipboard_Click(
        object sender,
        RoutedEventArgs e)
    {
        ImportFromClipboard = true;
        DialogResult = true;
    }

    private void File_Click(
        object sender,
        RoutedEventArgs e)
    {
        ImportFromClipboard = false;
        DialogResult = true;
    }
}