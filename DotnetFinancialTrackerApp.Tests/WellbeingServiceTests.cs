using NUnit.Framework;
using DotnetFinancialTrackerApp.Models;
using DotnetFinancialTrackerApp.Services;
using DotnetFinancialTrackerApp.Data;
using Microsoft.EntityFrameworkCore;

namespace DotnetFinancialTrackerApp.Tests;

[TestFixture]
public class WellbeingServiceTests
{
    private AppDbContext _context;
    private HydrationService _hydrationService;
    private MedicationService _medicationService;
    private HouseholdTaskService _taskService;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "WellbeingTestDb")
            .Options;

        _context = new AppDbContext(options);
        _hydrationService = new HydrationService(_context);
        _medicationService = new MedicationService(_context);
        _taskService = new HouseholdTaskService(_context);
    }

    [TearDown]
    public void Teardown()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Test]
    public async Task HydrationService_AddAndRetrieveEntries_ShouldWork()
    {
        var entry = new HydrationEntry
        {
            AssignedTo = "TestUser",
            Title = "Morning Water",
            GlassesTarget = 8,
            GlassesConsumed = 3,
            Date = DateTime.Today
        };

        await _hydrationService.AddEntryAsync(entry);
        var entries = await _hydrationService.GetEntriesByUserAsync("TestUser");

        Assert.That(entries.Count, Is.EqualTo(1));
        Assert.That(entries[0].Title, Is.EqualTo("Morning Water"));
    }

    [Test]
    public async Task HydrationEntry_IsOnTrack_ShouldBeCalculated()
    {
        var entry = new HydrationEntry
        {
            AssignedTo = "TestUser",
            Title = "Water",
            GlassesTarget = 8,
            GlassesConsumed = 6,
            Date = DateTime.Today
        };

        await _hydrationService.AddEntryAsync(entry);
        var entries = await _hydrationService.GetEntriesByUserAsync("TestUser");

        Assert.That(entries[0].IsOnTrack(), Is.True);
    }

    [Test]
    public async Task MedicationService_AddAndCheckOverdue_ShouldWork()
    {
        var medication = new MedicationReminder
        {
            AssignedTo = "TestUser",
            Title = "Pills",
            MedicationName = "Vitamin D",
            Date = DateTime.Today,
            ScheduledTime = TimeOnly.FromDateTime(DateTime.Now.AddHours(-1)),
            IsCompleted = false
        };

        await _medicationService.CreateAsync(medication);
        var meds = await _medicationService.GetByUserAsync("TestUser");

        Assert.That(meds.Count, Is.EqualTo(1));
        Assert.That(meds[0].IsOverdue(), Is.True);
    }

    [Test]
    public async Task HouseholdTaskService_CRUD_ShouldWork()
    {
        var task = new HouseholdTask
        {
            AssignedTo = "TestUser",
            Title = "Clean kitchen",
            Category = TaskCategory.Cleaning,
            Priority = TaskPriority.Medium
        };

        var created = await _taskService.CreateAsync(task);
        Assert.That(created.IsSuccess, Is.True);

        var all = await _taskService.GetAllAsync("TestUser");
        Assert.That(all.Count, Is.EqualTo(1));

        var first = all.First();
        first.IsCompleted = true;
        var updated = await _taskService.UpdateAsync(first);
        Assert.That(updated.IsSuccess, Is.True);
    }

    [Test]
    public void MedicationReminder_OverdueLogic_ShouldBeCorrect()
    {
        var medication = new MedicationReminder
        {
            AssignedTo = "TestUser",
            Title = "Morning Pill",
            MedicationName = "Vitamin D",
            Date = DateTime.Today,
            ScheduledTime = TimeOnly.FromDateTime(DateTime.Now.AddHours(-2)),
            IsCompleted = false
        };

        Assert.That(medication.IsOverdue(), Is.True);
    }

    [Test]
    public async Task HouseholdTask_Priority_ShouldReturnCorrectColor()
    {
        var highPriorityTask = new HouseholdTask
        {
            Title = "Fix leak",
            Priority = TaskPriority.High,
            Category = TaskCategory.Maintenance
        };

        var lowPriorityTask = new HouseholdTask
        {
            Title = "Water plants",
            Priority = TaskPriority.Low,
            Category = TaskCategory.Gardening
        };

        Assert.That(highPriorityTask.GetPriorityColor(), Is.EqualTo("error"));
        Assert.That(lowPriorityTask.GetPriorityColor(), Is.EqualTo("success"));
        Assert.That(highPriorityTask.GetItemType(), Is.EqualTo(WellbeingItemType.HouseholdTask));
    }

    [Test]
    public async Task TaskService_GetTaskSummary_ShouldGroupCorrectly()
    {
        var tasks = new List<HouseholdTask>
        {
            new() { AssignedTo = "TestUser", Title = "Task 1", Category = TaskCategory.Cleaning, IsCompleted = false },
            new() { AssignedTo = "TestUser", Title = "Task 2", Category = TaskCategory.Cleaning, IsCompleted = false },
            new() { AssignedTo = "TestUser", Title = "Task 3", Category = TaskCategory.Cooking, IsCompleted = false },
            new() { AssignedTo = "OtherUser", Title = "Task 4", Category = TaskCategory.Cleaning, IsCompleted = false }
        };

        foreach (var task in tasks)
        {
            await _taskService.CreateAsync(task);
        }

        var summary = await _taskService.GetTaskSummaryAsync("TestUser");

        Assert.That(summary.ContainsKey(TaskCategory.Cleaning), Is.True);
        Assert.That(summary[TaskCategory.Cleaning], Is.EqualTo(2));
        Assert.That(summary[TaskCategory.Cooking], Is.EqualTo(1));
        Assert.That(summary.ContainsKey(TaskCategory.Shopping), Is.False);
    }

    [Test]
    public void Polymorphism_WellbeingItems_ShouldBehaveDifferently()
    {
        List<WellbeingItem> items = new()
        {
            new HydrationEntry { Title = "Water", AssignedTo = "User1" },
            new MedicationReminder { Title = "Pills", AssignedTo = "User2" },
            new HouseholdTask { Title = "Cleaning", AssignedTo = "User3" }
        };

        var itemTypes = items.Select(item => item.GetItemType()).ToList();

        Assert.That(itemTypes.Count, Is.EqualTo(3));
        Assert.That(itemTypes.Contains(WellbeingItemType.Hydration), Is.True);
        Assert.That(itemTypes.Contains(WellbeingItemType.Medication), Is.True);
        Assert.That(itemTypes.Contains(WellbeingItemType.HouseholdTask), Is.True);

        foreach (var item in items)
        {
            item.MarkCompleted();
            Assert.That(item.IsCompleted, Is.True);
        }
    }

    [Test]
    public void GenericCollection_FilterByUser_ShouldWorkWithLinq()
    {
        var allItems = new List<WellbeingItem>
        {
            new HydrationEntry { AssignedTo = "Alice", Title = "Water 1" },
            new HydrationEntry { AssignedTo = "Bob", Title = "Water 2" },
            new MedicationReminder { AssignedTo = "Alice", Title = "Pill 1" },
            new HouseholdTask { AssignedTo = "Charlie", Title = "Task 1" }
        };

        var aliceItems = allItems
            .Where(item => item.AssignedTo == "Alice")
            .OrderBy(item => item.Title)
            .ToList();

        var completedItems = allItems
            .Where(item => item.IsCompleted)
            .Count();

        var userGroups = allItems
            .GroupBy(item => item.AssignedTo)
            .ToDictionary(g => g.Key, g => g.Count());

        Assert.That(aliceItems.Count, Is.EqualTo(2));
        Assert.That(aliceItems.All(item => item.AssignedTo == "Alice"), Is.True);
        Assert.That(userGroups["Alice"], Is.EqualTo(2));
        Assert.That(userGroups["Bob"], Is.EqualTo(1));
        Assert.That(userGroups["Charlie"], Is.EqualTo(1));
    }
}

