using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DotnetFinancialTrackerApp.Models;

public class Transaction
{
    [Key]
    public int Id { get; set; }

    // Foreign key relationships
    [Required]
    [MaxLength(50)]
    public string FamilyMemberId { get; set; } = string.Empty;

    [Required]
    public int CategoryId { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    [Required]
    public DateTime Date { get; set; } = DateTime.Today;

    [MaxLength(200)]
    public string? Description { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    [Required]
    public TransactionType Type { get; set; } = TransactionType.Expense;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // For tracking budget surplus transfers to savings
    public int? RelatedSavingsGoalId { get; set; }

    // Navigation properties
    [ForeignKey(nameof(FamilyMemberId))]
    public virtual FamilyMember? FamilyMember { get; set; }

    [ForeignKey(nameof(CategoryId))]
    public virtual TransactionCategory? Category { get; set; }

    [ForeignKey(nameof(RelatedSavingsGoalId))]
    public virtual SavingsGoal? RelatedSavingsGoal { get; set; }

    // Calculated properties
    public string DisplayAmount => Type.IsPositive() ? $"+{Amount:C}" : $"-{Amount:C}";

    public string UserName => FamilyMember?.Name ?? "Unknown";

    public string CategoryName => Category?.Name ?? "Uncategorized";

    public string TypeDisplayName => Type.GetDisplayName();

    public string TypeColor => Type.GetDisplayColor();

    public string TypeIcon => Type.GetIcon();

    // For backward compatibility with existing code
    [NotMapped]
    public string User
    {
        get => UserName;
        set { /* Ignore sets - use FamilyMemberId instead */ }
    }

    [NotMapped]
    public bool IsIncome => Type.IsPositive();

    // Methods
    public void UpdateTimestamp()
    {
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsSameMonth(DateTime compareDate)
    {
        return Date.Year == compareDate.Year && Date.Month == compareDate.Month;
    }

    public bool IsInDateRange(DateTime startDate, DateTime endDate)
    {
        return Date >= startDate && Date <= endDate;
    }
}

