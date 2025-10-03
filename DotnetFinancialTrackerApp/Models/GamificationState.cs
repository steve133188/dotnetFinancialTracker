namespace DotnetFinancialTrackerApp.Models;

public class GamificationState
{
    public int Id { get; set; }
    public int Points { get; set; }
    public int StreakCount { get; set; }
    public DateTime? LastActivityDate { get; set; }
}

