# Family Financial Tracker - Code Review Guide

A .NET MAUI application with Blazor  demonstrating core programming concepts including polymorphism, interfaces, generics, LINQ with lambda expressions, and comprehensive NUnit testing.

## Quick Start

### Prerequisites
- .NET 8.0 SDK (version 8.0.100 or later)
- Visual Studio 2022 17.8+ or VS Code with C# extension

### Running the Application

1. **Clone and restore dependencies**
   ```bash
   git clone [repository-url]
   cd DotnetFinancialTrackerApp
   dotnet restore
   ```

2. **Run tests to verify setup**
   ```bash
   dotnet test
   ```

3. **Launch application**
   ```bash
   # Windows
   dotnet build -f net8.0-windows10.0.19041.0 -t:Run

   # macOS
   dotnet build -f net8.0-maccatalyst -t:Run
   ```

4. **Default login PIN: 1234**

## Core Requirements Implementation

### Polymorphism Demonstration

| Requirement | File Location | Line Numbers | Implementation Details |
|-------------|---------------|--------------|----------------------|
| Method Overloading | `Services/TransactionsService.cs` | Lines 30, 42 | Two `GetAsync()` methods with different parameter signatures |
| Overload Usage | `DotnetFinancialTrackerApp.Tests/TransactionsServiceTests.cs` | Lines 41, 58 | Tests demonstrate both overload variants |

### Interface Implementation

| Interface | File Location | Purpose | Implementation |
|-----------|---------------|---------|----------------|
| ITransactionsService | `Services/ITransactionsService.cs` | Transaction operations contract | `Services/TransactionsService.cs` |
| ISavingsGoalService | `Services/ISavingsGoalService.cs` | Savings management contract | `Services/SavingsGoalService.cs` |
| IBudgetsService | `Services/IBudgetsService.cs` | Budget operations contract | `Services/BudgetsService.cs` |
| IUserService | `Services/IUserService.cs` | User management contract | `Services/UserService.cs` |
| Dependency Injection Setup | `MauiProgram.cs` | Lines 37-44 | Interface registration with DI container |

### NUnit Testing Framework

| Test File | Location | Test Methods | Coverage |
|-----------|----------|--------------|----------|
| TransactionsServiceTests | `DotnetFinancialTrackerApp.Tests/TransactionsServiceTests.cs` | Lines 34-87 | Method overloading, LINQ queries, data operations |
| SavingsGoalServiceTests | `DotnetFinancialTrackerApp.Tests/SavingsGoalServiceTests.cs` | Lines 35-114 | Generic collections, business logic validation |
| Test Attributes | Throughout test files | `[Test]`, `[SetUp]`, `[TearDown]` | Proper test lifecycle management |

### LINQ with Lambda Expressions

| Feature | File Location | Line Numbers | Implementation |
|---------|---------------|--------------|----------------|
| Complex Filtering | `Services/TransactionsService.cs` | Lines 50-74 | Multiple `.Where()` clauses with lambda expressions |
| Ordering Operations | `Services/TransactionsService.cs` | Lines 77-81 | `.OrderByDescending()` with lambda selectors |
| Entity Framework Integration | `Services/TransactionsService.cs` | Lines 45-48 | `.Include()` for eager loading with navigation properties |
| Collection Operations | `Services/SavingsGoalService.cs` | Lines 42-47 | LINQ queries on `IEnumerable<T>` collections |

### Generic Collections and Types

| Usage | File Location | Implementation | Purpose |
|-------|---------------|----------------|---------|
| Generic Interfaces | `Services/ISavingsGoalService.cs` | `IEnumerable<SavingsGoal>`, `Task<List<T>>` | Type-safe collection contracts |
| Generic Methods | `Services/SavingsGoalService.cs` | Return types with `<T>` parameters | Reusable, type-safe operations |
| Collection Types | Throughout Services | `List<T>`, `IEnumerable<T>`, `Dictionary<TKey, TValue>` | Type-safe data structures |

## GUI Implementation

### Screen Requirements

| Screen | File Location | UI Elements | Functionality |
|--------|---------------|-------------|---------------|
| Dashboard | `Components/Pages/Home.razor` | Cards, navigation, quick actions | Financial overview with multiple UI components |
| Transaction Management | `Components/Pages/Finance.razor` | Tables, forms, buttons, dialogs | CRUD operations with comprehensive UI elements |
| Reports | `Components/Pages/Reports.razor` | Charts, filters, date pickers | Data visualization and analytics |
| Settings | `Components/Pages/Settings.razor` | Tabs, forms, validation | User preferences and security |

### UI Element Categories

| Element Type | Implementation Files | Count | Purpose |
|--------------|---------------------|-------|---------|
| Cards | `Components/UI/UiStatCard.razor` | Multiple | Information display |
| Forms | `Components/Dialogs/*.razor` | 5+ | Data input and validation |
| Tables | `Components/Pages/Finance.razor` | 3+ | Data presentation |
| Buttons | Throughout components | 20+ | User interactions |
| Navigation | `Components/Layout/BottomNav.razor` | 2 systems | App navigation |
| Dialogs | `Components/Dialogs/` folder | 4+ | Modal interactions |

## Bonus Features

### Blazor  Implementation

| Requirement | File Location | Implementation |
|-------------|---------------|----------------|
| MAUI + Blazor Setup | `MauiProgram.cs` | Lines 27-28 | Cross-platform web UI instead of Windows Forms |
| Configuration | `MainPage.xaml` | Blazor component | Blazor hosting in native app |

### Entity Framework with SQLite

| Component | File Location | Implementation |
|-----------|---------------|----------------|
| DbContext | `Data/AppDbContext.cs` | Lines 7-153 | Complete EF Core setup with relationships |
| Database Configuration | `MauiProgram.cs` | Lines 34-35 | SQLite connection string and service registration |
| Model Relationships | `Data/AppDbContext.cs` | Lines 55-116 | Foreign keys, navigation properties, constraints |
| Migrations | Auto-generated | Schema management | Automatic database creation and updates |

## Testing Instructions

### Running Unit Tests
```bash
# All tests
dotnet test

# Specific test file
dotnet test --filter "TransactionsServiceTests"

# Verbose output
dotnet test --verbosity normal
```

### Key Test Validations
1. **Polymorphism Tests** - Verify method overloading works correctly
2. **LINQ Tests** - Confirm lambda expressions filter data properly
3. **Generic Tests** - Validate type safety in collections
4. **Integration Tests** - Test database operations through services

## Project Architecture

### Core Service Layer
```
Services/
├── ITransactionsService.cs      # Interface definition
├── TransactionsService.cs       # Implementation with polymorphism
├── ISavingsGoalService.cs       # Generic interface example
├── SavingsGoalService.cs        # Generic implementation
└── [Other services...]          # Additional interface implementations
```

### Data Layer
```
Data/
└── AppDbContext.cs              # Entity Framework context
Models/
├── Transaction.cs               # Primary entity
├── SavingsGoal.cs              # Related entity
└── [Other models...]           # Supporting entities
```

### Test Coverage
```
DotnetFinancialTrackerApp.Tests/
├── TransactionsServiceTests.cs  # Core requirement validation
├── SavingsGoalServiceTests.cs   # Additional service testing
└── [Test utilities...]          # Testing infrastructure
```

## Verification Checklist

### Code Requirements
- Polymorphism: Method overloading in `TransactionsService.cs`
- Interfaces: Multiple interface definitions and implementations
- NUnit Tests: Comprehensive test coverage with proper attributes
- LINQ + Lambda: Query expressions throughout service layer
- Generics: Type-safe collections and method parameters

### Interface Requirements
- 4+ GUI Screens: Home, Finance, Reports, Settings,TransactionsDetails,Wellbeing,Login
- 6+ UI Elements: Cards, tables, forms, buttons, navigation, dialogs

### Bonus Features
- Blazor with MAUI: Cross-platform UI implementation
- Entity Framework: SQLite database with full ORM features
- External Database: SQLite database with LINQ queries

### Application Functionality
-  Builds without errors on target platform
-  All tests pass successfully
-  Database creates automatically on first run
-  Data loads correctly
-  UI components render properly across screens

## Troubleshooting

### Windows Build Issues
```cmd
# Clear caches and restore
dotnet nuget locals all --clear
dotnet workload install maui

# Platform-specific build
dotnet build -f net8.0-windows10.0.19041.0
```

### Test Execution Problems
```bash
# Clean and rebuild
dotnet clean
dotnet restore
dotnet build
dotnet test
```

This application demonstrates all required programming concepts through practical implementation in a real-world financial management scenario, with comprehensive testing and cross-platform deployment capabilities.