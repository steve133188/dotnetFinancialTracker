using DotnetFinancialTrackerApp.Data;
using DotnetFinancialTrackerApp.Models;
using DotnetFinancialTrackerApp.Services;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace DotnetFinancialTrackerApp.Tests;

[TestFixture]
public class SavingsGoalServiceTests
{
    private AppDbContext _context = default!;
    private SavingsGoalService _service = default!;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        SeedFamilyData(_context);
        _service = new SavingsGoalService(_context);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Test]
    public async Task CreateAsync_AndRetrieveActiveGoals()
    {
        // Arrange
        var goal = new SavingsGoal
        {
            Title = "Emergency fund",
            TargetAmount = 5000m,
            CurrentAmount = 1200m,
            Category = "Emergency",
            FamilyId = "family-default",
            CreatedByMemberId = "member-alex",
            Color = "#000000"
        };

        // Act
        await _service.CreateAsync(goal);
        var activeGoals = await _service.GetActiveGoalsAsync("family-default");

        // Assert
        Assert.That(activeGoals.Count(), Is.EqualTo(1));
        Assert.That(activeGoals.First().ProgressPercentage, Is.GreaterThan(0));
    }

    [Test]
    public async Task AddContributionAsync_UpdatesCurrentAmountAndPersistsContribution()
    {
        // Arrange
        var goal = await _service.CreateAsync(new SavingsGoal
        {
            Title = "Holiday fund",
            TargetAmount = 3000m,
            Category = "Travel",
            FamilyId = "family-default",
            CreatedByMemberId = "member-alex"
        });

        // Act - demonstrates generics & LINQ in service
        var result = await _service.AddContributionAsync(goal.Id, 250m, "member-alex", "Weekly savings");
        var contributions = await _service.GetContributionsAsync(goal.Id);
        var updatedGoal = await _service.GetByIdAsync(goal.Id);

        // Assert
        Assert.That(result, Is.True);
        Assert.That(contributions.Count(), Is.EqualTo(1));
        Assert.That(updatedGoal!.CurrentAmount, Is.EqualTo(250m));
        Assert.That(contributions.First().Description, Does.Contain("Weekly savings"));
    }

    [Test]
    public async Task GetSavingsGoalSummaryAsync_ComputesAggregateMetrics()
    {
        // Arrange
        await _service.CreateAsync(new SavingsGoal
        {
            Title = "Laptop upgrade",
            TargetAmount = 2000m,
            CurrentAmount = 1000m,
            Category = "Education",
            FamilyId = "family-default",
            CreatedByMemberId = "member-alex"
        });

        await _service.CreateAsync(new SavingsGoal
        {
            Title = "Rainy day",
            TargetAmount = 1000m,
            CurrentAmount = 1000m,
            Category = "Emergency",
            FamilyId = "family-default",
            CreatedByMemberId = "member-jamie"
        });

        // Act
        var summary = await _service.GetSavingsGoalSummaryAsync("family-default");

        // Assert
        Assert.That(summary.ActiveGoalsCount, Is.EqualTo(1));
        Assert.That(summary.CompletedGoalsCount, Is.EqualTo(1));
        Assert.That(summary.TotalTargetAmount, Is.EqualTo(2000m));
    }

    private static void SeedFamilyData(AppDbContext context)
    {
        if (!context.FamilyAccounts.Any())
        {
            context.FamilyAccounts.Add(new FamilyAccount
            {
                FamilyId = "family-default",
                FamilyName = "Taylor Household",
                TotalBalance = 0m,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }

        if (!context.FamilyMembers.Any())
        {
            context.FamilyMembers.AddRange(new[]
            {
                new FamilyMember { Id = "member-alex", Name = "Alex" },
                new FamilyMember { Id = "member-jamie", Name = "Jamie" }
            });
        }

        context.SaveChanges();
    }
}
