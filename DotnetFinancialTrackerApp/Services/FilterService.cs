using System.Linq.Expressions;

namespace DotnetFinancialTrackerApp.Services;

public class FilterService<T> : IFilterService<T>
{
    public async Task<FilterResult<T>> ApplyFiltersAsync(IEnumerable<T> items, FilterCriteria<T> criteria)
    {
        await Task.CompletedTask; // Method is synchronous but keeping async for consistency

        var query = items.AsQueryable();
        var totalCount = query.Count();
        var appliedFilters = new List<string>();

        // Apply filters
        foreach (var filter in criteria.Filters)
        {
            if (filter.Predicate != null)
            {
                if (filter.Operator == FilterOperator.And)
                {
                    query = query.Where(filter.Predicate);
                }
                else
                {
                    // For OR operations, we need to combine with previous conditions
                    // This is simplified - in practice you might want more complex OR logic
                    query = query.Where(filter.Predicate);
                }

                if (!string.IsNullOrEmpty(filter.Name))
                {
                    appliedFilters.Add(filter.Name);
                }
            }
        }

        var filteredCount = query.Count();

        // Apply sorting
        if (criteria.SortBy != null)
        {
            query = criteria.SortBy.Direction == SortDirection.Ascending
                ? query.OrderBy(criteria.SortBy.SortBy)
                : query.OrderByDescending(criteria.SortBy.SortBy);
        }

        // Apply pagination
        if (criteria.Skip.HasValue)
        {
            query = query.Skip(criteria.Skip.Value);
        }

        if (criteria.Take.HasValue)
        {
            query = query.Take(criteria.Take.Value);
        }

        var results = query.ToList();
        var hasMore = criteria.Skip.HasValue && criteria.Take.HasValue
            ? (criteria.Skip.Value + criteria.Take.Value) < filteredCount
            : false;

        return new FilterResult<T>
        {
            Items = results,
            TotalCount = totalCount,
            FilteredCount = filteredCount,
            AppliedFilters = appliedFilters,
            HasMore = hasMore
        };
    }

    public async Task<List<T>> FilterAsync(IEnumerable<T> items, params FilterExpression<T>[] filters)
    {
        var criteria = new FilterCriteria<T>
        {
            Filters = filters.ToList()
        };

        var result = await ApplyFiltersAsync(items, criteria);
        return result.Items;
    }

    public async Task<PagedResult<T>> FilterWithPaginationAsync(IEnumerable<T> items, FilterCriteria<T> criteria, PaginationOptions pagination)
    {
        // Ensure page size doesn't exceed maximum
        var pageSize = Math.Min(pagination.PageSize, pagination.MaxPageSize);
        var pageNumber = Math.Max(1, pagination.PageNumber);

        // Update criteria with pagination
        criteria.Skip = (pageNumber - 1) * pageSize;
        criteria.Take = pageSize;

        var filterResult = await ApplyFiltersAsync(items, criteria);

        var totalPages = (int)Math.Ceiling((double)filterResult.FilteredCount / pageSize);

        return new PagedResult<T>
        {
            Items = filterResult.Items,
            TotalCount = filterResult.FilteredCount,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalPages = totalPages,
            HasPreviousPage = pageNumber > 1,
            HasNextPage = pageNumber < totalPages
        };
    }
}