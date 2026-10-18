<#
.SYNOPSIS
    Builds, tests, and validates the Rashtra solution from clean checkout.
.EXAMPLE
    .\build.ps1
#>

[CmdletBinding()]
param (
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host " Rashtra: Sovereign Republic - Build & Test" -ForegroundColor Cyan
Write-Host " Configuration: $Configuration" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

# 1. Restore
Write-Host "`n[1/4] Restoring .NET Dependencies..." -ForegroundColor Yellow
dotnet restore src/WorldState.sln
if ($LASTEXITCODE -ne 0) {
    Write-Error "Restore failed with exit code $LASTEXITCODE"
}

# 2. Build
Write-Host "`n[2/4] Compiling Solution ($Configuration)..." -ForegroundColor Yellow
dotnet build src/WorldState.sln -c $Configuration --no-restore /p:TreatWarningsAsErrors=true
if ($LASTEXITCODE -ne 0) {
    Write-Error "Build failed with exit code $LASTEXITCODE"
}

# 3. Test
Write-Host "`n[3/4] Running Domain Automated Tests..." -ForegroundColor Yellow
dotnet test src/WorldState.sln -c $Configuration --no-build --verbosity normal
if ($LASTEXITCODE -ne 0) {
    Write-Error "Automated tests failed with exit code $LASTEXITCODE"
}

# 4. Content Validator
Write-Host "`n[4/4] Validating Game Content Schemas..." -ForegroundColor Yellow
dotnet run --project tools/ContentValidator/ContentValidator.csproj -c $Configuration --no-build
if ($LASTEXITCODE -ne 0) {
    Write-Error "Content validation failed with exit code $LASTEXITCODE"
}

Write-Host "`n========================================" -ForegroundColor Green
Write-Host " All Build, Test & Validation Gates PASSED!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
exit 0
