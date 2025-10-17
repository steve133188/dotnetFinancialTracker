using DotnetFinancialTrackerApp.Models;

namespace DotnetFinancialTrackerApp.Services;

public interface IWellbeingDataService
{
    Task<WellbeingData> GetWellbeingDataAsync(string familyId);
    Task<BudgetData> GetBudgetDataAsync(string familyId);
    Task<List<SavingsGoal>> GetFilteredGoalsAsync(List<SavingsGoal> allGoals, string categoryFilter, string statusFilter);
    Task<List<string>> GetGoalCategoriesAsync(List<SavingsGoal> goals);
}

public class WellbeingData
{
    public List<SavingsGoal> AllGoals { get; set; } = new();
    public List<SavingsGoal> ActiveGoals { get; set; } = new();
    public decimal TotalSaved { get; set; }
    public decimal TotalTarget { get; set; }
    public double OverallProgress { get; set; }
    public List<string> Categories { get; set; } = new();
}

public class BudgetData
{
    public Budget? CurrentBudget { get; set; }
    public decimal MonthlyBudget { get; set; }
    public decimal MonthlySpending { get; set; }
    public decimal BudgetRemaining { get; set; }
    public double BudgetUsage { get; set; }
}