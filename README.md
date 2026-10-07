# Windows XIV Authenticator
<p align="center">
  <img
    src="https://github.com/JoeSparkx-Software/WindowsXIV-Authenticator/blob/main/WindowsXIVAuthenticator/assets/JSSAuthenticatorpic.png?raw=true"
    alt="Windows XIV Authenticator"
    width="420"
  />
</p>

Windows XIV Authenticator is a free Windows desktop authenticator designed for Final Fantasy XIV and XIVLauncher.

It provides a simple local TOTP workflow without requiring a paid password manager, cloud account, subscription, or separate online service.

The project is built around one principle:

**make the normal path easy enough that people cannot accidentally make a mess of it.**

## System requirements

Windows XIV Authenticator 2.x requires:

- Windows 11 x64
- TPM 2.0 enabled and available
- Windows Hello configured for the current Windows account

Version 2.x requires the Microsoft Platform Crypto Provider and does not fall back to software-backed key storage.

Unsupported Windows 11 installations without a usable TPM 2.0 should use the latest Windows XIV Authenticator 1.0.x release instead.

Version 1.0.x is the legacy compatibility line and uses the older Windows DPAPI-based storage model rather than the TPM-backed v2 vault.

## What it does

Windows XIV Authenticator can:

- import a standard TOTP QR code;
- import supported Google Authenticator exports;
- generate the current six-digit OTP;
- send the OTP directly to XIVLauncher;
- launch XIVLauncher and send a fresh OTP automatically;
- protect the authenticator vault using Windows security;
- use Windows Hello PIN, fingerprint, or face verification before unlocking the authenticator;
- provide a local CLI for launcher and integration use.

## Security model

Version 2 uses a local encrypted vault stored under:

`%LOCALAPPDATA%\WindowsXIVAuthenticator\vault.json`

The TOTP secret is encrypted using AES-256-GCM.

The AES vault key is protected by a non-exportable Windows CNG key using the Microsoft Platform Crypto Provider, allowing TPM-backed protection where supported by the user's system.

Windows Hello is used as the application-level unlock gate, giving users the familiar PIN, fingerprint, or face verification flow.

Once successfully unlocked, the authenticator secret may remain available in the application's memory for the lifetime of that process so the user is not repeatedly prompted during the same session.

This is intentionally a practical desktop application security model.

Windows XIV Authenticator is designed to protect secrets at rest and prevent casual access to the authenticator. It is not intended to provide a hardened security boundary against malware already running with the same Windows user privileges.

If an attacker already has arbitrary code execution as your Windows account, you have significantly larger security problems than an FFXIV OTP.

## Keep your phone as a backup

You do not need to choose between Windows XIV Authenticator and your phone.

TOTP allows the same secret to exist on more than one authenticator.

A sensible setup is:

1. keep the authenticator on your phone;
2. import the same TOTP secret into Windows XIV Authenticator;
3. use the desktop version for convenience;
4. keep the phone as an independent backup.

Windows XIV Authenticator is not intended to become your only recovery path.

## Local only

Windows XIV Authenticator does not require:

- a cloud account;
- a JSS account;
- a subscription;
- synchronisation through JSS servers;
- remote secret storage.

The authenticator vault remains on the local Windows user profile.

## XIVLauncher support

Windows XIV Authenticator can send a generated OTP directly to XIVLauncher when XIVLauncher's authenticator app / OTP macro support is enabled.

The application also includes a **Launch XIV** workflow that starts XIVLauncher, waits for it to become ready, generates a fresh OTP, and sends it automatically.

## CLI

Version 2 includes a small command-line client:

`xiv-auth.exe`

Current command:

```text
xiv-auth code
```

Expected behaviour:

- Windows Hello is requested;
- the local vault is unlocked;
- a fresh six-digit OTP is generated;
- the OTP alone is written to standard output;
- success returns exit code `0`;
- failures return a non-zero exit code and an error message on standard error.

Example:

```text
C:\> xiv-auth code
391902
```

The CLI exists primarily to provide a simple, stable integration path for launcher and provider development.

## Provider integrations

The planned XIVLauncher integration uses a generic provider model.

Windows XIV Authenticator will provide the free default implementation.

Other developers are welcome to build independent adapters for other services using the documented provider interface or CLI.

For example, somebody could choose to implement their own adapter for:

- Bitwarden;
- 1Password;
- KeePassXC;
- Proton;
- another authenticator or password manager;
- a completely custom provider.

JSS Software does not intend to build or maintain every third-party adapter.

The project will provide the interface and a working Windows XIV Authenticator implementation. Developers who want another provider can build and maintain their own adapter.

Independent provider integrations are specifically permitted by the project licence provided they do not redistribute or modify Windows XIV Authenticator itself without permission.

## Installation

Download the latest official installer from:

https://github.com/JoeSparkx-Software/WindowsXIV-Authenticator/releases

Public releases use the installer package rather than a portable ZIP to keep installation and upgrades predictable.

The default installation location is:

`%LOCALAPPDATA%\Programs\JSS Software\XIV Authenticator`

Application data is stored separately under:

`%LOCALAPPDATA%\WindowsXIVAuthenticator`

Uninstalling or upgrading the application should not automatically delete the user's authenticator vault.

## Updating from version 1

Version 1 stored the authenticator using Windows DPAPI.

Version 2 migrates the existing authenticator into the new encrypted vault.

The migration process:

1. detects the existing version 1 authenticator;
2. decrypts it using the original DPAPI mechanism;
3. creates the version 2 AES-encrypted vault;
4. reads the new vault back;
5. verifies that the migrated secret matches the original;
6. deletes the old version 1 authenticator file only after successful verification.

## Building

Requirements include:

- Windows;
- .NET 10 SDK;
- Inno Setup 6 for installer creation.

Build the solution:

```powershell
dotnet build .\WindowsXIVAuthenticator.slnx
```

Build the release installer:

```powershell
Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass
.\installer\build-release.ps1 -Version 2.0.0
```

Release output is written to:

`dist\`

The release builder generates:

- `WindowsXIVAuthenticator-Setup-<version>.exe`
- `SHA256SUMS.txt`

The installer includes both:

- `WindowsXIVAuthenticator.exe`
- `xiv-auth.exe`

## Project structure

```text
WindowsXIV-Authenticator
├── WindowsXIVAuthenticator
│   └── WPF desktop application
├── WindowsXIVAuthenticator.Core
│   └── shared vault, security and TOTP functionality
├── WindowsXIVAuthenticator.Cli
│   └── xiv-auth command-line client
├── installer
│   └── release and Inno Setup tooling
└── docs
```

## Licence

Windows XIV Authenticator is **source available**, not OSI open source.

You are welcome to use the official application free of charge.

You are also welcome to inspect and review the source.

If you want to modify, fork, repackage, redistribute, or commercially use Windows XIV Authenticator itself, ask first.

Independent provider DLLs, adapters, plugins, and integrations using the documented interface are permitted without prior approval, provided they do not contain or redistribute modified Windows XIV Authenticator code or binaries.

See `LICENSE` for the complete terms.

## Disclaimer

Windows XIV Authenticator is an independent community project.

It is not affiliated with, endorsed by, or sponsored by Square Enix, XIVLauncher, Dalamud, Goatcorp, Microsoft, or any password-manager/authenticator provider.

Final Fantasy XIV and related names and trademarks belong to their respective owners.

## Links

Repository:

https://github.com/JoeSparkx-Software/WindowsXIV-Authenticator

XIV Mods:

https://xivmods.com/

Support development:

https://ko-fi.com/joesparkx
