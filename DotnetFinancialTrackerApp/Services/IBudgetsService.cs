using DotnetFinancialTrackerApp.Models;

namespace DotnetFinancialTrackerApp.Services;

public interface IBudgetsService
{
    Task<List<Budget>> GetAsync(DateTime? month = null, string? familyId = null);
    Task<Budget?> GetByIdAsync(int id);
    Task<Budget> AddAsync(Budget budget);
    Task UpdateAsync(Budget budget);
    Task DeleteAsync(int id);
}
