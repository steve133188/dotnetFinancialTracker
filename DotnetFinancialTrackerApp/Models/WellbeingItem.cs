using System.ComponentModel.DataAnnotations;

namespace DotnetFinancialTrackerApp.Models;

public abstract class WellbeingItem
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.Now;

    [Required]
    [StringLength(50)]
    public string AssignedTo { get; set; } = string.Empty;

    public bool IsCompleted { get; set; }

    public DateTime? CompletedDate { get; set; }

    public abstract WellbeingItemType GetItemType();

    public virtual void MarkCompleted()
    {
        IsCompleted = true;
        CompletedDate = DateTime.Now;
    }

    public virtual void MarkIncomplete()
    {
        IsCompleted = false;
        CompletedDate = null;
    }
}

public enum WellbeingItemType
{
    Hydration,
    Medication,
    HouseholdTask
}