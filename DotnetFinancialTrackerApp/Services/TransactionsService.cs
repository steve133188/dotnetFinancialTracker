using DotnetFinancialTrackerApp.Data;
using DotnetFinancialTrackerApp.Models;
using Microsoft.EntityFrameworkCore;

namespace DotnetFinancialTrackerApp.Services;

// Transaction service with method overloading and LINQ filtering.
public class TransactionsService : ITransactionsService
{
    private readonly AppDbContext _db;

    // Constructor with dependency injection.
    public TransactionsService(AppDbContext db)
    {
        _db = db;
    }

    // Method overloading - returns all transactions.
    public Task<List<Transaction>> GetAsync() => GetAsync(null, null, null, null);

    // Returns filtered transactions using LINQ with lambda expressions.
    public async Task<List<Transaction>> GetAsync(string? user = null, string? category = null, DateTime? from = null, DateTime? to = null)
    {
        // Start with queryable and include related data
        var query = _db.Transactions
            .Include(t => t.Category)
            .Include(t => t.FamilyMember)
            .AsQueryable();

        // Filter by user with lambda expression
        if (!string.IsNullOrWhiteSpace(user))
        {
            var normalizedUser = user.Trim().ToLower();
            query = query.Where(t => t.FamilyMemberId.ToLower() == normalizedUser ||
                                    (t.FamilyMember != null && t.FamilyMember.Name.ToLower() == normalizedUser));
        }

        // Filter by category
        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(t => t.Category != null && t.Category.Name == category);
        }

        // Filter by date range
        if (from.HasValue)
        {
            query = query.Where(t => t.Date >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(t => t.Date <= to.Value);
        }

        // Order by date and execute query
        return await query
            .OrderByDescending(t => t.Date)
            .ThenByDescending(t => t.Id)
            .AsNoTracking()
            .ToListAsync();
    }

    // Returns transaction by ID.
    public async Task<Transaction?> GetByIdAsync(int id)
    {
        // Validate input parameter
        if (id <= 0)
        {
            throw new ArgumentException("Transaction ID must be greater than zero.", nameof(id));
        }

        return await _db.Transactions.FindAsync(id);
    }

    // Adds new transaction with timestamps.
    public async Task<Transaction> AddAsync(Transaction tx)
    {
        // Validate input
        if (tx == null)
        {
            throw new ArgumentNullException(nameof(tx), "Transaction cannot be null.");
        }

        if (tx.Amount <= 0)
        {
            throw new ArgumentException("Transaction amount must be greater than zero.", nameof(tx));
        }

        if (string.IsNullOrWhiteSpace(tx.Description))
        {
            throw new ArgumentException("Transaction description is required.", nameof(tx));
        }

        // Set timestamps
        tx.CreatedAt = DateTime.UtcNow;
        tx.UpdatedAt = DateTime.UtcNow;

        _db.Transactions.Add(tx);
        await _db.SaveChangesAsync();
        return tx;
    }

    // Updates existing transaction.
    public async Task UpdateAsync(Transaction tx)
    {
        // Validate input
        if (tx == null)
        {
            throw new ArgumentNullException(nameof(tx), "Transaction cannot be null.");
        }

        if (tx.Id <= 0)
        {
            throw new ArgumentException("Transaction ID must be greater than zero.", nameof(tx));
        }

        // Find existing transaction
        var existing = await _db.Transactions.FirstOrDefaultAsync(t => t.Id == tx.Id);
        if (existing is null)
        {
            throw new InvalidOperationException($"Transaction with ID {tx.Id} not found.");
        }

        // Validate amount
        if (tx.Amount <= 0)
        {
            throw new ArgumentException("Transaction amount must be greater than zero.", nameof(tx));
        }

        // Update properties
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

    // Deletes transaction by ID.
    public async Task DeleteAsync(int id)
    {
        // Validate input
        if (id <= 0)
        {
            throw new ArgumentException("Transaction ID must be greater than zero.", nameof(id));
        }

        var entity = await _db.Transactions.FindAsync(id);
        if (entity is null)
        {
            throw new InvalidOperationException($"Transaction with ID {id} not found.");
        }

        _db.Transactions.Remove(entity);
        await _db.SaveChangesAsync();
    }
}
