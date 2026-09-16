@echo off
REM Copies the add-in into MicroStation on this Windows PC.
set SCRIPT=%~dp0Install-ExportRgbColors.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File "%SCRIPT%" %*
if errorlevel 1 (
  echo.
  echo If access was denied, right-click this file and choose Run as administrator.
  pause
  exit /b 1
)
echo.
pause
