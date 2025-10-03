using DotnetFinancialTrackerApp.Models;

namespace DotnetFinancialTrackerApp.Services;

public interface IWellbeingService<T> where T : WellbeingItem
{
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> GetByUserAsync(string user);
    Task<T?> GetByIdAsync(int id);
    Task<T> CreateAsync(T item);
    Task<T> UpdateAsync(T item);
    Task DeleteAsync(int id);
}

public interface IHydrationService : IWellbeingService<HydrationEntry>
{
    Task<HydrationEntry?> GetTodayEntryAsync(string user);
    Task<IEnumerable<HydrationEntry>> GetWeeklyDataAsync(string user, DateTime startDate);
    Task<double> GetAverageCompletionAsync(string user, int days = 7);
}

public interface IMedicationService : IWellbeingService<MedicationReminder>
{
    Task<IEnumerable<MedicationReminder>> GetTodayRemindersAsync(string user);
    Task<IEnumerable<MedicationReminder>> GetOverdueRemindersAsync(string user);
    Task<IEnumerable<MedicationReminder>> GetUpcomingRemindersAsync(string user, int hours = 2);
}

public interface IHouseholdTaskService : IWellbeingService<HouseholdTask>
{
    Task<IEnumerable<HouseholdTask>> GetTasksByStatusAsync(string user, bool completed);
    Task<IEnumerable<HouseholdTask>> GetOverdueTasksAsync(string user);
    Task<IEnumerable<HouseholdTask>> GetTasksByCategoryAsync(string user, TaskCategory category);
    Task<Dictionary<TaskCategory, int>> GetTaskSummaryAsync(string user);
}