# PowerShell script to validate .NET 8 SDK installation on Windows
# Usage: .\check-dotnet-version.ps1

Write-Host "🔍 Checking .NET SDK Version Requirements..." -ForegroundColor Yellow
Write-Host ""

# Check if dotnet is installed
if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    Write-Host "❌ ERROR: .NET SDK is not installed or not in PATH" -ForegroundColor Red
    Write-Host "📥 Please install .NET 8.0 SDK from: https://dotnet.microsoft.com/download/dotnet/8.0" -ForegroundColor Yellow
    exit 1
}

# Get current .NET SDK version
$dotnetVersion = dotnet --version
Write-Host "🔧 Current .NET SDK Version: $dotnetVersion" -ForegroundColor Cyan

# Check if version is .NET 8.0.100 or higher
$requiredVersion = [System.Version]"8.0.100"
try {
    $currentVersion = [System.Version]$dotnetVersion

    if ($currentVersion -ge $requiredVersion) {
        Write-Host "✅ .NET SDK Version is compatible with this project" -ForegroundColor Green
        Write-Host "📋 Required: .NET 8.0.100 or later" -ForegroundColor Green
        Write-Host "🎯 Current: $dotnetVersion" -ForegroundColor Green
    } else {
        Write-Host "❌ ERROR: .NET SDK version is too old" -ForegroundColor Red
        Write-Host "📋 Required: .NET 8.0.100 or later" -ForegroundColor Yellow
        Write-Host "🎯 Current: $dotnetVersion" -ForegroundColor Red
        Write-Host "📥 Please update .NET SDK from: https://dotnet.microsoft.com/download/dotnet/8.0" -ForegroundColor Yellow
        exit 1
    }
} catch {
    Write-Host "⚠️  Could not parse .NET SDK version: $dotnetVersion" -ForegroundColor Yellow
    Write-Host "📥 Please ensure .NET 8.0 SDK is installed from: https://dotnet.microsoft.com/download/dotnet/8.0" -ForegroundColor Yellow
}

Write-Host ""

# Check for MAUI workload
Write-Host "🔍 Checking MAUI Workload..." -ForegroundColor Yellow
$workloads = dotnet workload list 2>&1
if ($workloads -match "maui") {
    Write-Host "✅ MAUI workload is installed" -ForegroundColor Green
} else {
    Write-Host "⚠️  MAUI workload not detected" -ForegroundColor Yellow
    Write-Host "📥 Install with: dotnet workload install maui" -ForegroundColor Yellow
}

Write-Host ""

# Check Windows SDK (for Windows builds)
if ($IsWindows -or $env:OS -eq "Windows_NT") {
    Write-Host "🔍 Checking Windows SDK..." -ForegroundColor Yellow
    $windowsSdkPath = "${env:ProgramFiles(x86)}\Windows Kits\10\bin"
    if (Test-Path $windowsSdkPath) {
        $sdkVersions = Get-ChildItem $windowsSdkPath -Directory | Where-Object { $_.Name -match "10\.0\.\d+\.\d+" } | Sort-Object Name -Descending
        if ($sdkVersions.Count -gt 0) {
            $latestSdk = $sdkVersions[0].Name
            Write-Host "✅ Windows SDK found: $latestSdk" -ForegroundColor Green
        } else {
            Write-Host "⚠️  Windows SDK not found in expected location" -ForegroundColor Yellow
        }
    } else {
        Write-Host "⚠️  Windows SDK not found" -ForegroundColor Yellow
        Write-Host "📥 Download from: https://developer.microsoft.com/en-us/windows/downloads/windows-sdk/" -ForegroundColor Yellow
    }
}

Write-Host ""
Write-Host "🚀 Ready to build! Run: dotnet restore && dotnet build" -ForegroundColor Green