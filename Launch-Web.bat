@echo off
setlocal
cd /d "%~dp0"

echo ===================================================
echo   Rashtra: Sovereign Republic - Web Dev Server
echo ===================================================
echo.
echo Starting local web server on port 3000...
npm run dev
