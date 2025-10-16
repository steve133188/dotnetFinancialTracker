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

    // Spec: method overloading used for polymorphism requirement (Assignment 2 - Code Requirement)
    public Task<List<Transaction>> GetAsync() => GetAsync(null, null, null, null);

    public async Task<List<Transaction>> GetAsync(string? user = null, string? category = null, DateTime? from = null, DateTime? to = null)
    {
        var query = _db.Transactions
            .Include(t => t.Category)
            .Include(t => t.FamilyMember)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(user))
        {
            var normalizedUser = user.Trim().ToLower();
            query = query.Where(t => t.FamilyMemberId.ToLower() == normalizedUser || (t.FamilyMember != null && t.FamilyMember.Name.ToLower() == normalizedUser));
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(t => t.Category != null && t.Category.Name == category);
        }

        if (from.HasValue)
        {
            query = query.Where(t => t.Date >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(t => t.Date <= to.Value);
        }

        return await query
            .OrderByDescending(t => t.Date)
            .ThenByDescending(t => t.Id)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Transaction?> GetByIdAsync(int id) => await _db.Transactions.FindAsync(id);

    public async Task<Transaction> AddAsync(Transaction tx)
    {
        tx.CreatedAt = DateTime.UtcNow;
        tx.UpdatedAt = DateTime.UtcNow;
        _db.Transactions.Add(tx);
        await _db.SaveChangesAsync();
        return tx;
    }

    public async Task UpdateAsync(Transaction tx)
    {
        var existing = await _db.Transactions.FirstOrDefaultAsync(t => t.Id == tx.Id);
        if (existing is null)
        {
            return;
        }

        existing.Amount = tx.Amount;
        existing.Date = tx.Date;
        existing.Description = tx.Description;
        existing.Notes = tx.Notes;
        existing.Type = tx.Type;
        existing.CategoryId = tx.CategoryId;
        existing.FamilyMemberId = tx.FamilyMemberId;
        existing.RelatedSavingsGoalId = tx.RelatedSavingsGoalId;
        existing.UpdateTimestamp();

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
