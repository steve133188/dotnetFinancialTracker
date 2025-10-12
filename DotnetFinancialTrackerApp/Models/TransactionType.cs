namespace DotnetFinancialTrackerApp.Models;

public enum TransactionType
{
    Expense = 0,    // Default - money going out
    Income = 1,     // Money coming in
    Transfer = 2,   // Transfer between accounts/goals
    Refund = 3      // Money returned from a previous expense
}

public static class TransactionTypeExtensions
{
    public static string GetDisplayName(this TransactionType type) => type switch
    {
        TransactionType.Expense => "Expense",
        TransactionType.Income => "Income",
        TransactionType.Transfer => "Transfer",
        TransactionType.Refund => "Refund",
        _ => "Unknown"
    };

    public static string GetDisplayColor(this TransactionType type) => type switch
    {
        TransactionType.Expense => "#F44336",    // Red
        TransactionType.Income => "#4CAF50",     // Green
        TransactionType.Transfer => "#2196F3",   // Blue
        TransactionType.Refund => "#FF9800",     // Orange
        _ => "#607D8B"                           // Gray
    };

    public static string GetIcon(this TransactionType type) => type switch
    {
        TransactionType.Expense => "remove",
        TransactionType.Income => "add",
        TransactionType.Transfer => "swap_horiz",
        TransactionType.Refund => "undo",
        _ => "help"
    };

    public static bool IsPositive(this TransactionType type) => type switch
    {
        TransactionType.Income => true,
        TransactionType.Refund => true,
        _ => false
    };
}