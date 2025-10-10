using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DotnetFinancialTrackerApp.Models
{
    public class SpendingLimit
    {
        [Key]
        public string LimitId { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string MemberId { get; set; } = "";

        [Required]
        public string Category { get; set; } = "";

        public decimal DailyLimit { get; set; }

        public decimal WeeklyLimit { get; set; }

        public decimal MonthlyLimit { get; set; }

        public decimal CurrentDailySpent { get; set; }

        public decimal CurrentWeeklySpent { get; set; }

        public decimal CurrentMonthlySpent { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public DateTime LastResetDate { get; set; } = DateTime.Today;

        public LimitType Type { get; set; } = LimitType.Category;

        public bool SendNotifications { get; set; } = true;

        public decimal WarningThreshold { get; set; } = 0.8m; // 80% threshold

        // Navigation properties
        [ForeignKey(nameof(MemberId))]
        public virtual FamilyMember? Member { get; set; }

        // Calculated properties
        public decimal RemainingDailyLimit => Math.Max(0, DailyLimit - CurrentDailySpent);

        public decimal RemainingWeeklyLimit => Math.Max(0, WeeklyLimit - CurrentWeeklySpent);

        public decimal RemainingMonthlyLimit => Math.Max(0, MonthlyLimit - CurrentMonthlySpent);

        public double DailyUsagePercentage => DailyLimit == 0 ? 0 : (double)(CurrentDailySpent / DailyLimit * 100);

        public double WeeklyUsagePercentage => WeeklyLimit == 0 ? 0 : (double)(CurrentWeeklySpent / WeeklyLimit * 100);

        public double MonthlyUsagePercentage => MonthlyLimit == 0 ? 0 : (double)(CurrentMonthlySpent / MonthlyLimit * 100);

        public bool IsDailyLimitExceeded => CurrentDailySpent > DailyLimit;

        public bool IsWeeklyLimitExceeded => CurrentWeeklySpent > WeeklyLimit;

        public bool IsMonthlyLimitExceeded => CurrentMonthlySpent > MonthlyLimit;

        public bool NeedsWarning => DailyUsagePercentage >= (double)(WarningThreshold * 100) ||
                                   WeeklyUsagePercentage >= (double)(WarningThreshold * 100) ||
                                   MonthlyUsagePercentage >= (double)(WarningThreshold * 100);

        // Methods
        public bool CanSpend(decimal amount, LimitPeriod period)
        {
            return period switch
            {
                LimitPeriod.Daily => CurrentDailySpent + amount <= DailyLimit,
                LimitPeriod.Weekly => CurrentWeeklySpent + amount <= WeeklyLimit,
                LimitPeriod.Monthly => CurrentMonthlySpent + amount <= MonthlyLimit,
                _ => true
            };
        }

        public void RecordSpending(decimal amount)
        {
            ResetIfNeeded();

            CurrentDailySpent += amount;
            CurrentWeeklySpent += amount;
            CurrentMonthlySpent += amount;
            UpdatedAt = DateTime.UtcNow;
        }

        public void ResetIfNeeded()
        {
            var today = DateTime.Today;

            // Reset daily if new day
            if (LastResetDate < today)
            {
                CurrentDailySpent = 0;
                LastResetDate = today;
            }

            // Reset weekly if new week (Monday)
            var weekStart = today.AddDays(-(int)today.DayOfWeek + 1);
            var lastWeekStart = LastResetDate.AddDays(-(int)LastResetDate.DayOfWeek + 1);
            if (weekStart > lastWeekStart)
            {
                CurrentWeeklySpent = 0;
            }

            // Reset monthly if new month
            if (today.Month != LastResetDate.Month || today.Year != LastResetDate.Year)
            {
                CurrentMonthlySpent = 0;
            }

            if (LastResetDate < today)
            {
                LastResetDate = today;
                UpdatedAt = DateTime.UtcNow;
            }
        }

        public string GetStatusMessage()
        {
            if (IsDailyLimitExceeded)
                return $"Daily limit exceeded by {CurrentDailySpent - DailyLimit:C}";

            if (IsWeeklyLimitExceeded)
                return $"Weekly limit exceeded by {CurrentWeeklySpent - WeeklyLimit:C}";

            if (IsMonthlyLimitExceeded)
                return $"Monthly limit exceeded by {CurrentMonthlySpent - MonthlyLimit:C}";

            if (NeedsWarning)
            {
                if (DailyUsagePercentage >= (double)(WarningThreshold * 100))
                    return $"Daily budget {DailyUsagePercentage:F0}% used";

                if (WeeklyUsagePercentage >= (double)(WarningThreshold * 100))
                    return $"Weekly budget {WeeklyUsagePercentage:F0}% used";

                if (MonthlyUsagePercentage >= (double)(WarningThreshold * 100))
                    return $"Monthly budget {MonthlyUsagePercentage:F0}% used";
            }

            return "Within limits";
        }

        public LimitStatus GetStatus()
        {
            if (IsDailyLimitExceeded || IsWeeklyLimitExceeded || IsMonthlyLimitExceeded)
                return LimitStatus.Exceeded;

            if (NeedsWarning)
                return LimitStatus.Warning;

            return LimitStatus.Normal;
        }
    }

    public class FamilyMemberGoal
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string MemberId { get; set; } = "";

        [Required]
        public string GoalId { get; set; } = "";

        public decimal ContributionTarget { get; set; }

        public decimal CurrentContribution { get; set; }

        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;

        // Navigation properties
        [ForeignKey(nameof(MemberId))]
        public virtual FamilyMember? Member { get; set; }

        [ForeignKey(nameof(GoalId))]
        public virtual FamilyGoal? Goal { get; set; }
    }

    public class GoalContribution
    {
        [Key]
        public string ContributionId { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string GoalId { get; set; } = "";

        [Required]
        public string MemberId { get; set; } = "";

        public decimal Amount { get; set; }

        public string? Note { get; set; }

        public DateTime ContributedAt { get; set; } = DateTime.UtcNow;

        public string? TransactionId { get; set; } // Link to transaction if applicable

        // Navigation properties
        [ForeignKey(nameof(GoalId))]
        public virtual FamilyGoal? Goal { get; set; }

        [ForeignKey(nameof(MemberId))]
        public virtual FamilyMember? Member { get; set; }
    }

    public class CardTransaction
    {
        [Key]
        public string TransactionId { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string CardId { get; set; } = "";

        public decimal Amount { get; set; }

        public string? MerchantName { get; set; }

        public string? Category { get; set; }

        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;

        public TransactionStatus Status { get; set; } = TransactionStatus.Completed;

        public string? Description { get; set; }

        public string? Location { get; set; }

        // Navigation properties
        [ForeignKey(nameof(CardId))]
        public virtual VirtualCard? Card { get; set; }
    }

    public enum LimitType
    {
        Category = 0,
        Merchant = 1,
        Total = 2,
        Online = 3,
        ATM = 4
    }

    public enum LimitPeriod
    {
        Daily = 0,
        Weekly = 1,
        Monthly = 2
    }

    public enum LimitStatus
    {
        Normal = 0,
        Warning = 1,
        Exceeded = 2
    }

    public enum TransactionStatus
    {
        Pending = 0,
        Completed = 1,
        Failed = 2,
        Cancelled = 3,
        Refunded = 4
    }
}