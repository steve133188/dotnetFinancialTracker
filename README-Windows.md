# Windows Build Guide

## 🚀 Quick Start for Windows

### Option 1: Use the Build Script (Recommended)
```batch
# Double-click this file or run in Command Prompt:
build-windows.bat
```

### Option 2: Manual Commands
```bash
# Clean and build
dotnet clean --framework net8.0-windows
dotnet restore
dotnet build --framework net8.0-windows

# Run the app
dotnet run --framework net8.0-windows
```

### Option 3: Visual Studio
1. Open `DotnetFinancialTrackerApp.sln` in Visual Studio
2. Set startup project to `DotnetFinancialTrackerApp`
3. Select **"Windows"** profile from debug dropdown
4. Press F5 or click "Start Debugging"

## 🔧 Troubleshooting

### "Project needs to be deployed" Error:
1. **Enable Developer Mode:**
   - Settings → Update & Security → For developers → Developer mode
   - Restart Visual Studio

2. **Use the correct debug profile:**
   - Select **"Windows"** (not "Windows Machine")
   - Uses `commandName: Project` instead of `MsixPackage`

3. **Alternative fix:**
   ```bash
   dotnet run --framework net8.0-windows
   ```

### Missing Workloads:
```bash
dotnet workload install maui
```

### Visual Studio Issues:
- Run Visual Studio as Administrator
- Build → Clean Solution → Rebuild Solution
- Try "Windows (Debug)" profile for debugging

## 📋 What's Changed

✅ **Simplified target framework:** `net8.0-windows` (instead of `net8.0-windows10.0.19041.0`)
✅ **No packaging required:** `WindowsPackageType=None`
✅ **Self-contained:** `WindowsAppSDKSelfContained=true`
✅ **Simple launch profiles:** Direct project execution
✅ **Lower OS requirements:** Windows 7+ support

## 🎯 Key Features
- 7 main app pages (Dashboard, Finance, Reports, Wellbeing, Settings, Login, Transaction Details)
- MudBlazor UI components with responsive design
- SQLite database with Entity Framework Core
- Cross-platform .NET MAUI with Blazor WebView
- Financial tracking, budgeting, and savings goals

The app should now build and run on Windows without deployment issues!