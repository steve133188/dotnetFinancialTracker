namespace DotnetFinancialTrackerApp.Models;

public enum TransactionType
{
    Expense = 0,    // Default - money going out
    Income = 1      // Money coming in
}

public static class TransactionTypeExtensions
{
    public static string GetDisplayName(this TransactionType type) => type switch
    {
        TransactionType.Expense => "Expense",
        TransactionType.Income => "Income",
        _ => "Unknown"
    };

    public static string GetDisplayColor(this TransactionType type) => type switch
    {
        TransactionType.Expense => "#F44336",    // Red
        TransactionType.Income => "#4CAF50",     // Green
        _ => "#607D8B"                           // Gray
    };

    public static string GetIcon(this TransactionType type) => type switch
    {
        TransactionType.Expense => "remove",
        TransactionType.Income => "add",
        _ => "help"
    };

    public static bool IsPositive(this TransactionType type) => type switch
    {
        TransactionType.Income => true,
        _ => false
    };
}