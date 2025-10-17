using DotnetFinancialTrackerApp.Models;

namespace DotnetFinancialTrackerApp.Services;

public interface IInsightService
{
    Task<InsightData> GetInsightDataAsync(PeriodType periodType, DateTime currentPeriodStart, string? selectedUserId = null);
    Task<List<CategorySummary>> GetCategoryBreakdownAsync(List<Transaction> transactions);
    Task<List<MemberSummary>> GetMemberExpenseSummariesAsync(List<Transaction> transactions);
    Task<List<MemberSummary>> GetMemberIncomeSummariesAsync(List<Transaction> transactions);
    Task<List<ChartDataPoint>> GetChartDataAsync(List<Transaction> transactions, DateTime start, PeriodType periodType);
    Task<List<ChartSeries>> GetLineChartSeriesAsync(List<Transaction> transactions, DateTime start, PeriodType periodType);
    (DateTime Start, DateTime End) GetCurrentPeriodRange(PeriodType periodType, DateTime currentPeriodStart);
    (DateTime Start, DateTime End) GetPreviousPeriodRange(PeriodType periodType, DateTime currentPeriodStart);
    List<Transaction> FilterTransactions(List<Transaction> allTransactions, DateTime start, DateTime end);
    DateTime GetPeriodStartForDate(DateTime date, PeriodType periodType);
}

public class InsightData
{
    public decimal FilteredIncome { get; set; }
    public decimal FilteredExpense { get; set; }
    public decimal PreviousExpense { get; set; }
    public decimal PreviousIncome { get; set; }
    public TrendDirection ExpenseTrend { get; set; }
    public TrendDirection IncomeTrend { get; set; }
    public List<Transaction> HighlightTransactions { get; set; } = new();
    public List<CategorySummary> TopCategories { get; set; } = new();
    public List<MemberSummary> MemberExpenseSummaries { get; set; } = new();
    public List<MemberSummary> MemberIncomeSummaries { get; set; } = new();
    public List<ChartDataPoint> ChartData { get; set; } = new();
    public List<ChartSeries> LineChartSeries { get; set; } = new();
    public List<string> XAxisLabels { get; set; } = new();
}

public class CategorySummary
{
    public string Name { get; set; } = "";
    public decimal Amount { get; set; }
    public int TransactionCount { get; set; }
    public double Percentage { get; set; }
}

public enum PeriodType
{
    Weekly,
    Monthly,
    Yearly
}

public enum TrendDirection
{
    Up,
    Down,
    Unchanged
}

public class ChartDataPoint
{
    public string Label { get; set; } = "";
    public decimal Amount { get; set; }
    public double XPosition { get; set; }
    public double YPosition { get; set; }
}

public class MemberSummary
{
    public string Name { get; set; } = "";
    public decimal TotalAmount { get; set; }
    public int TransactionCount { get; set; }
    public double Percentage { get; set; }
    public List<MemberCategorySummary> TopCategories { get; set; } = new();
}

public class MemberCategorySummary
{
    public string CategoryName { get; set; } = "";
    public decimal Amount { get; set; }
    public int TransactionCount { get; set; }
}

public class ChartSeries
{
    public string Name { get; set; } = "";
    public double[] Data { get; set; } = Array.Empty<double>();
}

