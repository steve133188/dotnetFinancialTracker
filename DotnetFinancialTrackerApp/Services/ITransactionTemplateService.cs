using DotnetFinancialTrackerApp.Models;

namespace DotnetFinancialTrackerApp.Services;

public interface ITransactionTemplateService
{
    Task<List<TransactionTemplate>> GetActiveTemplatesAsync(string user);
    Task<List<TransactionTemplate>> GetMostUsedTemplatesAsync(string user, int count = 6);
    Task<TransactionTemplate?> GetByIdAsync(int id);
    Task<TransactionTemplate> CreateTemplateAsync(TransactionTemplate template);
    Task UpdateTemplateAsync(TransactionTemplate template);
    Task DeleteTemplateAsync(int id);
    Task IncrementUsageAsync(int templateId);
    Task<List<string>> GetPopularCategoriesAsync(string user, int count = 8);
    Task SeedDefaultTemplatesAsync();
}