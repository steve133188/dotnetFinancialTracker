using DotnetFinancialTrackerApp.Models;

namespace DotnetFinancialTrackerApp.Services;

public interface INotificationService
{
    string CreateNotification(string message);
    string CreateNotification(string message, NotificationPriority priority);
    string CreateNotification(WellbeingItem item);
    string CreateNotification(MedicationReminder medication, bool isOverdue);
    List<string> CreateBulkNotifications<T>(IEnumerable<T> items) where T : WellbeingItem;
}

public class NotificationService : INotificationService
{
    // Method overloading demonstration (polymorphism through overloading)
    public string CreateNotification(string message)
    {
        return CreateNotification(message, NotificationPriority.Normal);
    }

    public string CreateNotification(string message, NotificationPriority priority)
    {
        var prefix = priority switch
        {
            NotificationPriority.Low => "ℹ️",
            NotificationPriority.Normal => "📝",
            NotificationPriority.High => "⚠️",
            NotificationPriority.Urgent => "🚨",
            _ => "📝"
        };

        return $"{prefix} {message}";
    }

    public string CreateNotification(WellbeingItem item)
    {
        // Polymorphic behavior based on item type
        return item switch
        {
            HydrationEntry hydration => CreateHydrationNotification(hydration),
            MedicationReminder medication => CreateMedicationNotification(medication),
            HouseholdTask task => CreateTaskNotification(task),
            _ => CreateNotification($"Wellbeing item: {item.Title}")
        };
    }

    public string CreateNotification(MedicationReminder medication, bool isOverdue)
    {
        var priority = isOverdue ? NotificationPriority.Urgent : NotificationPriority.High;
        var status = isOverdue ? "OVERDUE" : "DUE";
        var message = $"Medication {status}: {medication.MedicationName} ({medication.Dosage})";

        return CreateNotification(message, priority);
    }

    // Generic method with constraint (demonstrates generics)
    public List<string> CreateBulkNotifications<T>(IEnumerable<T> items) where T : WellbeingItem
    {
        return items
            .Where(item => !item.IsCompleted)  // LINQ with Lambda
            .Select(item => CreateNotification(item))  // LINQ with Lambda
            .OrderBy(notification => notification)  // LINQ with Lambda
            .ToList();
    }

    private string CreateHydrationNotification(HydrationEntry hydration)
    {
        var completion = hydration.GetCompletionPercentage();
        if (completion >= 100)
        {
            return CreateNotification($"Great job! Daily hydration goal achieved! 💧", NotificationPriority.Low);
        }
        else if (completion >= 75)
        {
            return CreateNotification($"Almost there! {hydration.GlassesConsumed}/{hydration.GlassesTarget} glasses", NotificationPriority.Normal);
        }
        else
        {
            return CreateNotification($"Remember to drink water! {hydration.GlassesConsumed}/{hydration.GlassesTarget} glasses", NotificationPriority.High);
        }
    }

    private string CreateMedicationNotification(MedicationReminder medication)
    {
        if (medication.IsCompleted)
        {
            return CreateNotification($"Medication taken: {medication.MedicationName}", NotificationPriority.Low);
        }
        else if (medication.IsOverdue())
        {
            return CreateNotification(medication, true);
        }
        else
        {
            return CreateNotification($"Upcoming: {medication.MedicationName} at {medication.ScheduledTime:HH:mm}", NotificationPriority.Normal);
        }
    }

    private string CreateTaskNotification(HouseholdTask task)
    {
        var priority = task.IsOverdue() ? NotificationPriority.Urgent :
                      task.Priority == TaskPriority.High ? NotificationPriority.High :
                      NotificationPriority.Normal;

        var status = task.IsOverdue() ? "OVERDUE" : "PENDING";
        return CreateNotification($"Task {status}: {task.Title} (Due: {task.DueDate:MMM dd})", priority);
    }
}

public enum NotificationPriority
{
    Low,
    Normal,
    High,
    Urgent
}