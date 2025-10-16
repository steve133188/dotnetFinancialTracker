# Dotnet Financial Tracker – Assignment Spec Mapping

The table below maps each requirement from the Spring 2025 Assignment‑2 specification to the concrete implementation in this repository so markers can jump straight to the evidence.

| Spec Requirement | Implementation Summary | Key Paths |
| - | - | - |
| GUI / Interface design (≥4 responsive screens & ≥6 UI element categories) | MudBlazor pages for Home, Finance, Reports, Wellbeing and Settings deliver responsive layouts with cards, charts, tables, button groups, dialogs and selectors. | `DotnetFinancialTrackerApp/Components/Pages/Home.razor`, `.../Finance.razor`, `.../Reports.razor`, `.../Wellbeing.razor`, `.../Settings.razor` |
| Communication between interfaces | Shared `AuthState` plus `NavigationManager` and injected services coordinate navigation and cross-page state (e.g. finance filters updating charts). | `DotnetFinancialTrackerApp/Services/AuthState.cs`, `DotnetFinancialTrackerApp/Components/Layout/MainLayout.razor`, `DotnetFinancialTrackerApp/Components/Pages/Finance.razor` |
| Collections, generics, delegates / LINQ | Services compose generic collections with LINQ/lambda filters for budgeting & savings analytics. | `DotnetFinancialTrackerApp/Services/TransactionsService.cs#L18-L66`, `DotnetFinancialTrackerApp/Services/SavingsGoalService.cs#L24-L108` |
| Enumerators, properties, extension methods | Enum-driven workflow with extension helpers for display metadata. | `DotnetFinancialTrackerApp/Models/TransactionType.cs` |
| File / database I/O & Entity Framework | SQLite-backed EF Core context seeds data and is consumed via dependency-injected services. | `DotnetFinancialTrackerApp/Data/AppDbContext.cs`, `DotnetFinancialTrackerApp/MauiProgram.cs` |
| High cohesion & low coupling (interfaces, DI) | Service interfaces with scoped registrations keep UI separated from persistence logic. | `DotnetFinancialTrackerApp/Services/ISavingsGoalService.cs`, `DotnetFinancialTrackerApp/MauiProgram.cs` |
| Polymorphism (inheritance / overloading) | `TransactionsService.GetAsync()` overloads enable flexible retrieval for dashboards and satisfy the polymorphism requirement. | `DotnetFinancialTrackerApp/Services/TransactionsService.cs#L13-L62` |
| Anonymous methods with LINQ | Filtering & projection use lambda expressions over EF Core queries and in-page analytics. | `DotnetFinancialTrackerApp/Services/TransactionsService.cs#L24-L62`, `DotnetFinancialTrackerApp/Components/Pages/Reports.razor#L220-L360` |
| Interfaces (≥2 examples) | Multiple service contracts drive DI (budgets, savings, transactions, users). | `DotnetFinancialTrackerApp/Services/IBudgetsService.cs`, `DotnetFinancialTrackerApp/Services/ISavingsGoalService.cs` |
| Generics / generic collections | Application models and services rely on `List<>`, `IEnumerable<>`, and EF Core generics. | `DotnetFinancialTrackerApp/Services/SavingsGoalService.cs`, `DotnetFinancialTrackerApp/Models/SavingsGoal.cs` |
| NUnit test cases | Automated tests cover transaction querying, contribution logic, and summary analytics. Run with `dotnet test`. | `DotnetFinancialTrackerApp.Tests/TransactionsServiceTests.cs`, `DotnetFinancialTrackerApp.Tests/SavingsGoalServiceTests.cs` |
| Bonus – Entity Framework / external DB | SQLite database with EF Core migration seeding satisfies bonus database criterion. | `DotnetFinancialTrackerApp/MauiProgram.cs`, `DotnetFinancialTrackerApp/Data/AppDbContext.cs` |

## Tests

```bash
dotnet test
```

The suite exercises the LINQ-heavy transaction filters, savings goal contributions, and summary analytics to demonstrate the required NUnit coverage.

## Build Commands

- **macOS (MacCatalyst):**
  ```bash
  dotnet build DotnetFinancialTrackerApp/DotnetFinancialTrackerApp.csproj -t:Run -f net8.0-maccatalyst -c Debug
  ```
- **Windows:**
  ```powershell
  dotnet build DotnetFinancialTrackerApp/DotnetFinancialTrackerApp.csproj -t:Run -f net8.0-windows10.0.19041.0 -c Debug
  ```
