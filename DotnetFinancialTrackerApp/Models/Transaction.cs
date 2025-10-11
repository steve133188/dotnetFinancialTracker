namespace DotnetFinancialTrackerApp.Models;

public class Transaction
{
    public int Id { get; set; }
    public string User { get; set; } = string.Empty; // e.g., "Parent", "Child"
    public string Category { get; set; } = string.Empty; // e.g., Food, Rent
    public decimal Amount { get; set; }
    public DateTime Date { get; set; } = DateTime.Today;
    public string? Description { get; set; }
    public string? Notes { get; set; }
    public bool IsIncome { get; set; } = false;
}

