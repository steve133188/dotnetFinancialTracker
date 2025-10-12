using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DotnetFinancialTrackerApp.Models;

public class SavingsGoalContribution
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int SavingsGoalId { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    [Required]
    [MaxLength(50)]
    public string ContributorMemberId { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Description { get; set; }

    public DateTime ContributionDate { get; set; } = DateTime.UtcNow;

    public bool IsReversal { get; set; } = false; // For handling withdrawals or corrections

    [MaxLength(50)]
    public string? SourceTransactionId { get; set; } // Link to originating transaction if applicable

    // Navigation properties
    [ForeignKey(nameof(SavingsGoalId))]
    public virtual SavingsGoal? SavingsGoal { get; set; }

    [ForeignKey(nameof(ContributorMemberId))]
    public virtual FamilyMember? Contributor { get; set; }

    // Methods
    public static SavingsGoalContribution CreateContribution(
        int savingsGoalId,
        decimal amount,
        string contributorMemberId,
        string? description = null)
    {
        return new SavingsGoalContribution
        {
            SavingsGoalId = savingsGoalId,
            Amount = Math.Abs(amount), // Ensure positive amount
            ContributorMemberId = contributorMemberId,
            Description = description,
            ContributionDate = DateTime.UtcNow,
            IsReversal = false
        };
    }

    public static SavingsGoalContribution CreateReversal(
        int savingsGoalId,
        decimal amount,
        string contributorMemberId,
        string description)
    {
        return new SavingsGoalContribution
        {
            SavingsGoalId = savingsGoalId,
            Amount = Math.Abs(amount), // Store as positive but marked as reversal
            ContributorMemberId = contributorMemberId,
            Description = $"Reversal: {description}",
            ContributionDate = DateTime.UtcNow,
            IsReversal = true
        };
    }
}