using DotnetFinancialTrackerApp.Data;
using DotnetFinancialTrackerApp.Models;
using Microsoft.EntityFrameworkCore;

namespace DotnetFinancialTrackerApp.Services;

public class TransactionsService : ITransactionsService
{
    private readonly AppDbContext _db;

    public TransactionsService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<Transaction>> GetAsync(string? user = null, string? category = null, DateTime? from = null, DateTime? to = null)
    {
        var q = _db.Transactions.AsQueryable();
        if (!string.IsNullOrWhiteSpace(user)) q = q.Where(t => t.User == user);
        if (!string.IsNullOrWhiteSpace(category)) q = q.Where(t => t.Category == category);
        if (from.HasValue) q = q.Where(t => t.Date >= from.Value);
        if (to.HasValue) q = q.Where(t => t.Date <= to.Value);
        return await q.OrderByDescending(t => t.Date).ThenByDescending(t => t.Id).ToListAsync();
    }

    public async Task<Transaction?> GetByIdAsync(int id) => await _db.Transactions.FindAsync(id);

    public async Task<Transaction> AddAsync(Transaction tx)
    {
        _db.Transactions.Add(tx);
        await _db.SaveChangesAsync();
        return tx;
    }

    public async Task UpdateAsync(Transaction tx)
    {
        _db.Transactions.Update(tx);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _db.Transactions.FindAsync(id);
        if (entity is null) return;
        _db.Transactions.Remove(entity);
        await _db.SaveChangesAsync();
    }
}

