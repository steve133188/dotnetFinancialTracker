using DotnetFinancialTrackerApp.Data;
using DotnetFinancialTrackerApp.Models;
using Microsoft.EntityFrameworkCore;

namespace DotnetFinancialTrackerApp.Services;

public class MedicationService : IMedicationService
{
    private readonly AppDbContext _context;

    public MedicationService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<MedicationReminder>> GetAllAsync()
    {
        return await _context.MedicationReminders
            .OrderBy(m => m.Date)
            .ThenBy(m => m.ScheduledTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<MedicationReminder>> GetByUserAsync(string user)
    {
        return await _context.MedicationReminders
            .Where(m => m.AssignedTo == user)
            .OrderBy(m => m.Date)
            .ThenBy(m => m.ScheduledTime)
            .ToListAsync();
    }

    public async Task<MedicationReminder?> GetByIdAsync(int id)
    {
        return await _context.MedicationReminders.FindAsync(id);
    }

    public async Task<MedicationReminder> CreateAsync(MedicationReminder item)
    {
        _context.MedicationReminders.Add(item);
        await _context.SaveChangesAsync();
        return item;
    }

    public async Task<MedicationReminder> UpdateAsync(MedicationReminder item)
    {
        _context.Entry(item).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return item;
    }

    public async Task DeleteAsync(int id)
    {
        var item = await _context.MedicationReminders.FindAsync(id);
        if (item != null)
        {
            _context.MedicationReminders.Remove(item);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<MedicationReminder>> GetTodayRemindersAsync(string user)
    {
        return await _context.MedicationReminders
            .Where(m => m.AssignedTo == user && m.Date.Date == DateTime.Today)
            .OrderBy(m => m.ScheduledTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<MedicationReminder>> GetOverdueRemindersAsync(string user)
    {
        var now = DateTime.Now;
        var today = DateTime.Today;

        return await _context.MedicationReminders
            .Where(m => m.AssignedTo == user &&
                       !m.IsCompleted &&
                       m.Date.Date <= today)
            .ToListAsync()
            .ContinueWith(task => task.Result.Where(m => m.IsOverdue()));
    }

    public async Task<IEnumerable<MedicationReminder>> GetUpcomingRemindersAsync(string user, int hours = 2)
    {
        var now = DateTime.Now;
        var cutoff = now.AddHours(hours);

        return await _context.MedicationReminders
            .Where(m => m.AssignedTo == user &&
                       !m.IsCompleted &&
                       m.Date.Date >= DateTime.Today)
            .ToListAsync()
            .ContinueWith(task => task.Result.Where(m =>
            {
                var scheduledDateTime = m.Date.Add(m.ScheduledTime.ToTimeSpan());
                return scheduledDateTime >= now && scheduledDateTime <= cutoff;
            }));
    }
}