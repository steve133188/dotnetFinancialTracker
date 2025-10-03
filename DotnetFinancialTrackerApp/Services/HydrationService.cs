using DotnetFinancialTrackerApp.Data;
using DotnetFinancialTrackerApp.Models;
using Microsoft.EntityFrameworkCore;

namespace DotnetFinancialTrackerApp.Services;

public class HydrationService : IHydrationService
{
    private readonly AppDbContext _context;

    public HydrationService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<HydrationEntry>> GetAllAsync()
    {
        return await _context.HydrationEntries
            .OrderByDescending(h => h.Date)
            .ToListAsync();
    }

    public async Task<IEnumerable<HydrationEntry>> GetByUserAsync(string user)
    {
        return await _context.HydrationEntries
            .Where(h => h.AssignedTo == user)
            .OrderByDescending(h => h.Date)
            .ToListAsync();
    }

    public async Task<HydrationEntry?> GetByIdAsync(int id)
    {
        return await _context.HydrationEntries.FindAsync(id);
    }

    public async Task<HydrationEntry> CreateAsync(HydrationEntry item)
    {
        _context.HydrationEntries.Add(item);
        await _context.SaveChangesAsync();
        return item;
    }

    public async Task<HydrationEntry> UpdateAsync(HydrationEntry item)
    {
        _context.Entry(item).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return item;
    }

    public async Task DeleteAsync(int id)
    {
        var item = await _context.HydrationEntries.FindAsync(id);
        if (item != null)
        {
            _context.HydrationEntries.Remove(item);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<HydrationEntry?> GetTodayEntryAsync(string user)
    {
        return await _context.HydrationEntries
            .Where(h => h.AssignedTo == user && h.Date.Date == DateTime.Today)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<HydrationEntry>> GetWeeklyDataAsync(string user, DateTime startDate)
    {
        var endDate = startDate.AddDays(7);
        return await _context.HydrationEntries
            .Where(h => h.AssignedTo == user && h.Date >= startDate && h.Date < endDate)
            .OrderBy(h => h.Date)
            .ToListAsync();
    }

    public async Task<double> GetAverageCompletionAsync(string user, int days = 7)
    {
        var startDate = DateTime.Today.AddDays(-days);
        var entries = await _context.HydrationEntries
            .Where(h => h.AssignedTo == user && h.Date >= startDate)
            .Select(h => h.GetCompletionPercentage())
            .ToListAsync();

        return entries.Any() ? entries.Average() : 0.0;
    }
}