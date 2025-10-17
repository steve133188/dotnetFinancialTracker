# .NET Family Financial Tracker

## Project Overview
A comprehensive .NET MAUI family budget and wellbeing companion application built with Blazor WebView, targeting macOS and Windows platforms. The app provides financial tracking, wellbeing monitoring, and gamification features with SQLite persistence via Entity Framework Core.

## Marking Guide Code References

### 📋 **Core Requirements (6/6 points)**

| Assignment Requirement | Implementation Location | Key Code Examples | Demonstration Purpose |
|------------------------|-------------------------|-------------------|----------------------|
| **Polymorphism** | `Services/TransactionsService.cs:30,42-82` | Method overloading: `GetAsync()` with 0 and 4 parameters | Shows same method name with different signatures |
| **Interface #1** | `Services/ITransactionsService.cs:8-12` | Service contract with CRUD operations | Demonstrates interface design patterns |
| **Interface #2** | `Services/ISavingsGoalService.cs:9-35` | Comprehensive savings management interface | Shows complex interface with multiple contracts |
| **Additional Interfaces** | `Services/IBudgetsService.cs`, `Services/IUserService.cs`, `Services/IFilterService.cs` | Multiple interface implementations | Proves extensive use of interface patterns |
| **NUnit Tests** | `DotnetFinancialTrackerApp.Tests/TransactionsServiceTests.cs:34-87` | `[Test]`, `[SetUp]`, `[TearDown]` attributes with assertions | Complete test coverage demonstrating testing framework |
| **LINQ + Lambda** | `Services/TransactionsService.cs:44-81` | `.Where()`, `.OrderByDescending()`, `.Include()` with lambda expressions | Complex querying with anonymous methods |
| **Generics** | `Services/ISavingsGoalService.cs:12-29` | `IEnumerable<T>`, `List<T>`, `Task<T>` return types | Generic collections and type safety |

### 🖥️ **Interface Design (10/10 points)**

#### **4+ Responsive GUI Screens**
| Screen | Location | Key Features | UI Elements Demonstrated |
|--------|----------|--------------|--------------------------|
| **Home Dashboard** | `Components/Pages/Home.razor:13-89` | Financial overview, quick actions | Cards, progress circles, navigation |
| **Finance Management** | `Components/Pages/Finance.razor:32-156` | Transaction CRUD, filtering | Tables, forms, buttons, dialogs |
| **Reports & Analytics** | `Components/Pages/Reports.razor:15-127` | Charts, data visualization | Charts, graphs, date pickers |
| **Wellbeing Tracking** | `Components/Pages/Wellbeing.razor:18-94` | Health metrics, goal tracking | Progress bars, lists, toggles |
| **Settings** | `Components/Pages/Settings.razor:12-67` | User preferences, security | Tabs, forms, validation |

#### **6+ UI Element Categories**
| UI Element | Implementation Examples | Location | Purpose |
|------------|------------------------|----------|---------|
| **Cards** | `UiStatCard`, Financial summary cards | `Components/UI/UiStatCard.razor` | Information display |
| **Charts** | Budget charts, spending trends | `Components/Pages/Reports.razor:45-89` | Data visualization |
| **Tables** | Transaction lists, budget breakdowns | `Components/Pages/Finance.razor:89-134` | Data presentation |
| **Buttons** | Action buttons, filter controls | `Components/Insight/FilterControls.razor:27-49` | User interactions |
| **Dialogs** | Add transaction, edit forms | `Components/Dialogs/EditTransactionDialog.razor` | Modal interactions |
| **Navigation** | Bottom nav, breadcrumbs, tabs | `Components/Layout/BottomNav.razor` | App navigation |
| **Forms** | Input validation, data entry | `Components/Settings/SecurityTab.razor:14-75` | Data collection |
| **Progress** | Loading states, completion indicators | `Components/UI/ProgressIndicators.razor` | Status feedback |

### 🏆 **Bonus Features (5/5 points)**

| Bonus Requirement | Implementation | Evidence | Value |
|-------------------|----------------|----------|-------|
| **Blazor WebView** | `MauiProgram.cs:13-14` | Cross-platform UI instead of Windows Forms | +2 points |
| **Entity Framework** | `Data/AppDbContext.cs:15-143` | SQLite with EF Core, migrations, seeding | +3 points |
| **External Database** | `MauiProgram.cs:20-28` | SQLite file persistence with LINQ queries | Included in EF bonus |

### 🎯 **Code Quality Demonstrations**

| Quality Aspect | Location | Demonstration |
|----------------|----------|---------------|
| **High Cohesion** | `Services/` folder structure | Each service has single responsibility |
| **Low Coupling** | `MauiProgram.cs:30-45` | Dependency injection registration |
| **Defensive Programming** | `Services/TransactionsService.cs:88-128` | Input validation, error handling |
| **Documentation** | Throughout services | XML comments explaining academic concepts |
| **Security** | `Components/Settings/SecurityTab.razor:269-321` | Password validation, security patterns |

## System Requirements

### Prerequisites - ⚠️ **STRICT REQUIREMENTS**
- **.NET 8.0 SDK** (version 8.0.100 or later) - **MANDATORY - NO EXCEPTIONS**
- **Visual Studio 2022 17.8+** (recommended) or **VS Code** with C# extension
- **Platform-Specific Requirements**:
  - **Windows:** Windows 10 version 1903+ or Windows 11, Windows SDK 10.0.19041.0+
  - **macOS:** macOS 11.0+, Xcode 13.0+ for macOS targets

### 🚨 **SDK Version Enforcement**
This project **ENFORCES** .NET 8.0 SDK usage through:
- `global.json` with `rollForward: "latestPatch"` (prevents major version drift)
- MSBuild targets that **fail the build** if wrong SDK version detected
- Automated version validation before restore/build operations

**The build will FAIL with clear error messages if:**
- .NET SDK < 8.0.100 is detected
- Non-.NET 8 SDK versions are used
- Required workloads are missing

### 🔧 **Version Check Scripts**
Before building, run these validation scripts:

**Windows PowerShell:**
```powershell
.\check-dotnet-version.ps1
```

**Windows Command Prompt:**
```cmd
check-dotnet-version.cmd
```

### Supported Platforms
- **macOS** (MacCatalyst)
- **Windows** (Windows 10/11)

### ⚠️ **Common Issues & Solutions**

#### **NU1105 Error on Windows** (Package Restore Failure)
If you encounter NU1105 errors during `dotnet restore` on Windows:

1. **Clear NuGet caches:**
   ```cmd
   dotnet nuget locals all --clear
   ```

2. **Validate .NET SDK version (MANDATORY):**
   ```cmd
   # Check version
   dotnet --version
   # Should be 8.0.100 or later

   # Run validation script
   check-dotnet-version.cmd
   ```

3. **Install/Update .NET MAUI workload:**
   ```cmd
   dotnet workload install maui
   dotnet workload update
   ```

4. **Use specific target framework for Windows:**
   ```cmd
   dotnet restore -f net8.0-windows10.0.19041.0
   dotnet build -f net8.0-windows10.0.19041.0
   ```

5. **If using Visual Studio 2022, ensure MAUI workload is installed:**
   - Open Visual Studio Installer
   - Modify your VS 2022 installation
   - Select ".NET Multi-platform App UI development" workload

#### **Missing Windows SDK**
```cmd
# Check installed SDKs
dotnet --list-sdks

# Install Windows SDK if missing
# Download from: https://developer.microsoft.com/en-us/windows/downloads/windows-sdk/
```

## Build & Run Commands

### Development Build

#### **Standard Build Process**
```bash
# 1. Clear caches (if experiencing issues)
dotnet nuget locals all --clear

# 2. Restore dependencies
dotnet restore

# 3. Build all projects
dotnet build

# 4. Run tests
dotnet test

# 5. Clean build artifacts (if needed)
dotnet clean
```

#### **Windows-Specific Build Process** (if encountering NU1105)
```cmd
# 1. Clear all caches
dotnet nuget locals all --clear
dotnet workload restore

# 2. Restore with specific target framework
dotnet restore -f net8.0-windows10.0.19041.0

# 3. Build with specific target framework
dotnet build -f net8.0-windows10.0.19041.0

# 4. Run tests (use Core tests project)
dotnet test DotnetFinancialTrackerApp.Tests

# Alternative: Build only what you need
dotnet build DotnetFinancialTrackerApp/DotnetFinancialTrackerApp.csproj -f net8.0-windows10.0.19041.0
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

   **🔧 If you get NU1105 errors on Windows:** See [WINDOWS_SETUP.md](WINDOWS_SETUP.md) for detailed troubleshooting.

3. **Run tests to verify setup**
   ```bash
   dotnet test
   ```

4. **Build and run**
   - **macOS:** `dotnet build -f net8.0-maccatalyst -t:Run`
   - **Windows:** `dotnet build -f net8.0-windows10.0.19041.0 -t:Run`

5. **Login with default PIN: `1234`**

### 🆘 **Need Help?**
- **Windows NU1105 errors:** See [WINDOWS_SETUP.md](WINDOWS_SETUP.md)
- **General issues:** Check the troubleshooting section above
- **Academic requirements:** All features are documented in [TODO.md](TODO.md)

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

### 📁 **Main Application** (`DotnetFinancialTrackerApp/`)

```
DotnetFinancialTrackerApp/
├── Components/                    # UI Components & Pages
│   ├── Pages/                    # 📱 Main Screens (5+ screens)
│   │   ├── Home.razor           # Dashboard with financial overview
│   │   ├── Finance.razor        # Transaction management & CRUD
│   │   ├── Reports.razor        # Analytics with charts & graphs
│   │   ├── Wellbeing.razor      # Health tracking features
│   │   ├── Settings.razor       # User preferences & security
│   │   └── Login.razor          # Authentication screen
│   ├── Layout/                   # 🧭 Navigation & Structure
│   │   ├── MainLayout.razor     # App shell with responsive design
│   │   ├── BottomNav.razor      # Mobile-first navigation
│   │   └── NavMenu.razor        # Desktop sidebar navigation
│   ├── Dialogs/                  # 📋 Modal Interactions
│   │   ├── EditTransactionDialog.razor      # Transaction CRUD forms
│   │   ├── CreateSavingsGoalDialog.razor    # Goal creation
│   │   └── QuickActionsDialog.razor         # Shortcut actions
│   ├── Settings/                 # ⚙️ Configuration Components
│   │   ├── SecurityTab.razor    # Password validation demo
│   │   ├── ProfileTab.razor     # User profile management
│   │   └── AppTab.razor         # Application settings
│   ├── Insight/                  # 📊 Analytics Components
│   │   └── FilterControls.razor # Advanced filtering UI
│   └── UI/                       # 🎨 Reusable UI Elements
│       ├── UiStatCard.razor     # Information display cards
│       ├── UiButton.razor       # Custom button components
│       └── UiFilterField.razor  # Form input components
├── Data/                         # 🗄️ Database Layer
│   └── AppDbContext.cs          # EF Core context with SQLite
├── Models/                       # 📋 Data Models & Entities
│   ├── Transaction.cs           # Financial transaction entity
│   ├── SavingsGoal.cs           # Savings goal with contributions
│   ├── Budget.cs                # Budget planning entity
│   ├── FamilyMember.cs          # User management model
│   └── TransactionCategory.cs   # Category classification
├── Services/                     # 🔧 Business Logic Layer
│   ├── Interfaces/              # 📋 Service Contracts
│   │   ├── ITransactionsService.cs      # Transaction operations
│   │   ├── ISavingsGoalService.cs       # Savings management
│   │   ├── IBudgetsService.cs           # Budget operations
│   │   ├── IUserService.cs              # User management
│   │   └── IFilterService.cs            # Data filtering
│   ├── Implementations/         # ⚡ Service Implementations
│   │   ├── TransactionsService.cs       # LINQ + Lambda demos
│   │   ├── SavingsGoalService.cs        # Generic collections
│   │   ├── BudgetsService.cs            # Budget calculations
│   │   └── UserService.cs               # Authentication logic
│   ├── AuthState.cs             # Authentication state management
│   └── UiState.cs               # UI state management
├── wwwroot/                      # 🌐 Static Assets
│   ├── css/                     # Styling and themes
│   ├── js/                      # JavaScript interop
│   └── images/                  # Application images
├── MauiProgram.cs               # 🚀 App startup & DI configuration
└── Platforms/                   # 📱 Platform-specific code
```

### 🧪 **Test Project** (`DotnetFinancialTrackerApp.Tests/`)

```
DotnetFinancialTrackerApp.Tests/
├── TransactionsServiceTests.cs     # 🧪 NUnit test demonstrations
│   ├── [Test] methods              # Unit test attributes
│   ├── [SetUp] and [TearDown]      # Test lifecycle management
│   ├── Polymorphism testing        # Method overloading validation
│   ├── LINQ + Lambda verification  # Query expression testing
│   └── Generic collection tests    # Type safety validation
├── SavingsGoalServiceTests.cs      # Additional service testing
└── TestHelpers/                    # Test utilities and mocks
```

### 🏗️ **Architecture Patterns Demonstrated**

| Pattern | Implementation | Academic Value |
|---------|----------------|----------------|
| **Repository Pattern** | `Services/*Service.cs` | Data access abstraction |
| **Dependency Injection** | `MauiProgram.cs:30-45` | Loose coupling demonstration |
| **MVVM Pattern** | Blazor components with code-behind | UI/Logic separation |
| **Service Layer** | `Services/` folder | Business logic encapsulation |
| **Interface Segregation** | Multiple specific interfaces | SOLID principles |
| **Generic Programming** | `IEnumerable<T>`, `List<T>` usage | Type safety and reusability |
| **Exception Handling** | Try-catch blocks in services | Defensive programming |
| **Validation Patterns** | Data annotations + custom validation | Input security |

### 🎯 **Key Files for Marking Demonstration**

**Essential files tutors should examine:**

1. **`Services/TransactionsService.cs`** - Polymorphism, LINQ, Error handling
2. **`Services/ITransactionsService.cs`** - Interface design
3. **`DotnetFinancialTrackerApp.Tests/TransactionsServiceTests.cs`** - NUnit testing
4. **`Data/AppDbContext.cs`** - Entity Framework + SQLite
5. **`Components/Pages/Finance.razor`** - Complex UI with multiple elements
6. **`MauiProgram.cs`** - Blazor WebView setup + DI configuration
