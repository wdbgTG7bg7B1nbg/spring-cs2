@echo off
title SPRING CS2 - Subtick Movement & Cloud Loader
color 0A
cls
echo ===================================================
echo     SPRING CS2 PREMIUM CLIENT LOADER v2.4.1
echo ===================================================
echo.
cd /d "%~dp0"

if exist "SpringLoader.exe" (
    echo Launching Spring Desktop GUI Client...
    start "" "SpringLoader.exe"
    exit /b
)

if exist "software\SpringClient.exe" (
    echo Launching Spring Desktop GUI Client...
    cd software
    start "" "SpringClient.exe"
    exit /b
)

if exist "software\publish\SpringClient.exe" (
    echo Launching Spring Desktop GUI Client...
    cd software\publish
    start "" "SpringClient.exe"
    exit /b
)

where node >nul 2>nul
if %ERRORLEVEL% EQU 0 (
    echo Launching Spring Interactive CLI Client...
    cd software
    node spring-loader.js
    pause
    exit /b
)

echo [ERROR] Spring Client could not be launched.
pause
