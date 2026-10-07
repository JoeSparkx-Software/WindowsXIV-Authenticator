# Windows XIV Authenticator CLI

`xiv-auth.exe` is the command-line interface included with Windows XIV Authenticator 2.0.

It is intended primarily for launcher, plugin, provider, and automation integrations that need to request the current OTP from the locally configured Windows XIV Authenticator vault.

The CLI uses the same vault, Windows Hello gate, TPM-backed key protection, AES-encrypted secret, and TOTP implementation as the desktop application.

## Installation

`xiv-auth.exe` is installed alongside the main application by the standard Windows XIV Authenticator installer.

Default installation location:

```text
%LOCALAPPDATA%\Programs\JSS Software\XIV Authenticator
```

Default executable path:

```text
%LOCALAPPDATA%\Programs\JSS Software\XIV Authenticator\xiv-auth.exe
```

The CLI is not currently added to the system `PATH`.

Call it using its full path or from the installation directory.

## Current command

Version 2.0 exposes one command:

```text
xiv-auth code
```

This requests the current six-digit TOTP code for the authenticator configured in Windows XIV Authenticator.

Windows XIV Authenticator currently stores one authenticator account, so no account name or identifier is required.

Example:

```powershell
& "$env:LOCALAPPDATA\Programs\JSS Software\XIV Authenticator\xiv-auth.exe" code
```

Successful output:

```text
391902
```

## Behaviour

When `xiv-auth code` is called:

1. The CLI checks that a Windows XIV Authenticator vault exists.
2. Windows Hello requests user verification.
3. The local vault is unlocked.
4. The authenticator secret is decrypted.
5. A fresh six-digit TOTP is generated.
6. The OTP is written to standard output.
7. The process exits with code `0`.

The TOTP secret itself is never written to standard output.

## Standard output

On success, `stdout` contains only the six-digit OTP followed by a normal line ending.

Example:

```text
391902
```

Integrations should treat any other standard output as invalid.

A consumer should validate the returned value against:

```regex
^\d{6}$
```

Do not parse human-readable messages from standard output.

## Standard error

Failures are written to `stderr`.

Examples include:

```text
Usage: xiv-auth code
```

```text
No authenticator is configured.
```

```text
Windows Hello verification was cancelled or failed.
```

```text
The authenticator secret could not be loaded.
```

```text
The generated OTP was invalid.
```

Unexpected failures use:

```text
xiv-auth failed: <message>
```

Consumers should primarily use the process exit code rather than depending on the exact wording of an error message.

Human-readable error text may change between versions.

## Exit codes

| Exit code | Meaning |
| ---: | --- |
| `0` | Success. A six-digit OTP was written to standard output. |
| `2` | Invalid command or arguments. |
| `3` | No authenticator vault is configured. |
| `4` | Windows Hello verification was cancelled or failed. |
| `5` | The authenticator secret could not be loaded. |
| `6` | OTP generation returned an invalid value. |
| `10` | Unexpected or unclassified failure. |

Exit codes other than `0` must be treated as failure.

Future versions may define additional non-zero exit codes.

## Integration contract

For integrations such as XIVLauncher provider DLLs, the recommended flow is:

```text
start xiv-auth.exe code
        ↓
wait for process completion
        ↓
exit code == 0?
        ↓
read stdout
        ↓
trim line ending
        ↓
validate exactly six digits
        ↓
use OTP
```

If the exit code is non-zero:

```text
read stderr
        ↓
show/log an appropriate failure state
        ↓
fall back to manual OTP entry where supported
```

Integrations should not:

- attempt to read the authenticator vault directly;
- attempt to obtain or store the TOTP secret;
- attempt to bypass Windows Hello;
- parse or depend on the internal `vault.json` format;
- assume the encrypted vault format will remain unchanged;
- log successful OTP values;
- retain returned OTP values longer than necessary.

## Example: PowerShell

```powershell
$Cli = Join-Path `
    $env:LOCALAPPDATA `
    "Programs\JSS Software\XIV Authenticator\xiv-auth.exe"

$Otp = & $Cli code

if ($LASTEXITCODE -ne 0) {
    throw "xiv-auth failed with exit code $LASTEXITCODE"
}

if ($Otp -notmatch '^\d{6}$') {
    throw "xiv-auth returned an invalid OTP"
}

$Otp
```

## Example: .NET

```csharp
using System.Diagnostics;
using System.Text.RegularExpressions;

var cliPath =
    Path.Combine(
        Environment.GetFolderPath(
            Environment.SpecialFolder.LocalApplicationData),
        "Programs",
        "JSS Software",
        "XIV Authenticator",
        "xiv-auth.exe");

var startInfo =
    new ProcessStartInfo
    {
        FileName = cliPath,
        UseShellExecute = false,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        CreateNoWindow = true
    };

startInfo.ArgumentList.Add("code");

using var process =
    new Process
    {
        StartInfo = startInfo
    };

process.Start();

var standardOutput =
    await process.StandardOutput.ReadToEndAsync();

var standardError =
    await process.StandardError.ReadToEndAsync();

await process.WaitForExitAsync();

if (process.ExitCode != 0)
{
    throw new InvalidOperationException(
        $"xiv-auth failed with exit code {process.ExitCode}: {standardError.Trim()}");
}

var otp =
    standardOutput.Trim();

if (!Regex.IsMatch(
        otp,
        @"^\d{6}$"))
{
    throw new InvalidOperationException(
        "xiv-auth returned an invalid OTP.");
}

// Use the OTP immediately.
// Do not log it.
```

## Windows Hello

The CLI deliberately invokes Windows Hello before requesting the authenticator secret.

Depending on the user's Windows configuration, verification may use:

- Windows Hello PIN;
- fingerprint;
- facial recognition;
- another Windows Hello method supported by the device.

If verification is cancelled or fails, the CLI returns exit code `4` and does not return an OTP.

## Vault location

The CLI uses the same vault as the desktop application:

```text
%LOCALAPPDATA%\WindowsXIVAuthenticator\vault.json
```

The vault is not intended to be accessed directly by integrations.

The file format is an implementation detail and may change in future versions.

Use the CLI rather than reading or modifying `vault.json`.

## Security model

Windows XIV Authenticator 2.0 uses:

- AES-256-GCM encryption for the stored TOTP secret;
- a random vault encryption key;
- a non-exportable Windows CNG key using the Microsoft Platform Crypto Provider to protect the vault key;
- Windows Hello as the application-level user verification gate.

The CLI shares this security implementation through `WindowsXIVAuthenticator.Core`.

The CLI returns the current OTP only.

It does not provide a command to return the underlying TOTP secret.

## OTP lifetime

The generated code follows the configured TOTP implementation used by Windows XIV Authenticator:

```text
SHA-1
6 digits
30-second period
```

Consumers should use the returned OTP immediately.

Do not cache OTP values between login attempts.

A future integration may add timing metadata or a fresher-code workflow if required, but this is not part of the 2.0 CLI contract.

## Compatibility

The CLI is currently intended for:

```text
Windows x64
.NET 10 self-contained release
```

The official installer contains the supported CLI build.

## Provider development

The CLI is intended to provide the reference integration path for Windows XIV Authenticator.

The planned XIVLauncher provider model will allow other developers to create their own independent provider DLLs for other services.

Those providers do not need to use `xiv-auth.exe`.

A provider may implement its own backend or call another service's supported API/CLI, provided it follows the relevant XIVLauncher provider contract.

JSS Software intends to maintain the Windows XIV Authenticator provider.

Third-party service adapters are expected to be maintained by their respective developers.

## Stability

For the 2.x series, integrations may rely on:

```text
xiv-auth code
```

returning:

- exit code `0` on success;
- one six-digit OTP on standard output;
- no secret or additional success text on standard output;
- a non-zero exit code on failure.

New commands may be added in future releases without changing the existing `code` command contract.

If a breaking CLI change ever becomes necessary, it should be introduced through a new major version rather than silently changing the 2.x behaviour.

## Troubleshooting

### `xiv-auth.exe` cannot be found

Confirm Windows XIV Authenticator was installed using the official installer.

Default path:

```text
%LOCALAPPDATA%\Programs\JSS Software\XIV Authenticator\xiv-auth.exe
```

### No authenticator is configured

Open the Windows XIV Authenticator desktop application and import the authenticator first.

### Windows Hello verification failed

Confirm Windows Hello is configured and working for the current Windows account.

Try unlocking the desktop application normally.

### The vault cannot be loaded

Open the desktop application and verify that the configured authenticator still works.

Do not manually edit `vault.json`.

### The OTP is rejected

Confirm:

- Windows date and time are correct;
- automatic time synchronisation is enabled;
- the correct authenticator was imported;
- the OTP was used immediately rather than cached.

TOTP depends on accurate system time.
