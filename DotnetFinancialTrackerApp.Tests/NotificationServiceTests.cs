using NUnit.Framework;
using DotnetFinancialTrackerApp.Services;
using DotnetFinancialTrackerApp.Models;

namespace DotnetFinancialTrackerApp.Tests;

[TestFixture]
public class NotificationServiceTests
{
    private NotificationService _notificationService;

    [SetUp]
    public void Setup()
    {
        _notificationService = new NotificationService();
    }

    [Test]
    public void CreateNotification_SimpleMessage_ShouldReturnFormattedMessage()
    {
        // Arrange
        var message = "Test notification";

        // Act
        var result = _notificationService.CreateNotification(message);

        // Assert
        Assert.That(result, Is.EqualTo("📝 Test notification"));
    }

    [Test]
    public void CreateNotification_WithPriority_ShouldReturnCorrectPrefix()
    {
        // Arrange
        var message = "Urgent task";

        // Act - Method overloading demonstration
        var urgent = _notificationService.CreateNotification(message, NotificationPriority.Urgent);
        var normal = _notificationService.CreateNotification(message, NotificationPriority.Normal);
        var low = _notificationService.CreateNotification(message, NotificationPriority.Low);

        // Assert
        Assert.That(urgent, Is.EqualTo("🚨 Urgent task"));
        Assert.That(normal, Is.EqualTo("📝 Urgent task"));
        Assert.That(low, Is.EqualTo("ℹ️ Urgent task"));
    }

    [Test]
    public void CreateBulkNotifications_WithGenericType_ShouldFilterAndFormat()
    {
        // Arrange - Demonstrating Generics and LINQ with Lambda expressions
        var tasks = new List<HouseholdTask>
        {
            new() { Title = "Task 1", IsCompleted = false, Category = TaskCategory.Cleaning, Priority = TaskPriority.High },
            new() { Title = "Task 2", IsCompleted = true, Category = TaskCategory.Cooking, Priority = TaskPriority.Medium },
            new() { Title = "Task 3", IsCompleted = false, Category = TaskCategory.Shopping, Priority = TaskPriority.Low }
        };

        // Act - Generic method call with LINQ filtering
        var notifications = _notificationService.CreateBulkNotifications(tasks);

        // Assert
        Assert.That(notifications.Count, Is.EqualTo(2)); // Only incomplete tasks
        Assert.That(notifications.All(n => n.Contains("PENDING")), Is.True);
    }

    [Test]
    public void CreateNotification_PolymorphicBehavior_ShouldHandleDifferentTypes()
    {
        // Arrange - Polymorphism demonstration
        var hydration = new HydrationEntry
        {
            Title = "Water",
            GlassesTarget = 8,
            GlassesConsumed = 6
        };

        var medication = new MedicationReminder
        {
            Title = "Pills",
            MedicationName = "Vitamin D",
            IsCompleted = false,
            Date = DateTime.Today,
            ScheduledTime = TimeOnly.FromDateTime(DateTime.Now.AddHours(-2))
        };

        // Act - Polymorphic method calls
        var hydrationNotification = _notificationService.CreateNotification(hydration);
        var medicationNotification = _notificationService.CreateNotification(medication);

        // Assert
        Assert.That(hydrationNotification, Does.Contain("Almost there"));
        Assert.That(medicationNotification, Does.Contain("OVERDUE"));
    }

    [Test]
    public void ValidationService_WithInvalidData_ShouldReturnErrors()
    {
        // Arrange
        var validationService = new ValidationService();
        var invalidTask = new HouseholdTask
        {
            Title = "", // Required field is empty
            AssignedTo = "", // Required field is empty
            EstimatedMinutes = -5 // Invalid range
        };

        // Act
        var result = validationService.Validate(invalidTask);

        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.ValidationErrors.Count, Is.GreaterThan(0));
        Assert.That(result.ValidationErrors.Any(e => e.Contains("Title")), Is.True);
    }
}

