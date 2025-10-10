using DotnetFinancialTrackerApp.Data;
using DotnetFinancialTrackerApp.Models;
using Microsoft.EntityFrameworkCore;

namespace DotnetFinancialTrackerApp.Services;

public class TransactionTemplateService : ITransactionTemplateService
{
    private readonly AppDbContext _context;

    public TransactionTemplateService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<TransactionTemplate>> GetActiveTemplatesAsync(string user)
    {
        return await _context.TransactionTemplates
            .Where(t => t.IsActive && (t.User == user || string.IsNullOrEmpty(t.User)))
            .OrderByDescending(t => t.UsageCount)
            .ThenByDescending(t => t.LastUsed)
            .ToListAsync();
    }

    public async Task<List<TransactionTemplate>> GetMostUsedTemplatesAsync(string user, int count = 6)
    {
        return await _context.TransactionTemplates
            .Where(t => t.IsActive && (t.User == user || string.IsNullOrEmpty(t.User)))
            .OrderByDescending(t => t.UsageCount)
            .ThenByDescending(t => t.LastUsed)
            .Take(count)
            .ToListAsync();
    }

    public async Task<TransactionTemplate?> GetByIdAsync(int id)
    {
        return await _context.TransactionTemplates.FindAsync(id);
    }

    public async Task<TransactionTemplate> CreateTemplateAsync(TransactionTemplate template)
    {
        _context.TransactionTemplates.Add(template);
        await _context.SaveChangesAsync();
        return template;
    }

    public async Task UpdateTemplateAsync(TransactionTemplate template)
    {
        _context.TransactionTemplates.Update(template);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteTemplateAsync(int id)
    {
        var template = await _context.TransactionTemplates.FindAsync(id);
        if (template != null)
        {
            _context.TransactionTemplates.Remove(template);
            await _context.SaveChangesAsync();
        }
    }

    public async Task IncrementUsageAsync(int templateId)
    {
        var template = await _context.TransactionTemplates.FindAsync(templateId);
        if (template != null)
        {
            template.UsageCount++;
            template.LastUsed = DateTime.Now;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<List<string>> GetPopularCategoriesAsync(string user, int count = 8)
    {
        // Get categories from recent transactions
        var recentCategories = await _context.Transactions
            .Where(t => t.User == user && t.Date >= DateTime.Now.AddDays(-30))
            .GroupBy(t => t.Category)
            .Select(g => new { Category = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(count)
            .Select(x => x.Category)
            .ToListAsync();

        // Fill with default categories if needed
        var defaultCategories = new[] { "Food", "Transportation", "Shopping", "Entertainment", "Bills", "Health", "Income", "Other" };
        var result = recentCategories.ToList();

        foreach (var category in defaultCategories)
        {
            if (!result.Contains(category) && result.Count < count)
            {
                result.Add(category);
            }
        }

        return result.Take(count).ToList();
    }

    public async Task SeedDefaultTemplatesAsync()
    {
        // Check if we already have templates
        if (await _context.TransactionTemplates.AnyAsync())
            return;

        var defaultTemplates = new[]
        {
            new TransactionTemplate { Name = "Coffee", Category = "Food", DefaultAmount = 5.00m, Icon = "Icons.Material.Filled.LocalCafe", Color = "#8D6E63" },
            new TransactionTemplate { Name = "Lunch", Category = "Food", DefaultAmount = 15.00m, Icon = "Icons.Material.Filled.Restaurant", Color = "#FF8A65" },
            new TransactionTemplate { Name = "Gas", Category = "Transportation", DefaultAmount = 40.00m, Icon = "Icons.Material.Filled.LocalGasStation", Color = "#4FC3F7" },
            new TransactionTemplate { Name = "Groceries", Category = "Food", DefaultAmount = 75.00m, Icon = "Icons.Material.Filled.ShoppingCart", Color = "#81C784" },
            new TransactionTemplate { Name = "Parking", Category = "Transportation", DefaultAmount = 8.00m, Icon = "Icons.Material.Filled.LocalParking", Color = "#90A4AE" },
            new TransactionTemplate { Name = "Salary", Category = "Income", DefaultAmount = 3000.00m, IsIncome = true, Icon = "Icons.Material.Filled.AccountBalance", Color = "#66BB6A" },
            new TransactionTemplate { Name = "Bills", Category = "Bills", DefaultAmount = 100.00m, Icon = "Icons.Material.Filled.Receipt", Color = "#F06292" },
            new TransactionTemplate { Name = "Entertainment", Category = "Entertainment", DefaultAmount = 25.00m, Icon = "Icons.Material.Filled.Movie", Color = "#BA68C8" }
        };

        _context.TransactionTemplates.AddRange(defaultTemplates);
        await _context.SaveChangesAsync();
    }
}