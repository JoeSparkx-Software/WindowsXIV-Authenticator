# Windows XIV Authenticator — Setup Guide

This guide explains how to configure Windows XIV Authenticator and connect it to XIVLauncher.

## 1. Install and open Windows XIV Authenticator

Launch **XIV Authenticator** from the Start Menu or desktop shortcut.

The main window provides access to:

- authenticator import
- the current 6-digit OTP
- the 30-second countdown timer
- XIVLauncher controls
- application settings

## 2. Import your authenticator

Windows XIV Authenticator is intended to act as an additional authenticator.

Importing your account does not remove it from Google Authenticator or another authenticator application.

### Google Authenticator

On your phone:

1. Open Google Authenticator.
2. Open the account transfer/export function.
3. Select the Square Enix / Final Fantasy XIV account.
4. Display the export QR code.
5. Take a screenshot or otherwise transfer the QR image securely to your PC.

In Windows XIV Authenticator:

1. Click **Import QR Image**.
2. Select the QR image.
3. If several accounts are contained in the export, select the Square Enix account.
4. Confirm the generated OTP matches the OTP shown on your phone.

After confirming the import, securely delete any temporary screenshot containing the QR code.

### Standard authenticator QR

Windows XIV Authenticator can also import standard `otpauth://` QR codes.

Click **Import QR Image** and select the QR image.

## 3. Verify the OTP

After import, the main window will show:

- the current 6-digit OTP
- a circular countdown timer
- the remaining lifetime of the current OTP

The OTP automatically changes every 30 seconds.

Before relying on the Windows copy, compare the displayed code with your existing authenticator.

## 4. Configure XIVLauncher

Open XIVLauncher.

In XIVLauncher settings, enable:

**Enable XL Authenticator app/OTP macro support**

Save the setting.

If this option was newly enabled, restart XIVLauncher before testing.

## 5. Configure the XIVLauncher path

Windows XIV Authenticator automatically checks the normal XIVLauncher location:

```text
%LOCALAPPDATA%\XIVLauncher\XIVLauncher.exe
```

If XIVLauncher is installed elsewhere:

1. Open **Settings** in Windows XIV Authenticator.
2. Click **Browse...**
3. Locate `XIVLauncher.exe`.
4. Select it.

The path is saved locally for future launches.

## 6. Test sending an OTP

Open XIVLauncher and leave it on the login screen.

In Windows XIV Authenticator, click:

**Send to XIVLauncher**

The current OTP should be passed directly to XIVLauncher.

Communication occurs only over the local machine using:

```text
127.0.0.1:4646
```

If XIVLauncher does not respond:

- confirm OTP macro support is enabled
- restart XIVLauncher
- confirm XIVLauncher is running
- check the configured executable path

## 7. Test one-click launch

Close XIVLauncher completely.

Click:

**Launch XIV**

Windows XIV Authenticator will:

1. Start XIVLauncher
2. Wait for its OTP listener
3. Generate the current OTP
4. Send the OTP to XIVLauncher

## 8. Desktop shortcuts

The installer is intended to create two shortcuts.

### XIV Authenticator

Opens the normal authenticator interface.

Use this shortcut to:

- view the current OTP
- import or update an authenticator
- configure XIVLauncher
- manually send an OTP
- access settings

### Launch XIV

Runs automatic launch mode using:

```text
WindowsXIVAuthenticator.exe --launch
```

This mode:

1. Loads the locally stored authenticator secret
2. Starts XIVLauncher
3. Waits for XIVLauncher's OTP listener
4. Generates the current OTP
5. Sends the OTP to XIVLauncher
6. Exits

## 9. Where data is stored

Application data is stored under:

```text
%LOCALAPPDATA%\WindowsXIVAuthenticator
```

The authenticator secret is stored encrypted using Windows DPAPI.

The plaintext TOTP secret is not stored in the application configuration file.

## 10. Security notes

Authenticator secrets should be treated like passwords.

Do not:

- upload authenticator export QR codes to public services
- commit QR screenshots to GitHub
- share TOTP secrets
- retain unnecessary QR screenshots after importing them

For additional information, see [SECURITY.md](SECURITY.md).

## Troubleshooting

### The OTP does not match my phone

Check that:

- the same authenticator account was imported
- Windows date and time are correct
- automatic time synchronization is enabled

TOTP codes depend on the system clock.

### XIVLauncher says the OTP was not received

Check that:

- XIVLauncher is running
- **Enable XL Authenticator app/OTP macro support** is enabled
- XIVLauncher was restarted after enabling the setting
- the app is configured with the correct XIVLauncher executable

### XIVLauncher cannot be found

Open **Settings**, click **Browse...**, and select `XIVLauncher.exe`.

### I changed Windows user accounts

The authenticator secret is protected with Windows DPAPI using `CurrentUser` scope.

A secret encrypted under one Windows account cannot simply be copied to another account and decrypted there. Import the authenticator again under the new Windows account.
