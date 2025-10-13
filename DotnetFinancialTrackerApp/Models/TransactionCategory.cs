using System.ComponentModel.DataAnnotations;

namespace DotnetFinancialTrackerApp.Models;

public class TransactionCategory
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty; // e.g., Food, Rent, Salary, Income

    [MaxLength(200)]
    public string? Description { get; set; }

    [MaxLength(20)]
    public string? Icon { get; set; } // Material icon name

    [MaxLength(7)]
    public string? Color { get; set; } // Hex color code

    public bool IsIncomeCategory { get; set; } = false; // True for income categories

    public bool IsActive { get; set; } = true;

    public int SortOrder { get; set; } = 0; // For ordering in UI

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

    // Methods
    public static List<TransactionCategory> GetDefaultCategories()
    {
        return new List<TransactionCategory>
        {
            // Income Categories
            new() { Name = "Salary", Description = "Primary income from employment", Icon = "work", Color = "#000000", IsIncomeCategory = true, SortOrder = 1 },
            new() { Name = "Freelance", Description = "Income from freelance work", Icon = "business_center", Color = "#222222", IsIncomeCategory = true, SortOrder = 2 },
            new() { Name = "Investment", Description = "Investment returns and dividends", Icon = "trending_up", Color = "#444444", IsIncomeCategory = true, SortOrder = 3 },
            new() { Name = "Other Income", Description = "Miscellaneous income", Icon = "attach_money", Color = "#666666", IsIncomeCategory = true, SortOrder = 4 },

            // Expense Categories
            new() { Name = "Groceries", Description = "Food and household supplies", Icon = "shopping_cart", Color = "#2a2a2a", IsIncomeCategory = false, SortOrder = 10 },
            new() { Name = "Dining Out", Description = "Restaurants and takeout", Icon = "restaurant", Color = "#3c3c3c", IsIncomeCategory = false, SortOrder = 11 },
            new() { Name = "Transportation", Description = "Gas, public transit, ride-sharing", Icon = "directions_car", Color = "#4e4e4e", IsIncomeCategory = false, SortOrder = 12 },
            new() { Name = "Housing", Description = "Rent, mortgage, utilities", Icon = "home", Color = "#5f5f5f", IsIncomeCategory = false, SortOrder = 13 },
            new() { Name = "Healthcare", Description = "Medical expenses and insurance", Icon = "local_hospital", Color = "#707070", IsIncomeCategory = false, SortOrder = 14 },
            new() { Name = "Entertainment", Description = "Movies, games, subscriptions", Icon = "movie", Color = "#828282", IsIncomeCategory = false, SortOrder = 15 },
            new() { Name = "Shopping", Description = "Clothing, electronics, miscellaneous", Icon = "shopping_bag", Color = "#949494", IsIncomeCategory = false, SortOrder = 16 },
            new() { Name = "Education", Description = "School, courses, books", Icon = "school", Color = "#a6a6a6", IsIncomeCategory = false, SortOrder = 17 },
            new() { Name = "Travel", Description = "Vacation and travel expenses", Icon = "flight", Color = "#b8b8b8", IsIncomeCategory = false, SortOrder = 18 },
            new() { Name = "Personal Care", Description = "Haircuts, beauty, gym", Icon = "face", Color = "#cacaca", IsIncomeCategory = false, SortOrder = 19 },
            new() { Name = "Other", Description = "Miscellaneous expenses", Icon = "more_horiz", Color = "#dcdcdc", IsIncomeCategory = false, SortOrder = 20 }
        };
    }
}
