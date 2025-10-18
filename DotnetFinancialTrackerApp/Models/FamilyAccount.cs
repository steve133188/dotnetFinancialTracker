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

        // Simplified calculated properties for MVP
        public int ActiveMemberCount => Members?.Count() ?? 0;

        // Calculations moved to service layer
    }
}