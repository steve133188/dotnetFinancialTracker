using System.Text;
using System.Text.Json;
using DotnetFinancialTrackerApp.Models;
using Microsoft.Maui.Storage;

namespace DotnetFinancialTrackerApp.Services;

public class ExportService : IExportService
{
    public async Task<string> ExportTransactionsCsvAsync(IEnumerable<Transaction> items, string fileName = "transactions.csv")
    {
        var sb = new StringBuilder();
        sb.AppendLine("Id,User,Category,Amount,IsIncome,Date,Description");
        foreach (var t in items)
        {
            var desc = (t.Description ?? string.Empty).Replace('"', '\'');
            sb.AppendLine($"{t.Id},\"{t.User}\",\"{t.Category}\",{t.Amount},{t.IsIncome},{t.Date:O},\"{desc}\"");
        }
        var path = Path.Combine(FileSystem.AppDataDirectory, fileName);
        await File.WriteAllTextAsync(path, sb.ToString(), Encoding.UTF8);
        return path;
    }

    public async Task<string> ExportTransactionsJsonAsync(IEnumerable<Transaction> items, string fileName = "transactions.json")
    {
        var json = JsonSerializer.Serialize(items, new JsonSerializerOptions { WriteIndented = true });
        var path = Path.Combine(FileSystem.AppDataDirectory, fileName);
        await File.WriteAllTextAsync(path, json, Encoding.UTF8);
        return path;
    }
}
