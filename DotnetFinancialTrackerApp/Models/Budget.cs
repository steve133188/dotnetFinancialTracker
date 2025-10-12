using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DotnetFinancialTrackerApp.Models;

public class Budget
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string FamilyId { get; set; } = string.Empty;

    // Budget is now family-only, no category support

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Limit { get; set; }

    [Required]
    public DateTime Month { get; set; } = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

    [MaxLength(100)]
    public string? Name { get; set; } = "Family Budget"; // Always family budget

    [MaxLength(300)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    // Removed IsOverallBudget as all budgets are family-wide now

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(50)]
    public string CreatedByMemberId { get; set; } = string.Empty;

    // Navigation properties
    [ForeignKey(nameof(FamilyId))]
    public virtual FamilyAccount? Family { get; set; }

    [ForeignKey(nameof(CreatedByMemberId))]
    public virtual FamilyMember? CreatedBy { get; set; }

    // Calculated properties
    public string DisplayName => Name ?? "Family Budget";

    public DateTime MonthEnd => Month.AddMonths(1).AddDays(-1);

    public bool IsCurrentMonth => Month.Year == DateTime.Today.Year && Month.Month == DateTime.Today.Month;

    public int DaysInMonth => DateTime.DaysInMonth(Month.Year, Month.Month);

    public int DaysRemaining => IsCurrentMonth ? (MonthEnd - DateTime.Today).Days + 1 : 0;

    // Methods
    public decimal GetSpentAmount(IEnumerable<Transaction> transactions)
    {
        var monthStart = Month;
        var monthEnd = Month.AddMonths(1);

        var relevantTransactions = transactions.Where(t =>
            t.Date >= monthStart &&
            t.Date < monthEnd &&
            !t.Type.IsPositive());

        // Family budget includes all non-income transactions
        return relevantTransactions.Sum(t => t.Amount);
    }

    public decimal GetRemainingAmount(IEnumerable<Transaction> transactions)
    {
        return Math.Max(0, Limit - GetSpentAmount(transactions));
    }

    public double GetUsagePercentage(IEnumerable<Transaction> transactions)
    {
        if (Limit <= 0) return 0;
        return Math.Min(100, (double)(GetSpentAmount(transactions) / Limit) * 100);
    }

    public bool IsOverBudget(IEnumerable<Transaction> transactions)
    {
        return GetSpentAmount(transactions) > Limit;
    }

    public void UpdateTimestamp()
    {
        UpdatedAt = DateTime.UtcNow;
    }

    public static Budget CreateFamilyBudget(string familyId, decimal limit, string createdByMemberId, string? description = null)
    {
        return new Budget
        {
            FamilyId = familyId,
            Limit = limit,
            Name = "Family Budget",
            Description = description ?? "Monthly spending limit for the entire family",
            CreatedByMemberId = createdByMemberId,
            Month = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1)
        };
    }
}

