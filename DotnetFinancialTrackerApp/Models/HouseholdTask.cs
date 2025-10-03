using System.ComponentModel.DataAnnotations;

namespace DotnetFinancialTrackerApp.Models;

public class HouseholdTask : WellbeingItem
{
    [Required]
    public TaskCategory Category { get; set; }

    [Required]
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;

    public DateTime DueDate { get; set; } = DateTime.Today;

    public int EstimatedMinutes { get; set; }

    public int? ActualMinutes { get; set; }

    public string? Notes { get; set; }

    public override WellbeingItemType GetItemType() => WellbeingItemType.HouseholdTask;

    public override void MarkCompleted()
    {
        base.MarkCompleted();
        ActualMinutes ??= EstimatedMinutes;
    }

    public bool IsOverdue()
    {
        return !IsCompleted && DateTime.Now.Date > DueDate.Date;
    }

    public string GetPriorityColor()
    {
        return Priority switch
        {
            TaskPriority.Low => "success",
            TaskPriority.Medium => "warning",
            TaskPriority.High => "error",
            _ => "default"
        };
    }

    public string GetCategoryIcon()
    {
        return Category switch
        {
            TaskCategory.Cleaning => "cleaning_services",
            TaskCategory.Cooking => "restaurant",
            TaskCategory.Shopping => "shopping_cart",
            TaskCategory.Maintenance => "build",
            TaskCategory.Laundry => "local_laundry_service",
            TaskCategory.Gardening => "yard",
            TaskCategory.Other => "task_alt",
            _ => "task"
        };
    }
}

public enum TaskCategory
{
    Cleaning,
    Cooking,
    Shopping,
    Maintenance,
    Laundry,
    Gardening,
    Other
}

public enum TaskPriority
{
    Low,
    Medium,
    High
}