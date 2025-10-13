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

    public static string GetDisplayColor(this TransactionType type) => "#000000"; // Consistent black color

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