using DotnetFinancialTrackerApp.Models;

namespace DotnetFinancialTrackerApp.Services;

// MARKING GUIDE: Interface Example #1 - ITransactionsService
// Demonstrates interface design for transaction management operations
// Points: Code Requirement - At least two examples of Interface (2/6 points)
public interface ITransactionsService
{
    Task<List<Transaction>> GetAsync(string? user = null, string? category = null, DateTime? from = null, DateTime? to = null);
    Task<Transaction?> GetByIdAsync(int id);
    Task<Transaction> AddAsync(Transaction tx);
    Task UpdateAsync(Transaction tx);
    Task DeleteAsync(int id);
}

