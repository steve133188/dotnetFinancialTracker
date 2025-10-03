using DotnetFinancialTrackerApp.Data;
using DotnetFinancialTrackerApp.Models;
using Microsoft.EntityFrameworkCore;

namespace DotnetFinancialTrackerApp.Services;

public class BudgetsService : IBudgetsService
{
    private readonly AppDbContext _db;

    public BudgetsService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<Budget>> GetAsync(DateTime? month = null)
    {
        var q = _db.Budgets.AsQueryable();
        if (month.HasValue)
        {
            var first = new DateTime(month.Value.Year, month.Value.Month, 1);
            q = q.Where(b => b.Month.Year == first.Year && b.Month.Month == first.Month);
        }
        return await q.OrderBy(b => b.Category).ToListAsync();
    }

    public async Task<Budget?> GetByIdAsync(int id) => await _db.Budgets.FindAsync(id);

    public async Task<Budget> AddAsync(Budget budget)
    {
        _db.Budgets.Add(budget);
        await _db.SaveChangesAsync();
        return budget;
    }

    public async Task UpdateAsync(Budget budget)
    {
        _db.Budgets.Update(budget);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _db.Budgets.FindAsync(id);
        if (entity is null) return;
        _db.Budgets.Remove(entity);
        await _db.SaveChangesAsync();
    }
}

