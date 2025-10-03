using System.ComponentModel.DataAnnotations;

namespace DotnetFinancialTrackerApp.Models;

public class MedicationReminder : WellbeingItem
{
    [Required]
    [StringLength(100)]
    public string MedicationName { get; set; } = string.Empty;

    [StringLength(50)]
    public string Dosage { get; set; } = string.Empty;

    [Required]
    public TimeOnly ScheduledTime { get; set; }

    public DateTime Date { get; set; } = DateTime.Today;

    public MedicationFrequency Frequency { get; set; } = MedicationFrequency.Daily;

    public DateTime? TakenTime { get; set; }

    public override WellbeingItemType GetItemType() => WellbeingItemType.Medication;

    public override void MarkCompleted()
    {
        base.MarkCompleted();
        TakenTime = DateTime.Now;
    }

    public override void MarkIncomplete()
    {
        base.MarkIncomplete();
        TakenTime = null;
    }

    public bool IsOverdue()
    {
        if (IsCompleted) return false;

        var scheduledDateTime = Date.Add(ScheduledTime.ToTimeSpan());
        return DateTime.Now > scheduledDateTime.AddMinutes(30);
    }

    public string GetStatusText()
    {
        if (IsCompleted)
            return $"Taken at {TakenTime:HH:mm}";

        if (IsOverdue())
            return "Overdue";

        return $"Due at {ScheduledTime:HH:mm}";
    }
}

public enum MedicationFrequency
{
    Daily,
    TwiceDaily,
    ThreeTimesDaily,
    Weekly,
    AsNeeded
}