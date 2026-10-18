<#
.SYNOPSIS
    Packages the Rashtra: Sovereign Republic desktop release for Windows x64.
.PARAMETER Configuration
    Build configuration (default: "Release").
.PARAMETER Version
    Release version string (default: "1.0.0").
.EXAMPLE
    .\package.ps1
#>

[CmdletBinding()]
param (
    [string]$Configuration = "Release",
    [string]$Version = "1.0.0"
)

$ErrorActionPreference = "Stop"

$RootDir = $PSScriptRoot
$DistRoot = Join-Path $RootDir "dist"
$PackageName = "Rashtra-v$Version-win-x64"
$PackageDir = Join-Path $DistRoot $PackageName
$ZipPath = Join-Path $DistRoot "$PackageName.zip"

Write-Host "==================================================" -ForegroundColor Cyan
Write-Host " Rashtra: Sovereign Republic - Windows Packaging" -ForegroundColor Cyan
Write-Host " Version: $Version | Configuration: $Configuration" -ForegroundColor Cyan
Write-Host " Target: $PackageDir" -ForegroundColor Cyan
Write-Host "==================================================" -ForegroundColor Cyan

# 1. Clean previous build artifact
if (Test-Path $PackageDir) {
    Write-Host "`n[1/6] Cleaning previous distribution directory..." -ForegroundColor Yellow
    Remove-Item -Path $PackageDir -Recurse -Force
}
if (Test-Path $ZipPath) {
    Remove-Item -Path $ZipPath -Force
}
New-Item -ItemType Directory -Path $PackageDir -Force | Out-Null

# 2. Build & Test Verification
Write-Host "`n[2/6] Running Build, Test & Content Validation Gates..." -ForegroundColor Yellow
& "$RootDir\build.ps1" -Configuration $Configuration
if ($LASTEXITCODE -ne 0) {
    Write-Error "Build verification gates failed with exit code $LASTEXITCODE"
}

# 3. Publish Binaries
Write-Host "`n[3/6] Publishing Binaries (win-x64)..." -ForegroundColor Yellow
$BinDir = Join-Path $PackageDir "bin"
dotnet publish "$RootDir\src\WorldState.Domain\WorldState.Domain.csproj" `
    -c $Configuration `
    -r win-x64 `
    --self-contained false `
    -o $BinDir
if ($LASTEXITCODE -ne 0) {
    Write-Error "Dotnet publish failed with exit code $LASTEXITCODE"
}

# Also publish ContentValidator utility
$ToolsDir = Join-Path $PackageDir "tools\ContentValidator"
dotnet publish "$RootDir\tools\ContentValidator\ContentValidator.csproj" `
    -c $Configuration `
    -r win-x64 `
    --self-contained false `
    -o $ToolsDir
if ($LASTEXITCODE -ne 0) {
    Write-Error "ContentValidator publish failed with exit code $LASTEXITCODE"
}

# 4. Copy Game Content Data
Write-Host "`n[4/6] Bundling Game Content Data..." -ForegroundColor Yellow
$ContentDest = Join-Path $PackageDir "content"
Copy-Item -Path "$RootDir\content" -Destination $ContentDest -Recurse -Force

# 5. Copy Licenses and Documentation
Write-Host "`n[5/6] Bundling Licenses, Guides, and Documentation..." -ForegroundColor Yellow
$DocsDest = Join-Path $PackageDir "docs"
New-Item -ItemType Directory -Path $DocsDest -Force | Out-Null

$FilesToCopy = @(
    "LICENSE",
    "README.md",
    "docs\legal\ASSET-AND-DATA-PROVENANCE.md",
    "docs\guides\CRASH-AND-DIAGNOSTICS-GUIDE.md",
    "docs\guides\DESKTOP-CONTROLS-GUIDE.md"
)

foreach ($file in $FilesToCopy) {
    $srcPath = Join-Path $RootDir $file
    if (Test-Path $srcPath) {
        Copy-Item -Path $srcPath -Destination $DocsDest -Force
    }
}

if (Test-Path "$RootDir\RELEASE-NOTES-v1.0.0.md") {
    Copy-Item -Path "$RootDir\RELEASE-NOTES-v1.0.0.md" -Destination $PackageDir -Force
}

# 6. Generate Checksums and Zip Archive
Write-Host "`n[6/6] Generating SHA256 Checksums and Compressing Distribution..." -ForegroundColor Yellow

$ChecksumFile = Join-Path $PackageDir "SHA256SUMS.txt"
$AllFiles = Get-ChildItem -Path $PackageDir -Recurse -File | Where-Object { $_.FullName -ne $ChecksumFile }

$ChecksumLines = foreach ($f in $AllFiles) {
    $hash = (Get-FileHash -Path $f.FullName -Algorithm SHA256).Hash.ToLower()
    $relPath = $f.FullName.Substring($PackageDir.Length + 1).Replace("\", "/")
    "$hash  $relPath"
}
$ChecksumLines | Set-Content -Path $ChecksumFile -Encoding utf8

# Create zip archive
Compress-Archive -Path "$PackageDir\*" -DestinationPath $ZipPath -CompressionLevel Optimal

$ZipItem = Get-Item $ZipPath
$ZipHash = (Get-FileHash -Path $ZipPath -Algorithm SHA256).Hash

Write-Host "`n==================================================" -ForegroundColor Green
Write-Host " Windows Desktop Release Packaging COMPLETE!" -ForegroundColor Green
Write-Host " Output Folder: $PackageDir" -ForegroundColor Green
Write-Host " Zip Archive:   $ZipPath ($([math]::Round($ZipItem.Length / 1MB, 2)) MB)" -ForegroundColor Green
Write-Host " Zip SHA256:    $ZipHash" -ForegroundColor Green
Write-Host " Total Files:   $($AllFiles.Count)" -ForegroundColor Green
Write-Host "==================================================" -ForegroundColor Green

exit 0
