using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DotnetFinancialTrackerApp.Models;

public class SavingsGoal
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Subtitle { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal TargetAmount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal CurrentAmount { get; set; }

    [Required]
    [MaxLength(50)]
    public string Category { get; set; } = string.Empty; // Emergency, Travel, Home, Education, etc.

    [MaxLength(500)]
    public string? Description { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public DateTime? TargetDate { get; set; }

    public bool IsActive { get; set; } = true;

    [Required]
    public string FamilyId { get; set; } = string.Empty;

    [MaxLength(50)]
    public string CreatedByMemberId { get; set; } = string.Empty;

    public GoalPriority Priority { get; set; } = GoalPriority.Medium;

    [MaxLength(7)]
    public string Color { get; set; } = "#000000";

    // Navigation properties
    [ForeignKey(nameof(FamilyId))]
    public virtual FamilyAccount? Family { get; set; }

    [ForeignKey(nameof(CreatedByMemberId))]
    public virtual FamilyMember? CreatedBy { get; set; }

    public virtual ICollection<SavingsGoalContribution> Contributions { get; set; } = new List<SavingsGoalContribution>();

    // Calculated properties
    public double ProgressPercentage => TargetAmount > 0 ? Math.Min((double)(CurrentAmount / TargetAmount) * 100, 100) : 0;

    public decimal RemainingAmount => Math.Max(0, TargetAmount - CurrentAmount);

    public bool IsCompleted => CurrentAmount >= TargetAmount;

    public TimeSpan? TimeRemaining => TargetDate.HasValue ? TargetDate.Value - DateTime.Now : null;

    public decimal EstimatedMonthlyContribution
    {
        get
        {
            if (!TargetDate.HasValue || TargetDate.Value <= DateTime.Now)
                return RemainingAmount;

            var monthsRemaining = (decimal)((TargetDate.Value - DateTime.Now).TotalDays / 30.44);
            return monthsRemaining > 0 ? RemainingAmount / monthsRemaining : RemainingAmount;
        }
    }

    // Methods
    public void AddContribution(decimal amount, string contributorMemberId, string? description = null)
    {
        CurrentAmount += amount;
        Contributions.Add(new SavingsGoalContribution
        {
            SavingsGoalId = Id,
            Amount = amount,
            ContributorMemberId = contributorMemberId,
            Description = description,
            ContributionDate = DateTime.UtcNow
        });
    }

    public bool CanContribute(decimal amount) => amount > 0 && IsActive;

    public void UpdateProgress()
    {
        CurrentAmount = Contributions.Where(c => !c.IsReversal).Sum(c => c.Amount)
                       - Contributions.Where(c => c.IsReversal).Sum(c => c.Amount);
    }
}
