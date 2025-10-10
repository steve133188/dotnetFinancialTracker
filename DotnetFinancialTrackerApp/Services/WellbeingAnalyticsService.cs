using DotnetFinancialTrackerApp.Data;
using DotnetFinancialTrackerApp.Models;
using Microsoft.EntityFrameworkCore;

namespace DotnetFinancialTrackerApp.Services;

public interface IWellbeingAnalyticsService
{
    Task<Dictionary<string, WellbeingStats>> GetUserStatsAsync();
    Task<List<T>> GetItemsByPredicate<T>(Func<T, bool> predicate) where T : WellbeingItem;
    Task<TResult> AggregateData<T, TResult>(
        Func<IQueryable<T>, TResult> aggregateFunc) where T : WellbeingItem;
}

public class WellbeingAnalyticsService : IWellbeingAnalyticsService
{
    private readonly AppDbContext _context;

    public WellbeingAnalyticsService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Dictionary<string, WellbeingStats>> GetUserStatsAsync()
    {
        var hydrationEntries = await _context.HydrationEntries.ToListAsync();
        var medications = await _context.MedicationReminders.ToListAsync();
        var tasks = await _context.HouseholdTasks.ToListAsync();

        var allUsers = hydrationEntries.Select(h => h.AssignedTo)
            .Union(medications.Select(m => m.AssignedTo))
            .Union(tasks.Select(t => t.AssignedTo))
            .Distinct()
            .ToList();

        return allUsers.ToDictionary(user => user, user => new WellbeingStats
        {
            HydrationCompletionRate = CalculateHydrationRate(hydrationEntries, user),
            MedicationComplianceRate = CalculateMedicationRate(medications, user),
            TaskCompletionRate = CalculateTaskRate(tasks, user),
            TotalItems = hydrationEntries.Count(h => h.AssignedTo == user) +
                        medications.Count(m => m.AssignedTo == user) +
                        tasks.Count(t => t.AssignedTo == user)
        });
    }

    public async Task<List<T>> GetItemsByPredicate<T>(Func<T, bool> predicate) where T : WellbeingItem
    {
        return typeof(T).Name switch
        {
            nameof(HydrationEntry) => (await _context.HydrationEntries.ToListAsync())
                .Cast<T>()
                .Where(predicate)
                .ToList(),

            nameof(MedicationReminder) => (await _context.MedicationReminders.ToListAsync())
                .Cast<T>()
                .Where(predicate)
                .ToList(),

            nameof(HouseholdTask) => (await _context.HouseholdTasks.ToListAsync())
                .Cast<T>()
                .Where(predicate)
                .ToList(),

            _ => new List<T>()
        };
    }

    public async Task<TResult> AggregateData<T, TResult>(
        Func<IQueryable<T>, TResult> aggregateFunc) where T : WellbeingItem
    {
        return   typeof(T).Name switch
        {
            nameof(HydrationEntry) => aggregateFunc(_context.HydrationEntries.Cast<T>()),
            nameof(MedicationReminder) => aggregateFunc(_context.MedicationReminders.Cast<T>()),
            nameof(HouseholdTask) => aggregateFunc(_context.HouseholdTasks.Cast<T>()),
            _ => default(TResult)!
        };
    }

    private double CalculateHydrationRate(List<HydrationEntry> entries, string user)
    {
        var userEntries = entries.Where(h => h.AssignedTo == user).ToList();
        if (!userEntries.Any()) return 0.0;

        return userEntries.Average(h => h.GetCompletionPercentage());
    }

    private double CalculateMedicationRate(List<MedicationReminder> medications, string user)
    {
        var userMedications = medications.Where(m => m.AssignedTo == user).ToList();
        if (!userMedications.Any()) return 0.0;

        var completed = userMedications.Count(m => m.IsCompleted);
        return (double)completed / userMedications.Count * 100.0;
    }

    private double CalculateTaskRate(List<HouseholdTask> tasks, string user)
    {
        var userTasks = tasks.Where(t => t.AssignedTo == user).ToList();
        if (!userTasks.Any()) return 0.0;

        var completed = userTasks.Count(t => t.IsCompleted);
        return (double)completed / userTasks.Count * 100.0;
    }
}

public class WellbeingStats
{
    public double HydrationCompletionRate { get; set; }
    public double MedicationComplianceRate { get; set; }
    public double TaskCompletionRate { get; set; }
    public int TotalItems { get; set; }

    public double OverallWellbeingScore =>
        (HydrationCompletionRate + MedicationComplianceRate + TaskCompletionRate) / 3.0;
}