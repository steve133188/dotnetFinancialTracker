using DotnetFinancialTrackerApp.Models;

namespace DotnetFinancialTrackerApp.Services;

public interface ITransactionsService
{
    Task<List<Transaction>> GetAsync(string? user = null, string? category = null, DateTime? from = null, DateTime? to = null);
    Task<Transaction?> GetByIdAsync(int id);
    Task<Transaction> AddAsync(Transaction tx);
    Task UpdateAsync(Transaction tx);
    Task DeleteAsync(int id);
}

