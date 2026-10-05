# Installer files

Copy this `installer` folder into the root of the WindowsXIV-Authenticator repository.

Expected layout:

```text
WindowsXIV-Authenticator/
├─ WindowsXIVAuthenticator.slnx
├─ WindowsXIVAuthenticator/
├─ README.md
├─ LICENSE
├─ docs/
│  ├─ SETUP.md
│  └─ SECURITY.md
└─ installer/
   ├─ WindowsXIVAuthenticator.iss
   ├─ build-release.ps1
   └─ assets/
      ├─ xivauthenticator.ico
      ├─ xivlaunch.ico
      ├─ xivauthenticator.png
      └─ xivlaunch.png
```

Install Inno Setup 6, then from the repository root run:

```powershell
.\installer\build-release.ps1 -Version 0.1.0
```

The script automatically:

1. Publishes a self-contained Windows x64 Release build.
2. Creates a portable ZIP.
3. Compiles the Inno Setup installer.
4. Generates SHA-256 hashes for the installer and portable ZIP.
5. Recalculates the hashes and fails if verification does not match.

Output is written to:

```text
dist\
```

The installer creates these shortcuts:

- `XIV Authenticator`
- `Launch XIV`

`Launch XIV` invokes the same executable with `--launch`.
