@echo off
setlocal
cd /d "%~dp0"

echo ===================================================
echo   Rashtra: Sovereign Republic - Desktop Launcher
echo ===================================================
echo.

if exist "release\Rashtra-Desktop-x64\Rashtra.exe" (
    echo Launching standalone executable...
    start "" "release\Rashtra-Desktop-x64\Rashtra.exe"
    exit /b 0
)

echo Standalone release not yet built. Packaging now...
call npm run package:desktop
if %ERRORLEVEL% EQU 0 (
    echo Launching standalone executable...
    start "" "release\Rashtra-Desktop-x64\Rashtra.exe"
) else (
    echo.
    echo Packaging encountered an error. Please verify Node and .NET 8 SDK are installed.
    pause
)
