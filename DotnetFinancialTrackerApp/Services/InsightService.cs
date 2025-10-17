using System.Globalization;
using DotnetFinancialTrackerApp.Models;

namespace DotnetFinancialTrackerApp.Services;

public class InsightService : IInsightService
{
    private readonly ITransactionsService _transactionsService;
    private readonly IUserService _userService;

    public InsightService(ITransactionsService transactionsService, IUserService userService)
    {
        _transactionsService = transactionsService;
        _userService = userService;
    }

    public async Task<InsightData> GetInsightDataAsync(PeriodType periodType, DateTime currentPeriodStart, string? selectedUserId = null)
    {
        var allTransactions = await _transactionsService.GetAsync();
        var familyUsers = await _userService.GetUsersAsync();

        var (start, end) = GetCurrentPeriodRange(periodType, currentPeriodStart);
        var (prevStart, prevEnd) = GetPreviousPeriodRange(periodType, currentPeriodStart);

        var currentTransactions = FilterTransactions(allTransactions, start, end);
        var previousTransactions = FilterTransactions(allTransactions, prevStart, prevEnd);

        // Apply user filter if selected
        if (!string.IsNullOrEmpty(selectedUserId))
        {
            var selectedUser = familyUsers.FirstOrDefault(u => u.Id.ToString() == selectedUserId);
            if (selectedUser != null)
            {
                currentTransactions = currentTransactions.Where(t => t.User == selectedUser.Name).ToList();
                previousTransactions = previousTransactions.Where(t => t.User == selectedUser.Name).ToList();
            }
        }

        var insightData = new InsightData
        {
            FilteredIncome = currentTransactions.Where(t => t.IsIncome).Sum(t => t.Amount),
            FilteredExpense = currentTransactions.Where(t => !t.IsIncome).Sum(t => t.Amount),
            PreviousExpense = previousTransactions.Where(t => !t.IsIncome).Sum(t => t.Amount),
            PreviousIncome = previousTransactions.Where(t => t.IsIncome).Sum(t => t.Amount),
            HighlightTransactions = currentTransactions
                .OrderByDescending(t => t.Date)
                .ThenByDescending(t => t.Id)
                .ToList(),
            TopCategories = await GetCategoryBreakdownAsync(currentTransactions),
            MemberExpenseSummaries = await GetMemberExpenseSummariesAsync(currentTransactions),
            MemberIncomeSummaries = await GetMemberIncomeSummariesAsync(currentTransactions),
            ChartData = await GetChartDataAsync(currentTransactions, start, periodType),
            LineChartSeries = await GetLineChartSeriesAsync(currentTransactions, start, periodType)
        };

        // Calculate trends
        insightData.ExpenseTrend = insightData.FilteredExpense == insightData.PreviousExpense ? TrendDirection.Unchanged :
                                  insightData.FilteredExpense > insightData.PreviousExpense ? TrendDirection.Up : TrendDirection.Down;
        insightData.IncomeTrend = insightData.FilteredIncome == insightData.PreviousIncome ? TrendDirection.Unchanged :
                                 insightData.FilteredIncome > insightData.PreviousIncome ? TrendDirection.Up : TrendDirection.Down;

        // Set XAxis labels from chart data
        insightData.XAxisLabels = insightData.ChartData.Select(p => p.Label).ToList();

        return insightData;
    }

    public async Task<List<CategorySummary>> GetCategoryBreakdownAsync(List<Transaction> transactions)
    {
        await Task.CompletedTask; // Placeholder for async operation

        var expenseTransactions = transactions.Where(t => !t.IsIncome).ToList();
        var totalExpenses = expenseTransactions.Sum(t => t.Amount);

        return expenseTransactions
            .GroupBy(t => NormalizeCategory(t.Category?.Name))
            .Select(g => new CategorySummary
            {
                Name = g.Key,
                Amount = g.Sum(t => t.Amount),
                TransactionCount = g.Count(),
                Percentage = totalExpenses > 0 ? (double)(g.Sum(t => t.Amount) / totalExpenses) : 0
            })
            .OrderByDescending(c => c.Amount)
            .ToList();
    }

    public async Task<List<MemberSummary>> GetMemberExpenseSummariesAsync(List<Transaction> transactions)
    {
        var expenseTransactions = transactions.Where(t => !t.IsIncome && !string.IsNullOrEmpty(t.User)).ToList();
        return await BuildMemberSummaries(expenseTransactions, includeCategories: true);
    }

    public async Task<List<MemberSummary>> GetMemberIncomeSummariesAsync(List<Transaction> transactions)
    {
        var incomeTransactions = transactions.Where(t => t.IsIncome && !string.IsNullOrEmpty(t.User)).ToList();
        return await BuildMemberSummaries(incomeTransactions, includeCategories: false);
    }

    public async Task<List<ChartDataPoint>> GetChartDataAsync(List<Transaction> transactions, DateTime start, PeriodType periodType)
    {
        await Task.CompletedTask; // Placeholder for async operation

        var expenseTransactions = transactions.Where(t => !t.IsIncome).ToList();
        if (!expenseTransactions.Any()) return new List<ChartDataPoint>();

        var points = CreateChartPoints(expenseTransactions, start, periodType);
        return points;
    }

    public async Task<List<ChartSeries>> GetLineChartSeriesAsync(List<Transaction> transactions, DateTime start, PeriodType periodType)
    {
        await Task.CompletedTask; // Placeholder for async operation

        if (!transactions.Any()) return new List<ChartSeries>();

        var expenseTransactions = transactions.Where(t => !t.IsIncome).ToList();
        var incomeTransactions = transactions.Where(t => t.IsIncome).ToList();

        var expenseChartPoints = CreateChartPoints(expenseTransactions, start, periodType);
        var incomeChartPoints = CreateChartPoints(incomeTransactions, start, periodType);

        var spendingData = expenseChartPoints.Select(p => (double)p.Amount).ToArray();
        var incomeData = incomeChartPoints.Select(p => (double)p.Amount).ToArray();

        // Calculate balance (cumulative: income - expenses)
        var balanceData = new double[spendingData.Length];
        var runningBalance = 0.0;

        for (int i = 0; i < spendingData.Length; i++)
        {
            runningBalance += incomeData[i] - spendingData[i];
            balanceData[i] = runningBalance;
        }

        return new List<ChartSeries>
        {
            new ChartSeries
            {
                Name = "Spending",
                Data = spendingData
            },
            new ChartSeries
            {
                Name = "Income",
                Data = incomeData
            },
            new ChartSeries
            {
                Name = "Balance",
                Data = balanceData
            }
        };
    }

    public (DateTime Start, DateTime End) GetCurrentPeriodRange(PeriodType periodType, DateTime currentPeriodStart)
    {
        return periodType switch
        {
            PeriodType.Weekly => (currentPeriodStart, currentPeriodStart.AddDays(7).AddTicks(-1)),
            PeriodType.Monthly => (currentPeriodStart, currentPeriodStart.AddMonths(1).AddTicks(-1)),
            PeriodType.Yearly => (currentPeriodStart, currentPeriodStart.AddYears(1).AddTicks(-1)),
            _ => (currentPeriodStart, currentPeriodStart.AddMonths(1).AddTicks(-1))
        };
    }

    public (DateTime Start, DateTime End) GetPreviousPeriodRange(PeriodType periodType, DateTime currentPeriodStart)
    {
        return periodType switch
        {
            PeriodType.Weekly => (currentPeriodStart.AddDays(-7), currentPeriodStart.AddTicks(-1)),
            PeriodType.Monthly => (currentPeriodStart.AddMonths(-1), currentPeriodStart.AddTicks(-1)),
            PeriodType.Yearly => (currentPeriodStart.AddYears(-1), currentPeriodStart.AddTicks(-1)),
            _ => (currentPeriodStart.AddMonths(-1), currentPeriodStart.AddTicks(-1))
        };
    }

    public List<Transaction> FilterTransactions(List<Transaction> allTransactions, DateTime start, DateTime end)
    {
        return allTransactions.Where(t =>
            t.Date.Date >= start.Date && t.Date.Date <= end.Date).ToList();
    }

    public DateTime GetPeriodStartForDate(DateTime date, PeriodType periodType)
    {
        return periodType switch
        {
            PeriodType.Weekly => GetWeekStart(date),
            PeriodType.Monthly => new DateTime(date.Year, date.Month, 1),
            PeriodType.Yearly => new DateTime(date.Year, 1, 1),
            _ => date.Date
        };
    }

    private DateTime GetWeekStart(DateTime date)
    {
        var culture = CultureInfo.CurrentCulture;
        var firstDay = culture.DateTimeFormat.FirstDayOfWeek;
        var diff = (7 + (date.DayOfWeek - firstDay)) % 7;
        return date.Date.AddDays(-diff);
    }

    private async Task<List<MemberSummary>> BuildMemberSummaries(List<Transaction> transactions, bool includeCategories)
    {
        await Task.CompletedTask; // Placeholder for async operation

        var summaries = new List<MemberSummary>();

        if (!transactions.Any())
        {
            return summaries;
        }

        var totalAmount = transactions.Sum(t => t.Amount);
        if (totalAmount == 0)
        {
            return summaries;
        }

        var memberGroups = transactions.GroupBy(t => t.User).ToList();

        foreach (var group in memberGroups)
        {
            var memberTransactions = group.ToList();
            var memberTotal = memberTransactions.Sum(t => t.Amount);
            var percentage = (double)(memberTotal / totalAmount);

            var topCategories = includeCategories
                ? memberTransactions
                    .Where(t => t.Category != null)
                    .GroupBy(t => t.Category!.Name)
                    .Select(g => new MemberCategorySummary
                    {
                        CategoryName = g.Key,
                        Amount = g.Sum(t => t.Amount),
                        TransactionCount = g.Count()
                    })
                    .OrderByDescending(c => c.Amount)
                    .Take(3)
                    .ToList()
                : new List<MemberCategorySummary>();

            summaries.Add(new MemberSummary
            {
                Name = group.Key ?? "Unknown",
                TotalAmount = memberTotal,
                TransactionCount = memberTransactions.Count,
                Percentage = percentage,
                TopCategories = topCategories
            });
        }

        return summaries.OrderByDescending(m => m.TotalAmount).ToList();
    }

    private List<ChartDataPoint> CreateChartPoints(List<Transaction> transactions, DateTime start, PeriodType periodType)
    {
        return periodType switch
        {
            PeriodType.Weekly => BuildWeeklyChartPoints(transactions, start),
            PeriodType.Monthly => BuildMonthlyChartPoints(transactions, start),
            PeriodType.Yearly => BuildYearlyChartPoints(transactions, start),
            _ => new List<ChartDataPoint>()
        };
    }

    private List<ChartDataPoint> BuildWeeklyChartPoints(List<Transaction> transactions, DateTime start)
    {
        var points = new List<ChartDataPoint>();
        for (int day = 0; day < 7; day++)
        {
            var dayStart = start.AddDays(day);
            var dayEnd = dayStart.AddDays(1);
            var amount = transactions.Where(t => t.Date.Date >= dayStart.Date && t.Date.Date < dayEnd.Date).Sum(t => t.Amount);

            points.Add(new ChartDataPoint
            {
                Label = dayStart.ToString("ddd"),
                Amount = amount
            });
        }
        AssignEvenlySpacedXPositions(points);
        CalculateYPositions(points);
        return points;
    }

    private List<ChartDataPoint> BuildMonthlyChartPoints(List<Transaction> transactions, DateTime start)
    {
        var points = new List<ChartDataPoint>();
        var monthEnd = start.AddMonths(1);
        var rollingWeekStart = GetWeekStart(start);

        while (rollingWeekStart < monthEnd)
        {
            var rangeStart = rollingWeekStart < start ? start : rollingWeekStart;
            var rangeEndExclusive = rollingWeekStart.AddDays(7);

            if (rangeEndExclusive > monthEnd)
            {
                rangeEndExclusive = monthEnd;
            }

            var amount = transactions
                .Where(t => t.Date >= rangeStart && t.Date < rangeEndExclusive)
                .Sum(t => t.Amount);

            var rangeEndInclusive = rangeEndExclusive.AddDays(-1);
            var label = rangeStart.Month == rangeEndInclusive.Month
                ? $"{rangeStart:MMM d}-{rangeEndInclusive:dd}"
                : $"{rangeStart:MMM d}-{rangeEndInclusive:MMM d}";

            points.Add(new ChartDataPoint
            {
                Label = label,
                Amount = amount
            });

            rollingWeekStart = rollingWeekStart.AddDays(7);
        }
        AssignEvenlySpacedXPositions(points);
        CalculateYPositions(points);
        return points;
    }

    private List<ChartDataPoint> BuildYearlyChartPoints(List<Transaction> transactions, DateTime start)
    {
        var points = new List<ChartDataPoint>();
        for (int month = 0; month < 12; month++)
        {
            var monthStart = start.AddMonths(month);
            var monthEnd = monthStart.AddMonths(1);
            var amount = transactions.Where(t => t.Date >= monthStart && t.Date < monthEnd).Sum(t => t.Amount);

            points.Add(new ChartDataPoint
            {
                Label = monthStart.ToString("MMM"),
                Amount = amount
            });
        }
        AssignEvenlySpacedXPositions(points);
        CalculateYPositions(points);
        return points;
    }

    private void AssignEvenlySpacedXPositions(List<ChartDataPoint> points)
    {
        if (!points.Any())
        {
            return;
        }

        var divisions = Math.Max(1, points.Count - 1);

        for (var i = 0; i < points.Count; i++)
        {
            points[i].XPosition = divisions == 0 ? 0 : (i / (double)divisions) * 100;
        }
    }

    private void CalculateYPositions(List<ChartDataPoint> points)
    {
        if (!points.Any()) return;

        var maxAmount = points.Max(p => p.Amount);
        if (maxAmount == 0) return;

        foreach (var point in points)
        {
            point.YPosition = (double)(point.Amount / maxAmount) * 80; // 80% max height
        }
    }

    private static string NormalizeCategory(string? category)
        => string.IsNullOrWhiteSpace(category) ? "Uncategorized" : category.Trim();
}