using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DotnetFinancialTrackerApp.Models
{
    public class FamilyInsights
    {
        [Key]
        public string InsightsId { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string FamilyId { get; set; } = "";

        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

        public DateTime PeriodStart { get; set; }

        public DateTime PeriodEnd { get; set; }

        // Financial Health Metrics
        public decimal TotalIncome { get; set; }

        public decimal TotalExpenses { get; set; }

        public decimal NetSavings { get; set; }

        public double SavingsRate { get; set; }

        public decimal AverageTransactionAmount { get; set; }

        public int TotalTransactions { get; set; }

        // Category Analysis
        public string? TopSpendingCategory { get; set; }

        public decimal TopCategoryAmount { get; set; }

        public double TopCategoryPercentage { get; set; }

        // Member Insights
        public string? HighestSpenderMember { get; set; }

        public decimal HighestSpenderAmount { get; set; }

        public string? MostActiveMember { get; set; }

        public int MostActiveTransactionCount { get; set; }

        // Goals and Progress
        public int TotalGoals { get; set; }

        public int CompletedGoals { get; set; }

        public double AverageGoalProgress { get; set; }

        public decimal TotalGoalTarget { get; set; }

        public decimal TotalGoalAchieved { get; set; }

        // Trends and Predictions
        public double MonthOverMonthGrowth { get; set; }

        public double SpendingTrend { get; set; } // Positive = increasing, Negative = decreasing

        public decimal ProjectedMonthEndBalance { get; set; }

        public string? SpendingPattern { get; set; } // "Consistent", "Front-loaded", "Back-loaded", "Irregular"

        // AI-Generated Insights
        public string? KeyInsight { get; set; }

        public string? PrimaryRecommendation { get; set; }

        public string? BudgetAlert { get; set; }

        public string? SavingsOpportunity { get; set; }

        public double FinancialHealthScore { get; set; } // 0-100

        // Navigation properties
        [ForeignKey(nameof(FamilyId))]
        public virtual FamilyAccount? Family { get; set; }

        // Calculated properties
        public decimal ExpenseIncomeRatio => TotalIncome == 0 ? 0 : TotalExpenses / TotalIncome;

        public bool IsHealthy => FinancialHealthScore >= 70;

        public bool HasConcerns => FinancialHealthScore < 50;

        public string HealthDescription => FinancialHealthScore switch
        {
            >= 90 => "Excellent financial health",
            >= 80 => "Very good financial health",
            >= 70 => "Good financial health",
            >= 60 => "Fair financial health",
            >= 50 => "Needs attention",
            >= 30 => "Concerning trends",
            _ => "Requires immediate action"
        };

        public string SpendingTrendDescription => SpendingTrend switch
        {
            > 10 => "Spending increasing rapidly",
            > 5 => "Spending increasing moderately",
            > 0 => "Spending increasing slightly",
            0 => "Spending stable",
            > -5 => "Spending decreasing slightly",
            > -10 => "Spending decreasing moderately",
            _ => "Spending decreasing significantly"
        };

        // Methods
        public void CalculateFinancialHealthScore()
        {
            var score = 100.0;

            // Penalize high expense ratio
            if (ExpenseIncomeRatio > 0.9m) score -= 30;
            else if (ExpenseIncomeRatio > 0.8m) score -= 20;
            else if (ExpenseIncomeRatio > 0.7m) score -= 10;

            // Reward savings rate
            if (SavingsRate >= 0.2) score += 10;
            else if (SavingsRate >= 0.1) score += 5;
            else if (SavingsRate < 0) score -= 20;

            // Consider goal progress
            if (TotalGoals > 0)
            {
                var goalCompletionRate = (double)CompletedGoals / TotalGoals;
                if (goalCompletionRate >= 0.8) score += 10;
                else if (goalCompletionRate >= 0.5) score += 5;
                else if (goalCompletionRate < 0.2) score -= 10;
            }

            // Consider spending trends
            if (SpendingTrend > 15) score -= 15;
            else if (SpendingTrend > 5) score -= 10;
            else if (SpendingTrend < -5) score += 5;

            FinancialHealthScore = Math.Max(0, Math.Min(100, score));
        }

        public List<string> GetRecommendations()
        {
            var recommendations = new List<string>();

            if (ExpenseIncomeRatio > 0.9m)
                recommendations.Add("Consider reducing expenses or increasing income");

            if (SavingsRate < 0.1)
                recommendations.Add("Try to save at least 10% of your income");

            if (TotalGoals == 0)
                recommendations.Add("Set up family savings goals to stay motivated");

            if (SpendingTrend > 10)
                recommendations.Add("Monitor your spending - it's increasing rapidly");

            if (!string.IsNullOrEmpty(TopSpendingCategory) && TopCategoryPercentage > 40)
                recommendations.Add($"Consider budgeting for {TopSpendingCategory} - it's {TopCategoryPercentage:F0}% of spending");

            if (recommendations.Count == 0)
                recommendations.Add("Great job! Your family finances are on track");

            return recommendations;
        }

        public Dictionary<string, object> GetKeyMetrics()
        {
            return new Dictionary<string, object>
            {
                { "Financial Health Score", $"{FinancialHealthScore:F0}/100" },
                { "Savings Rate", SavingsRate.ToString("P1") },
                { "Monthly Net", NetSavings.ToString("C") },
                { "Top Category", TopSpendingCategory ?? "N/A" },
                { "Goals Progress", TotalGoals > 0 ? $"{CompletedGoals}/{TotalGoals}" : "No goals set" },
                { "Trend", SpendingTrendDescription }
            };
        }
    }

    public class SmartSuggestion
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public SuggestionType Type { get; set; }
        public SuggestionPriority Priority { get; set; }
        public decimal? PotentialSavings { get; set; }
        public string? ActionText { get; set; }
        public string? ActionUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActionable { get; set; } = true;
    }

    public enum SuggestionType
    {
        Savings = 0,
        Budget = 1,
        Goal = 2,
        Spending = 3,
        Investment = 4,
        Emergency = 5
    }

    public enum SuggestionPriority
    {
        Low = 0,
        Medium = 1,
        High = 2,
        Urgent = 3
    }

    public class FamilyHealthScore
    {
        public double OverallScore { get; set; }
        public double SavingsScore { get; set; }
        public double SpendingScore { get; set; }
        public double GoalsScore { get; set; }
        public double BudgetScore { get; set; }
        public string Grade => OverallScore switch
        {
            >= 90 => "A+",
            >= 85 => "A",
            >= 80 => "B+",
            >= 75 => "B",
            >= 70 => "C+",
            >= 65 => "C",
            >= 60 => "D+",
            >= 55 => "D",
            _ => "F"
        };
        public List<string> Strengths { get; set; } = new();
        public List<string> AreasForImprovement { get; set; } = new();
    }
}