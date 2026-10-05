using WindowsXIVAuthenticator.Services;

namespace WindowsXIVAuthenticator;

public partial class MainWindow : Window
{
    private string? _activeSecret;
    private readonly DispatcherTimer _otpTimer;

    public MainWindow()
    {
        InitializeComponent();

        _otpTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };

        _otpTimer.Tick += OtpTimer_Tick;

        LoadSavedAuthenticator();

        _otpTimer.Start();
    }

    private void Hyperlink_RequestNavigate(
        object sender,
        RequestNavigateEventArgs e)
    {
        if (!string.Equals(
                e.Uri.AbsoluteUri,
                AppConstants.GitHubRepositoryUrl,
                StringComparison.OrdinalIgnoreCase))
        {
            MessageBox.Show(
                "This link is not permitted.",
                "Blocked link",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            e.Handled = true;
            return;
        }

        Process.Start(new ProcessStartInfo
        {
            FileName = AppConstants.GitHubRepositoryUrl,
            UseShellExecute = true
        });

        e.Handled = true;
    }

    private async void SendToLauncher_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_activeSecret))
        {
            MessageBox.Show(
                "No authenticator account is loaded.",
                "No OTP available",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            return;
        }

        try
        {
            var otp = TotpService.GenerateCode(_activeSecret);

            await XivLauncherService.SendOtpAsync(otp);

            MessageBox.Show(
                "OTP sent to XIVLauncher.",
                "Success",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        catch (HttpRequestException)
        {
            MessageBox.Show(
                "Could not connect to XIVLauncher.\n\n" +
                "Make sure XIVLauncher is open and " +
                "\"Enable XL Authenticator app/OTP macro support\" is enabled.",
                "XIVLauncher not available",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"The OTP could not be sent.\n\n{ex.Message}",
                "Send failed",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private async void LaunchXiv_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_activeSecret))
        {
            MessageBox.Show(
                "No authenticator account is loaded.",
                "No OTP available",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            return;
        }

        try
        {
            var otp =
                TotpService.GenerateCode(_activeSecret);

            await XivLauncherService.LaunchAndSendOtpAsync(otp);

            MessageBox.Show(
                "XIVLauncher started and OTP sent.",
                "Success",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        catch (FileNotFoundException ex)
        {
            MessageBox.Show(
                $"XIVLauncher could not be found.\n\n{ex.FileName}",
                "Launcher not found",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
        catch (TimeoutException ex)
        {
            MessageBox.Show(
                ex.Message,
                "XIVLauncher timeout",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"XIVLauncher could not be started.\n\n{ex.Message}",
                "Launch failed",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void UpdateCountdownVisual(int remaining)
    {
        CountdownTextBlock.Text = remaining.ToString();

        const double canvasSize = 60;
        const double strokeThickness = 4;
        const double maxSeconds = 30.0;

        var progress =
            Math.Clamp(remaining / maxSeconds, 0.0, 1.0);

        if (progress <= 0)
        {
            CountdownPath.Data = null;
            return;
        }

        if (progress >= 1.0)
            progress = 0.9999;

        var radius =
            (canvasSize - strokeThickness) / 2;

        var center =
            canvasSize / 2;

        const double startAngle = -90.0;

        var endAngle =
            startAngle + (progress * 359.999);

        var startPoint =
            PointOnCircle(
                center,
                radius,
                startAngle);

        var endPoint =
            PointOnCircle(
                center,
                radius,
                endAngle);

        var figure = new PathFigure
        {
            StartPoint = startPoint,
            IsClosed = false,
            IsFilled = false
        };

        figure.Segments.Add(
            new ArcSegment
            {
                Point = endPoint,
                Size = new Size(radius, radius),
                SweepDirection =
                    SweepDirection.Clockwise,
                IsLargeArc = progress > 0.5
            });

        var geometry =
            new PathGeometry();

        geometry.Figures.Add(figure);

        CountdownPath.Data = geometry;
    }

    private static Point PointOnCircle(
        double center,
        double radius,
        double angleDegrees)
    {
        var angleRadians =
            angleDegrees * Math.PI / 180.0;

        return new Point(
            center +
            radius * Math.Cos(angleRadians),
            center +
            radius * Math.Sin(angleRadians));
    }

    private void OtpTimer_Tick(
        object? sender,
        EventArgs e)
    {
        RefreshOtpDisplay();
    }

    private void RefreshOtpDisplay()
    {
        if (string.IsNullOrWhiteSpace(_activeSecret))
        {
            OtpTextBlock.Text = "------";
            RemainingTextBlock.Text = "";
            CountdownTextBlock.Text = "";
            CountdownPath.Data = null;

            return;
        }

        OtpTextBlock.Text =
            TotpService.GenerateCode(_activeSecret);

        var remaining =
            TotpService.GetRemainingSeconds();

        RemainingTextBlock.Text =
            $"{remaining} second{(remaining == 1 ? "" : "s")} remaining";

        UpdateCountdownVisual(remaining);
    }

    private void LoadSavedAuthenticator()
    {
        try
        {
            if (!AuthenticatorStore.Exists())
                return;

            var stored =
                AuthenticatorStore.Load();

            var secret =
                AuthenticatorStore.GetSecret();

            if (stored is null ||
                string.IsNullOrWhiteSpace(secret))
            {
                return;
            }

            _activeSecret = secret;

            SecretTextBox.Text = "";

            Title =
                $"Windows XIV Authenticator — {stored.DisplayName}";

            RefreshOtpDisplay();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"The saved authenticator could not be loaded.\n\n{ex.Message}",
                "Authenticator load failed",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void GenerateOtp_Click(
        object sender,
        RoutedEventArgs e)
    {
        try
        {
            var enteredSecret =
                SecretTextBox.Text.Trim();

            var secret =
                !string.IsNullOrWhiteSpace(enteredSecret)
                    ? enteredSecret
                    : _activeSecret;

            if (string.IsNullOrWhiteSpace(secret))
            {
                MessageBox.Show(
                    "Enter a Base32 secret first.",
                    "No secret",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                return;
            }

            _activeSecret = secret;

            RefreshOtpDisplay();
        }
        catch
        {
            MessageBox.Show(
                "The secret could not be read. Make sure it is a valid Base32 authenticator secret.",
                "Invalid secret",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void ImportQr_Click(
        object sender,
        RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Title =
                "Select authenticator QR image",

            Filter =
                "Image files|*.png;*.jpg;*.jpeg;*.bmp;*.webp|All files|*.*"
        };

        if (dialog.ShowDialog() != true)
            return;

        try
        {
            var qrText =
                QrImportService.DecodeQrFromImage(
                    dialog.FileName);

            if (string.IsNullOrWhiteSpace(qrText))
            {
                MessageBox.Show(
                    "No QR code could be detected in that image.",
                    "QR not found",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            if (qrText.StartsWith(
                    "otpauth-migration://",
                    StringComparison.OrdinalIgnoreCase))
            {
                HandleGoogleAuthenticatorImport(
                    qrText);

                return;
            }

            HandleStandardOtpImport(qrText);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"The image could not be imported.\n\n{ex.Message}",
                "Import failed",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void HandleGoogleAuthenticatorImport(
        string qrText)
    {
        var googleImport =
            GoogleAuthenticatorImportService.Decode(
                qrText);

        if (googleImport.Entries.Count == 0)
        {
            MessageBox.Show(
                "The Google Authenticator export contained no accounts.",
                "Nothing to import",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            return;
        }

        GoogleAuthenticatorEntry selectedEntry;

        if (googleImport.Entries.Count == 1)
        {
            selectedEntry =
                googleImport.Entries[0];
        }
        else
        {
            var picker =
                new GoogleImportWindow(
                    googleImport.Entries)
                {
                    Owner = this
                };

            if (picker.ShowDialog() != true ||
                picker.SelectedEntry is null)
            {
                return;
            }

            selectedEntry =
                picker.SelectedEntry;
        }

        ImportAuthenticatorEntry(
            selectedEntry.Secret,
            selectedEntry.DisplayName);
    }

    private void HandleStandardOtpImport(
        string qrText)
    {
        var secret =
            OtpUriService.ExtractSecret(qrText);

        if (string.IsNullOrWhiteSpace(secret))
        {
            MessageBox.Show(
                "A QR code was found, but it is not a supported TOTP QR code.",
                "Unsupported QR",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            return;
        }

        ImportAuthenticatorEntry(
            secret,
            "Authenticator account");
    }

    private void ImportAuthenticatorEntry(
        string secret,
        string displayName)
    {
        AuthenticatorStore.Save(
            secret,
            displayName);

        _activeSecret = secret;

        SecretTextBox.Text = "";

        Title =
            $"Windows XIV Authenticator — {displayName}";

        RefreshOtpDisplay();

        MessageBox.Show(
            $"Imported and securely saved:\n\n{displayName}",
            "Authenticator import",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }
    private void Settings_Click(
    object sender,
    RoutedEventArgs e)
    {
        var settingsWindow = new SettingsWindow
        {
            Owner = this
        };

        settingsWindow.ShowDialog();
    }
}