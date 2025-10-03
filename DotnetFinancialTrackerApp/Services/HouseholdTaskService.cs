using DotnetFinancialTrackerApp.Data;
using DotnetFinancialTrackerApp.Models;
using Microsoft.EntityFrameworkCore;

namespace DotnetFinancialTrackerApp.Services;

public class HouseholdTaskService : IHouseholdTaskService
{
    private readonly AppDbContext _context;

    public HouseholdTaskService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<HouseholdTask>> GetAllAsync()
    {
        return await _context.HouseholdTasks
            .OrderBy(t => t.DueDate)
            .ThenBy(t => t.Priority)
            .ToListAsync();
    }

    public async Task<IEnumerable<HouseholdTask>> GetByUserAsync(string user)
    {
        return await _context.HouseholdTasks
            .Where(t => t.AssignedTo == user)
            .OrderBy(t => t.DueDate)
            .ThenBy(t => t.Priority)
            .ToListAsync();
    }

    public async Task<HouseholdTask?> GetByIdAsync(int id)
    {
        return await _context.HouseholdTasks.FindAsync(id);
    }

    public async Task<HouseholdTask> CreateAsync(HouseholdTask item)
    {
        _context.HouseholdTasks.Add(item);
        await _context.SaveChangesAsync();
        return item;
    }

    public async Task<HouseholdTask> UpdateAsync(HouseholdTask item)
    {
        _context.Entry(item).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return item;
    }

    public async Task DeleteAsync(int id)
    {
        var item = await _context.HouseholdTasks.FindAsync(id);
        if (item != null)
        {
            _context.HouseholdTasks.Remove(item);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<HouseholdTask>> GetTasksByStatusAsync(string user, bool completed)
    {
        return await _context.HouseholdTasks
            .Where(t => t.AssignedTo == user && t.IsCompleted == completed)
            .OrderBy(t => completed ? t.CompletedDate : t.DueDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<HouseholdTask>> GetOverdueTasksAsync(string user)
    {
        return await _context.HouseholdTasks
            .Where(t => t.AssignedTo == user &&
                       !t.IsCompleted &&
                       t.DueDate.Date < DateTime.Today)
            .OrderBy(t => t.DueDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<HouseholdTask>> GetTasksByCategoryAsync(string user, TaskCategory category)
    {
        return await _context.HouseholdTasks
            .Where(t => t.AssignedTo == user && t.Category == category)
            .OrderBy(t => t.DueDate)
            .ToListAsync();
    }

    public async Task<Dictionary<TaskCategory, int>> GetTaskSummaryAsync(string user)
    {
        var tasks = await _context.HouseholdTasks
            .Where(t => t.AssignedTo == user && !t.IsCompleted)
            .GroupBy(t => t.Category)
            .Select(g => new { Category = g.Key, Count = g.Count() })
            .ToListAsync();

        return tasks.ToDictionary(x => x.Category, x => x.Count);
    }
}