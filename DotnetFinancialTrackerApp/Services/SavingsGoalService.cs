using DotnetFinancialTrackerApp.Data;
using DotnetFinancialTrackerApp.Models;
using Microsoft.EntityFrameworkCore;

namespace DotnetFinancialTrackerApp.Services;

public class SavingsGoalService : ISavingsGoalService
{
    private readonly AppDbContext _context;

    public SavingsGoalService(AppDbContext context)
    {
        _context = context;
    }

    // Basic CRUD operations
    public async Task<IEnumerable<SavingsGoal>> GetAllAsync(string familyId)
    {
        // Spec: LINQ with lambda expressions & generic collections satisfy "Anonymous method with LINQ" + "Generics" requirement.
        return await _context.SavingsGoals
            .Where(g => g.FamilyId == familyId)
            .Include(g => g.Contributions)
            .Include(g => g.CreatedBy)
            .OrderByDescending(g => g.CreatedDate)
            .ToListAsync();
    }

    public async Task<SavingsGoal?> GetByIdAsync(int id)
    {
        return await _context.SavingsGoals
            .Include(g => g.Contributions)
                .ThenInclude(c => c.Contributor)
            .Include(g => g.CreatedBy)
            .Include(g => g.Family)
            .FirstOrDefaultAsync(g => g.Id == id);
    }

    public async Task<SavingsGoal> CreateAsync(SavingsGoal savingsGoal)
    {
        savingsGoal.CreatedDate = DateTime.UtcNow;
        _context.SavingsGoals.Add(savingsGoal);
        await _context.SaveChangesAsync();
        return savingsGoal;
    }

    public async Task<SavingsGoal> UpdateAsync(SavingsGoal savingsGoal)
    {
        _context.SavingsGoals.Update(savingsGoal);
        await _context.SaveChangesAsync();
        return savingsGoal;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var goal = await _context.SavingsGoals.FindAsync(id);
        if (goal == null) return false;

        _context.SavingsGoals.Remove(goal);
        await _context.SaveChangesAsync();
        return true;
    }

    // Goal-specific operations
    public async Task<IEnumerable<SavingsGoal>> GetActiveGoalsAsync(string familyId)
    {
        return await _context.SavingsGoals
            .Where(g => g.FamilyId == familyId && g.IsActive && g.CurrentAmount < g.TargetAmount)
            .Include(g => g.Contributions)
            .OrderBy(g => g.TargetDate ?? DateTime.MaxValue)
            .ToListAsync();
    }

    public async Task<IEnumerable<SavingsGoal>> GetCompletedGoalsAsync(string familyId)
    {
        return await _context.SavingsGoals
            .Where(g => g.FamilyId == familyId && g.CurrentAmount >= g.TargetAmount)
            .Include(g => g.Contributions)
            .OrderByDescending(g => g.CreatedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<SavingsGoal>> GetGoalsByCategoryAsync(string familyId, string category)
    {
        return await _context.SavingsGoals
            .Where(g => g.FamilyId == familyId && g.Category == category)
            .Include(g => g.Contributions)
            .OrderByDescending(g => g.CreatedDate)
            .ToListAsync();
    }

    // Contribution operations
    public async Task<bool> AddContributionAsync(int goalId, decimal amount, string contributorMemberId, string? description = null)
    {
        var goal = await _context.SavingsGoals.FindAsync(goalId);
        if (goal == null || !goal.CanContribute(amount)) return false;

        var contribution = SavingsGoalContribution.CreateContribution(goalId, amount, contributorMemberId, description);
        goal.CurrentAmount += amount;

        _context.SavingsGoalContributions.Add(contribution);
        _context.SavingsGoals.Update(goal);

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> AddBudgetSurplusContributionAsync(int goalId, decimal amount, string contributorMemberId, int? sourceTransactionId = null)
    {
        var description = sourceTransactionId.HasValue
            ? $"Budget surplus transfer from transaction {sourceTransactionId}"
            : "Budget surplus transfer";

        var contribution = SavingsGoalContribution.CreateContribution(goalId, amount, contributorMemberId, description);
        if (sourceTransactionId.HasValue)
        {
            contribution.SourceTransactionId = sourceTransactionId.ToString();
        }

        return await AddContributionAsync(goalId, amount, contributorMemberId, description);
    }

    public async Task<IEnumerable<SavingsGoalContribution>> GetContributionsAsync(int goalId)
    {
        return await _context.SavingsGoalContributions
            .Where(c => c.SavingsGoalId == goalId)
            .Include(c => c.Contributor)
            .OrderByDescending(c => c.ContributionDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<SavingsGoalContribution>> GetMemberContributionsAsync(string memberId, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _context.SavingsGoalContributions
            .Where(c => c.ContributorMemberId == memberId);

        if (fromDate.HasValue)
            query = query.Where(c => c.ContributionDate >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(c => c.ContributionDate <= toDate.Value);

        return await query
            .Include(c => c.SavingsGoal)
            .OrderByDescending(c => c.ContributionDate)
            .ToListAsync();
    }

    // Analytics and insights
    public async Task<decimal> GetTotalSavingsAsync(string familyId)
    {
        return await _context.SavingsGoals
            .Where(g => g.FamilyId == familyId && g.IsActive)
            .SumAsync(g => g.CurrentAmount);
    }

    public async Task<decimal> GetMonthlyContributionsAsync(string familyId, DateTime month)
    {
        var monthStart = new DateTime(month.Year, month.Month, 1);
        var monthEnd = monthStart.AddMonths(1);

        return await _context.SavingsGoalContributions
            .Where(c => c.SavingsGoal!.FamilyId == familyId &&
                       c.ContributionDate >= monthStart &&
                       c.ContributionDate < monthEnd &&
                       !c.IsReversal)
            .SumAsync(c => c.Amount);
    }

    public async Task<SavingsGoalSummary> GetSavingsGoalSummaryAsync(string familyId)
    {
        var goals = await GetAllAsync(familyId);
        var activeGoals = goals.Where(g => g.IsActive && !g.IsCompleted);
        var completedGoals = goals.Where(g => g.IsCompleted);

        var totalTarget = activeGoals.Sum(g => g.TargetAmount);
        var totalCurrent = activeGoals.Sum(g => g.CurrentAmount);

        // Calculate monthly contribution average over last 6 months
        var sixMonthsAgo = DateTime.UtcNow.AddMonths(-6);
        var monthlyContributions = new List<decimal>();

        for (int i = 0; i < 6; i++)
        {
            var month = DateTime.UtcNow.AddMonths(-i);
            var contributions = await GetMonthlyContributionsAsync(familyId, month);
            monthlyContributions.Add(contributions);
        }

        return new SavingsGoalSummary
        {
            TotalTargetAmount = totalTarget,
            TotalCurrentAmount = totalCurrent,
            TotalRemainingAmount = totalTarget - totalCurrent,
            OverallProgress = totalTarget > 0 ? (double)(totalCurrent / totalTarget) * 100 : 0,
            ActiveGoalsCount = activeGoals.Count(),
            CompletedGoalsCount = completedGoals.Count(),
            MonthlyContributionAverage = monthlyContributions.Any() ? monthlyContributions.Average() : 0,
            NextGoalCompletionEstimate = EstimateNextCompletion(activeGoals)
        };
    }

    public async Task<IEnumerable<SavingsGoalProgress>> GetProgressReportAsync(string familyId, DateTime? fromDate = null, DateTime? toDate = null)
    {
        fromDate ??= DateTime.UtcNow.AddMonths(-1);
        toDate ??= DateTime.UtcNow;

        var goals = await GetAllAsync(familyId);
        var progressList = new List<SavingsGoalProgress>();

        foreach (var goal in goals)
        {
            var contributionsInPeriod = goal.Contributions
                .Where(c => c.ContributionDate >= fromDate && c.ContributionDate <= toDate && !c.IsReversal)
                .Sum(c => c.Amount);

            var startAmount = goal.CurrentAmount - contributionsInPeriod;

            progressList.Add(new SavingsGoalProgress
            {
                GoalId = goal.Id,
                GoalTitle = goal.Title,
                StartAmount = Math.Max(0, startAmount),
                EndAmount = goal.CurrentAmount,
                ContributionAmount = contributionsInPeriod,
                ProgressPercentage = goal.ProgressPercentage,
                PeriodStart = fromDate.Value,
                PeriodEnd = toDate.Value
            });
        }

        return progressList.OrderByDescending(p => p.ContributionAmount);
    }

    // Goal management
    public async Task<bool> MarkAsCompletedAsync(int goalId)
    {
        var goal = await _context.SavingsGoals.FindAsync(goalId);
        if (goal == null) return false;

        // Update calculated current amount based on contributions
        goal.UpdateProgress();

        if (goal.CurrentAmount >= goal.TargetAmount)
        {
            goal.IsActive = false; // Mark as inactive when completed
        }

        _context.SavingsGoals.Update(goal);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ActivateGoalAsync(int goalId)
    {
        var goal = await _context.SavingsGoals.FindAsync(goalId);
        if (goal == null) return false;

        goal.IsActive = true;
        _context.SavingsGoals.Update(goal);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeactivateGoalAsync(int goalId)
    {
        var goal = await _context.SavingsGoals.FindAsync(goalId);
        if (goal == null) return false;

        goal.IsActive = false;
        _context.SavingsGoals.Update(goal);
        await _context.SaveChangesAsync();
        return true;
    }

    // Helper methods
    private static DateTime? EstimateNextCompletion(IEnumerable<SavingsGoal> activeGoals)
    {
        var goalWithEarliestTarget = activeGoals
            .Where(g => g.TargetDate.HasValue)
            .OrderBy(g => g.TargetDate)
            .FirstOrDefault();

        return goalWithEarliestTarget?.TargetDate;
    }
}
