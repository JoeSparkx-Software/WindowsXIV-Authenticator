using WindowsXIVAuthenticator.Services;

namespace WindowsXIVAuthenticator;



public partial class GoogleImportWindow : Window
{
    public GoogleAuthenticatorEntry? SelectedEntry { get; private set; }

    public GoogleImportWindow(
        IEnumerable<GoogleAuthenticatorEntry> entries)
    {
        InitializeComponent();

        AccountsListBox.ItemsSource = entries.ToList();

        if (AccountsListBox.Items.Count > 0)
            AccountsListBox.SelectedIndex = 0;
    }

    private void Import_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (AccountsListBox.SelectedItem
            is not GoogleAuthenticatorEntry entry)
        {
            MessageBox.Show(
                "Select an account first.",
                "No account selected",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );

            return;
        }

        SelectedEntry = entry;
        DialogResult = true;
    }

    private void Cancel_Click(
        object sender,
        RoutedEventArgs e)
    {
        DialogResult = false;
    }
}