using DotnetFinancialTrackerApp.Data;
using DotnetFinancialTrackerApp.Models;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace DotnetFinancialTrackerApp.Services
{
    public class FamilyBankingService : IFamilyBankingService
    {
        private readonly AppDbContext _context;
        private readonly ITransactionsService _transactionsService;
        private readonly IGamificationService _gamificationService;

        public FamilyBankingService(AppDbContext context, ITransactionsService transactionsService, IGamificationService gamificationService)
        {
            _context = context;
            _transactionsService = transactionsService;
            _gamificationService = gamificationService;
        }

        // Family Account Management
        public async Task<FamilyAccount?> GetFamilyAccountAsync(string familyId)
        {
            return await _context.FamilyAccounts
                .Include(f => f.Members)
                    .ThenInclude(m => m.Card)
                .Include(f => f.Goals)
                .Include(f => f.Insights)
                .FirstOrDefaultAsync(f => f.FamilyId == familyId);
        }

        public async Task<FamilyAccount> CreateFamilyAccountAsync(string familyName, string ownerName)
        {
            var familyAccount = new FamilyAccount
            {
                FamilyName = familyName,
                TotalBalance = 0m,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.FamilyAccounts.Add(familyAccount);

            // Create the primary parent member
            var parentMember = new FamilyMember
            {
                FamilyId = familyAccount.FamilyId,
                Name = ownerName,
                Role = "Parent",
                Balance = 0m,
                SpendingLimit = 5000m,
                IsOnline = true,
                LastActivity = DateTime.UtcNow
            };

            _context.FamilyMembers.Add(parentMember);

            // Create virtual card for parent
            var virtualCard = new VirtualCard
            {
                MemberId = parentMember.MemberId,
                DisplayNumber = GenerateCardNumber(),
                DailyLimit = 500m,
                CardColor = "#01FFFF",
                ExpiryDate = DateTime.UtcNow.AddYears(3)
            };

            _context.VirtualCards.Add(virtualCard);

            await _context.SaveChangesAsync();
            return familyAccount;
        }

        public async Task<bool> UpdateFamilyAccountAsync(FamilyAccount familyAccount)
        {
            try
            {
                familyAccount.UpdatedAt = DateTime.UtcNow;
                _context.FamilyAccounts.Update(familyAccount);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteFamilyAccountAsync(string familyId)
        {
            try
            {
                var familyAccount = await _context.FamilyAccounts.FindAsync(familyId);
                if (familyAccount == null) return false;

                _context.FamilyAccounts.Remove(familyAccount);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        // Family Member Management
        public async Task<List<FamilyMember>> GetFamilyMembersAsync(string familyId)
        {
            return await _context.FamilyMembers
                .Include(m => m.Card)
                .Include(m => m.SpendingLimits)
                .Where(m => m.FamilyId == familyId && m.IsActive)
                .OrderBy(m => m.Role == "Parent" ? 0 : m.Role == "Teen" ? 1 : 2)
                .ThenBy(m => m.Name)
                .ToListAsync();
        }

        public async Task<FamilyMember?> GetFamilyMemberAsync(string memberId)
        {
            return await _context.FamilyMembers
                .Include(m => m.Card)
                .Include(m => m.SpendingLimits)
                .Include(m => m.Goals)
                .FirstOrDefaultAsync(m => m.MemberId == memberId);
        }

        public async Task<FamilyMember> AddFamilyMemberAsync(string familyId, string name, string role, decimal initialBalance = 0)
        {
            var member = new FamilyMember
            {
                FamilyId = familyId,
                Name = name,
                Role = role,
                Balance = initialBalance,
                SpendingLimit = role switch
                {
                    "Parent" => 5000m,
                    "Teen" => 300m,
                    "Child" => 75m,
                    _ => 100m
                },
                MonthlyAllowance = role switch
                {
                    "Teen" => 200m,
                    "Child" => 50m,
                    _ => 0m
                },
                IsOnline = false,
                LastActivity = DateTime.UtcNow
            };

            _context.FamilyMembers.Add(member);

            // Create virtual card
            var card = new VirtualCard
            {
                MemberId = member.MemberId,
                DisplayNumber = GenerateCardNumber(),
                DailyLimit = role switch
                {
                    "Parent" => 500m,
                    "Teen" => 100m,
                    "Child" => 25m,
                    _ => 50m
                },
                CardColor = role switch
                {
                    "Parent" => "#01FFFF",
                    "Teen" => "#FF6B6B",
                    "Child" => "#4ECDC4",
                    _ => "#95E1D3"
                },
                ExpiryDate = DateTime.UtcNow.AddYears(3)
            };

            _context.VirtualCards.Add(card);

            await _context.SaveChangesAsync();
            return member;
        }

        public async Task<bool> UpdateFamilyMemberAsync(FamilyMember member)
        {
            try
            {
                member.UpdatedAt = DateTime.UtcNow;
                _context.FamilyMembers.Update(member);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> RemoveFamilyMemberAsync(string memberId)
        {
            try
            {
                var member = await _context.FamilyMembers.FindAsync(memberId);
                if (member == null) return false;

                member.IsActive = false;
                member.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> TransferMoneyBetweenMembersAsync(string fromMemberId, string toMemberId, decimal amount, string? note = null)
        {
            try
            {
                var fromMember = await _context.FamilyMembers.FindAsync(fromMemberId);
                var toMember = await _context.FamilyMembers.FindAsync(toMemberId);

                if (fromMember == null || toMember == null || fromMember.Balance < amount)
                    return false;

                fromMember.Balance -= amount;
                toMember.Balance += amount;

                fromMember.UpdateActivity();
                toMember.UpdateActivity();

                // Record transactions
                var outgoingTransaction = new Transaction
                {
                    Amount = amount,
                    IsIncome = false,
                    Description = $"Transfer to {toMember.Name}",
                    Category = "Transfer",
                    User = fromMember.Name,
                    Date = DateTime.UtcNow,
                    // Notes = note
                };

                var incomingTransaction = new Transaction
                {
                    Amount = amount,
                    IsIncome = true,
                    Description = $"Transfer from {fromMember.Name}",
                    Category = "Transfer",
                    User = toMember.Name,
                    Date = DateTime.UtcNow,
                    // Notes = note
                };

                await _transactionsService.AddAsync(outgoingTransaction);
                await _transactionsService.AddAsync(incomingTransaction);

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        // Virtual Card Management
        public async Task<VirtualCard?> GetMemberCardAsync(string memberId)
        {
            return await _context.VirtualCards
                .Include(c => c.Member)
                .FirstOrDefaultAsync(c => c.MemberId == memberId);
        }

        public async Task<VirtualCard> CreateVirtualCardAsync(string memberId, decimal dailyLimit, string cardColor = "#01FFFF")
        {
            var card = new VirtualCard
            {
                MemberId = memberId,
                DisplayNumber = GenerateCardNumber(),
                DailyLimit = dailyLimit,
                CardColor = cardColor,
                ExpiryDate = DateTime.UtcNow.AddYears(3)
            };

            _context.VirtualCards.Add(card);
            await _context.SaveChangesAsync();
            return card;
        }

        public async Task<bool> UpdateCardLimitsAsync(string cardId, decimal dailyLimit)
        {
            try
            {
                var card = await _context.VirtualCards.FindAsync(cardId);
                if (card == null) return false;

                card.DailyLimit = dailyLimit;
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ToggleCardStatusAsync(string cardId, bool isActive)
        {
            try
            {
                var card = await _context.VirtualCards.FindAsync(cardId);
                if (card == null) return false;

                card.IsActive = isActive;
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> RegenerateCardAsync(string cardId)
        {
            try
            {
                var card = await _context.VirtualCards.FindAsync(cardId);
                if (card == null) return false;

                card.RegenerateCard();
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        // Balance and Transaction Management
        public async Task<decimal> GetRealTimeBalanceAsync(string memberId)
        {
            var member = await _context.FamilyMembers.FindAsync(memberId);
            return member?.Balance ?? 0m;
        }

        public async Task<bool> UpdateMemberBalanceAsync(string memberId, decimal amount, bool isIncome, string? description = null)
        {
            try
            {
                var member = await _context.FamilyMembers.FindAsync(memberId);
                if (member == null) return false;

                member.RecordTransaction(amount, isIncome);

                // Create transaction record
                var transaction = new Transaction
                {
                    Amount = amount,
                    IsIncome = isIncome,
                    Description = description ?? (isIncome ? "Income" : "Expense"),
                    Category = isIncome ? "Income" : "General",
                    User = member.Name,
                    Date = DateTime.UtcNow
                };

                await _transactionsService.AddAsync(transaction);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> RecordCardTransactionAsync(string cardId, decimal amount, string? merchantName = null, string? category = null)
        {
            try
            {
                var card = await _context.VirtualCards.Include(c => c.Member).FirstOrDefaultAsync(c => c.CardId == cardId);
                if (card?.Member == null || !card.CanSpend(amount)) return false;

                card.RecordSpending(amount);
                card.Member.RecordTransaction(amount, false);

                // Record card transaction
                var cardTransaction = new CardTransaction
                {
                    CardId = cardId,
                    Amount = amount,
                    MerchantName = merchantName ?? "Unknown Merchant",
                    Category = category ?? "General",
                    TransactionDate = DateTime.UtcNow,
                    Status = TransactionStatus.Completed
                };

                _context.CardTransactions.Add(cardTransaction);

                // Record in main transactions
                var transaction = new Transaction
                {
                    Amount = amount,
                    IsIncome = false,
                    Description = merchantName ?? "Card Transaction",
                    Category = category ?? "General",
                    User = card.Member.Name,
                    Date = DateTime.UtcNow
                };

                await _transactionsService.AddAsync(transaction);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        // Spending Limits (simplified implementation)
        public async Task<List<SpendingLimit>> GetMemberSpendingLimitsAsync(string memberId)
        {
            return await _context.SpendingLimits
                .Where(sl => sl.MemberId == memberId && sl.IsActive)
                .ToListAsync();
        }

        public async Task<SpendingLimit> CreateSpendingLimitAsync(string memberId, string category, decimal dailyLimit, decimal weeklyLimit, decimal monthlyLimit)
        {
            var limit = new SpendingLimit
            {
                MemberId = memberId,
                Category = category,
                DailyLimit = dailyLimit,
                WeeklyLimit = weeklyLimit,
                MonthlyLimit = monthlyLimit
            };

            _context.SpendingLimits.Add(limit);
            await _context.SaveChangesAsync();
            return limit;
        }

        public async Task<bool> UpdateSpendingLimitAsync(SpendingLimit limit)
        {
            try
            {
                limit.UpdatedAt = DateTime.UtcNow;
                _context.SpendingLimits.Update(limit);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> CheckSpendingLimitAsync(string memberId, string category, decimal amount)
        {
            var limits = await GetMemberSpendingLimitsAsync(memberId);
            var categoryLimit = limits.FirstOrDefault(l => l.Category.Equals(category, StringComparison.OrdinalIgnoreCase));

            if (categoryLimit == null) return true; // No limit set

            categoryLimit.ResetIfNeeded();
            return categoryLimit.CanSpend(amount, LimitPeriod.Daily) &&
                   categoryLimit.CanSpend(amount, LimitPeriod.Weekly) &&
                   categoryLimit.CanSpend(amount, LimitPeriod.Monthly);
        }

        public async Task<bool> DeleteSpendingLimitAsync(string limitId)
        {
            try
            {
                var limit = await _context.SpendingLimits.FindAsync(limitId);
                if (limit == null) return false;

                limit.IsActive = false;
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        // Family Goals (simplified implementation)
        public async Task<List<FamilyGoal>> GetFamilyGoalsAsync(string familyId)
        {
            return await _context.FamilyGoals
                .Include(g => g.Participants)
                .Where(g => g.FamilyId == familyId && !g.IsArchived)
                .OrderBy(g => g.TargetDate)
                .ToListAsync();
        }

        public async Task<FamilyGoal?> GetFamilyGoalAsync(string goalId)
        {
            return await _context.FamilyGoals
                .Include(g => g.Participants)
                .Include(g => g.Contributions)
                .FirstOrDefaultAsync(g => g.GoalId == goalId);
        }

        public async Task<FamilyGoal> CreateFamilyGoalAsync(string familyId, string title, decimal targetAmount, DateTime targetDate, List<string>? participantIds = null)
        {
            var goal = new FamilyGoal
            {
                FamilyId = familyId,
                Title = title,
                TargetAmount = targetAmount,
                TargetDate = targetDate,
                CurrentAmount = 0m
            };

            _context.FamilyGoals.Add(goal);

            if (participantIds != null)
            {
                foreach (var memberId in participantIds)
                {
                    var participant = new FamilyMemberGoal
                    {
                        MemberId = memberId,
                        GoalId = goal.GoalId,
                        ContributionTarget = targetAmount / participantIds.Count
                    };
                    _context.FamilyMemberGoals.Add(participant);
                }
            }

            await _context.SaveChangesAsync();
            return goal;
        }

        public async Task<bool> UpdateFamilyGoalAsync(FamilyGoal goal)
        {
            try
            {
                goal.UpdatedAt = DateTime.UtcNow;
                _context.FamilyGoals.Update(goal);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> AddGoalContributionAsync(string goalId, string memberId, decimal amount, string? note = null)
        {
            try
            {
                var goal = await _context.FamilyGoals.FindAsync(goalId);
                var member = await _context.FamilyMembers.FindAsync(memberId);

                if (goal == null || member == null || member.Balance < amount) return false;

                goal.AddContribution(memberId, amount, note);
                member.Balance -= amount;

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> RemoveGoalContributionAsync(string goalId, decimal amount)
        {
            try
            {
                var goal = await _context.FamilyGoals.FindAsync(goalId);
                if (goal == null) return false;

                goal.RemoveContribution(amount);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteFamilyGoalAsync(string goalId)
        {
            try
            {
                var goal = await _context.FamilyGoals.FindAsync(goalId);
                if (goal == null) return false;

                goal.Archive();
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        // Insights and Analytics (simplified implementation)
        public async Task<FamilyInsights> GetFamilyInsightsAsync(string familyId, DateTime? periodStart = null, DateTime? periodEnd = null)
        {
            var start = periodStart ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            var end = periodEnd ?? start.AddMonths(1).AddTicks(-1);

            var transactions = await _transactionsService.GetAsync(from: start, to: end);
            var familyMembers = await GetFamilyMembersAsync(familyId);
            var familyGoals = await GetFamilyGoalsAsync(familyId);

            var insights = new FamilyInsights
            {
                FamilyId = familyId,
                PeriodStart = start,
                PeriodEnd = end,
                GeneratedAt = DateTime.UtcNow,
                TotalIncome = transactions.Where(t => t.IsIncome).Sum(t => t.Amount),
                TotalExpenses = transactions.Where(t => !t.IsIncome).Sum(t => t.Amount),
                TotalTransactions = transactions.Count,
                TotalGoals = familyGoals.Count,
                CompletedGoals = familyGoals.Count(g => g.IsComplete),
                AverageGoalProgress = familyGoals.Any() ? familyGoals.Average(g => g.ProgressPercentage) : 0
            };

            insights.NetSavings = insights.TotalIncome - insights.TotalExpenses;
            insights.SavingsRate = insights.TotalIncome == 0 ? 0 : (double)(insights.NetSavings / insights.TotalIncome);
            insights.AverageTransactionAmount = insights.TotalTransactions == 0 ? 0 : (insights.TotalIncome + insights.TotalExpenses) / insights.TotalTransactions;

            // Calculate category insights
            var categorySpending = transactions
                .Where(t => !t.IsIncome)
                .GroupBy(t => t.Category ?? "Uncategorized")
                .OrderByDescending(g => g.Sum(t => t.Amount))
                .FirstOrDefault();

            if (categorySpending != null)
            {
                insights.TopSpendingCategory = categorySpending.Key;
                insights.TopCategoryAmount = categorySpending.Sum(t => t.Amount);
                insights.TopCategoryPercentage = insights.TotalExpenses == 0 ? 0 : (double)(insights.TopCategoryAmount / insights.TotalExpenses * 100);
            }

            // Calculate member insights
            var memberSpending = transactions
                .Where(t => !t.IsIncome)
                .GroupBy(t => t.User ?? "Unknown")
                .OrderByDescending(g => g.Sum(t => t.Amount))
                .FirstOrDefault();

            if (memberSpending != null)
            {
                insights.HighestSpenderMember = memberSpending.Key;
                insights.HighestSpenderAmount = memberSpending.Sum(t => t.Amount);
            }

            var memberActivity = transactions
                .GroupBy(t => t.User ?? "Unknown")
                .OrderByDescending(g => g.Count())
                .FirstOrDefault();

            if (memberActivity != null)
            {
                insights.MostActiveMember = memberActivity.Key;
                insights.MostActiveTransactionCount = memberActivity.Count();
            }

            insights.CalculateFinancialHealthScore();

            _context.FamilyInsights.Add(insights);
            await _context.SaveChangesAsync();

            return insights;
        }

        public async Task<List<SmartSuggestion>> GetSpendingRecommendationsAsync(string familyId)
        {
            var insights = await GetFamilyInsightsAsync(familyId);
            var suggestions = new List<SmartSuggestion>();

            if (insights.SavingsRate < 0.1)
            {
                suggestions.Add(new SmartSuggestion
                {
                    Title = "Increase Your Savings Rate",
                    Description = "Your family is currently saving less than 10% of income. Consider setting up automatic transfers to savings.",
                    Type = SuggestionType.Savings,
                    Priority = SuggestionPriority.High,
                    PotentialSavings = insights.TotalIncome * 0.1m
                });
            }

            if (!string.IsNullOrEmpty(insights.TopSpendingCategory) && insights.TopCategoryPercentage > 40)
            {
                suggestions.Add(new SmartSuggestion
                {
                    Title = $"Monitor {insights.TopSpendingCategory} Spending",
                    Description = $"{insights.TopSpendingCategory} represents {insights.TopCategoryPercentage:F0}% of your family's spending. Consider setting a budget.",
                    Type = SuggestionType.Budget,
                    Priority = SuggestionPriority.Medium
                });
            }

            return suggestions;
        }

        public async Task<FamilyHealthScore> CalculateFamilyFinancialHealthAsync(string familyId)
        {
            var insights = await GetFamilyInsightsAsync(familyId);
            var familyGoals = await GetFamilyGoalsAsync(familyId);

            var healthScore = new FamilyHealthScore
            {
                SavingsScore = insights.SavingsRate >= 0.2 ? 100 : insights.SavingsRate * 500,
                SpendingScore = insights.ExpenseIncomeRatio <= 0.7m ? 100 : Math.Max(0, 100 - (double)(insights.ExpenseIncomeRatio - 0.7m) * 1000),
                BudgetScore = 85, // Simplified
                GoalsScore = familyGoals.Any() ? familyGoals.Average(g => g.ProgressPercentage) : 0
            };

            healthScore.OverallScore = (healthScore.SavingsScore + healthScore.SpendingScore + healthScore.BudgetScore + healthScore.GoalsScore) / 4;

            return healthScore;
        }

        // Utility methods and simplified implementations for remaining interface methods
        public async Task<Dictionary<string, decimal>> GetCategorySpendingAsync(string familyId, DateTime periodStart, DateTime periodEnd)
        {
            var transactions = await _transactionsService.GetAsync(from: periodStart, to: periodEnd);
            return transactions
                .Where(t => !t.IsIncome)
                .GroupBy(t => t.Category ?? "Uncategorized")
                .ToDictionary(g => g.Key, g => g.Sum(t => t.Amount));
        }

        public async Task<Dictionary<string, decimal>> GetMemberSpendingAsync(string familyId, DateTime periodStart, DateTime periodEnd)
        {
            var transactions = await _transactionsService.GetAsync(from: periodStart, to: periodEnd);
            return transactions
                .Where(t => !t.IsIncome)
                .GroupBy(t => t.User ?? "Unknown")
                .ToDictionary(g => g.Key, g => g.Sum(t => t.Amount));
        }

        public async Task<bool> UpdateMemberActivityAsync(string memberId)
        {
            try
            {
                var member = await _context.FamilyMembers.FindAsync(memberId);
                if (member == null) return false;

                member.UpdateActivity();
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<FamilyMember>> GetOnlineMembersAsync(string familyId)
        {
            return await _context.FamilyMembers
                .Where(m => m.FamilyId == familyId && m.IsOnline && m.IsActive)
                .ToListAsync();
        }

        public async Task<List<Achievement>> GetFamilyAchievementsAsync(string familyId)
        {
            return await _gamificationService.GetAchievementsAsync();
        }

        public async Task<bool> AwardFamilyAchievementAsync(string familyId, string achievementTitle, string description, int points)
        {
            // Simplified implementation
            return true;
        }

        public async Task<List<string>> GetPendingAlertsAsync(string familyId)
        {
            // Simplified implementation
            return new List<string>();
        }

        public async Task<bool> SendSpendingAlertAsync(string memberId, string message)
        {
            // Simplified implementation
            return true;
        }

        public async Task<bool> SendGoalMilestoneAlertAsync(string goalId, string message)
        {
            // Simplified implementation
            return true;
        }

        public async Task<bool> MarkAlertAsReadAsync(string alertId)
        {
            // Simplified implementation
            return true;
        }

        public async Task<bool> SyncFamilyDataAsync(string familyId)
        {
            // Simplified implementation
            return true;
        }

        public async Task<string> ExportFamilyDataAsync(string familyId, string format = "json")
        {
            var familyAccount = await GetFamilyAccountAsync(familyId);
            return System.Text.Json.JsonSerializer.Serialize(familyAccount);
        }

        public async Task<bool> ImportFamilyDataAsync(string familyId, string data, string format = "json")
        {
            // Simplified implementation
            return true;
        }

        public async Task<Dictionary<string, object>> GetFamilyStatisticsAsync(string familyId)
        {
            var familyMembers = await GetFamilyMembersAsync(familyId);
            var totalBalance = await GetFamilyTotalBalanceAsync(familyId);
            var savingsRate = await GetFamilySavingsRateAsync(familyId);

            return new Dictionary<string, object>
            {
                { "TotalMembers", familyMembers.Count },
                { "TotalBalance", totalBalance },
                { "SavingsRate", savingsRate },
                { "OnlineMembers", familyMembers.Count(m => m.IsOnline) }
            };
        }

        public async Task<List<Transaction>> GetRecentFamilyTransactionsAsync(string familyId, int count = 10)
        {
            var familyMembers = await GetFamilyMembersAsync(familyId);
            var memberNames = familyMembers.Select(m => m.Name).ToHashSet();

            var allTransactions = await _transactionsService.GetAsync();
            return allTransactions
                .Where(t => memberNames.Contains(t.User ?? ""))
                .OrderByDescending(t => t.Date)
                .Take(count)
                .ToList();
        }

        public async Task<decimal> GetFamilyTotalBalanceAsync(string familyId)
        {
            var familyMembers = await GetFamilyMembersAsync(familyId);
            return familyMembers.Sum(m => m.Balance);
        }

        public async Task<double> GetFamilySavingsRateAsync(string familyId)
        {
            var insights = await GetFamilyInsightsAsync(familyId);
            return insights.SavingsRate;
        }

        private static string GenerateCardNumber()
        {
            var random = new Random();
            return random.Next(1000, 9999).ToString();
        }
    }
}