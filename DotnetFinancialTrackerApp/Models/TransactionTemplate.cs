namespace DotnetFinancialTrackerApp.Models;

public class TransactionTemplate
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty; // e.g., "Coffee", "Lunch", "Gas"
    public string Category { get; set; } = string.Empty; // e.g., "Food", "Transportation"
    public decimal? DefaultAmount { get; set; } // Optional preset amount
    public bool IsIncome { get; set; } = false;
    public string Icon { get; set; } = string.Empty; // Material Design icon name
    public string Color { get; set; } = "#01FFFF"; // Theme color
    public int UsageCount { get; set; } = 0; // Track popularity for smart ordering
    public DateTime LastUsed { get; set; } = DateTime.Now;
    public bool IsActive { get; set; } = true;
    public string User { get; set; } = string.Empty; // User-specific templates
}