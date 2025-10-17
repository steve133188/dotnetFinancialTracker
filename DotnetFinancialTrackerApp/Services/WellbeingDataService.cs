using DotnetFinancialTrackerApp.Models;

namespace DotnetFinancialTrackerApp.Services;

public class WellbeingDataService : IWellbeingDataService
{
    private readonly ISavingsGoalService _savingsGoalService;
    private readonly IBudgetsService _budgetsService;
    private readonly ITransactionsService _transactionsService;

    public WellbeingDataService(
        ISavingsGoalService savingsGoalService,
        IBudgetsService budgetsService,
        ITransactionsService transactionsService)
    {
        _savingsGoalService = savingsGoalService;
        _budgetsService = budgetsService;
        _transactionsService = transactionsService;
    }

    public async Task<WellbeingData> GetWellbeingDataAsync(string familyId)
    {
        var allGoals = await _savingsGoalService.GetAllAsync(familyId);
        var goalsList = allGoals.ToList();
        var activeGoals = goalsList.Where(g => g.IsActive && !g.IsCompleted).ToList();

        var totalSaved = goalsList.Sum(g => g.CurrentAmount);
        var totalTarget = activeGoals.Sum(g => g.TargetAmount);
        var overallProgress = totalTarget > 0 ? (double)(totalSaved / totalTarget) * 100 : 0;

        var categories = goalsList.Select(g => g.Category).Distinct().OrderBy(c => c).ToList();

        return new WellbeingData
        {
            AllGoals = goalsList,
            ActiveGoals = activeGoals,
            TotalSaved = totalSaved,
            TotalTarget = totalTarget,
            OverallProgress = overallProgress,
            Categories = categories
        };
    }

    public async Task<BudgetData> GetBudgetDataAsync(string familyId)
    {
        var currentMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        var nextMonth = currentMonth.AddMonths(1);

        var budgets = await _budgetsService.GetAsync(null, familyId);

        // Update outdated budgets
        var outdatedBudgets = budgets.Where(b => b.Month < currentMonth && b.IsActive).ToList();
        foreach (var past in outdatedBudgets)
        {
            past.IsActive = false;
            past.UpdateTimestamp();
            await _budgetsService.UpdateAsync(past);
        }

        var currentBudget = budgets.FirstOrDefault(b => b.Month == currentMonth);
        if (currentBudget is not null && !currentBudget.IsActive)
        {
            currentBudget.IsActive = true;
            currentBudget.UpdateTimestamp();
            await _budgetsService.UpdateAsync(currentBudget);
        }

        decimal monthlyBudget = 0;
        decimal monthlySpending = 0;
        decimal budgetRemaining = 0;
        double budgetUsage = 0;

        if (currentBudget is not null)
        {
            monthlyBudget = currentBudget.Limit;

            var monthTransactions = await _transactionsService.GetAsync(from: currentMonth, to: nextMonth);
            monthlySpending = monthTransactions.Where(t => !t.IsIncome).Sum(t => t.Amount);
            budgetRemaining = monthlyBudget > 0 ? monthlyBudget - monthlySpending : 0;
            budgetUsage = monthlyBudget > 0
                ? Math.Min(100d, Math.Max(0d, (double)(monthlySpending / monthlyBudget) * 100))
                : 0;
        }

        return new BudgetData
        {
            CurrentBudget = currentBudget,
            MonthlyBudget = monthlyBudget,
            MonthlySpending = monthlySpending,
            BudgetRemaining = budgetRemaining,
            BudgetUsage = budgetUsage
        };
    }

    public async Task<List<SavingsGoal>> GetFilteredGoalsAsync(List<SavingsGoal> allGoals, string categoryFilter, string statusFilter)
    {
        await Task.CompletedTask; // Method is synchronous but keeping async for consistency

        var filtered = allGoals.AsEnumerable();

        if (!string.IsNullOrEmpty(categoryFilter))
        {
            filtered = filtered.Where(g => g.Category == categoryFilter);
        }

        if (!string.IsNullOrEmpty(statusFilter))
        {
            filtered = statusFilter switch
            {
                "Active" => filtered.Where(g => g.IsActive && !g.IsCompleted),
                "Completed" => filtered.Where(g => g.IsCompleted),
                _ => filtered
            };
        }

        return filtered.OrderByDescending(g => g.CreatedDate).ToList();
    }

    public async Task<List<string>> GetGoalCategoriesAsync(List<SavingsGoal> goals)
    {
        await Task.CompletedTask; // Method is synchronous but keeping async for consistency
        return goals.Select(g => g.Category).Distinct().OrderBy(c => c).ToList();
    }
}