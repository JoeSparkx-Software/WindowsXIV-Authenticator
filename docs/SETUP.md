# Windows XIV Authenticator — Setup Guide

This guide explains how to install, configure, and use Windows XIV Authenticator 2.0 with XIVLauncher.

Windows XIV Authenticator is designed to act as an additional local authenticator. You can keep the same TOTP account on your phone as a backup while using the Windows application for convenience.

## Supported systems

Windows XIV Authenticator 2.x is intended for supported Windows 11 systems with:

- Windows 11 x64
- TPM 2.0 enabled and available
- Windows Hello configured for the current Windows account

Windows XIV Authenticator 2.x requires Windows 11 x64 and a working TPM 2.0. The application requires the Microsoft Platform Crypto Provider and does not fall back to software-backed key storage.

Unsupported Windows 11 installations without a usable TPM 2.0 should use the legacy Windows XIV Authenticator 1.0.x release.

If your PC does not meet the v2 hardware requirements, install the latest Windows XIV Authenticator 1.0.x release instead.

Version 1.0.x remains available as the legacy compatibility release and uses Windows DPAPI-based storage. It does not provide the TPM-backed vault used by version 2.x.

---

## 1. Install Windows XIV Authenticator

Download the latest official installer from:

```text
https://github.com/JoeSparkx-Software/WindowsXIV-Authenticator/releases
```

Run:

```text
WindowsXIVAuthenticator-Setup-2.0.0.exe
```

The default installation location is:

```text
%LOCALAPPDATA%\Programs\JSS Software\XIV Authenticator
```

The installer includes:

```text
WindowsXIVAuthenticator.exe
xiv-auth.exe
```

The installer does not intentionally remove your authenticator vault when upgrading or uninstalling the application.

Application data is stored separately under:

```text
%LOCALAPPDATA%\WindowsXIVAuthenticator
```

---

## 2. Open Windows XIV Authenticator

Launch **XIV Authenticator** from the Start Menu or desktop shortcut.

The main window provides access to:

- authenticator import;
- current six-digit OTP generation;
- the 30-second countdown;
- sending the current OTP to XIVLauncher;
- launching XIVLauncher and sending a fresh OTP;
- application settings.

Version 2 uses Windows Hello as the normal unlock gate for the authenticator vault.

Depending on your Windows configuration, this may use:

- Windows Hello PIN;
- fingerprint;
- facial recognition;
- another supported Windows Hello method.

---

## 3. Import your authenticator

Windows XIV Authenticator is intended to act as an additional authenticator.

Importing your account does not remove it from Google Authenticator or another authenticator application.

Keeping the same authenticator on your phone is recommended as an independent backup.

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

---

## 4. What happens after import

Version 2 stores the authenticator in a local encrypted vault:

```text
%LOCALAPPDATA%\WindowsXIVAuthenticator\vault.json
```

The TOTP secret is protected using AES-256-GCM with a random 256-bit vault key.

That vault key is wrapped using a non-exportable Windows CNG key through the Microsoft Platform Crypto Provider.

Windows Hello is used as the application-level unlock gate.

The plaintext TOTP secret is not intentionally stored in `vault.json`.

For the full architecture and threat model, see `docs\SECURITY.md`.

---

## 5. Verify the OTP

After importing the authenticator, generate an OTP and compare it with the code shown by your existing authenticator.

The code should match if both devices have the same TOTP registration and correct system time.

Windows XIV Authenticator uses:

```text
SHA-1
6 digits
30-second period
```

If the OTP does not match your phone, check Windows date and time synchronisation before relying on the Windows copy.

---

## 6. Windows Hello behaviour

When the application first needs to unlock the authenticator in a new process, Windows Hello is requested.

Example:

```text
Open app
→ Generate OTP
→ Windows Hello prompt
→ OTP displayed
```

After successful verification, the desktop application may keep the authenticator available in memory for the lifetime of that process.

Additional Generate, Send, or Launch actions during the same application session normally do not repeatedly prompt for Windows Hello.

Closing and reopening the application requires verification again on the next unlock.

---

## 7. Configure XIVLauncher

Open XIVLauncher.

In XIVLauncher settings, enable:

**Enable XL Authenticator app/OTP macro support**

Save the setting.

If this option was newly enabled, restart XIVLauncher before testing.

Windows XIV Authenticator currently uses XIVLauncher's local OTP listener:

```text
http://127.0.0.1:4646/ffxivlauncher/<OTP>
```

This communication remains on the local PC.

Only the current six-digit OTP is sent. The underlying TOTP secret is not sent to XIVLauncher.

---

## 8. Configure the XIVLauncher path

Windows XIV Authenticator automatically checks the normal XIVLauncher location:

```text
%LOCALAPPDATA%\XIVLauncher\XIVLauncher.exe
```

If XIVLauncher is installed elsewhere:

1. Open **Settings** in Windows XIV Authenticator.
2. Click **Browse...**
3. Locate `XIVLauncher.exe`.
4. Select it.

The selected path is saved locally for future launches.

---

## 9. Test sending an OTP

Open XIVLauncher and leave it on the login screen.

In Windows XIV Authenticator, click **Send to XIVLauncher**.

If the authenticator has not yet been unlocked in the current application process:

1. Windows Hello will appear.
2. Verify your identity.
3. The vault will be unlocked.
4. The current OTP will be sent to XIVLauncher.

If XIVLauncher does not respond:

- confirm OTP macro support is enabled;
- restart XIVLauncher;
- confirm XIVLauncher is running;
- check the configured XIVLauncher executable path.

---

## 10. Test one-click launch

Close XIVLauncher completely.

Click **Launch XIV**.

Windows XIV Authenticator will:

1. request Windows Hello if the vault is not already unlocked in the current process;
2. start XIVLauncher;
3. wait for XIVLauncher's local OTP listener;
4. generate the current OTP;
5. send the OTP to XIVLauncher.

The application does not send the underlying authenticator secret to XIVLauncher.

---

## 11. Desktop shortcuts

The installer creates two application shortcuts.

### XIV Authenticator

Opens the normal desktop interface.

Use this shortcut to:

- import an authenticator;
- generate/view the current OTP;
- configure XIVLauncher;
- send an OTP manually;
- launch XIVLauncher;
- access settings.

### Launch XIV

Runs automatic launch mode using:

```text
WindowsXIVAuthenticator.exe --launch
```

This mode:

1. checks that the version 2 vault exists;
2. requests Windows Hello;
3. unlocks the local vault;
4. starts XIVLauncher;
5. waits for XIVLauncher's OTP listener;
6. generates the current OTP;
7. sends the OTP;
8. exits.

---

## 12. Command-line interface

Windows XIV Authenticator 2.0 also installs:

```text
xiv-auth.exe
```

The current command is:

```text
xiv-auth code
```

Example:

```powershell
& "$env:LOCALAPPDATA\Programs\JSS Software\XIV Authenticator\xiv-auth.exe" code
```

On success:

1. Windows Hello is requested.
2. The vault is unlocked.
3. A fresh OTP is generated.
4. The terminal receives only the six-digit OTP.

Example:

```text
391902
```

The CLI is primarily intended for launcher, provider, plugin, and local integration use.

For the full CLI contract, exit codes, and integration examples, see `docs\CLI.md`.

---

## 13. Version 1 upgrade

Version 1 stored the authenticator using Windows DPAPI.

Version 2 automatically migrates the old authenticator when:

```text
a version 1 authenticator exists
AND
a version 2 vault does not already exist
```

The migration process:

1. reads the existing version 1 authenticator;
2. decrypts it using the original DPAPI mechanism;
3. creates the new version 2 vault;
4. reads the new vault back;
5. verifies the migrated secret matches the original;
6. deletes the old version 1 authenticator file only after successful verification.

After a successful migration, normal use continues through the version 2 vault.

---

## 14. Where data is stored

### Application files

Default installation path:

```text
%LOCALAPPDATA%\Programs\JSS Software\XIV Authenticator
```

### Application data

Stored under:

```text
%LOCALAPPDATA%\WindowsXIVAuthenticator
```

The main version 2 vault is:

```text
%LOCALAPPDATA%\WindowsXIVAuthenticator\vault.json
```

Do not manually edit `vault.json`.

Do not assume that copying `vault.json` to another PC is a usable backup.

The protected vault key depends on Windows cryptographic key material on the local system.

---

## 15. Backup and recovery

Keep another trusted authenticator copy where practical.

For example:

```text
Phone authenticator
+
Windows XIV Authenticator
```

Both can generate the same OTP when they contain the same TOTP secret.

This gives you an independent recovery route if:

- Windows is reinstalled;
- the PC fails;
- the local platform key is lost;
- the Windows vault becomes unavailable.

Windows XIV Authenticator should not be treated as your only account-recovery mechanism.

---

## 16. Security notes

Authenticator secrets and QR codes should be treated like passwords.

Do not:

- upload authenticator QR codes to public services;
- post QR screenshots in Discord or forums;
- commit QR images or secrets to GitHub;
- retain unnecessary export screenshots;
- manually edit the vault;
- log OTP values generated through the CLI.

Windows XIV Authenticator protects secrets at rest and adds a Windows Hello application gate.

It is not intended to defend against arbitrary malware already running with the same privileges as your Windows account.

For full details, see `docs\SECURITY.md`.

---

# Troubleshooting

## Windows Hello does not appear

Windows Hello must be available for the current Windows user.

Check **Windows Settings → Accounts → Sign-in options** and confirm a supported Windows Hello method is configured.

## Windows Hello verification was cancelled or failed

Retry the action and complete the Windows Hello prompt.

If the prompt repeatedly fails, verify Windows Hello works elsewhere in Windows.

## The OTP does not match my phone

Check that:

- the same authenticator account was imported;
- Windows date and time are correct;
- automatic time synchronisation is enabled;
- your phone time is also correct.

TOTP depends on accurate time.

## XIVLauncher says the OTP was not received

Check that:

- XIVLauncher is running;
- **Enable XL Authenticator app/OTP macro support** is enabled;
- XIVLauncher was restarted after enabling the setting;
- the application is configured with the correct XIVLauncher executable.

## XIVLauncher cannot be found

Open **Settings**, click **Browse...**, and select `XIVLauncher.exe`.

## `xiv-auth.exe` cannot be found

The CLI is installed alongside the desktop application.

Default path:

```text
%LOCALAPPDATA%\Programs\JSS Software\XIV Authenticator\xiv-auth.exe
```

## The CLI says no authenticator is configured

Open the desktop application and import an authenticator first.

The CLI uses the same local version 2 vault as the desktop application.

## I changed Windows user accounts or PCs

The version 2 vault depends on Windows cryptographic key material on the original system/user environment.

Copying `vault.json` alone is not intended to make the authenticator portable.

Import the authenticator again under the new Windows environment.

## I uninstalled the application

The installer is designed not to intentionally remove:

```text
%LOCALAPPDATA%\WindowsXIVAuthenticator
```

so the vault can survive reinstall/upgrade operations.

If you are permanently removing the application and want to remove the local authenticator data as well, delete that directory only after ensuring you still have another working authenticator or recovery method.

---

## More information

Security architecture:

```text
docs\SECURITY.md
```

CLI integration:

```text
docs\CLI.md
```

Official releases:

```text
https://github.com/JoeSparkx-Software/WindowsXIV-Authenticator/releases
```

Project repository:

```text
https://github.com/JoeSparkx-Software/WindowsXIV-Authenticator
```
