using DotnetFinancialTrackerApp.Models;

namespace DotnetFinancialTrackerApp.Services
{
    public interface IFamilyBankingService
    {
        // Family Account Management
        Task<FamilyAccount?> GetFamilyAccountAsync(string familyId);
        Task<FamilyAccount> CreateFamilyAccountAsync(string familyName, string ownerName);
        Task<bool> UpdateFamilyAccountAsync(FamilyAccount familyAccount);
        Task<bool> DeleteFamilyAccountAsync(string familyId);

        // Family Member Management
        Task<List<FamilyMember>> GetFamilyMembersAsync(string familyId);
        Task<FamilyMember?> GetFamilyMemberAsync(string memberId);
        Task<FamilyMember> AddFamilyMemberAsync(string familyId, string name, string role, decimal initialBalance = 0);
        Task<bool> UpdateFamilyMemberAsync(FamilyMember member);
        Task<bool> RemoveFamilyMemberAsync(string memberId);
        Task<bool> TransferMoneyBetweenMembersAsync(string fromMemberId, string toMemberId, decimal amount, string? note = null);

        // Virtual Card Management
        Task<VirtualCard?> GetMemberCardAsync(string memberId);
        Task<VirtualCard> CreateVirtualCardAsync(string memberId, decimal dailyLimit, string cardColor = "#01FFFF");
        Task<bool> UpdateCardLimitsAsync(string cardId, decimal dailyLimit);
        Task<bool> ToggleCardStatusAsync(string cardId, bool isActive);
        Task<bool> RegenerateCardAsync(string cardId);

        // Balance and Transaction Management
        Task<decimal> GetRealTimeBalanceAsync(string memberId);
        Task<bool> UpdateMemberBalanceAsync(string memberId, decimal amount, bool isIncome, string? description = null);
        Task<bool> RecordCardTransactionAsync(string cardId, decimal amount, string? merchantName = null, string? category = null);

        // Spending Limits
        Task<List<SpendingLimit>> GetMemberSpendingLimitsAsync(string memberId);
        Task<SpendingLimit> CreateSpendingLimitAsync(string memberId, string category, decimal dailyLimit, decimal weeklyLimit, decimal monthlyLimit);
        Task<bool> UpdateSpendingLimitAsync(SpendingLimit limit);
        Task<bool> CheckSpendingLimitAsync(string memberId, string category, decimal amount);
        Task<bool> DeleteSpendingLimitAsync(string limitId);

        // Family Goals
        Task<List<FamilyGoal>> GetFamilyGoalsAsync(string familyId);
        Task<FamilyGoal?> GetFamilyGoalAsync(string goalId);
        Task<FamilyGoal> CreateFamilyGoalAsync(string familyId, string title, decimal targetAmount, DateTime targetDate, List<string>? participantIds = null);
        Task<bool> UpdateFamilyGoalAsync(FamilyGoal goal);
        Task<bool> AddGoalContributionAsync(string goalId, string memberId, decimal amount, string? note = null);
        Task<bool> RemoveGoalContributionAsync(string goalId, decimal amount);
        Task<bool> DeleteFamilyGoalAsync(string goalId);

        // Insights and Analytics
        Task<FamilyInsights> GetFamilyInsightsAsync(string familyId, DateTime? periodStart = null, DateTime? periodEnd = null);
        Task<List<SmartSuggestion>> GetSpendingRecommendationsAsync(string familyId);
        Task<FamilyHealthScore> CalculateFamilyFinancialHealthAsync(string familyId);
        Task<Dictionary<string, decimal>> GetCategorySpendingAsync(string familyId, DateTime periodStart, DateTime periodEnd);
        Task<Dictionary<string, decimal>> GetMemberSpendingAsync(string familyId, DateTime periodStart, DateTime periodEnd);

        // Family Activity and Status
        Task<bool> UpdateMemberActivityAsync(string memberId);
        Task<List<FamilyMember>> GetOnlineMembersAsync(string familyId);
        Task<List<Achievement>> GetFamilyAchievementsAsync(string familyId);
        Task<bool> AwardFamilyAchievementAsync(string familyId, string achievementTitle, string description, int points);

        // Notifications and Alerts
        Task<List<string>> GetPendingAlertsAsync(string familyId);
        Task<bool> SendSpendingAlertAsync(string memberId, string message);
        Task<bool> SendGoalMilestoneAlertAsync(string goalId, string message);
        Task<bool> MarkAlertAsReadAsync(string alertId);

        // Backup and Sync
        Task<bool> SyncFamilyDataAsync(string familyId);
        Task<string> ExportFamilyDataAsync(string familyId, string format = "json");
        Task<bool> ImportFamilyDataAsync(string familyId, string data, string format = "json");

        // Family Statistics
        Task<Dictionary<string, object>> GetFamilyStatisticsAsync(string familyId);
        Task<List<Transaction>> GetRecentFamilyTransactionsAsync(string familyId, int count = 10);
        Task<decimal> GetFamilyTotalBalanceAsync(string familyId);
        Task<double> GetFamilySavingsRateAsync(string familyId);
    }
}