# Windows XIV Authenticator — Security

Windows XIV Authenticator is intentionally designed with a small security and network surface.

This document describes how authenticator secrets are stored, where network communication occurs, how releases are verified, and what data the application does and does not transmit.

## Authenticator secret storage

The TOTP secret is stored locally on the Windows PC.

Windows XIV Authenticator encrypts the secret using the Windows Data Protection API (DPAPI) with:

```text
DataProtectionScope.CurrentUser
```

This means the encrypted authenticator secret is tied to the Windows user account that created it.

The stored configuration does not contain the plaintext TOTP secret.

Application data is stored under:

```text
%LOCALAPPDATA%\WindowsXIVAuthenticator
```

Users should still treat access to their Windows account as security-sensitive.

## QR code handling

Authenticator QR codes contain security-sensitive data.

A QR code used to configure a TOTP authenticator may contain the underlying secret required to generate future OTP codes.

Windows XIV Authenticator supports importing:

- standard `otpauth://` QR codes
- Google Authenticator migration/export QR codes

Users should:

- avoid uploading QR screenshots to public or third-party services
- delete temporary QR screenshots after import when no longer required
- never commit QR images or exported secrets to source control
- treat exported authenticator QR codes like passwords

## Google Authenticator exports

Google Authenticator migration QR codes may contain multiple authenticator accounts.

Windows XIV Authenticator decodes the migration payload locally and allows the user to select the account they want to import.

Importing an account does not remove or invalidate the copy stored in Google Authenticator.

## OTP generation

OTP generation occurs locally.

Windows XIV Authenticator uses standard time-based one-time password generation:

- SHA-1
- 6 digits
- 30-second interval

The authenticator secret does not need to leave the machine in order to generate OTP codes.

## XIVLauncher communication

Windows XIV Authenticator communicates with XIVLauncher using XIVLauncher's local OTP listener.

The current OTP is sent to:

```text
http://127.0.0.1:4646/ffxivlauncher/<OTP>
```

`127.0.0.1` is the local loopback interface.

This communication does not leave the PC.

The authenticator secret itself is not sent to XIVLauncher.

Only the current six-digit OTP is submitted.

## External network access

The application is intended to have no general-purpose external web access.

External network activity is limited to GitHub for project-related functionality such as:

- opening the project repository in the user's default browser
- future release/update checks
- future release/security-patch delivery

The canonical project repository is:

```text
https://github.com/JoeSparkx/WindowsXIV-Authenticator
```

Update functionality should only trust release information associated with this repository.

The application is not intended to contact arbitrary remote services.

## No telemetry

Windows XIV Authenticator does not intentionally provide:

- telemetry
- analytics
- advertising
- remote logging
- cloud OTP generation
- cloud secret storage
- account tracking

## Process launching

Windows XIV Authenticator starts XIVLauncher directly from the configured executable path.

The application does not intentionally invoke:

- `cmd.exe`
- PowerShell
- arbitrary command strings
- user-supplied command-line scripts

The XIVLauncher executable path is either:

- detected from the normal local installation location, or
- selected explicitly by the user in Settings

External hyperlinks are restricted to approved project destinations.

## Release integrity and SHA-256 checksums

Official Windows XIV Authenticator release assets are intended to be accompanied by SHA-256 checksums.

Checksum generation is part of the release packaging workflow and should be performed automatically rather than manually.

For each release, the packaging process should:

1. Build the application.
2. Build the installer and any portable release archive.
3. Generate a SHA-256 hash for each distributable release asset.
4. Write the hashes to a `SHA256SUMS.txt` file.
5. Verify the generated hashes before publication.
6. Publish the installer/archive and checksum file together in the same GitHub Release.

An example `SHA256SUMS.txt` file may look like:

```text
<sha256-hash>  WindowsXIVAuthenticator-Setup-1.0.0.exe
<sha256-hash>  WindowsXIVAuthenticator-Portable-1.0.0.zip
```

Users can verify a downloaded release on Windows with PowerShell:

```powershell
Get-FileHash .\WindowsXIVAuthenticator-Setup-1.0.0.exe -Algorithm SHA256
```

The resulting hash should exactly match the value published in `SHA256SUMS.txt` for that file.

SHA-256 checksums provide integrity verification against the checksum published with the release. They help users confirm that a downloaded file has not been altered or corrupted.

Checksums alone do not prove publisher identity if both the download and published checksum are obtained from an untrusted or compromised source. Code signing may be added later to provide stronger publisher-authenticity assurances.

## Update trust model

The JSS Software GitHub repository is the canonical release and update source.

The intended trust source is:

```text
JoeSparkx/WindowsXIV-Authenticator
```

Automatic update functionality should:

- use only the canonical GitHub repository as its release source
- reject arbitrary or user-supplied update URLs
- require HTTPS
- validate that release metadata belongs to `JoeSparkx/WindowsXIV-Authenticator`
- use published SHA-256 hashes to verify downloaded release assets before installation
- fail safely if checksum verification does not match

The updater should never execute or install an update whose calculated SHA-256 hash differs from the hash published for that release.

## Threat model and limitations

Windows XIV Authenticator protects the stored secret from casual plaintext disclosure, but it cannot fully protect a secret from software already running with access to the user's Windows session.

Examples include:

- malware running as the current user
- process memory inspection
- compromise of the Windows account
- compromise of the PC itself
- screenshots or exported QR codes retained elsewhere

DPAPI should therefore be viewed as local secret protection, not as a substitute for endpoint security.

## Lost or compromised secret

If you believe your TOTP secret has been exposed:

1. Disable or remove the affected software authenticator through the Square Enix account management process.
2. Configure a new authenticator secret.
3. Re-import the new authenticator into Windows XIV Authenticator.
4. Remove any copies of the old QR code or secret.

## Reporting security issues

Security issues should be reported through the project repository:

```text
https://github.com/JoeSparkx/WindowsXIV-Authenticator
```

Do not include real authenticator secrets, QR codes, passwords, recovery codes, or account credentials in public issue reports.

## Attribution

Windows XIV Authenticator was originally created by Adam Crickett / JSS Software.

Copyright (c) 2026 JSS Software.

The project is distributed under the MIT Licence. Please retain the original copyright and licence notice when modifying, redistributing, or adapting the project.
