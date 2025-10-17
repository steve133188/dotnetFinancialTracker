@echo off
echo Building .NET MAUI Family Budget App for Windows...
echo.

REM Clean previous builds
echo [1/4] Cleaning previous builds...
dotnet clean --configuration Release --framework net8.0-windows
if %errorlevel% neq 0 goto error

REM Restore packages
echo [2/4] Restoring packages...
dotnet restore
if %errorlevel% neq 0 goto error

REM Build for Windows
echo [3/4] Building for Windows...
dotnet build --configuration Release --framework net8.0-windows --verbosity minimal
if %errorlevel% neq 0 goto error

REM Run the application
echo [4/4] Starting application...
echo.
echo Application starting... Press Ctrl+C to stop.
dotnet run --configuration Release --framework net8.0-windows

goto end

:error
echo.
echo ERROR: Build failed!
echo.
echo Troubleshooting steps:
echo 1. Ensure you have .NET 8 SDK installed
echo 2. Enable Developer Mode in Windows Settings
echo 3. Try running: dotnet workload install maui
echo 4. Restart Visual Studio as Administrator
echo.
pause
exit /b 1

:end
echo.
echo Build completed successfully!
pause