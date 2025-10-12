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
            .ConfigureFonts(fonts => {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                // Inter font loaded via Google Fonts in CSS - no local registration needed
            });

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
        builder.Services.AddScoped<ISavingsGoalService, SavingsGoalService>();
        // Removed: IGamificationService - not part of MVP
        builder.Services.AddScoped<IUserService, UserService>();
        // Removed: Wellbeing and AI services - not part of MVP

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
        if (!db.Users.Any())
        {
            // Create a default user with PIN 1234 for first-run convenience
            var salt = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(16));
            using var sha = System.Security.Cryptography.SHA256.Create();
            var hash = Convert.ToBase64String(sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes("1234:" + salt)));
            db.Users.Add(new Models.UserProfile { Name = "You", Salt = salt, PinHash = hash });
        }

        // Ensure family accounts and members exist first
        await EnsureFamilyDataExists(db);

        // Seed Budgets with proper relationships
        if (!db.Budgets.Any())
        {
            var month = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            var defaultFamily = await db.FamilyAccounts.FirstOrDefaultAsync();
            var defaultMember = await db.FamilyMembers.FirstOrDefaultAsync();

            if (defaultFamily != null && defaultMember != null)
            {
                // Create overall family budget
                var overallBudget = Models.Budget.CreateOverallFamilyBudget(
                    defaultFamily.FamilyId,
                    2600m,
                    defaultMember.MemberId);
                db.Budgets.Add(overallBudget);
            }
        }

        // Seed Transactions with proper relationships
        if (!db.Transactions.Any())
        {
            var defaultMember = await db.FamilyMembers.FirstOrDefaultAsync();
            var salaryCategory = await db.TransactionCategories.FirstOrDefaultAsync(c => c.Name == "Salary");
            var housingCategory = await db.TransactionCategories.FirstOrDefaultAsync(c => c.Name == "Housing");
            var groceriesCategory = await db.TransactionCategories.FirstOrDefaultAsync(c => c.Name == "Groceries");

            if (defaultMember != null && salaryCategory != null && housingCategory != null && groceriesCategory != null)
            {
                db.Transactions.AddRange(new[]
                {
                    new Models.Transaction
                    {
                        FamilyMemberId = defaultMember.MemberId,
                        CategoryId = salaryCategory.Id,
                        Amount = 5000,
                        Type = Models.TransactionType.Income,
                        Date = DateTime.Today.AddDays(-10),
                        Description = "Monthly Salary"
                    },
                    new Models.Transaction
                    {
                        FamilyMemberId = defaultMember.MemberId,
                        CategoryId = housingCategory.Id,
                        Amount = 1800,
                        Date = DateTime.Today.AddDays(-9),
                        Description = "Monthly Rent"
                    },
                    new Models.Transaction
                    {
                        FamilyMemberId = defaultMember.MemberId,
                        CategoryId = groceriesCategory.Id,
                        Amount = 45.50m,
                        Date = DateTime.Today.AddDays(-2),
                        Description = "Weekly Groceries"
                    },
                });
            }
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
            // Force database recreation to handle schema changes (including new Notes column)
            // This is safe for development - in production you'd use migrations
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();
        }
        catch
        {
            // As a last resort try recreating the database
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();
        }
    }

    private static async Task EnsureFamilyDataExists(AppDbContext db)
    {
        // This method ensures there's at least one family account and member for seeding
        if (!await db.FamilyAccounts.AnyAsync())
        {
            var familyAccount = new Models.FamilyAccount
            {
                FamilyId = Guid.NewGuid().ToString(),
                FamilyName = "Sample Family",
                TotalBalance = 2500m
            };
            db.FamilyAccounts.Add(familyAccount);

            var defaultMember = new Models.FamilyMember
            {
                FamilyId = familyAccount.FamilyId,
                Name = "Parent",
                Role = "Parent",
                Balance = 2500m,
                MonthlyAllowance = 0m,
                IsOnline = true,
                SpendingLimit = 1500m,
                SpentThisMonth = 800m,
                TransactionsThisMonth = 18,
                SavingsGoalProgress = 0.60,
                AchievementPoints = 120
            };
            db.FamilyMembers.Add(defaultMember);

            await db.SaveChangesAsync();
        }
    }
}
