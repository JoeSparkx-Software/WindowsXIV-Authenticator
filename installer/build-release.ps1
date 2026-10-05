param(
    [Parameter(Mandatory = $false)]
    [ValidatePattern('^\d+\.\d+\.\d+([\-+][0-9A-Za-z\.-]+)?$')]
    [string]$Version = "0.1.0"
)

$ErrorActionPreference = "Stop"

$RepoRoot = Split-Path -Parent $PSScriptRoot
$Solution = Join-Path $RepoRoot "WindowsXIVAuthenticator.slnx"
$Project = Join-Path $RepoRoot "WindowsXIVAuthenticator\WindowsXIVAuthenticator.csproj"
$PublishDir = Join-Path $RepoRoot "publish\win-x64"
$DistDir = Join-Path $RepoRoot "dist"
$InstallerScript = Join-Path $PSScriptRoot "WindowsXIVAuthenticator.iss"

Write-Host "Building Windows XIV Authenticator $Version..." -ForegroundColor Cyan

if (Test-Path $PublishDir) {
    Remove-Item $PublishDir -Recurse -Force
}

if (Test-Path $DistDir) {
    Remove-Item $DistDir -Recurse -Force
}

New-Item -ItemType Directory -Force -Path $PublishDir | Out-Null
New-Item -ItemType Directory -Force -Path $DistDir | Out-Null

dotnet restore $Solution
if ($LASTEXITCODE -ne 0) { throw "dotnet restore failed." }

dotnet publish $Project `
    -c Release `
    -r win-x64 `
    --self-contained true `
    -p:Version=$Version `
    -o $PublishDir

if ($LASTEXITCODE -ne 0) { throw "dotnet publish failed." }

# Create a portable ZIP from the exact published files.
$PortableZip = Join-Path $DistDir "WindowsXIVAuthenticator-Portable-$Version.zip"
Compress-Archive -Path (Join-Path $PublishDir "*") -DestinationPath $PortableZip -CompressionLevel Optimal

# Find Inno Setup compiler.
$IsccCandidates = @(
    "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe",
    "${env:ProgramFiles}\Inno Setup 6\ISCC.exe"
)

$Iscc = $IsccCandidates | Where-Object { Test-Path $_ } | Select-Object -First 1

if (-not $Iscc) {
    throw "Inno Setup 6 compiler (ISCC.exe) was not found. Install Inno Setup 6 and run this script again."
}

& $Iscc "/DAppVersion=$Version" $InstallerScript
if ($LASTEXITCODE -ne 0) { throw "Inno Setup compilation failed." }

$Installer = Join-Path $DistDir "WindowsXIVAuthenticator-Setup-$Version.exe"

if (-not (Test-Path $Installer)) {
    throw "Expected installer was not created: $Installer"
}

# Generate and verify SHA-256 checksums automatically.
$ReleaseFiles = @(
    $Installer,
    $PortableZip
)

$ChecksumPath = Join-Path $DistDir "SHA256SUMS.txt"

$ChecksumLines = foreach ($File in $ReleaseFiles) {
    $Hash = Get-FileHash -Path $File -Algorithm SHA256
    "$($Hash.Hash.ToLowerInvariant())  $([System.IO.Path]::GetFileName($File))"
}

$ChecksumLines | Set-Content -Path $ChecksumPath -Encoding ascii

Write-Host ""
Write-Host "Verifying SHA-256 checksums..." -ForegroundColor Cyan

foreach ($File in $ReleaseFiles) {
    $ExpectedLine = $ChecksumLines | Where-Object {
        $_ -like "*  $([System.IO.Path]::GetFileName($File))"
    }

    $Expected = ($ExpectedLine -split '\s+')[0]
    $Actual = (Get-FileHash -Path $File -Algorithm SHA256).Hash.ToLowerInvariant()

    if ($Expected -ne $Actual) {
        throw "SHA-256 verification failed for $File"
    }

    Write-Host "OK  $([System.IO.Path]::GetFileName($File))"
}

Write-Host ""
Write-Host "Release package complete:" -ForegroundColor Green
Write-Host "  $Installer"
Write-Host "  $PortableZip"
Write-Host "  $ChecksumPath"
