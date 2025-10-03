using DotnetFinancialTrackerApp.Data;
using DotnetFinancialTrackerApp.Models;
using Microsoft.EntityFrameworkCore;

namespace DotnetFinancialTrackerApp.Services;

public class GamificationService : IGamificationService
{
    private readonly AppDbContext _db;

    public GamificationService(AppDbContext db)
    {
        _db = db;
    }

    public async Task EvaluateAsync()
    {
        // Ensure a single state row exists
        var state = await _db.GamificationStates.FirstOrDefaultAsync();
        if (state == null)
        {
            state = new GamificationState { Points = 0, StreakCount = 0 };
            _db.GamificationStates.Add(state);
            await _db.SaveChangesAsync();
        }

        // Calculate streak: consecutive days with at least one transaction
        var today = DateTime.Today;
        int streak = 0;
        for (var d = today; d >= today.AddDays(-30); d = d.AddDays(-1))
        {
            bool any = await _db.Transactions.AnyAsync(t => t.Date.Date == d);
            if (any) streak++;
            else break;
        }
        state.StreakCount = streak;
        state.LastActivityDate = await _db.Transactions
            .OrderByDescending(t => t.Date)
            .Select(t => (DateTime?)t.Date)
            .FirstOrDefaultAsync();

        // Points = sum of achievements
        state.Points = await _db.Achievements.SumAsync(a => a.Points);

        // Award achievements
        await AwardFirstTransactionAsync();
        await AwardTenTransactionsAsync();
        await AwardMonthlyOnTrackAsync();

        await _db.SaveChangesAsync();
    }

    private async Task AwardFirstTransactionAsync()
    {
        if (!await _db.Transactions.AnyAsync()) return;
        var key = "first_tx";
        if (await _db.Achievements.AnyAsync(a => a.Key == key)) return;
        _db.Achievements.Add(new Achievement
        {
            Key = key,
            Title = "First Step",
            Description = "Logged your first transaction",
            Points = 5,
            AchievedAt = DateTime.UtcNow
        });
    }

    private async Task AwardTenTransactionsAsync()
    {
        var count = await _db.Transactions.CountAsync();
        var key = "ten_tx";
        if (count >= 10 && !await _db.Achievements.AnyAsync(a => a.Key == key))
        {
            _db.Achievements.Add(new Achievement
            {
                Key = key,
                Title = "In the Groove",
                Description = "Logged 10 transactions",
                Points = 10,
                AchievedAt = DateTime.UtcNow
            });
        }
    }

    private async Task AwardMonthlyOnTrackAsync()
    {
        var first = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        var next = first.AddMonths(1).AddDays(-1);
        var totalBudgetD = await _db.Budgets
            .Where(b => b.Month.Year == first.Year && b.Month.Month == first.Month)
            .Select(b => (double)b.Limit)
            .SumAsync();
        var totalBudget = (decimal)totalBudgetD;
        if (totalBudget <= 0) return; // no budget
        var spentD = await _db.Transactions
            .Where(t => !t.IsIncome && t.Date >= first && t.Date <= next)
            .Select(t => (double)t.Amount)
            .SumAsync();
        var spent = (decimal)spentD;
        var usage = totalBudget == 0 ? 0 : spent / totalBudget;

        // Mid-month on track: before or at day 20 keep usage <= 0.8
        if (DateTime.Today.Day >= 15 && DateTime.Today.Day <= 25 && usage <= 0.80m)
        {
            var key = $"on_track_80_{first:yyyyMM}";
            if (!await _db.Achievements.AnyAsync(a => a.Key == key))
            {
                _db.Achievements.Add(new Achievement
                {
                    Key = key,
                    Title = "On Track",
                    Description = "On track to hit your monthly budget",
                    Points = 20,
                    AchievedAt = DateTime.UtcNow
                });
            }
        }
    }

    public async Task<GamificationSummary> GetSummaryAsync()
    {
        var first = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        var next = first.AddMonths(1).AddDays(-1);
        var totalBudgetD = await _db.Budgets
            .Where(b => b.Month.Year == first.Year && b.Month.Month == first.Month)
            .Select(b => (double)b.Limit)
            .SumAsync();
        var spentD = await _db.Transactions
            .Where(t => !t.IsIncome && t.Date >= first && t.Date <= next)
            .Select(t => (double)t.Amount)
            .SumAsync();
        var totalBudget = (decimal)totalBudgetD;
        var spent = (decimal)spentD;

        var state = await _db.GamificationStates.FirstOrDefaultAsync() ?? new GamificationState();

        return new GamificationSummary
        {
            Points = state.Points,
            Streak = state.StreakCount,
            BudgetLimit = totalBudget > 0 ? totalBudget : null,
            SpentThisMonth = spent
        };
    }

    public Task<List<Achievement>> GetAchievementsAsync()
    {
        return _db.Achievements.OrderByDescending(a => a.AchievedAt).ToListAsync();
    }
}
