using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DotnetFinancialTrackerApp.Models
{
    public class VirtualCard
    {
        [Key]
        public string CardId { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string DisplayNumber { get; set; } = ""; // Last 4 digits

        [Required]
        public string CardColor { get; set; } = "#01FFFF"; // Hex color code

        public bool IsActive { get; set; } = true;

        public decimal SpentToday { get; set; }

        public decimal DailyLimit { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ExpiryDate { get; set; }

        public CardType Type { get; set; } = CardType.Virtual;

        public string? CardBrand { get; set; } = "Family Bank";

        // Security fields (encrypted in real implementation)
        public string? CardNumber { get; set; } // Should be encrypted
        public string? CVV { get; set; } // Should be encrypted
        public string? PIN { get; set; } // Should be hashed

        // Foreign key
        [Required]
        public string MemberId { get; set; } = "";

        // Navigation properties
        [ForeignKey(nameof(MemberId))]
        public virtual FamilyMember? Member { get; set; }

        public virtual ICollection<CardTransaction> Transactions { get; set; } = new List<CardTransaction>();

        // Calculated properties
        public decimal RemainingDailyLimit => DailyLimit - SpentToday;

        public double DailyUsagePercentage => DailyLimit == 0 ? 0 : (double)(SpentToday / DailyLimit * 100);

        public bool CanSpend(decimal amount) => IsActive && SpentToday + amount <= DailyLimit;

        public string MaskedNumber => CardNumber?.Length >= 4 ? $"**** **** **** {DisplayNumber}" : $"**** {DisplayNumber}";

        public bool IsExpired => ExpiryDate.HasValue && ExpiryDate.Value < DateTime.UtcNow;

        public bool IsNearExpiry => ExpiryDate.HasValue && ExpiryDate.Value.AddDays(-30) <= DateTime.UtcNow;

        // Methods
        public void RecordSpending(decimal amount)
        {
            SpentToday += amount;
        }

        public void ResetDailySpending()
        {
            SpentToday = 0;
        }

        public string GenerateDisplayNumber()
        {
            var random = new Random();
            return random.Next(1000, 9999).ToString();
        }

        public void RegenerateCard()
        {
            DisplayNumber = GenerateDisplayNumber();
            CreatedAt = DateTime.UtcNow;
            ExpiryDate = DateTime.UtcNow.AddYears(3);
        }
    }

    public enum CardType
    {
        Virtual = 0,
        Physical = 1,
        Prepaid = 2
    }
}