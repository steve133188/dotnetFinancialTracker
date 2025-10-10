using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DotnetFinancialTrackerApp.Models
{
    public class FamilyMember
    {
        [Key]
        public string MemberId { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string Name { get; set; } = "";

        [Required]
        public string Role { get; set; } = ""; // Parent, Teen, Child

        public decimal Balance { get; set; }

        public decimal MonthlyAllowance { get; set; }

        public bool IsOnline { get; set; }

        public DateTime? LastActivity { get; set; }

        public decimal SpendingLimit { get; set; }

        public decimal SpentThisMonth { get; set; }

        public int TransactionsThisMonth { get; set; }

        public double SavingsGoalProgress { get; set; }

        public int AchievementPoints { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public string? ProfileImageUrl { get; set; }

        public string? NotificationToken { get; set; }

        // Foreign key
        [Required]
        public string FamilyId { get; set; } = "";

        // Navigation properties
        [ForeignKey(nameof(FamilyId))]
        public virtual FamilyAccount? Family { get; set; }

        public virtual VirtualCard? Card { get; set; }

        public virtual ICollection<SpendingLimit> SpendingLimits { get; set; } = new List<SpendingLimit>();

        public virtual ICollection<FamilyMemberGoal> Goals { get; set; } = new List<FamilyMemberGoal>();

        // Calculated properties
        public decimal RemainingMonthlyLimit => SpendingLimit - SpentThisMonth;

        public double SpendingRatio => SpendingLimit == 0 ? 0 : (double)(SpentThisMonth / SpendingLimit);

        public bool IsOverBudget => SpentThisMonth > SpendingLimit;

        public decimal DailyAverageSpending
        {
            get
            {
                var daysInMonth = DateTime.DaysInMonth(DateTime.Today.Year, DateTime.Today.Month);
                var currentDay = DateTime.Today.Day;
                return currentDay == 0 ? 0 : SpentThisMonth / currentDay;
            }
        }

        public TimeSpan? TimeSinceLastActivity => LastActivity.HasValue ? DateTime.UtcNow - LastActivity.Value : null;

        public string StatusDescription => IsOnline ? "Online" : TimeSinceLastActivity?.TotalMinutes switch
        {
            < 60 => "Just now",
            < 1440 => $"{(int)(TimeSinceLastActivity.Value.TotalHours)} hours ago",
            _ => $"{(int)(TimeSinceLastActivity.Value.TotalDays)} days ago"
        };

        public MemberRoleType GetRoleType() => Role?.ToLower() switch
        {
            "parent" => MemberRoleType.Parent,
            "teen" => MemberRoleType.Teen,
            "child" => MemberRoleType.Child,
            _ => MemberRoleType.Child
        };

        public bool CanSpend(decimal amount) => Balance >= amount && SpentThisMonth + amount <= SpendingLimit;

        public void UpdateActivity()
        {
            LastActivity = DateTime.UtcNow;
            IsOnline = true;
            UpdatedAt = DateTime.UtcNow;
        }

        public void RecordTransaction(decimal amount, bool isIncome)
        {
            if (isIncome)
            {
                Balance += amount;
            }
            else
            {
                Balance -= amount;
                SpentThisMonth += amount;
            }

            TransactionsThisMonth++;
            UpdateActivity();
        }
    }

    public enum MemberRoleType
    {
        Parent = 0,
        Teen = 1,
        Child = 2
    }
}