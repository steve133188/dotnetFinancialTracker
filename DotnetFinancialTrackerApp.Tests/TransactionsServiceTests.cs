using DotnetFinancialTrackerApp.Data;
using DotnetFinancialTrackerApp.Models;
using DotnetFinancialTrackerApp.Services;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace DotnetFinancialTrackerApp.Tests;

[TestFixture]
public class TransactionsServiceTests
{
    private AppDbContext _context = default!;
    private TransactionsService _service = default!;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        SeedReferenceData(_context);
        _service = new TransactionsService(_context);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Test]
    public async Task GetAsync_NoFilters_UsesOverloadAndReturnsAll()
    {
        // Arrange
        await SeedTransactionsAsync();

        // Act - overload demonstrates polymorphism via method overloading
        var results = await _service.GetAsync();

        // Assert
        Assert.That(results.Count, Is.EqualTo(3));
        // Spec: confirm ordering (latest first) for functional requirement
        Assert.That(results.First().Description, Is.EqualTo("Grocery run"));
    }

    [Test]
    public async Task GetAsync_FilterByFamilyMemberAndDateRange()
    {
        // Arrange
        await SeedTransactionsAsync();
        var start = DateTime.Today.AddDays(-3);
        var end = DateTime.Today.AddDays(1);

        // Act - LINQ with lambda filters satisfy anonymous method requirement
        var results = await _service.GetAsync(user: "member-jamie", from: start, to: end);

        // Assert
        Assert.That(results.Count, Is.EqualTo(1));
        Assert.That(results.Single().Amount, Is.EqualTo(150m));
    }

    [Test]
    public async Task AddAsync_SetsTimestampsAndPersists()
    {
        // Arrange
        var transaction = new Transaction
        {
            Amount = 250m,
            Description = "Utility bill",
            Date = DateTime.Today,
            FamilyMemberId = "member-alex",
            CategoryId = 1,
            Type = TransactionType.Expense
        };

        // Act
        var saved = await _service.AddAsync(transaction);
        var stored = await _context.Transactions.FindAsync(saved.Id);

        // Assert
        Assert.That(saved.CreatedAt, Is.Not.EqualTo(default(DateTime)));
        Assert.That(stored, Is.Not.Null);
        Assert.That(stored!.Description, Is.EqualTo("Utility bill"));
    }

    private static void SeedReferenceData(AppDbContext context)
    {
        if (!context.FamilyMembers.Any())
        {
            context.FamilyMembers.AddRange(new[]
            {
                new FamilyMember { Id = "member-alex", Name = "Alex" },
                new FamilyMember { Id = "member-jamie", Name = "Jamie" },
                new FamilyMember { Id = "member-sam", Name = "Sam" }
            });
        }

        if (!context.TransactionCategories.Any())
        {
            context.TransactionCategories.AddRange(new[]
            {
                new TransactionCategory { Id = 1, Name = "Groceries" },
                new TransactionCategory { Id = 2, Name = "Transport" }
            });
        }

        context.SaveChanges();
    }

    private async Task SeedTransactionsAsync()
    {
        var transactions = new[]
        {
            new Transaction
            {
                Amount = 150m,
                Description = "Family dinner",
                Date = DateTime.Today.AddDays(-2),
                FamilyMemberId = "member-jamie",
                CategoryId = 1,
                Type = TransactionType.Expense
            },
            new Transaction
            {
                Amount = 80m,
                Description = "Fuel top-up",
                Date = DateTime.Today.AddDays(-6),
                FamilyMemberId = "member-alex",
                CategoryId = 2,
                Type = TransactionType.Expense
            },
            new Transaction
            {
                Amount = 120m,
                Description = "Grocery run",
                Date = DateTime.Today,
                FamilyMemberId = "member-sam",
                CategoryId = 1,
                Type = TransactionType.Expense
            }
        };

        _context.Transactions.AddRange(transactions);
        await _context.SaveChangesAsync();
    }
}
