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
        builder.Services.AddScoped<IFamilyMemberService, FamilyMemberService>();
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
        // Ensure a default family record exists for the simplified MVP workflow
        if (!db.FamilyAccounts.Any())
        {
            db.FamilyAccounts.Add(new FamilyAccount
            {
                FamilyId = "family-default",
                FamilyName = "Family"
            });
        }

        // Create default user if none exists
        if (!db.FamilyMembers.Any())
        {
            db.FamilyMembers.Add(new FamilyMember
            {
                Name = "You",
                Pin = "1234"
            });
        }

        await db.SaveChangesAsync();
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

    // EnsureFamilyDataExists method removed - no demo data seeding
}
