@echo off
title SPRING CS2 - Subtick Movement & Cloud Loader
color 0A
cls
echo ===================================================
echo     SPRING CS2 PREMIUM CLIENT LOADER v2.4.1
echo ===================================================
echo.
cd /d "%~dp0"

if exist "SpringClient.exe" (
    echo Launching Spring Desktop GUI Client...
    start "" "SpringClient.exe"
    exit /b
)

if exist "publish\SpringClient.exe" (
    echo Launching Spring Desktop GUI Client...
    start "" "publish\SpringClient.exe"
    exit /b
)

where node >nul 2>nul
if %ERRORLEVEL% EQU 0 (
    echo Launching Spring Interactive CLI Client...
    node spring-loader.js
    pause
    exit /b
)

echo [ERROR] Neither SpringClient.exe nor Node.js could be started.
pause
