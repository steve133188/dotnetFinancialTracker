using DotnetFinancialTrackerApp.Data;
using DotnetFinancialTrackerApp.Models;
using Microsoft.EntityFrameworkCore;

namespace DotnetFinancialTrackerApp.Services;

/// <summary>
/// Transaction service implementing ITransactionsService interface.
/// Demonstrates: Interface implementation, method overloading polymorphism, LINQ with lambda expressions,
/// Entity Framework integration, and dependency injection principles.
/// </summary>
public class TransactionsService : ITransactionsService
{
    private readonly AppDbContext _db;

    /// <summary>
    /// Constructor demonstrates dependency injection pattern for loose coupling.
    /// AppDbContext is injected to enable testability and separation of concerns.
    /// </summary>
    public TransactionsService(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Polymorphism demonstration: Method overloading with no parameters.
    /// This overload calls the main method with null parameters for convenience.
    /// Shows how same method name can have different signatures (polymorphism requirement).
    /// </summary>
    public Task<List<Transaction>> GetAsync() => GetAsync(null, null, null, null);

    /// <summary>
    /// Main transaction retrieval method demonstrating LINQ with lambda expressions.
    /// Shows Entity Framework integration, optional parameters, and complex filtering.
    /// Implements the repository pattern for data access abstraction.
    /// </summary>
    /// <param name="user">Optional user filter - demonstrates flexible querying</param>
    /// <param name="category">Optional category filter for transaction types</param>
    /// <param name="from">Optional start date for date range filtering</param>
    /// <param name="to">Optional end date for date range filtering</param>
    /// <returns>Filtered and ordered list of transactions</returns>
    public async Task<List<Transaction>> GetAsync(string? user = null, string? category = null, DateTime? from = null, DateTime? to = null)
    {
        // Entity Framework: Start with queryable interface for building dynamic queries
        var query = _db.Transactions
            .Include(t => t.Category)    // Eager loading to prevent N+1 queries
            .Include(t => t.FamilyMember) // Related data inclusion for complete objects
            .AsQueryable();

        // LINQ with Lambda: User filtering with null-safe string operations
        if (!string.IsNullOrWhiteSpace(user))
        {
            var normalizedUser = user.Trim().ToLower();
            // Lambda expression demonstrating complex conditional logic
            query = query.Where(t => t.FamilyMemberId.ToLower() == normalizedUser ||
                                    (t.FamilyMember != null && t.FamilyMember.Name.ToLower() == normalizedUser));
        }

        // LINQ with Lambda: Category filtering using navigation properties
        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(t => t.Category != null && t.Category.Name == category);
        }

        // LINQ with Lambda: Date range filtering with nullable DateTime handling
        if (from.HasValue)
        {
            query = query.Where(t => t.Date >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(t => t.Date <= to.Value);
        }

        // LINQ with Lambda: Complex ordering and execution
        return await query
            .OrderByDescending(t => t.Date)    // Primary sort by date (newest first)
            .ThenByDescending(t => t.Id)       // Secondary sort by ID for consistency
            .AsNoTracking()                    // Performance optimization for read-only queries
            .ToListAsync();                    // Async execution with generic List<T> return
    }

    /// <summary>
    /// Retrieves a single transaction by ID using Entity Framework FindAsync.
    /// Demonstrates simple entity lookup with nullable return type for safety.
    /// </summary>
    public async Task<Transaction?> GetByIdAsync(int id)
    {
        // Enhanced error handling: Validate input parameter
        if (id <= 0)
        {
            throw new ArgumentException("Transaction ID must be greater than zero.", nameof(id));
        }

        return await _db.Transactions.FindAsync(id);
    }

    /// <summary>
    /// Adds a new transaction with automatic timestamp management.
    /// Demonstrates Entity Framework Add/SaveChanges pattern and business logic encapsulation.
    /// </summary>
    public async Task<Transaction> AddAsync(Transaction tx)
    {
        // Enhanced error handling: Validate input
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

        // Business logic: Automatic timestamp management
        tx.CreatedAt = DateTime.UtcNow;
        tx.UpdatedAt = DateTime.UtcNow;

        _db.Transactions.Add(tx);
        await _db.SaveChangesAsync();
        return tx;
    }

    /// <summary>
    /// Updates an existing transaction using Entity Framework change tracking.
    /// Demonstrates defensive programming with null checks and selective property updates.
    /// </summary>
    public async Task UpdateAsync(Transaction tx)
    {
        // Enhanced error handling: Validate input
        if (tx == null)
        {
            throw new ArgumentNullException(nameof(tx), "Transaction cannot be null.");
        }

        if (tx.Id <= 0)
        {
            throw new ArgumentException("Transaction ID must be greater than zero.", nameof(tx));
        }

        // Entity Framework: Query with error handling
        var existing = await _db.Transactions.FirstOrDefaultAsync(t => t.Id == tx.Id);
        if (existing is null)
        {
            throw new InvalidOperationException($"Transaction with ID {tx.Id} not found.");
        }

        // Enhanced validation: Check business rules
        if (tx.Amount <= 0)
        {
            throw new ArgumentException("Transaction amount must be greater than zero.", nameof(tx));
        }

        // Selective property updates to prevent overwriting system fields
        existing.Amount = tx.Amount;
        existing.Date = tx.Date;
        existing.Description = tx.Description;
        existing.Notes = tx.Notes;
        existing.Type = tx.Type;
        existing.CategoryId = tx.CategoryId;
        existing.FamilyMemberId = tx.FamilyMemberId;
        existing.RelatedSavingsGoalId = tx.RelatedSavingsGoalId;
        existing.UpdateTimestamp(); // Business logic: Update timestamp

        await _db.SaveChangesAsync();
    }

    /// <summary>
    /// Deletes a transaction by ID with existence validation.
    /// Demonstrates safe deletion patterns with error handling.
    /// </summary>
    public async Task DeleteAsync(int id)
    {
        // Enhanced error handling: Validate input
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
