﻿using WindowsXIVAuthenticator.Services;

namespace WindowsXIVAuthenticator;

public partial class App : Application
{
    protected override async void OnStartup(
        StartupEventArgs e)
    {
        base.OnStartup(e);

        try
        {
            if (AuthenticatorMigrationService.MigrationRequired())
            {
                await AuthenticatorMigrationService.MigrateAsync();

                MessageBox.Show(
                    "Authenticator successfully migrated to the v2 TPM-backed vault.",
                    "Authenticator migration",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Authenticator migration failed.\n\n{ex.Message}",
                "Authenticator migration",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            Shutdown();
            return;
        }

        if (e.Args.Any(arg =>
                arg.Equals(
                    "--launch",
                    StringComparison.OrdinalIgnoreCase)))
        {
            await RunLaunchModeAsync();
            Shutdown();
            return;
        }

        var mainWindow =
            new MainWindow();

        mainWindow.Show();
    }

    private static async Task RunLaunchModeAsync()
    {
        try
        {
            if (!AuthenticatorVaultStore.Exists())
            {
                MessageBox.Show(
                    "No authenticator account is configured.\n\n" +
                    "Open Windows XIV Authenticator normally first and import your authenticator.",
                    "Authenticator not configured",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            var verified =
                await WindowsHelloService.VerifyAsync(
                    "Verify your identity to launch Final Fantasy XIV");

            if (!verified)
                return;

            var secret =
                await AuthenticatorVaultStore.GetSecretAsync();

            if (string.IsNullOrWhiteSpace(secret))
            {
                MessageBox.Show(
                    "The saved authenticator secret could not be loaded.",
                    "Authenticator unavailable",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                return;
            }

            await XivLauncherService.LaunchAndSendOtpAsync(
                () => TotpService.GenerateCode(
                    secret));
        }
        catch (FileNotFoundException)
        {
            MessageBox.Show(
                "XIVLauncher could not be found.\n\n" +
                "Open Windows XIV Authenticator normally and configure the launcher path in Settings.",
                "XIVLauncher not found",
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
                $"FFXIV could not be launched.\n\n{ex.Message}",
                "Launch failed",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }
}