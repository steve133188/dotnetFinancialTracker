@echo off
REM Batch script to validate .NET 8 SDK installation on Windows
REM Usage: check-dotnet-version.cmd

echo.
echo 🔍 Checking .NET SDK Version Requirements...
echo.

REM Check if dotnet is installed
dotnet --version >nul 2>&1
if %errorlevel% neq 0 (
    echo ❌ ERROR: .NET SDK is not installed or not in PATH
    echo 📥 Please install .NET 8.0 SDK from: https://dotnet.microsoft.com/download/dotnet/8.0
    exit /b 1
)

REM Get current .NET SDK version
for /f "tokens=*" %%i in ('dotnet --version') do set DOTNET_VERSION=%%i
echo 🔧 Current .NET SDK Version: %DOTNET_VERSION%

REM Simple version check (assuming format 8.x.xxx)
echo %DOTNET_VERSION% | findstr /r "^8\." >nul
if %errorlevel% equ 0 (
    echo ✅ .NET SDK Version is compatible with this project
    echo 📋 Required: .NET 8.0.100 or later
    echo 🎯 Current: %DOTNET_VERSION%
) else (
    echo ❌ ERROR: .NET SDK version is not .NET 8
    echo 📋 Required: .NET 8.0.100 or later
    echo 🎯 Current: %DOTNET_VERSION%
    echo 📥 Please install .NET 8.0 SDK from: https://dotnet.microsoft.com/download/dotnet/8.0
    exit /b 1
)

echo.

REM Check for MAUI workload
echo 🔍 Checking MAUI Workload...
dotnet workload list | findstr "maui" >nul
if %errorlevel% equ 0 (
    echo ✅ MAUI workload is installed
) else (
    echo ⚠️  MAUI workload not detected
    echo 📥 Install with: dotnet workload install maui
)

echo.
echo 🚀 Ready to build! Run: dotnet restore ^&^& dotnet build
echo.