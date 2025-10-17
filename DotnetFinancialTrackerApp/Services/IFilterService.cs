using System.Linq.Expressions;

namespace DotnetFinancialTrackerApp.Services;

public interface IFilterService<T>
{
    Task<FilterResult<T>> ApplyFiltersAsync(IEnumerable<T> items, FilterCriteria<T> criteria);
    Task<List<T>> FilterAsync(IEnumerable<T> items, params FilterExpression<T>[] filters);
    Task<PagedResult<T>> FilterWithPaginationAsync(IEnumerable<T> items, FilterCriteria<T> criteria, PaginationOptions pagination);
}

public class FilterCriteria<T>
{
    public List<FilterExpression<T>> Filters { get; set; } = new();
    public SortCriteria<T>? SortBy { get; set; }
    public int? Skip { get; set; }
    public int? Take { get; set; }
    public bool IncludeCount { get; set; } = true;
}

public class FilterExpression<T>
{
    public Expression<Func<T, bool>> Predicate { get; set; } = null!;
    public FilterOperator Operator { get; set; } = FilterOperator.And;
    public string? Name { get; set; }
    public bool IsRequired { get; set; } = false;
}

public class SortCriteria<T>
{
    public Expression<Func<T, object>> SortBy { get; set; } = null!;
    public SortDirection Direction { get; set; } = SortDirection.Ascending;
}

public class FilterResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int FilteredCount { get; set; }
    public List<string> AppliedFilters { get; set; } = new();
    public bool HasMore { get; set; }
}

public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public bool HasPreviousPage { get; set; }
    public bool HasNextPage { get; set; }
}

public class PaginationOptions
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int MaxPageSize { get; set; } = 100;
}

public enum FilterOperator
{
    And,
    Or
}

public enum SortDirection
{
    Ascending,
    Descending
}