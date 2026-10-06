using WindowsXIVAuthenticator.Services;

namespace WindowsXIVAuthenticator;

public partial class SettingsWindow : Window
{
    public SettingsWindow()
    {
        InitializeComponent();

        LoadCurrentPath();
        LoadWindowsHelloSetting();
    }

    private void LoadWindowsHelloSetting()
    {
        RequireWindowsHelloCheckBox.IsChecked =
            AppSettingsService.GetRequireWindowsHello();
    }

    private void RequireWindowsHello_Checked(
        object sender,
        RoutedEventArgs e)
    {
        AppSettingsService.SetRequireWindowsHello(true);
    }

    private void RequireWindowsHello_Unchecked(
        object sender,
        RoutedEventArgs e)
    {
        AppSettingsService.SetRequireWindowsHello(false);
    }

    private void LoadCurrentPath()
    {
        var path =
            AppSettingsService.GetLauncherPath();

        if (!string.IsNullOrWhiteSpace(path))
        {
            LauncherPathTextBox.Text = path;

            StatusTextBlock.Text =
                "XIVLauncher found.";
        }
        else
        {
            LauncherPathTextBox.Text = "";

            StatusTextBlock.Text =
                "XIVLauncher could not be found automatically.";
        }
    }

    private void Browse_Click(
        object sender,
        RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Title = "Locate XIVLauncher.exe",
            Filter =
                "XIVLauncher|XIVLauncher.exe|Executable files|*.exe"
        };

        if (dialog.ShowDialog() != true)
            return;

        try
        {
            AppSettingsService.SetLauncherPath(
                dialog.FileName);

            LoadCurrentPath();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                ex.Message,
                "Invalid launcher path",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void UseDefault_Click(
        object sender,
        RoutedEventArgs e)
    {
        AppSettingsService.ResetLauncherPath();

        LoadCurrentPath();

        if (string.IsNullOrWhiteSpace(
                LauncherPathTextBox.Text))
        {
            MessageBox.Show(
                "XIVLauncher was not found in the default location.",
                "Default location not found",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
    }

    private void Close_Click(
        object sender,
        RoutedEventArgs e)
    {
        Close();
    }
}