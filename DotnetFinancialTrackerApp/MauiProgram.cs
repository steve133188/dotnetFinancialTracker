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

namespace DotnetFinancialTrackerApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts => { fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular"); });

        builder.Services.AddMauiBlazorWebView();
        builder.Services.AddMudServices();

        // Initialize SQLite (required on iOS/MacCatalyst)
        Batteries_V2.Init();

        // SQLite EF Core setup
        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "financial-tracker.db");
        builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite($"Data Source={dbPath}"));
        builder.Services.AddScoped<ITransactionsService, TransactionsService>();
        builder.Services.AddScoped<ITransactionTemplateService, TransactionTemplateService>();
        builder.Services.AddScoped<IBudgetsService, BudgetsService>();
        builder.Services.AddScoped<IGamificationService, GamificationService>();
        builder.Services.AddScoped<IExportService, ExportService>();
        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddScoped<IHydrationService, HydrationService>();
        builder.Services.AddScoped<IMedicationService, MedicationService>();
        builder.Services.AddScoped<IHouseholdTaskService, HouseholdTaskService>();
        builder.Services.AddScoped<IWellbeingAnalyticsService, WellbeingAnalyticsService>();
        builder.Services.AddScoped<IValidationService, ValidationService>();
        builder.Services.AddScoped<INotificationService, NotificationService>();

        // Family Banking Services
        builder.Services.AddScoped<IFamilyBankingService, FamilyBankingService>();
        builder.Services.AddScoped<IFamilyAIService, FamilyAIService>();

        builder.Services.AddSingleton<AuthState>();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        var app = builder.Build();

        // Ensure database exists and schema is up-to-date on startup
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated();
            EnsureSchemaUpToDate(db);
            Seed(db);

            // Seed transaction templates after main seeding
            var templateService = scope.ServiceProvider.GetRequiredService<ITransactionTemplateService>();
            templateService.SeedDefaultTemplatesAsync().Wait();
        }

        return app;
    }

    private static void Seed(AppDbContext db)
    {
        if (!db.Users.Any())
        {
            // Create a default user with PIN 1234 for first-run convenience
            var salt = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(16));
            using var sha = System.Security.Cryptography.SHA256.Create();
            var hash = Convert.ToBase64String(sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes("1234:" + salt)));
            db.Users.Add(new Models.UserProfile { Name = "You", Salt = salt, PinHash = hash });
        }

        if (!db.Budgets.Any())
        {
            var month = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            db.Budgets.AddRange(new[]
            {
                new Models.Budget { Category = "Food", Limit = 600, Month = month },
                new Models.Budget { Category = "Transport", Limit = 200, Month = month },
                new Models.Budget { Category = "Rent", Limit = 1800, Month = month },
            });
        }

        if (!db.Transactions.Any())
        {
            db.Transactions.AddRange(new[]
            {
                new Models.Transaction { User = "Parent", Category = "Salary", Amount = 5000, IsIncome = true, Date = DateTime.Today.AddDays(-10), Description = "Monthly" },
                new Models.Transaction { User = "Parent", Category = "Rent", Amount = 1800, Date = DateTime.Today.AddDays(-9), Description = "Apartment" },
                new Models.Transaction { User = "Child", Category = "Food", Amount = 45.50m, Date = DateTime.Today.AddDays(-2), Description = "Groceries" },
            });
        }

        // Seed Family Banking data
        if (!db.FamilyAccounts.Any())
        {
            // Create a sample family account
            var familyAccount = new Models.FamilyAccount
            {
                FamilyName = "Smith Family",
                TotalBalance = 4495.50m,
                CreatedAt = DateTime.UtcNow.AddDays(-30),
                UpdatedAt = DateTime.UtcNow
            };
            db.FamilyAccounts.Add(familyAccount);

            // Create family members
            var parentMember = new Models.FamilyMember
            {
                FamilyId = familyAccount.FamilyId,
                Name = "Mom (Sarah)",
                Role = "Parent",
                Balance = 2500m,
                MonthlyAllowance = 0m,
                IsOnline = true,
                LastActivity = DateTime.UtcNow.AddMinutes(-15),
                SpendingLimit = 5000m,
                SpentThisMonth = 1200m,
                TransactionsThisMonth = 23,
                SavingsGoalProgress = 0.75,
                AchievementPoints = 150
            };
            db.FamilyMembers.Add(parentMember);

            var dadMember = new Models.FamilyMember
            {
                FamilyId = familyAccount.FamilyId,
                Name = "Dad (Mike)",
                Role = "Parent",
                Balance = 1800m,
                MonthlyAllowance = 0m,
                IsOnline = false,
                LastActivity = DateTime.UtcNow.AddHours(-2),
                SpendingLimit = 4000m,
                SpentThisMonth = 980m,
                TransactionsThisMonth = 18,
                SavingsGoalProgress = 0.60,
                AchievementPoints = 120
            };
            db.FamilyMembers.Add(dadMember);

            var teenMember = new Models.FamilyMember
            {
                FamilyId = familyAccount.FamilyId,
                Name = "Emma",
                Role = "Teen",
                Balance = 150m,
                MonthlyAllowance = 200m,
                IsOnline = true,
                LastActivity = DateTime.UtcNow.AddMinutes(-5),
                SpendingLimit = 300m,
                SpentThisMonth = 180m,
                TransactionsThisMonth = 12,
                SavingsGoalProgress = 0.40,
                AchievementPoints = 85
            };
            db.FamilyMembers.Add(teenMember);

            var childMember = new Models.FamilyMember
            {
                FamilyId = familyAccount.FamilyId,
                Name = "Jake",
                Role = "Child",
                Balance = 45.50m,
                MonthlyAllowance = 50m,
                IsOnline = false,
                LastActivity = DateTime.UtcNow.AddHours(-4),
                SpendingLimit = 75m,
                SpentThisMonth = 35m,
                TransactionsThisMonth = 6,
                SavingsGoalProgress = 0.80,
                AchievementPoints = 65
            };
            db.FamilyMembers.Add(childMember);

            // Create virtual cards
            db.VirtualCards.AddRange(new[]
            {
                new Models.VirtualCard { MemberId = parentMember.MemberId, DisplayNumber = "4521", DailyLimit = 500m, CardColor = "#01FFFF", ExpiryDate = DateTime.UtcNow.AddYears(3) },
                new Models.VirtualCard { MemberId = dadMember.MemberId, DisplayNumber = "7834", DailyLimit = 400m, CardColor = "#01FFFF", ExpiryDate = DateTime.UtcNow.AddYears(3) },
                new Models.VirtualCard { MemberId = teenMember.MemberId, DisplayNumber = "2910", DailyLimit = 100m, CardColor = "#FF6B6B", ExpiryDate = DateTime.UtcNow.AddYears(3) },
                new Models.VirtualCard { MemberId = childMember.MemberId, DisplayNumber = "5647", DailyLimit = 25m, CardColor = "#4ECDC4", ExpiryDate = DateTime.UtcNow.AddYears(3) }
            });

            // Create family goals
            db.FamilyGoals.AddRange(new[]
            {
                new Models.FamilyGoal
                {
                    FamilyId = familyAccount.FamilyId,
                    Title = "Vacation Fund",
                    Description = "Summer family vacation to Hawaii",
                    CurrentAmount = 2400m,
                    TargetAmount = 5000m,
                    TargetDate = DateTime.UtcNow.AddMonths(6),
                    Type = Models.GoalType.Vacation,
                    Priority = Models.GoalPriority.High
                },
                new Models.FamilyGoal
                {
                    FamilyId = familyAccount.FamilyId,
                    Title = "Emergency Fund",
                    Description = "6 months of family expenses",
                    CurrentAmount = 8500m,
                    TargetAmount = 10000m,
                    TargetDate = DateTime.UtcNow.AddMonths(3),
                    Type = Models.GoalType.Emergency,
                    Priority = Models.GoalPriority.Critical
                },
                new Models.FamilyGoal
                {
                    FamilyId = familyAccount.FamilyId,
                    Title = "College Fund",
                    Description = "Emma's college education savings",
                    CurrentAmount = 1200m,
                    TargetAmount = 1000m,
                    TargetDate = DateTime.UtcNow.AddMonths(-1),
                    Type = Models.GoalType.Education,
                    Priority = Models.GoalPriority.High,
                    IsArchived = false
                }
            });
        }

        db.SaveChanges();
    }

    private static void EnsureSchemaUpToDate(AppDbContext db)
    {
        try
        {
            var required = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "Transactions", "TransactionTemplates", "Budgets", "Achievements", "GamificationStates", "Users",
                "HydrationEntries", "MedicationReminders", "HouseholdTasks",
                "FamilyAccounts", "FamilyMembers", "VirtualCards", "FamilyGoals", "FamilyInsights",
                "SpendingLimits", "FamilyMemberGoals", "GoalContributions", "CardTransactions"
            };
            var existing = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            DbConnection conn = db.Database.GetDbConnection();
            var wasClosed = conn.State == System.Data.ConnectionState.Closed;
            if (wasClosed) conn.Open();
            try
            {
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table'";
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var name = reader.GetString(0);
                    existing.Add(name);
                }
            }
            finally
            {
                if (wasClosed) conn.Close();
            }

            if (!required.IsSubsetOf(existing))
            {
                // Recreate schema if tables are missing (dev convenience)
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();
            }
        }
        catch
        {
            // As a last resort try recreating the database
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();
        }
    }
}
