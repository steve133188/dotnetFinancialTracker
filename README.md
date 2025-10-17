# .NET Family Financial Tracker

## Project Overview
A comprehensive .NET MAUI family budget and wellbeing companion application built with Blazor WebView, targeting macOS and Windows platforms. The app provides financial tracking, wellbeing monitoring, and gamification features with SQLite persistence via Entity Framework Core.

## Marking Guide Code References

| Assignment Requirement | Implementation Location | Description |
|------------------------|-------------------------|-------------|
| **Polymorphism** | `Services/TransactionsService.cs:16-17` | Method overloading for `GetAsync()` |
| **Interface #1** | `Services/ITransactionsService.cs` | Transaction management interface |
| **Interface #2** | `Services/ISavingsGoalService.cs` | Savings goal management interface |
| **NUnit Tests** | `DotnetFinancialTrackerApp.Tests/TransactionsServiceTests.cs` | Comprehensive test coverage |
| **LINQ + Lambda** | `Services/TransactionsService.cs:21-51` | Data filtering with lambda expressions |
| **Generics** | `Services/ISavingsGoalService.cs:8-29` | `IEnumerable<T>`, `List<T>` collections |
| **GUI Screen #1** | `Components/Pages/Home.razor` | Home dashboard |
| **GUI Screen #2** | `Components/Pages/Finance.razor` | Finance management |
| **GUI Screen #3** | `Components/Pages/Reports.razor` | Reports & analytics |
| **GUI Screen #4** | `Components/Pages/Wellbeing.razor` | Wellbeing tracking |
| **GUI Screen #5** | `Components/Pages/Settings.razor` | Settings & configuration |
| **UI Elements** | Throughout `Components/` | Cards, charts, tables, buttons, dialogs, navigation, forms, progress |
| **Bonus: Blazor** | `MauiProgram.cs:13-14` | Blazor WebView implementation |
| **Bonus: Entity Framework** | `Data/AppDbContext.cs` | SQLite with EF Core |

## System Requirements

### Prerequisites
- **.NET 8.0 SDK** (required)
- **Visual Studio 2022** (recommended) or **VS Code**
- **Platform SDKs**:
  - macOS: Xcode for macOS targets
  - Windows: Windows SDK for Windows targets

### Supported Platforms
- **macOS** (MacCatalyst)
- **Windows** (Windows 10/11)

## Build & Run Commands

### Development Build
```bash
# Restore dependencies
dotnet restore

# Build all projects
dotnet build

# Run tests
dotnet test

# Clean build artifacts
dotnet clean
```

### Platform-Specific Builds

#### macOS (MacCatalyst)
```bash
dotnet build -f net8.0-maccatalyst
dotnet build -f net8.0-maccatalyst -t:Run
```

#### Windows
```bash
dotnet build -f net8.0-windows10.0.19041.0
dotnet build -f net8.0-windows10.0.19041.0 -t:Run
```


## Quick Start

1. **Clone the repository**
   ```bash
   git clone [repository-url]
   cd DotnetFinancialTrackerApp
   ```

2. **Install dependencies**
   ```bash
   dotnet restore
   ```

3. **Run tests to verify setup**
   ```bash
   dotnet test
   ```

4. **Build and run (macOS example)**
   ```bash
   dotnet build -f net8.0-maccatalyst -t:Run
   ```

5. **Login with default PIN: `1234`**

## Key Features
- **Multi-user financial tracking** with transaction management
- **Budget planning and monitoring** with visual analytics
- **Savings goals** with contribution tracking
- **Wellbeing integration** for holistic family management
- **Responsive design** with MudBlazor components
- **Cross-platform** support for macOS and Windows via .NET MAUI + Blazor
- **Comprehensive error handling** and input validation
- **SQLite database** with Entity Framework Core integration

## Project Structure
```
DotnetFinancialTrackerApp/
├── Components/
│   ├── Pages/           # Main application screens
│   ├── Layout/          # Navigation and layout components
│   └── Dialogs/         # Modal dialogs
├── Data/                # Entity Framework context
├── Models/              # Data models
├── Services/            # Business logic and interfaces
└── wwwroot/            # Static assets and CSS

DotnetFinancialTrackerApp.Tests/
└── *.cs                # NUnit test files
```

For detailed marking guide compliance and enhancement roadmap, see [TODO.md](TODO.md).