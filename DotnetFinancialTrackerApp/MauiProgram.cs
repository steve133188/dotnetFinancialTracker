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

        db.SaveChanges();
    }

    private static void EnsureSchemaUpToDate(AppDbContext db)
    {
        try
        {
            var required = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "Transactions", "Budgets", "Achievements", "GamificationStates", "Users",
                "HydrationEntries", "MedicationReminders", "HouseholdTasks"
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
