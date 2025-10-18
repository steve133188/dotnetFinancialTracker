using DotnetFinancialTrackerApp.Models;

namespace DotnetFinancialTrackerApp.Services;

// Transaction service interface with CRUD operations.
public interface ITransactionsService
{
    Task<List<Transaction>> GetAsync(string? user = null, string? category = null, DateTime? from = null, DateTime? to = null);
    Task<Transaction?> GetByIdAsync(int id);
    Task<Transaction> AddAsync(Transaction tx);
    Task UpdateAsync(Transaction tx);
    Task DeleteAsync(int id);
}

