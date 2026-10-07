# Windows XIV Authenticator — Security

Windows XIV Authenticator is designed around a deliberately small local security boundary.

Version 2 replaces the version 1 DPAPI-only storage model with a layered local vault:

```text
TOTP secret
    ↓
AES-256-GCM encrypted payload
    ↓
random 256-bit vault key
    ↓
RSA-OAEP-SHA256 wrapped vault key
    ↓
non-exportable Windows CNG key
    ↓
Microsoft Platform Crypto Provider / TPM-backed protection is required for Windows XIV Authenticator 2.x
```

Windows Hello is used separately as the application-level user verification gate before the application or CLI unlocks the vault.

This document describes that architecture, the threat model, migration behaviour, OTP handling, local integrations, network activity, and release integrity.

---

## 1. Security goals

Windows XIV Authenticator is intended to:

- avoid storing the TOTP secret in plaintext on disk;
- protect the stored secret using authenticated encryption;
- prevent the vault encryption key from being stored directly in `vault.json`;
- use Windows platform key protection for the vault key;
- require Windows Hello verification before normal vault unlock operations;
- keep OTP generation local;
- expose only the current OTP to supported integrations;
- avoid cloud secret storage, telemetry, analytics, or remote logging;
- keep the implementation small enough to audit.

It is not intended to act as a hardened security boundary against arbitrary malicious software already executing as the same Windows user.

## Supported security baseline

Windows XIV Authenticator 2.x intentionally requires a supported Windows 11 environment with a working TPM 2.0.

The v2 vault does not silently fall back to software-backed CNG key storage when the Microsoft Platform Crypto Provider is unavailable.

This is a deliberate security boundary.

Systems that cannot provide a usable TPM 2.0 should use the latest Windows XIV Authenticator 1.0.x compatibility release instead.

The 1.0.x line uses the previous Windows DPAPI-based secret-storage model and does not provide the TPM-backed vault protections described in this document.

---

## 2. Vault location

Version 2 stores its authenticator vault at:

```text
%LOCALAPPDATA%\WindowsXIVAuthenticator\vault.json
```

The vault file contains:

- a vault format version;
- a display name;
- the wrapped AES vault key;
- the AES-GCM nonce;
- the encrypted TOTP secret;
- the AES-GCM authentication tag.

The plaintext TOTP secret is not intentionally written to the vault file.

The vault format is an implementation detail and should not be read or modified directly by integrations.

---

## 3. Secret encryption — AES-256-GCM

The TOTP secret is encrypted using AES in Galois/Counter Mode:

```text
AES-256-GCM
```

For each saved authenticator, Windows XIV Authenticator generates a random:

```text
256-bit / 32-byte vault key
```

The encrypted secret uses:

```text
Nonce: 12 bytes
Authentication tag: 16 bytes
```

AES-GCM provides both:

- confidentiality; and
- integrity/authentication of the encrypted payload.

If the encrypted payload or authentication tag is modified, decryption should fail rather than returning silently corrupted plaintext.

Random cryptographic material is generated using the Windows/.NET cryptographic random-number facilities.

---

## 4. Vault key protection

The 256-bit AES vault key is not stored directly in `vault.json`.

Instead, it is encrypted using an RSA key managed through Windows Cryptography Next Generation (CNG).

The application currently uses:

```text
Provider:
Microsoft Platform Crypto Provider

CNG key name:
WindowsXIVAuthenticator.VaultKey.AppGate.v2

Export policy:
None

Key usage:
Decryption

Vault-key wrapping:
RSA-OAEP-SHA256
```

The RSA key is created as non-exportable.

Where the Microsoft Platform Crypto Provider is backed by a compatible TPM, the private key material is protected by the platform provider rather than being exported into the application.

The wrapped AES key stored in `vault.json` is therefore not sufficient by itself to decrypt the authenticator secret.

Copying `vault.json` to another machine does not copy the associated CNG private key.

---

## 5. Windows Hello

Windows Hello is used as an application-level verification gate.

Before the desktop application or CLI performs a normal vault unlock, it requests user verification through:

```text
Windows.Security.Credentials.UI.UserConsentVerifier
```

Depending on the Windows device and user configuration, this may present:

- Windows Hello PIN;
- fingerprint;
- facial recognition;
- another supported Windows Hello verification method.

The application continues only when Windows reports:

```text
UserConsentVerificationResult.Verified
```

If Windows Hello is unavailable, cancelled, or fails, the requested unlock operation fails.

### Important distinction

Windows Hello and the TPM/CNG vault-key protection are separate parts of the design.

Windows Hello is the **application-level unlock gate**.

The Microsoft Platform Crypto Provider is the **cryptographic protection layer for the vault key**.

The application does not claim that Windows Hello itself directly encrypts the vault contents.

This separation is intentional and avoids forcing users through the full Windows account-password prompt that some higher-protection CNG UI policies can produce.

---

## 6. Unlock flow

A normal vault unlock follows this sequence:

```text
User requests OTP / Send / Launch / CLI code
        ↓
Windows Hello verification
        ↓
Load vault.json
        ↓
Open protected CNG RSA key
        ↓
RSA-OAEP-SHA256 unwraps the 256-bit AES vault key
        ↓
AES-256-GCM decrypts the TOTP secret
        ↓
Generate current TOTP locally
```

The temporary AES vault-key byte array is explicitly zeroed after encryption or decryption operations.

Plaintext byte buffers used during AES operations are also explicitly zeroed after use where the implementation has direct access to those buffers.

---

## 7. In-memory secret lifetime

The desktop application may keep the successfully unlocked TOTP secret in process memory for the lifetime of the current application session.

This avoids repeatedly asking the user for Windows Hello verification for every OTP generation, send, or launch action.

For example:

```text
first Generate
→ Windows Hello
→ vault unlocked

second Generate in same app process
→ no additional Hello prompt
```

Closing and reopening the desktop application clears that process state and requires verification again on the next unlock.

Like any application handling a secret, Windows XIV Authenticator cannot guarantee that a plaintext value that has existed in process memory is impossible to inspect by software with sufficient access to that process or Windows session.

---

## 8. CLI security

Version 2 includes:

```text
xiv-auth.exe
```

The supported command is:

```text
xiv-auth code
```

The CLI:

1. checks for the local vault;
2. requests Windows Hello verification;
3. unlocks the same version 2 vault used by the desktop application;
4. generates the current TOTP locally;
5. writes only the six-digit OTP to standard output.

The underlying TOTP secret is not intentionally written to standard output or standard error.

The CLI does not provide a command for exporting the TOTP secret.

Integrations should use the CLI or another documented interface rather than reading `vault.json` directly.

Successful OTP values should not be logged or retained longer than required for the immediate login operation.

---

## 9. OTP generation

OTP generation occurs locally.

Windows XIV Authenticator currently uses:

```text
Algorithm: SHA-1
Digits:    6
Period:    30 seconds
```

The authenticator secret does not need to leave the machine in order to calculate an OTP.

Consumers should use generated OTPs immediately rather than caching them between login attempts.

---

## 10. XIVLauncher communication

Windows XIV Authenticator supports XIVLauncher's local OTP listener.

The current six-digit OTP is sent to:

```text
http://127.0.0.1:4646/ffxivlauncher/<OTP>
```

`127.0.0.1` is the local loopback interface.

The request is intended to remain on the local PC.

The underlying TOTP secret is not sent to XIVLauncher.

Only the current short-lived OTP is submitted.

---

## 11. QR code handling

Authenticator QR codes contain security-sensitive material.

A standard TOTP QR code can contain the secret required to generate all future OTP values until that authenticator registration is replaced.

Windows XIV Authenticator supports importing:

- standard `otpauth://` QR codes;
- supported Google Authenticator migration/export QR codes.

QR decoding is performed locally.

Users should:

- avoid uploading authenticator QR screenshots to public or third-party services;
- delete temporary QR screenshots after import when no longer required;
- never commit QR images or authenticator secrets to source control;
- treat exported authenticator QR codes like passwords.

---

## 12. Google Authenticator exports

Google Authenticator migration QR codes may contain multiple accounts.

Windows XIV Authenticator decodes the migration payload locally and allows the user to select the account to import.

Importing an account does not remove or invalidate the copy already present in Google Authenticator.

Keeping the same TOTP secret on a phone can provide an independent backup authenticator.

---

## 13. Version 1 to version 2 migration

Version 1 used Windows DPAPI with:

```text
DataProtectionScope.CurrentUser
```

Version 2 retains legacy DPAPI support only for migration.

Migration is performed only when:

```text
the version 1 authenticator exists
AND
the version 2 vault does not already exist
```

The migration process is designed to fail conservatively:

1. read the existing version 1 authenticator metadata;
2. decrypt the version 1 secret using the original DPAPI mechanism;
3. create the version 2 AES-GCM vault;
4. read the newly created version 2 vault back;
5. compare the original and migrated secret values using a fixed-time byte comparison;
6. delete the new vault if verification fails;
7. delete the old version 1 authenticator file only after successful verification.

This avoids deliberately deleting the legacy copy before the new vault has been successfully written and read back.

After successful migration, the version 1 file is no longer required.

---

## 14. Cryptographic memory cleanup

Where the application directly controls sensitive byte arrays, it uses:

```text
CryptographicOperations.ZeroMemory
```

to clear them after use.

Current examples include:

- the random AES vault key after vault save;
- the unwrapped AES vault key after vault read;
- plaintext UTF-8 buffers used during AES operations;
- encrypted temporary key buffers where appropriate;
- byte arrays used during migration verification.

This is a defence-in-depth measure.

Managed strings, framework internals, operating-system components, and copied memory cannot be guaranteed to be synchronously erased by application code.

---

## 15. File-writing behaviour

The version 2 vault is written using a temporary file:

```text
vault.json.tmp
```

The application then replaces/moves it into:

```text
vault.json
```

The temporary file is removed in cleanup if it remains.

This reduces the chance of leaving the primary vault file partially written if the write operation fails.

It is not intended to provide transactional durability against every possible power-loss or filesystem failure scenario.

---

## 16. External network access

Windows XIV Authenticator is designed without general-purpose remote secret storage.

It does not intentionally provide:

- cloud OTP generation;
- cloud secret synchronisation;
- telemetry;
- analytics;
- advertising;
- remote logging;
- JSS account tracking.

Project-related functionality may use or open GitHub resources for operations such as:

- project/repository links;
- release information;
- update checks;
- future security-patch delivery.

The canonical project repository is:

```text
https://github.com/JoeSparkx-Software/WindowsXIV-Authenticator
```

The application should not use arbitrary third-party update sources.

---

## 17. Process launching

Windows XIV Authenticator starts XIVLauncher directly from the configured executable path.

The application is not intended to launch XIVLauncher through:

- `cmd.exe`;
- PowerShell;
- arbitrary user-supplied command strings;
- arbitrary scripts.

The XIVLauncher path is either detected from its normal local installation location or selected by the user.

---

## 18. Release integrity

Official release packaging generates a SHA-256 checksum for the installer.

The current public release format is intended to contain:

```text
WindowsXIVAuthenticator-Setup-<version>.exe
SHA256SUMS.txt
```

The CLI is included inside the installer rather than published as a separate portable package.

The release build process:

1. restores the solution;
2. publishes the desktop application;
3. publishes `xiv-auth.exe`;
4. adds the CLI to the installer payload;
5. builds the Inno Setup installer;
6. calculates the installer's SHA-256 hash;
7. writes that value to `SHA256SUMS.txt`;
8. recalculates and verifies the hash before reporting the release package complete.

Users can verify an installer with PowerShell:

```powershell
Get-FileHash .\WindowsXIVAuthenticator-Setup-2.0.0.exe -Algorithm SHA256
```

The resulting value should match the value published in `SHA256SUMS.txt`.

### Checksum limitation

A SHA-256 checksum verifies that a downloaded file matches the file for which that checksum was produced.

It does not by itself prove publisher identity if an attacker can replace both the installer and the published checksum.

Code signing may provide stronger publisher-authenticity guarantees if added in the future.

---

## 19. Update trust model

The canonical project and release source is:

```text
JoeSparkx-Software/WindowsXIV-Authenticator
```

Any future automatic installer update mechanism should:

- use only the canonical project release source;
- use HTTPS;
- reject arbitrary user-supplied update URLs;
- verify the downloaded installer against the release SHA-256 value;
- fail closed if integrity verification does not match;
- never execute an installer that fails verification.

The current security architecture does not depend on an automatic updater being present.

---

## 20. Threat model

Windows XIV Authenticator is primarily designed to protect against:

- accidental plaintext storage;
- somebody copying `vault.json` and expecting to recover the secret directly;
- casual access by another person using the PC without successfully passing the application's Windows Hello gate;
- corruption or tampering with the AES-GCM encrypted payload;
- accidental disclosure through application configuration;
- unnecessary remote transmission of the authenticator secret.

The security design deliberately raises the cost of obtaining the stored TOTP secret from disk.

---

## 21. Threats outside the intended boundary

Windows XIV Authenticator does **not** claim to fully defend against an attacker who already has arbitrary code execution with the same privileges as the logged-in Windows user.

Examples include:

- malware running as the current user;
- code injection into the authenticator process;
- process-memory inspection;
- API hooking;
- compromised Windows sessions;
- administrative or kernel-level compromise;
- malicious accessibility/input tooling;
- screenshots or QR exports retained elsewhere;
- an attacker controlling the system before or after Windows Hello verification.

The Windows Hello check is an application-level gate, not a magical isolation boundary against same-user malware.

Once a legitimate process has decrypted a secret in order to calculate an OTP, sufficiently privileged hostile software on that endpoint may be able to observe that process or its outputs.

The project therefore does not claim to offer the same threat model as a dedicated hardware authenticator.

---

## 22. Why there is no broker or system service

A stronger architecture could place secret handling in a separate privileged broker or service and expose only narrow IPC operations.

Windows XIV Authenticator 2.0 intentionally does not do this.

The application is designed for an FFXIV TOTP use case rather than financial, cryptocurrency, or other high-value authentication infrastructure.

A permanent broker/service would significantly increase:

- code size;
- installation complexity;
- IPC complexity;
- privilege-boundary complexity;
- maintenance burden;
- audit surface.

For this project, the current layered local vault plus Windows Hello gate provides the intended balance between usability, security, and maintainability.

---

## 23. Backup and recovery

The encrypted vault is deliberately tied to local Windows cryptographic protection.

Users should not assume that copying:

```text
vault.json
```

to another PC is a usable backup.

A safer recovery approach is to retain the same TOTP registration on another trusted authenticator, such as a phone, or retain the account provider's official recovery mechanism.

If Windows must be reinstalled, the user changes to another PC, or the local platform key becomes unavailable, the authenticator may need to be imported again.

---

## 24. Lost or compromised secret

If the underlying TOTP secret or QR code is believed to have been exposed:

1. remove or replace the affected software authenticator through the Square Enix account-management process;
2. create a new authenticator registration/secret;
3. update trusted authenticators with the new registration;
4. import the new secret into Windows XIV Authenticator;
5. securely remove obsolete QR images or exported secrets.

Deleting an application file does not revoke a TOTP secret that has already been copied by an attacker.

The authenticator registration itself must be replaced.

---

## 25. Reporting security issues

Security issues should be reported through the official project repository:

```text
https://github.com/JoeSparkx-Software/WindowsXIV-Authenticator
```

Do not include real:

- TOTP secrets;
- authenticator QR codes;
- passwords;
- recovery codes;
- Square Enix credentials;
- Windows credentials

in public issue reports.

A reproduction should use synthetic/test secrets wherever possible.

---

## 26. Source and licence

The source code is published so the security implementation can be reviewed and audited.

Windows XIV Authenticator 2.0 is intended to use the project's custom source-available licence rather than the former MIT licence.

Refer to the repository `LICENSE` file for the current legally applicable terms.

Security research and source review should not require exposing a real user's authenticator secret.
