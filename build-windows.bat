@echo off
echo =================================================
echo Building .NET MAUI Family Budget App for Windows
echo =================================================
echo.

REM Check if we're on Windows
if not "%OS%"=="Windows_NT" (
    echo ERROR: This script is for Windows only.
    pause
    exit /b 1
)

REM Install required workloads first
echo [1/5] Installing MAUI workload for Windows...
dotnet workload install maui --skip-workload-sets
if %errorlevel% neq 0 (
    echo WARNING: Could not install workload, continuing anyway...
)

REM Clear caches
echo [2/5] Clearing NuGet caches...
dotnet nuget locals all --clear

REM Restore packages (Windows only)
echo [3/5] Restoring packages for Windows...
dotnet restore --verbosity minimal
if %errorlevel% neq 0 goto error

REM Build for Windows only
echo [4/5] Building for Windows...
dotnet build --configuration Release --verbosity minimal
if %errorlevel% neq 0 goto error

REM Run the application
echo [5/5] Starting application...
echo.
echo ========================================
echo Application starting...
echo Default login PIN: 1234
echo Press Ctrl+C to stop.
echo ========================================
echo.
dotnet run --configuration Release

goto end

:error
echo.
echo ===============================================
echo ERROR: Build failed!
echo ===============================================
echo.
echo Troubleshooting steps:
echo 1. Ensure you have .NET 8 SDK installed
echo 2. Enable Developer Mode in Windows Settings:
echo    Settings ^> Update ^& Security ^> For developers ^> Developer mode
echo 3. Install Visual Studio 2022 with MAUI workload
echo 4. Try running as Administrator
echo 5. Run: dotnet --info (to check installation)
echo.
echo Alternative commands to try:
echo   dotnet workload install maui
echo   dotnet workload restore
echo   dotnet clean
echo.
pause
exit /b 1

:end
echo.
echo ===============================================
echo Build completed successfully!
echo App should be running now...
echo ===============================================
pause