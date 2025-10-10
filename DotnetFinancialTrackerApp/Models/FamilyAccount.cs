using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DotnetFinancialTrackerApp.Models
{
    public class FamilyAccount
    {
        [Key]
        public string FamilyId { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string FamilyName { get; set; } = "";

        public decimal TotalBalance { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public string? FamilyImageUrl { get; set; }

        public string? BankSortCode { get; set; }

        public string? BankAccountNumber { get; set; }

        // Navigation properties
        public virtual ICollection<FamilyMember> Members { get; set; } = new List<FamilyMember>();

        public virtual ICollection<FamilyGoal> Goals { get; set; } = new List<FamilyGoal>();

        public virtual FamilyInsights? Insights { get; set; }

        // Calculated properties
        public decimal TotalMemberBalances => Members?.Sum(m => m.Balance) ?? 0m;

        public int ActiveMemberCount => Members?.Count(m => m.IsActive) ?? 0;

        public decimal MonthlyIncome => Members?.Sum(m => m.MonthlyAllowance) ?? 0m;

        public decimal MonthlySpending => Members?.Sum(m => m.SpentThisMonth) ?? 0m;

        public double SavingsRate => MonthlyIncome == 0 ? 0 : (double)((MonthlyIncome - MonthlySpending) / MonthlyIncome);
    }
}