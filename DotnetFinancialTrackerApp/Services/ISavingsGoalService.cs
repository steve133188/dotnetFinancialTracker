using DotnetFinancialTrackerApp.Models;

namespace DotnetFinancialTrackerApp.Services;

// Savings goal service interface with generic collections.
public interface ISavingsGoalService
{
    // Basic CRUD operations
    Task<IEnumerable<SavingsGoal>> GetAllAsync(string familyId);
    Task<SavingsGoal?> GetByIdAsync(int id);
    Task<SavingsGoal> CreateAsync(SavingsGoal savingsGoal);
    Task<SavingsGoal> UpdateAsync(SavingsGoal savingsGoal);
    Task<bool> DeleteAsync(int id);

    // Goal-specific operations
    Task<IEnumerable<SavingsGoal>> GetActiveGoalsAsync(string familyId);
    Task<IEnumerable<SavingsGoal>> GetCompletedGoalsAsync(string familyId);
    Task<IEnumerable<SavingsGoal>> GetGoalsByCategoryAsync(string familyId, string category);

    // Contribution operations
    Task<bool> AddContributionAsync(int goalId, decimal amount, string contributorMemberId, string? description = null);
    Task<bool> AddBudgetSurplusContributionAsync(int goalId, decimal amount, string contributorMemberId, int? sourceTransactionId = null);
    Task<IEnumerable<SavingsGoalContribution>> GetContributionsAsync(int goalId);
    Task<IEnumerable<SavingsGoalContribution>> GetMemberContributionsAsync(string memberId, DateTime? fromDate = null, DateTime? toDate = null);

    // Analytics and insights
    Task<decimal> GetTotalSavingsAsync(string familyId);
    Task<decimal> GetMonthlyContributionsAsync(string familyId, DateTime month);
    Task<SavingsGoalSummary> GetSavingsGoalSummaryAsync(string familyId);
    Task<IEnumerable<SavingsGoalProgress>> GetProgressReportAsync(string familyId, DateTime? fromDate = null, DateTime? toDate = null);

    // Goal management
    Task<bool> MarkAsCompletedAsync(int goalId);
    Task<bool> ActivateGoalAsync(int goalId);
    Task<bool> DeactivateGoalAsync(int goalId);
}

// Supporting models for analytics
public class SavingsGoalSummary
{
    public decimal TotalTargetAmount { get; set; }
    public decimal TotalCurrentAmount { get; set; }
    public decimal TotalRemainingAmount { get; set; }
    public double OverallProgress { get; set; }
    public int ActiveGoalsCount { get; set; }
    public int CompletedGoalsCount { get; set; }
    public decimal MonthlyContributionAverage { get; set; }
    public DateTime? NextGoalCompletionEstimate { get; set; }
}

public class SavingsGoalProgress
{
    public int GoalId { get; set; }
    public string GoalTitle { get; set; } = string.Empty;
    public decimal StartAmount { get; set; }
    public decimal EndAmount { get; set; }
    public decimal ContributionAmount { get; set; }
    public double ProgressPercentage { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
}