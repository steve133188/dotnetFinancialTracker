using Microsoft.Extensions.Logging;
using MudBlazor.Services;
using Microsoft.EntityFrameworkCore;
using DotnetFinancialTrackerApp.Data;
using DotnetFinancialTrackerApp.Models;
using DotnetFinancialTrackerApp.Services;
using Microsoft.Maui.Storage;
using SQLitePCL;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Data.Common;
using System.Linq;

namespace DotnetFinancialTrackerApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts => {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                // Inter font loaded via Google Fonts in CSS - no local registration needed
            });

        builder.Services.AddMauiBlazorWebView();
        builder.Services.AddMudServices();
        builder.Services.AddBlazorWebViewDeveloperTools();
        // Initialize SQLite (required on iOS/MacCatalyst)
        Batteries_V2.Init();

        // SQLite EF Core setup
        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "financial-tracker.db");
        builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite($"Data Source={dbPath}"));
        // Register service interfaces with dependency injection.
        builder.Services.AddScoped<ITransactionsService, TransactionsService>();
        builder.Services.AddScoped<ITransactionTemplateService, TransactionTemplateService>();
        builder.Services.AddScoped<IBudgetsService, BudgetsService>();
        builder.Services.AddScoped<ISavingsGoalService, SavingsGoalService>();
        builder.Services.AddScoped<IFamilyMemberService, FamilyMemberService>();
        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddScoped<IInsightService, InsightService>();
        builder.Services.AddScoped<IWellbeingDataService, WellbeingDataService>();
        builder.Services.AddScoped(typeof(IFilterService<>), typeof(FilterService<>));


        builder.Services.AddSingleton<AuthState>();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        var app = builder.Build();

        // Ensure database exists and schema is up-to-date on startup
        try
        {
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                EnsureSchemaUpToDate(db);
                Seed(db).Wait();

                // Seed transaction templates after main seeding
                var templateService = scope.ServiceProvider.GetRequiredService<ITransactionTemplateService>();
                templateService.SeedDefaultTemplatesAsync().Wait();
            }
        }
        catch (Exception ex)
        {
            // Log the error but don't crash the app
            System.Diagnostics.Debug.WriteLine($"Database initialization error: {ex.Message}");
            // The app can still run, database will be created on first access
        }

        return app;
    }

    private static async Task Seed(AppDbContext db)
    {
        if (db.Transactions.Any())
        {
            return;
        }

        var today = DateTime.Today;
        var currentMonthStart = new DateTime(today.Year, today.Month, 1);
        var previousMonthStart = currentMonthStart.AddMonths(-1);
        var twoMonthsAgoStart = currentMonthStart.AddMonths(-2);
        var familyId = "family-default";

        var familyAccount = await db.FamilyAccounts.FirstOrDefaultAsync(f => f.FamilyId == familyId);
        if (familyAccount == null)
        {
            familyAccount = new FamilyAccount
            {
                FamilyId = familyId,
                FamilyName = "Taylor Household",
                TotalBalance = 0m,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                BankAccountNumber = "****8214",
                BankSortCode = "11-22-33"
            };
            db.FamilyAccounts.Add(familyAccount);
        }
        else
        {
            familyAccount.FamilyName = "Taylor Household";
            familyAccount.BankAccountNumber ??= "****8214";
            familyAccount.BankSortCode ??= "11-22-33";
        }

        var members = new List<FamilyMember>
        {
            new()
            {
                Id = "member-alex",
                Name = "Alex Taylor",
                Pin = "1234",
                CreatedAt = DateTime.UtcNow.AddMonths(-6)
            },
            new()
            {
                Id = "member-jamie",
                Name = "Jamie Taylor",
                Pin = "2345",
                CreatedAt = DateTime.UtcNow.AddMonths(-5)
            },
            new()
            {
                Id = "member-sam",
                Name = "Sam Taylor",
                Pin = "3456",
                CreatedAt = DateTime.UtcNow.AddMonths(-3)
            }
        };

        foreach (var member in members)
        {
            if (!await db.FamilyMembers.AnyAsync(m => m.Id == member.Id))
            {
                db.FamilyMembers.Add(member);
            }
        }

        await db.SaveChangesAsync();

        if (!db.Budgets.Any())
        {
            db.Budgets.AddRange(new[]
            {
                new Budget
                {
                    FamilyId = familyId,
                    Limit = 4200m,
                    Month = currentMonthStart,
                    Name = $"{currentMonthStart:MMMM} Budget",
                    Description = "Current month family-wide spending plan.",
                    CreatedByMemberId = "member-alex",
                    CreatedAt = currentMonthStart.AddDays(-1),
                    UpdatedAt = DateTime.UtcNow
                },
                new Budget
                {
                    FamilyId = familyId,
                    Limit = 4000m,
                    Month = previousMonthStart,
                    Name = $"{previousMonthStart:MMMM} Budget",
                    Description = "Historical budget snapshot for comparison.",
                    CreatedByMemberId = "member-alex",
                    CreatedAt = previousMonthStart.AddDays(-1),
                    UpdatedAt = previousMonthStart.AddMonths(1).AddDays(1)
                },
                new Budget
                {
                    FamilyId = familyId,
                    Limit = 3800m,
                    Month = twoMonthsAgoStart,
                    Name = $"{twoMonthsAgoStart:MMMM} Budget",
                    Description = "Baseline month to show trend improvements.",
                    CreatedByMemberId = "member-jamie",
                    CreatedAt = twoMonthsAgoStart.AddDays(-1),
                    UpdatedAt = twoMonthsAgoStart.AddMonths(1).AddDays(1)
                }
            });
        }

        var savingsGoals = new List<SavingsGoal>();
        if (!db.SavingsGoals.Any())
        {
            savingsGoals.AddRange(new[]
            {
                new SavingsGoal
                {
                    Title = "Emergency Cushion",
                    Subtitle = "3 months of essential expenses",
                    Description = "Building a safety net so the family can cover living costs without stress.",
                    TargetAmount = 5000m,
                    CurrentAmount = 0m,
                    Category = "Emergency",
                    CreatedDate = twoMonthsAgoStart,
                    TargetDate = currentMonthStart.AddMonths(6),
                    FamilyId = familyId,
                    CreatedByMemberId = "member-alex",
                    Priority = GoalPriority.Critical,
                    Color = "#111827"
                },
                new SavingsGoal
                {
                    Title = "Summer Getaway",
                    Subtitle = "School holiday trip to the coast",
                    Description = "Family vacation fund to cover travel, lodging, and experiences for August break.",
                    TargetAmount = 2400m,
                    CurrentAmount = 0m,
                    Category = "Vacation",
                    CreatedDate = previousMonthStart,
                    TargetDate = currentMonthStart.AddMonths(2),
                    FamilyId = familyId,
                    CreatedByMemberId = "member-jamie",
                    Priority = GoalPriority.High,
                    Color = "#2563EB"
                },
                new SavingsGoal
                {
                    Title = "College Fund",
                    Subtitle = "Sam's first-year tuition savings",
                    Description = "Long-term savings for Sam's university costs starting next autumn.",
                    TargetAmount = 15000m,
                    CurrentAmount = 0m,
                    Category = "Education",
                    CreatedDate = twoMonthsAgoStart,
                    TargetDate = currentMonthStart.AddMonths(18),
                    FamilyId = familyId,
                    CreatedByMemberId = "member-alex",
                    Priority = GoalPriority.High,
                    Color = "#14B8A6"
                }
            });

            db.SavingsGoals.AddRange(savingsGoals);
            await db.SaveChangesAsync();
        }
        else
        {
            savingsGoals = await db.SavingsGoals.ToListAsync();
        }

        var emergencyGoal = savingsGoals.FirstOrDefault(g => g.Title == "Emergency Cushion");
        var getawayGoal = savingsGoals.FirstOrDefault(g => g.Title == "Summer Getaway");
        var collegeGoal = savingsGoals.FirstOrDefault(g => g.Title == "College Fund");

        if (!db.SavingsGoalContributions.Any())
        {
            var contributions = new List<SavingsGoalContribution>();

            if (emergencyGoal != null)
            {
                contributions.AddRange(new[]
                {
                    new SavingsGoalContribution
                    {
                        SavingsGoalId = emergencyGoal.Id,
                        Amount = 800m,
                        ContributorMemberId = "member-alex",
                        Description = "Initial deposit from salary",
                        ContributionDate = previousMonthStart.AddDays(4)
                    },
                    new SavingsGoalContribution
                    {
                        SavingsGoalId = emergencyGoal.Id,
                        Amount = 450m,
                        ContributorMemberId = "member-jamie",
                        Description = "Shared contribution after freelance payout",
                        ContributionDate = currentMonthStart.AddDays(3)
                    },
                    new SavingsGoalContribution
                    {
                        SavingsGoalId = emergencyGoal.Id,
                        Amount = 350m,
                        ContributorMemberId = "member-alex",
                        Description = "Automatic transfer to emergency cushion",
                        ContributionDate = currentMonthStart.AddDays(12)
                    }
                });
            }

            if (getawayGoal != null)
            {
                contributions.AddRange(new[]
                {
                    new SavingsGoalContribution
                    {
                        SavingsGoalId = getawayGoal.Id,
                        Amount = 500m,
                        ContributorMemberId = "member-jamie",
                        Description = "Vacation savings kick-off",
                        ContributionDate = previousMonthStart.AddDays(10)
                    },
                    new SavingsGoalContribution
                    {
                        SavingsGoalId = getawayGoal.Id,
                        Amount = 250m,
                        ContributorMemberId = "member-sam",
                        Description = "Allowance savings for activities",
                        ContributionDate = currentMonthStart.AddDays(8)
                    }
                });
            }

            if (collegeGoal != null)
            {
                contributions.Add(new SavingsGoalContribution
                {
                    SavingsGoalId = collegeGoal.Id,
                    Amount = 900m,
                    ContributorMemberId = "member-alex",
                    Description = "Monthly college deposit",
                    ContributionDate = currentMonthStart.AddDays(6)
                });
            }

            if (contributions.Count > 0)
            {
                db.SavingsGoalContributions.AddRange(contributions);

                foreach (var goal in savingsGoals)
                {
                    var total = contributions
                        .Where(c => c.SavingsGoalId == goal.Id && !c.IsReversal)
                        .Sum(c => c.Amount);
                    goal.CurrentAmount = total;
                }
            }
        }

        if (!db.FamilyGoals.Any())
        {
            db.FamilyGoals.AddRange(new[]
            {
                new FamilyGoal
                {
                    GoalId = "goal-upgrade-car",
                    Title = "Upgrade Family Car",
                    Description = "Set aside funds for a safer hybrid SUV before winter.",
                    TargetAmount = 22000m,
                    CurrentAmount = 5200m,
                    TargetDate = currentMonthStart.AddMonths(9),
                    Type = GoalType.Purchase,
                    Priority = GoalPriority.High,
                    Color = "#F97316",
                    FamilyId = familyId,
                    CreatedAt = twoMonthsAgoStart,
                    UpdatedAt = currentMonthStart
                },
                new FamilyGoal
                {
                    GoalId = "goal-backyard-refresh",
                    Title = "Backyard Refresh",
                    Description = "Create a welcoming outdoor space for weekend hangouts and gardening.",
                    TargetAmount = 4500m,
                    CurrentAmount = 1800m,
                    TargetDate = currentMonthStart.AddMonths(4),
                    Type = GoalType.Family,
                    Priority = GoalPriority.Medium,
                    Color = "#10B981",
                    FamilyId = familyId,
                    CreatedAt = previousMonthStart,
                    UpdatedAt = currentMonthStart
                }
            });
        }

        var categoryLookup = await db.TransactionCategories.ToDictionaryAsync(c => c.Name, c => c.Id);
        int GetCategoryId(string categoryName)
        {
            return categoryLookup.TryGetValue(categoryName, out var id)
                ? id
                : categoryLookup.Values.First();
        }

        if (!db.Transactions.Any())
        {
            var transactions = new List<Transaction>
            {
                new()
                {
                    FamilyMemberId = "member-alex",
                    CategoryId = GetCategoryId("Salary"),
                    Amount = 4200m,
                    Date = currentMonthStart.AddDays(1),
                    Description = "Alex salary deposit",
                    Notes = "Includes performance bonus adjustments",
                    Type = TransactionType.Income,
                    CreatedAt = currentMonthStart.AddDays(1),
                    UpdatedAt = currentMonthStart.AddDays(1)
                },
                new()
                {
                    FamilyMemberId = "member-jamie",
                    CategoryId = GetCategoryId("Freelance"),
                    Amount = 1200m,
                    Date = currentMonthStart.AddDays(2),
                    Description = "Jamie client project payout",
                    Notes = "Design sprint completion",
                    Type = TransactionType.Income,
                    CreatedAt = currentMonthStart.AddDays(2),
                    UpdatedAt = currentMonthStart.AddDays(2)
                },
                new()
                {
                    FamilyMemberId = "member-alex",
                    CategoryId = GetCategoryId("Investment"),
                    Amount = 250m,
                    Date = currentMonthStart.AddDays(5),
                    Description = "Quarterly ETF dividends",
                    Type = TransactionType.Income,
                    CreatedAt = currentMonthStart.AddDays(5),
                    UpdatedAt = currentMonthStart.AddDays(5)
                },
                new()
                {
                    FamilyMemberId = "member-alex",
                    CategoryId = GetCategoryId("Housing"),
                    Amount = 1800m,
                    Date = currentMonthStart.AddDays(2),
                    Description = "Mortgage and utilities",
                    Notes = "Covers electricity, gas, and water",
                    Type = TransactionType.Expense,
                    CreatedAt = currentMonthStart.AddDays(2),
                    UpdatedAt = currentMonthStart.AddDays(2)
                },
                new()
                {
                    FamilyMemberId = "member-jamie",
                    CategoryId = GetCategoryId("Groceries"),
                    Amount = 320m,
                    Date = currentMonthStart.AddDays(4),
                    Description = "Weekly groceries",
                    Notes = "Farmer's market and supermarket",
                    Type = TransactionType.Expense,
                    CreatedAt = currentMonthStart.AddDays(4),
                    UpdatedAt = currentMonthStart.AddDays(4)
                },
                new()
                {
                    FamilyMemberId = "member-sam",
                    CategoryId = GetCategoryId("Dining Out"),
                    Amount = 85m,
                    Date = currentMonthStart.AddDays(6),
                    Description = "Family diner night",
                    Notes = "Celebrating report card win",
                    Type = TransactionType.Expense,
                    CreatedAt = currentMonthStart.AddDays(6),
                    UpdatedAt = currentMonthStart.AddDays(6)
                },
                new()
                {
                    FamilyMemberId = "member-alex",
                    CategoryId = GetCategoryId("Transportation"),
                    Amount = 140m,
                    Date = currentMonthStart.AddDays(7),
                    Description = "Fuel and transit top-ups",
                    Type = TransactionType.Expense,
                    CreatedAt = currentMonthStart.AddDays(7),
                    UpdatedAt = currentMonthStart.AddDays(7)
                },
                new()
                {
                    FamilyMemberId = "member-jamie",
                    CategoryId = GetCategoryId("Education"),
                    Amount = 200m,
                    Date = currentMonthStart.AddDays(10),
                    Description = "STEM summer camp registration",
                    Type = TransactionType.Expense,
                    CreatedAt = currentMonthStart.AddDays(10),
                    UpdatedAt = currentMonthStart.AddDays(10),
                    RelatedSavingsGoalId = collegeGoal?.Id
                },
                new()
                {
                    FamilyMemberId = "member-jamie",
                    CategoryId = GetCategoryId("Travel"),
                    Amount = 300m,
                    Date = currentMonthStart.AddDays(11),
                    Description = "Vacation rental deposit",
                    Notes = "Reserved beachfront cabin",
                    Type = TransactionType.Expense,
                    CreatedAt = currentMonthStart.AddDays(11),
                    UpdatedAt = currentMonthStart.AddDays(11),
                    RelatedSavingsGoalId = getawayGoal?.Id
                },
                new()
                {
                    FamilyMemberId = "member-alex",
                    CategoryId = GetCategoryId("Entertainment"),
                    Amount = 60m,
                    Date = currentMonthStart.AddDays(9),
                    Description = "Streaming and gaming subscriptions",
                    Type = TransactionType.Expense,
                    CreatedAt = currentMonthStart.AddDays(9),
                    UpdatedAt = currentMonthStart.AddDays(9)
                },
                new()
                {
                    FamilyMemberId = "member-alex",
                    CategoryId = GetCategoryId("Salary"),
                    Amount = 4100m,
                    Date = previousMonthStart.AddDays(2),
                    Description = "Alex salary deposit",
                    Type = TransactionType.Income,
                    CreatedAt = previousMonthStart.AddDays(2),
                    UpdatedAt = previousMonthStart.AddDays(2)
                },
                new()
                {
                    FamilyMemberId = "member-jamie",
                    CategoryId = GetCategoryId("Freelance"),
                    Amount = 950m,
                    Date = previousMonthStart.AddDays(3),
                    Description = "Freelance UX audit",
                    Type = TransactionType.Income,
                    CreatedAt = previousMonthStart.AddDays(3),
                    UpdatedAt = previousMonthStart.AddDays(3)
                },
                new()
                {
                    FamilyMemberId = "member-jamie",
                    CategoryId = GetCategoryId("Groceries"),
                    Amount = 310m,
                    Date = previousMonthStart.AddDays(5),
                    Description = "Weekly groceries",
                    Type = TransactionType.Expense,
                    CreatedAt = previousMonthStart.AddDays(5),
                    UpdatedAt = previousMonthStart.AddDays(5)
                },
                new()
                {
                    FamilyMemberId = "member-alex",
                    CategoryId = GetCategoryId("Housing"),
                    Amount = 1800m,
                    Date = previousMonthStart.AddDays(6),
                    Description = "Mortgage and utilities",
                    Type = TransactionType.Expense,
                    CreatedAt = previousMonthStart.AddDays(6),
                    UpdatedAt = previousMonthStart.AddDays(6)
                },
                new()
                {
                    FamilyMemberId = "member-sam",
                    CategoryId = GetCategoryId("Entertainment"),
                    Amount = 45m,
                    Date = previousMonthStart.AddDays(12),
                    Description = "Movie outing with friends",
                    Type = TransactionType.Expense,
                    CreatedAt = previousMonthStart.AddDays(12),
                    UpdatedAt = previousMonthStart.AddDays(12)
                },
                new()
                {
                    FamilyMemberId = "member-alex",
                    CategoryId = GetCategoryId("Travel"),
                    Amount = 150m,
                    Date = previousMonthStart.AddDays(15),
                    Description = "Flights reservation hold",
                    Type = TransactionType.Expense,
                    CreatedAt = previousMonthStart.AddDays(15),
                    UpdatedAt = previousMonthStart.AddDays(15),
                    RelatedSavingsGoalId = getawayGoal?.Id
                }
            };

            db.Transactions.AddRange(transactions);

            var totalIncome = transactions
                .Where(t => t.Type == TransactionType.Income)
                .Sum(t => t.Amount);
            var totalExpenses = transactions
                .Where(t => t.Type == TransactionType.Expense)
                .Sum(t => t.Amount);

            familyAccount.TotalBalance = totalIncome - totalExpenses;
            familyAccount.UpdatedAt = DateTime.UtcNow;
        }

        await db.SaveChangesAsync();
    }

    private static void EnsureSchemaUpToDate(AppDbContext db)
    {
        try
        {
            if (db.Database.GetPendingMigrations().Any())
            {
                db.Database.Migrate();
            }
            else
            {
                db.Database.EnsureCreated();
            }
        }
        catch
        {
            db.Database.EnsureCreated();
        }
    }

    // EnsureFamilyDataExists method removed - no demo data seeding
}
