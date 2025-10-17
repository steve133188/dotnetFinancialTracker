using System.Linq.Expressions;
using DotnetFinancialTrackerApp.Models;

namespace DotnetFinancialTrackerApp.Services;

/// <summary>
/// Fluent builder for creating filter criteria with type safety
/// </summary>
public class FilterBuilder<T>
{
    private readonly FilterCriteria<T> _criteria = new();

    public static FilterBuilder<T> Create() => new();

    public FilterBuilder<T> Where(Expression<Func<T, bool>> predicate, string? name = null, bool isRequired = false)
    {
        _criteria.Filters.Add(new FilterExpression<T>
        {
            Predicate = predicate,
            Operator = FilterOperator.And,
            Name = name,
            IsRequired = isRequired
        });
        return this;
    }

    public FilterBuilder<T> Or(Expression<Func<T, bool>> predicate, string? name = null, bool isRequired = false)
    {
        _criteria.Filters.Add(new FilterExpression<T>
        {
            Predicate = predicate,
            Operator = FilterOperator.Or,
            Name = name,
            IsRequired = isRequired
        });
        return this;
    }

    public FilterBuilder<T> OrderBy<TKey>(Expression<Func<T, TKey>> keySelector)
    {
        _criteria.SortBy = new SortCriteria<T>
        {
            SortBy = keySelector as Expression<Func<T, object>> ??
                     Expression.Lambda<Func<T, object>>(
                         Expression.Convert(keySelector.Body, typeof(object)),
                         keySelector.Parameters),
            Direction = SortDirection.Ascending
        };
        return this;
    }

    public FilterBuilder<T> OrderByDescending<TKey>(Expression<Func<T, TKey>> keySelector)
    {
        _criteria.SortBy = new SortCriteria<T>
        {
            SortBy = keySelector as Expression<Func<T, object>> ??
                     Expression.Lambda<Func<T, object>>(
                         Expression.Convert(keySelector.Body, typeof(object)),
                         keySelector.Parameters),
            Direction = SortDirection.Descending
        };
        return this;
    }

    public FilterBuilder<T> Skip(int count)
    {
        _criteria.Skip = count;
        return this;
    }

    public FilterBuilder<T> Take(int count)
    {
        _criteria.Take = count;
        return this;
    }

    public FilterCriteria<T> Build() => _criteria;
}

/// <summary>
/// Predefined filters for common domain objects
/// </summary>
public static class TransactionFilters
{
    public static FilterExpression<Transaction> ByDateRange(DateTime from, DateTime to, string? name = null)
        => new()
        {
            Predicate = t => t.Date >= from && t.Date <= to,
            Name = name ?? $"Date range: {from:MM/dd} - {to:MM/dd}",
            IsRequired = false
        };

    public static FilterExpression<Transaction> ByCategory(string category, string? name = null)
        => new()
        {
            Predicate = t => t.Category != null && t.Category.Name == category,
            Name = name ?? $"Category: {category}",
            IsRequired = false
        };

    public static FilterExpression<Transaction> ByUser(string userName, string? name = null)
        => new()
        {
            Predicate = t => t.User == userName,
            Name = name ?? $"User: {userName}",
            IsRequired = false
        };

    public static FilterExpression<Transaction> ByType(bool isIncome, string? name = null)
        => new()
        {
            Predicate = t => t.IsIncome == isIncome,
            Name = name ?? (isIncome ? "Income only" : "Expenses only"),
            IsRequired = false
        };

    public static FilterExpression<Transaction> ByAmountRange(decimal minAmount, decimal maxAmount, string? name = null)
        => new()
        {
            Predicate = t => t.Amount >= minAmount && t.Amount <= maxAmount,
            Name = name ?? $"Amount: {minAmount:C} - {maxAmount:C}",
            IsRequired = false
        };

    public static FilterExpression<Transaction> HasDescription(string? name = null)
        => new()
        {
            Predicate = t => !string.IsNullOrEmpty(t.Description),
            Name = name ?? "Has description",
            IsRequired = false
        };
}

public static class SavingsGoalFilters
{
    public static FilterExpression<SavingsGoal> ByStatus(bool isCompleted, string? name = null)
        => new()
        {
            Predicate = g => g.IsCompleted == isCompleted,
            Name = name ?? (isCompleted ? "Completed" : "Active"),
            IsRequired = false
        };

    public static FilterExpression<SavingsGoal> ByCategory(string category, string? name = null)
        => new()
        {
            Predicate = g => g.Category == category,
            Name = name ?? $"Category: {category}",
            IsRequired = false
        };

    public static FilterExpression<SavingsGoal> ByTargetDateRange(DateTime from, DateTime to, string? name = null)
        => new()
        {
            Predicate = g => g.TargetDate.HasValue && g.TargetDate >= from && g.TargetDate <= to,
            Name = name ?? $"Target date: {from:MM/dd} - {to:MM/dd}",
            IsRequired = false
        };

    public static FilterExpression<SavingsGoal> ByProgressRange(double minPercentage, double maxPercentage, string? name = null)
        => new()
        {
            Predicate = g => g.ProgressPercentage >= minPercentage && g.ProgressPercentage <= maxPercentage,
            Name = name ?? $"Progress: {minPercentage}% - {maxPercentage}%",
            IsRequired = false
        };

    public static FilterExpression<SavingsGoal> IsActive(string? name = null)
        => new()
        {
            Predicate = g => g.IsActive && !g.IsCompleted,
            Name = name ?? "Active goals",
            IsRequired = false
        };
}

public static class BudgetFilters
{
    public static FilterExpression<Budget> ByMonth(DateTime month, string? name = null)
        => new()
        {
            Predicate = b => b.Month.Year == month.Year && b.Month.Month == month.Month,
            Name = name ?? $"Month: {month:MMM yyyy}",
            IsRequired = false
        };

    public static FilterExpression<Budget> ByYear(int year, string? name = null)
        => new()
        {
            Predicate = b => b.Month.Year == year,
            Name = name ?? $"Year: {year}",
            IsRequired = false
        };

    public static FilterExpression<Budget> IsActive(string? name = null)
        => new()
        {
            Predicate = b => b.IsActive,
            Name = name ?? "Active budgets",
            IsRequired = false
        };

    public static FilterExpression<Budget> ByLimitRange(decimal minLimit, decimal maxLimit, string? name = null)
        => new()
        {
            Predicate = b => b.Limit >= minLimit && b.Limit <= maxLimit,
            Name = name ?? $"Budget: {minLimit:C} - {maxLimit:C}",
            IsRequired = false
        };
}

/// <summary>
/// Helper methods for common filtering patterns
/// </summary>
public static class FilterHelpers
{
    public static FilterExpression<T> SearchText<T>(Expression<Func<T, string?>> textProperty, string searchTerm, string? name = null)
    {
        var parameter = textProperty.Parameters[0];
        var property = textProperty.Body;

        // Create expression: textProperty != null && textProperty.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
        var nullCheck = Expression.NotEqual(property, Expression.Constant(null));
        var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string), typeof(StringComparison) });
        var containsCall = Expression.Call(
            property,
            containsMethod!,
            Expression.Constant(searchTerm),
            Expression.Constant(StringComparison.OrdinalIgnoreCase)
        );

        var combinedExpression = Expression.AndAlso(nullCheck, containsCall);
        var lambda = Expression.Lambda<Func<T, bool>>(combinedExpression, parameter);

        return new FilterExpression<T>
        {
            Predicate = lambda,
            Name = name ?? $"Search: '{searchTerm}'",
            IsRequired = false
        };
    }

    public static FilterExpression<T> IsInList<T, TProperty>(Expression<Func<T, TProperty>> property, IEnumerable<TProperty> values, string? name = null)
    {
        var parameter = property.Parameters[0];
        var propertyAccess = property.Body;

        // Create expression: values.Contains(property)
        var valuesConstant = Expression.Constant(values);
        var containsMethod = typeof(Enumerable).GetMethods()
            .First(m => m.Name == "Contains" && m.GetParameters().Length == 2)
            .MakeGenericMethod(typeof(TProperty));

        var containsCall = Expression.Call(containsMethod, valuesConstant, propertyAccess);
        var lambda = Expression.Lambda<Func<T, bool>>(containsCall, parameter);

        return new FilterExpression<T>
        {
            Predicate = lambda,
            Name = name ?? $"In list ({values.Count()} items)",
            IsRequired = false
        };
    }

    public static FilterExpression<T> DateInRange<T>(Expression<Func<T, DateTime>> dateProperty, DateTime from, DateTime to, string? name = null)
    {
        var parameter = dateProperty.Parameters[0];
        var property = dateProperty.Body;

        var fromConstant = Expression.Constant(from);
        var toConstant = Expression.Constant(to);

        var greaterThanOrEqual = Expression.GreaterThanOrEqual(property, fromConstant);
        var lessThanOrEqual = Expression.LessThanOrEqual(property, toConstant);
        var combinedExpression = Expression.AndAlso(greaterThanOrEqual, lessThanOrEqual);

        var lambda = Expression.Lambda<Func<T, bool>>(combinedExpression, parameter);

        return new FilterExpression<T>
        {
            Predicate = lambda,
            Name = name ?? $"Date: {from:MM/dd} - {to:MM/dd}",
            IsRequired = false
        };
    }
}