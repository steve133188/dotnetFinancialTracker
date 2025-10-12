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

    // For overall family budget, CategoryId can be null
    // For category-specific budgets, CategoryId links to TransactionCategory
    public int? CategoryId { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Limit { get; set; }

    [Required]
    public DateTime Month { get; set; } = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

    [MaxLength(100)]
    public string? Name { get; set; } // e.g., "Overall Family Budget", "Groceries Budget"

    [MaxLength(300)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public bool IsOverallBudget { get; set; } = false; // True for family-wide budget, False for category-specific

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(50)]
    public string CreatedByMemberId { get; set; } = string.Empty;

    // Navigation properties
    [ForeignKey(nameof(FamilyId))]
    public virtual FamilyAccount? Family { get; set; }

    [ForeignKey(nameof(CategoryId))]
    public virtual TransactionCategory? Category { get; set; }

    [ForeignKey(nameof(CreatedByMemberId))]
    public virtual FamilyMember? CreatedBy { get; set; }

    // Calculated properties
    public string DisplayName => IsOverallBudget ? "Family Budget" : Category?.Name ?? Name ?? "Budget";

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

        if (IsOverallBudget)
        {
            // For overall budget, sum all non-income transactions
            return relevantTransactions.Sum(t => t.Amount);
        }
        else if (CategoryId.HasValue)
        {
            // For category budget, sum only transactions in this category
            return relevantTransactions.Where(t => t.CategoryId == CategoryId.Value).Sum(t => t.Amount);
        }

        return 0;
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

    public static Budget CreateOverallFamilyBudget(string familyId, decimal limit, string createdByMemberId)
    {
        return new Budget
        {
            FamilyId = familyId,
            CategoryId = null,
            Limit = limit,
            Name = "Overall Family Budget",
            Description = "Total monthly spending limit for the entire family",
            IsOverallBudget = true,
            CreatedByMemberId = createdByMemberId,
            Month = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1)
        };
    }

    public static Budget CreateCategoryBudget(string familyId, int categoryId, decimal limit, string createdByMemberId, string? description = null)
    {
        return new Budget
        {
            FamilyId = familyId,
            CategoryId = categoryId,
            Limit = limit,
            Description = description,
            IsOverallBudget = false,
            CreatedByMemberId = createdByMemberId,
            Month = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1)
        };
    }
}

