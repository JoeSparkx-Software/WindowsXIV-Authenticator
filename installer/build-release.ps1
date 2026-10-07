param(
    [Parameter(Mandatory = $false)]
    [ValidatePattern('^\d+\.\d+\.\d+([\-+][0-9A-Za-z\.-]+)?$')]
    [string]$Version = "2.0.0"
)

$ErrorActionPreference = "Stop"

$RepoRoot = Split-Path -Parent $PSScriptRoot

$Solution =
    Join-Path `
        $RepoRoot `
        "WindowsXIVAuthenticator.slnx"

$GuiProject =
    Join-Path `
        $RepoRoot `
        "WindowsXIVAuthenticator\WindowsXIVAuthenticator.csproj"

$CliProject =
    Join-Path `
        $RepoRoot `
        "WindowsXIVAuthenticator.Cli\WindowsXIVAuthenticator.Cli.csproj"

$PublishDir =
    Join-Path `
        $RepoRoot `
        "publish\win-x64"

$CliPublishDir =
    Join-Path `
        $RepoRoot `
        "publish\cli-win-x64"

$DistDir =
    Join-Path `
        $RepoRoot `
        "dist"

$InstallerScript =
    Join-Path `
        $PSScriptRoot `
        "WindowsXIVAuthenticator.iss"

Write-Host "Building Windows XIV Authenticator $Version..." -ForegroundColor Cyan

if (Test-Path $PublishDir) {
    Remove-Item $PublishDir -Recurse -Force
}

if (Test-Path $CliPublishDir) {
    Remove-Item $CliPublishDir -Recurse -Force
}

if (Test-Path $DistDir) {
    Remove-Item $DistDir -Recurse -Force
}

New-Item `
    -ItemType Directory `
    -Force `
    -Path $PublishDir |
    Out-Null

New-Item `
    -ItemType Directory `
    -Force `
    -Path $CliPublishDir |
    Out-Null

New-Item `
    -ItemType Directory `
    -Force `
    -Path $DistDir |
    Out-Null

dotnet restore $Solution

if ($LASTEXITCODE -ne 0) {
    throw "dotnet restore failed."
}

Write-Host ""
Write-Host "Publishing GUI..." -ForegroundColor Cyan

dotnet publish $GuiProject `
    -c Release `
    -r win-x64 `
    --self-contained true `
    -p:Version=$Version `
    -o $PublishDir

if ($LASTEXITCODE -ne 0) {
    throw "GUI publish failed."
}

Write-Host ""
Write-Host "Publishing xiv-auth CLI..." -ForegroundColor Cyan

dotnet publish $CliProject `
    -c Release `
    -r win-x64 `
    --self-contained true `
    -p:Version=$Version `
    -p:PublishSingleFile=true `
    -p:DebugType=None `
    -p:DebugSymbols=false `
    -o $CliPublishDir

if ($LASTEXITCODE -ne 0) {
    throw "CLI publish failed."
}

$CliExecutable =
    Join-Path `
        $CliPublishDir `
        "xiv-auth.exe"

if (-not (Test-Path $CliExecutable)) {
    throw "Expected CLI executable was not created: $CliExecutable"
}

Copy-Item `
    -Path $CliExecutable `
    -Destination $PublishDir `
    -Force

$InstalledCli =
    Join-Path `
        $PublishDir `
        "xiv-auth.exe"

if (-not (Test-Path $InstalledCli)) {
    throw "xiv-auth.exe was not copied into the installer payload."
}

Write-Host ""
Write-Host "CLI added to installer payload:" -ForegroundColor Green
Write-Host "  $InstalledCli"

# Find Inno Setup compiler.
$IsccCandidates = @(
    "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe",
    "${env:ProgramFiles}\Inno Setup 6\ISCC.exe"
)

$Iscc =
    $IsccCandidates |
    Where-Object { Test-Path $_ } |
    Select-Object -First 1

if (-not $Iscc) {
    throw "Inno Setup 6 compiler (ISCC.exe) was not found. Install Inno Setup 6 and run this script again."
}

Write-Host ""
Write-Host "Building installer..." -ForegroundColor Cyan

& $Iscc "/DAppVersion=$Version" $InstallerScript

if ($LASTEXITCODE -ne 0) {
    throw "Inno Setup compilation failed."
}

$Installer =
    Join-Path `
        $DistDir `
        "WindowsXIVAuthenticator-Setup-$Version.exe"

if (-not (Test-Path $Installer)) {
    throw "Expected installer was not created: $Installer"
}

# Generate and verify SHA-256 checksum.
$ChecksumPath =
    Join-Path `
        $DistDir `
        "SHA256SUMS.txt"

$InstallerHash =
    Get-FileHash `
        -Path $Installer `
        -Algorithm SHA256

$ChecksumLine =
    "$($InstallerHash.Hash.ToLowerInvariant())  $([System.IO.Path]::GetFileName($Installer))"

$ChecksumLine |
    Set-Content `
        -Path $ChecksumPath `
        -Encoding ascii

Write-Host ""
Write-Host "Verifying SHA-256 checksum..." -ForegroundColor Cyan

$Expected =
    ($ChecksumLine -split '\s+')[0]

$Actual =
    (Get-FileHash `
        -Path $Installer `
        -Algorithm SHA256).Hash.ToLowerInvariant()

if ($Expected -ne $Actual) {
    throw "SHA-256 verification failed for $Installer"
}

Write-Host "OK  $([System.IO.Path]::GetFileName($Installer))"

Write-Host ""
Write-Host "Release package complete:" -ForegroundColor Green
Write-Host "  $Installer"
Write-Host "  $ChecksumPath"
Write-Host ""
Write-Host "Installer payload includes:" -ForegroundColor Green
Write-Host "  WindowsXIVAuthenticator.exe"
Write-Host "  xiv-auth.exe"