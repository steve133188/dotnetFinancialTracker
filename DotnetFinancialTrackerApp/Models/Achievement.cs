namespace DotnetFinancialTrackerApp.Models;

public class Achievement
{
    public int Id { get; set; }
    public string Key { get; set; } = string.Empty; // unique key per rule/month
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Points { get; set; }
    public DateTime AchievedAt { get; set; } = DateTime.UtcNow;
}

