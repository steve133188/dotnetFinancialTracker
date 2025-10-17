# Windows Setup Guide - NU1105 Error Solutions

## 🚨 **Quick Fix for NU1105 Errors**

If you're getting NU1105 package restore errors on Windows, follow these steps **in order**:

### **Step 1: Verify Prerequisites**
```cmd
# Check .NET SDK version (must be 8.0.100+)
dotnet --version

# Check installed workloads
dotnet workload list

# Check available SDKs
dotnet --list-sdks
```

### **Step 2: Clean Environment**
```cmd
# Clear all NuGet caches
dotnet nuget locals all --clear

# Clear temporary files
rd /s /q "%TEMP%\NuGetScratch"
rd /s /q "%USERPROFILE%\.nuget\packages"

# Clear MSBuild cache
rd /s /q "%LOCALAPPDATA%\Microsoft\MSBuild"
```

### **Step 3: Install/Update MAUI Workload**
```cmd
# Uninstall and reinstall MAUI workload
dotnet workload uninstall maui
dotnet workload install maui

# Or update if already installed
dotnet workload update
```

### **Step 4: Restore and Build**
```cmd
# Navigate to project directory
cd DotnetFinancialTrackerApp

# Restore with specific target framework
dotnet restore -f net8.0-windows10.0.19041.0 --verbosity detailed

# Build with specific target framework
dotnet build -f net8.0-windows10.0.19041.0 --verbosity detailed

# Run tests
dotnet test DotnetFinancialTrackerApp.Tests
```

## **Alternative Solutions**

### **Solution A: Use Visual Studio 2022**
1. Open Visual Studio Installer
2. Modify your VS 2022 installation
3. Ensure these workloads are installed:
   - ✅ **.NET Multi-platform App UI development**
   - ✅ **ASP.NET and web development**
   - ✅ **.NET desktop development**

### **Solution B: Manual Package Restore**
```cmd
# Force package restore with specific sources
dotnet restore --source https://api.nuget.org/v3/index.json --source https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet-tools/nuget/v3/index.json

# Build only the test project first
dotnet build DotnetFinancialTrackerApp.Tests/DotnetFinancialTrackerApp.Tests.csproj

# Then build the main project
dotnet build DotnetFinancialTrackerApp/DotnetFinancialTrackerApp.csproj -f net8.0-windows10.0.19041.0
```

### **Solution C: Environment Variables**
```cmd
# Set environment variables to force compatibility
set DOTNET_ROLL_FORWARD=Major
set DOTNET_CLI_FORCE_UTF8_ENCODING=1
set MSBUILD_EXE_PATH=%ProgramFiles%\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe

# Then try restore and build
dotnet restore
dotnet build
```

## **Verification Steps**

After applying fixes, verify everything works:

```cmd
# 1. Check project can restore
dotnet restore --verbosity normal

# 2. Check project can build
dotnet build --configuration Debug

# 3. Check tests run
dotnet test --verbosity normal

# 4. Check specific platform build
dotnet build -f net8.0-windows10.0.19041.0 --configuration Release
```

## **Common Issues & Solutions**

### **Issue: "The current .NET SDK does not support targeting .NET 8.0"**
**Solution:** Install .NET 8.0 SDK from https://dotnet.microsoft.com/download/dotnet/8.0

### **Issue: "NETSDK1147: To build this project, the following workloads must be installed: maui"**
**Solution:** Run `dotnet workload install maui`

### **Issue: "MSB4236: The SDK 'Microsoft.NET.Sdk.Razor' specified could not be found"**
**Solution:**
```cmd
dotnet workload install aspire
dotnet workload install maui
```

### **Issue: "Package restore failed"**
**Solution:** Check your internet connection and corporate firewall settings. You may need to configure NuGet for corporate proxy.

## **Corporate/Proxy Environment**

If you're behind a corporate firewall:

```cmd
# Configure NuGet for proxy (replace with your proxy details)
dotnet nuget config set http_proxy http://proxy.company.com:8080
dotnet nuget config set https_proxy https://proxy.company.com:8080

# Add trusted sources
dotnet nuget add source https://api.nuget.org/v3/index.json -n nuget.org
```

## **Last Resort: Local Development**

If all else fails, you can develop on the test project only:

```cmd
# Work with just the test project (no MAUI dependencies)
cd DotnetFinancialTrackerApp.Tests
dotnet restore
dotnet build
dotnet test

# The test project demonstrates all required academic features:
# - Polymorphism, Interfaces, LINQ, Generics, NUnit tests
```

## **Getting Help**

If you continue having issues:

1. **Check your exact error message** - NU1105 can have different underlying causes
2. **Run with verbose logging:** `dotnet restore --verbosity detailed`
3. **Check Windows Event Viewer** for system-level issues
4. **Try building from Visual Studio 2022** instead of command line
5. **Consider using GitHub Codespaces** or **Visual Studio Online** for development

## **Success Indicators**

You know it's working when:
- ✅ `dotnet restore` completes without errors
- ✅ `dotnet build` completes without errors
- ✅ `dotnet test` runs and shows test results
- ✅ You can see the application's academic features in the code