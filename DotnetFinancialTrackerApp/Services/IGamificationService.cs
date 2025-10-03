using DotnetFinancialTrackerApp.Models;

namespace DotnetFinancialTrackerApp.Services;

public class GamificationSummary
{
    public int Points { get; set; }
    public int Streak { get; set; }
    public decimal? BudgetLimit { get; set; }
    public decimal SpentThisMonth { get; set; }
    public double? UsagePercent => BudgetLimit.HasValue && BudgetLimit.Value > 0
        ? Math.Clamp((double)(SpentThisMonth / BudgetLimit.Value) * 100.0, 0, 999)
        : null;
}

public interface IGamificationService
{
    Task EvaluateAsync();
    Task<GamificationSummary> GetSummaryAsync();
    Task<List<Achievement>> GetAchievementsAsync();
}

