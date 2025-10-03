using DotnetFinancialTrackerApp.Models;

namespace DotnetFinancialTrackerApp.Services;

public interface IExportService
{
    Task<string> ExportTransactionsCsvAsync(IEnumerable<Transaction> items, string fileName = "transactions.csv");
    Task<string> ExportTransactionsJsonAsync(IEnumerable<Transaction> items, string fileName = "transactions.json");
}

