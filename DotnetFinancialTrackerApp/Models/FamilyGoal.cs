using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DotnetFinancialTrackerApp.Models
{
    public class FamilyGoal
    {
        [Key]
        public string GoalId { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string Title { get; set; } = "";

        public string? Description { get; set; }

        public decimal TargetAmount { get; set; }

        public decimal CurrentAmount { get; set; }

        public DateTime TargetDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public GoalType Type { get; set; } = GoalType.Family;

        public GoalPriority Priority { get; set; } = GoalPriority.Medium;

        public string? ImageUrl { get; set; }

        public string? Color { get; set; } = "#01FFFF";

        public bool IsArchived { get; set; } = false;

        // Foreign key
        [Required]
        public string FamilyId { get; set; } = "";

        // Navigation properties
        [ForeignKey(nameof(FamilyId))]
        public virtual FamilyAccount? Family { get; set; }

        // Removed: FamilyMemberGoal and GoalContribution - simplified for MVP

        // Calculated properties
        public decimal RemainingAmount => Math.Max(0, TargetAmount - CurrentAmount);

        public double ProgressPercentage => TargetAmount == 0 ? 0 : Math.Min(100, (double)(CurrentAmount / TargetAmount * 100));

        public bool IsComplete => CurrentAmount >= TargetAmount;

        public bool IsOverdue => DateTime.Today > TargetDate && !IsComplete;

        public int DaysRemaining => Math.Max(0, (TargetDate - DateTime.Today).Days);

        public decimal MonthlyRequiredAmount
        {
            get
            {
                if (IsComplete || DaysRemaining <= 0) return 0;
                var monthsRemaining = Math.Max(1, DaysRemaining / 30.0m);
                return RemainingAmount / monthsRemaining;
            }
        }

        public decimal WeeklyRequiredAmount
        {
            get
            {
                if (IsComplete || DaysRemaining <= 0) return 0;
                var weeksRemaining = Math.Max(1, DaysRemaining / 7.0m);
                return RemainingAmount / weeksRemaining;
            }
        }

        public GoalStatus Status
        {
            get
            {
                if (IsComplete) return GoalStatus.Completed;
                if (IsOverdue) return GoalStatus.Overdue;
                if (ProgressPercentage >= 80) return GoalStatus.NearComplete;
                if (ProgressPercentage >= 50) return GoalStatus.OnTrack;
                if (DaysRemaining <= 30) return GoalStatus.BehindSchedule;
                return GoalStatus.InProgress;
            }
        }

        public List<string> ParticipantIds => new List<string>();

        // Methods
        public void AddContribution(string memberId, decimal amount, string? note = null)
        {
            CurrentAmount += amount;
            UpdatedAt = DateTime.UtcNow;
        }

        public void RemoveContribution(decimal amount)
        {
            CurrentAmount = Math.Max(0, CurrentAmount - amount);
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateProgress(decimal newAmount)
        {
            CurrentAmount = Math.Max(0, newAmount);
            UpdatedAt = DateTime.UtcNow;
        }

        public void MarkComplete()
        {
            CurrentAmount = TargetAmount;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Archive()
        {
            IsArchived = true;
            UpdatedAt = DateTime.UtcNow;
        }

        public TimeSpan GetTimeToTarget() => TargetDate - DateTime.Today;

        public string GetProgressDescription()
        {
            return Status switch
            {
                GoalStatus.Completed => "Goal achieved! 🎉",
                GoalStatus.Overdue => $"Overdue by {Math.Abs(DaysRemaining)} days",
                GoalStatus.NearComplete => $"Almost there! {RemainingAmount:C} to go",
                GoalStatus.OnTrack => $"On track - {DaysRemaining} days remaining",
                GoalStatus.BehindSchedule => $"Behind schedule - need {MonthlyRequiredAmount:C}/month",
                _ => $"{ProgressPercentage:F0}% complete"
            };
        }
    }

    public enum GoalType
    {
        Family = 0,
        Individual = 1,
        Emergency = 2,
        Vacation = 3,
        Education = 4,
        Purchase = 5,
        Charity = 6
    }

    public enum GoalPriority
    {
        Low = 0,
        Medium = 1,
        High = 2,
        Critical = 3
    }

    public enum GoalStatus
    {
        InProgress = 0,
        OnTrack = 1,
        BehindSchedule = 2,
        NearComplete = 3,
        Completed = 4,
        Overdue = 5,
        Paused = 6
    }
}