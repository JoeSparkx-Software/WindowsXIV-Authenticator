# Windows XIV Authenticator
<p align="center">
  <img
    src="https://github.com/JoeSparkx-Software/WindowsXIV-Authenticator/blob/main/WindowsXIVAuthenticator/assets/JSSAuthenticatorpic.png?raw=true"
    alt="Windows XIV Authenticator"
    width="420"
  />
</p>
A lightweight Windows authenticator for Final Fantasy XIV and XIVLauncher.

Built by **JSS Software** https://xivmods.com

Windows XIV Authenticator stores your FFXIV TOTP secret locally, generates the same 6-digit OTP codes as standard authenticator apps, and can send the current OTP directly to XIVLauncher.

It is designed for people who want a convenient Windows-based authenticator while still keeping their existing authenticator on their phone.

## Features

- Generate standard 6-digit TOTP codes
- Import authenticator accounts from QR code images
- Import Google Authenticator export QR codes
- Select individual accounts from multi-account Google Authenticator exports
- Store authenticator secrets securely using Windows DPAPI
- Automatically refresh OTP codes every 30 seconds
- Visual countdown timer
- Send OTP codes directly to XIVLauncher
- Launch XIVLauncher and automatically submit the current OTP
- Configurable XIVLauncher executable location
- Designed for desktop shortcuts and one-click launching
- No telemetry
- No analytics
- No cloud storage
- No remote OTP generation

## Desktop shortcuts

The installer is intended to create two shortcuts:

### XIV Authenticator

Opens the normal Windows XIV Authenticator interface.

Use this to:

- view the current OTP
- import or update an authenticator
- configure XIVLauncher
- manually send an OTP
- access application settings

### Launch XIV

Runs Windows XIV Authenticator in automatic launch mode:

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

## Security model

Authenticator secrets are stored locally on the Windows PC.

Secrets are encrypted using the Windows Data Protection API (DPAPI) with `CurrentUser` scope. The stored encrypted value is therefore tied to the Windows user account that created it.

Windows XIV Authenticator does not upload or transmit authenticator secrets.

Network activity is intentionally limited to:

- `127.0.0.1:4646` for local communication with XIVLauncher
- GitHub for project links and future release/update checks

The application does not use telemetry, analytics, advertising, remote logging, or cloud-based OTP generation.

Treat authenticator QR codes, exported authenticator images, and TOTP secrets like passwords. Anyone who obtains the underlying secret can generate valid OTP codes.

## XIVLauncher integration

Windows XIV Authenticator uses XIVLauncher's supported OTP macro listener.

XIVLauncher must have:

**Enable XL Authenticator app/OTP macro support**

enabled in its settings.

The OTP is sent locally to:

```text
http://127.0.0.1:4646/ffxivlauncher/<OTP>
```

No external service is involved in this process.

## Setup

See the full setup guide:

[Setup Guide](docs/SETUP.md)

## Requirements

- Windows 10 or Windows 11
- XIVLauncher
- A configured Final Fantasy XIV software authenticator / TOTP account

The current development version targets .NET 10.

## Supported imports

Currently supported:

- Standard `otpauth://` QR codes
- Google Authenticator export QR codes
- PNG
- JPG / JPEG
- BMP
- WebP

Planned:

- Webcam QR scanning
- Clipboard image import

## XIVLauncher path

Windows XIV Authenticator automatically checks the normal XIVLauncher location:

```text
%LOCALAPPDATA%\XIVLauncher\XIVLauncher.exe
```

If XIVLauncher is installed elsewhere, its location can be configured in **Settings**.

## Local data

Application data is stored under:

```text
%LOCALAPPDATA%\WindowsXIVAuthenticator
```

The authenticator secret is stored encrypted using Windows DPAPI.

The plaintext TOTP secret is not stored in the application configuration file.

## Building from source

Clone the repository:

```powershell
git clone https://github.com/JoeSparkx/WindowsXIV-Authenticator.git
cd WindowsXIV-Authenticator
```

Build:

```powershell
dotnet build WindowsXIVAuthenticator.slnx
```

Run the normal interface:

```powershell
dotnet run --project .\WindowsXIVAuthenticator
```

Test automatic launch mode:

```powershell
dotnet run --project .\WindowsXIVAuthenticator -- --launch
```

## Third-party components

This project uses open-source libraries including:

- Otp.NET
- ZXing.Net
- Google.Protobuf

See the project file and NuGet metadata for current dependency versions and licences.

## Project status

Windows XIV Authenticator is currently under active development.

Installer packages and public releases will be added once setup documentation, security review, packaging, and release testing are complete.

## Disclaimer

Windows XIV Authenticator is an unofficial community project.

It is not affiliated with, endorsed by, or supported by Square Enix, Final Fantasy XIV, XIVLauncher, or Goatcorp.

FINAL FANTASY XIV and related names and trademarks belong to their respective owners.

## Licence

Windows XIV Authenticator is released under the MIT Licence.

## JSS Software

Developed by **JSS Software**.

Repository:

https://github.com/JoeSparkx/WindowsXIV-Authenticator

Copyright (c) 2026 JSS Software

## Other JSS Software projects

### XIV Mods

**XIV Mods** is a curated catalogue of Final Fantasy XIV Dalamud plugins, repositories, guides and community resources.

https://xivmods.com

Discord:

https://discord.xivmods.com

GitHub:

https://github.com/JoeSparkx

XIV Mods is a separate community project and is not required to use Windows XIV Authenticator.

## Attribution

This project was originally created by Adam Crickett / JSS Software.

You are welcome to modify, fork, redistribute, and adapt the project under the terms of the MIT Licence. Please retain the original copyright and licence notice.
