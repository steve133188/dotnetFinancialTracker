using DotnetFinancialTrackerApp.Models;

namespace DotnetFinancialTrackerApp.Services
{
    public interface IFamilyAIService
    {
        // Conversational AI
        Task<string> GetConversationalInsightAsync(string question, string familyId, string? memberId = null);
        Task<List<SmartSuggestion>> GetPersonalizedSuggestionsAsync(string familyId, string memberId);
        Task<string> ProcessVoiceCommandAsync(string voiceInput, string familyId, string memberId);

        // Financial Analysis
        Task<string> AnalyzeSpendingPatternsAsync(string familyId, DateTime? periodStart = null, DateTime? periodEnd = null);
        Task<string> PredictMonthlyExpensesAsync(string familyId);
        Task<string> RecommendBudgetAdjustmentsAsync(string familyId);
        Task<List<string>> IdentifySpendingAnomaliesAsync(string familyId);

        // Goal and Savings Assistance
        Task<string> SuggestSavingsGoalAsync(string familyId, decimal targetAmount, DateTime targetDate);
        Task<string> OptimizeGoalContributionsAsync(string goalId);
        Task<List<string>> GetGoalMotivationMessagesAsync(string goalId);
        Task<string> CalculateGoalFeasibilityAsync(string goalId);

        // Family Financial Education
        Task<string> GetFinancialTipAsync(string memberId, string category = "general");
        Task<List<string>> GetAgeAppropriateFinancialAdviceAsync(string memberId);
        Task<string> ExplainFinancialConceptAsync(string concept, string memberId);
        Task<string> GenerateFamilyFinancialReportAsync(string familyId);

        // Real-time Assistance
        Task<string> GetTransactionAdviceAsync(string memberId, decimal amount, string category, string? merchant = null);
        Task<string> CheckSpendingBeforeTransactionAsync(string memberId, decimal amount, string category);
        Task<bool> ShouldBlockTransactionAsync(string memberId, decimal amount, string category);
        Task<string> GetPostTransactionInsightAsync(string memberId, decimal amount, string category);

        // Proactive Notifications
        Task<List<string>> GetDailyFinancialRemindersAsync(string familyId);
        Task<List<string>> GetWeeklyFinancialSummaryAsync(string familyId);
        Task<List<string>> GetMonthlyGoalUpdatesAsync(string familyId);
        Task<bool> ShouldSendBudgetAlertAsync(string memberId, string category);

        // Data Insights
        Task<string> IdentifyMoneySavingOpportunitiesAsync(string familyId);
        Task<string> CompareFamilyToAveragesAsync(string familyId);
        Task<List<string>> GetTrendAnalysisAsync(string familyId);
        Task<string> PredictFutureBalanceAsync(string memberId, int daysAhead = 30);

        // Behavioral Analysis
        Task<string> AnalyzeFamilySpendingBehaviorAsync(string familyId);
        Task<string> GetSpendingPersonalityProfileAsync(string memberId);
        Task<List<string>> SuggestBehaviorChangesAsync(string memberId);
        Task<bool> DetectUnusualSpendingPatternAsync(string memberId, decimal amount, string category);

        // Emergency and Crisis Management
        Task<string> GetEmergencyBudgetAdviceAsync(string familyId);
        Task<List<string>> CreateDebtManagementPlanAsync(string familyId);
        Task<string> AssessFinancialRiskAsync(string familyId);
        Task<string> SuggestIncomeImprovementAsync(string familyId);

        // Family Communication
        Task<string> GenerateFamilyMeetingAgendaAsync(string familyId);
        Task<string> MediateFinancialDisagreementAsync(string familyId, string scenario);
        Task<List<string>> SuggestFamilyFinancialActivitiesAsync(string familyId);
        Task<string> CreateChildFriendlyFinancialExplanationAsync(string concept, string childMemberId);

        // Machine Learning and Personalization
        Task<bool> LearnFromUserFeedbackAsync(string interaction, bool wasHelpful, string? feedback = null);
        Task<string> GetPersonalizedDashboardInsightAsync(string memberId);
        Task<List<string>> PredictUserQuestionsAsync(string memberId);
        Task<bool> UpdateUserPreferencesAsync(string memberId, Dictionary<string, object> preferences);
    }

    public class AIInteraction
    {
        public string InteractionId { get; set; } = Guid.NewGuid().ToString();
        public string FamilyId { get; set; } = "";
        public string? MemberId { get; set; }
        public string UserInput { get; set; } = "";
        public string AIResponse { get; set; } = "";
        public AIInteractionType Type { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public bool WasHelpful { get; set; }
        public string? UserFeedback { get; set; }
        public Dictionary<string, object> Context { get; set; } = new();
    }

    public enum AIInteractionType
    {
        Question = 0,
        Insight = 1,
        Suggestion = 2,
        Alert = 3,
        Education = 4,
        Analysis = 5,
        Prediction = 6
    }

    public class ConversationContext
    {
        public string FamilyId { get; set; } = "";
        public string? MemberId { get; set; }
        public List<string> RecentTopics { get; set; } = new();
        public Dictionary<string, object> UserPreferences { get; set; } = new();
        public DateTime LastInteraction { get; set; } = DateTime.UtcNow;
        public string? CurrentGoal { get; set; }
        public decimal? PendingTransactionAmount { get; set; }
        public string? PendingTransactionCategory { get; set; }
    }
}