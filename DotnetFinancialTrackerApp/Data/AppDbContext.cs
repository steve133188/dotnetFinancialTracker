using DotnetFinancialTrackerApp.Models;
using Microsoft.EntityFrameworkCore;

namespace DotnetFinancialTrackerApp.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // Core Financial Entities
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<TransactionCategory> TransactionCategories => Set<TransactionCategory>();
    public DbSet<TransactionTemplate> TransactionTemplates => Set<TransactionTemplate>();
    public DbSet<Budget> Budgets => Set<Budget>();

    // Savings Entities
    public DbSet<SavingsGoal> SavingsGoals => Set<SavingsGoal>();
    public DbSet<SavingsGoalContribution> SavingsGoalContributions => Set<SavingsGoalContribution>();

    // User Management
    public DbSet<UserProfile> Users => Set<UserProfile>();

    // Family Banking Entities (essential for MVP)
    public DbSet<FamilyAccount> FamilyAccounts => Set<FamilyAccount>();
    public DbSet<FamilyMember> FamilyMembers => Set<FamilyMember>();
    public DbSet<FamilyGoal> FamilyGoals => Set<FamilyGoal>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure decimal precision for financial amounts
        modelBuilder.Entity<Transaction>()
            .Property(t => t.Amount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Budget>()
            .Property(b => b.Limit)
            .HasPrecision(18, 2);

        modelBuilder.Entity<SavingsGoal>()
            .Property(s => s.TargetAmount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<SavingsGoal>()
            .Property(s => s.CurrentAmount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<SavingsGoalContribution>()
            .Property(c => c.Amount)
            .HasPrecision(18, 2);

        // Configure relationships

        // Transaction relationships
        modelBuilder.Entity<Transaction>()
            .HasOne(t => t.FamilyMember)
            .WithMany()
            .HasForeignKey(t => t.FamilyMemberId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Transaction>()
            .HasOne(t => t.Category)
            .WithMany(c => c.Transactions)
            .HasForeignKey(t => t.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Transaction>()
            .HasOne(t => t.RelatedSavingsGoal)
            .WithMany()
            .HasForeignKey(t => t.RelatedSavingsGoalId)
            .OnDelete(DeleteBehavior.SetNull);

        // Budget relationships
        modelBuilder.Entity<Budget>()
            .HasOne(b => b.Family)
            .WithMany()
            .HasForeignKey(b => b.FamilyId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Budget>()
            .HasOne(b => b.Category)
            .WithMany(c => c.Budgets)
            .HasForeignKey(b => b.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Budget>()
            .HasOne(b => b.CreatedBy)
            .WithMany()
            .HasForeignKey(b => b.CreatedByMemberId)
            .OnDelete(DeleteBehavior.Restrict);

        // SavingsGoal relationships
        modelBuilder.Entity<SavingsGoal>()
            .HasOne(s => s.Family)
            .WithMany()
            .HasForeignKey(s => s.FamilyId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<SavingsGoal>()
            .HasOne(s => s.CreatedBy)
            .WithMany()
            .HasForeignKey(s => s.CreatedByMemberId)
            .OnDelete(DeleteBehavior.Restrict);

        // SavingsGoalContribution relationships
        modelBuilder.Entity<SavingsGoalContribution>()
            .HasOne(c => c.SavingsGoal)
            .WithMany(s => s.Contributions)
            .HasForeignKey(c => c.SavingsGoalId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<SavingsGoalContribution>()
            .HasOne(c => c.Contributor)
            .WithMany()
            .HasForeignKey(c => c.ContributorMemberId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes for performance
        modelBuilder.Entity<Transaction>()
            .HasIndex(t => new { t.FamilyMemberId, t.Date })
            .HasDatabaseName("IX_Transaction_Member_Date");

        modelBuilder.Entity<Transaction>()
            .HasIndex(t => new { t.CategoryId, t.Date })
            .HasDatabaseName("IX_Transaction_Category_Date");

        modelBuilder.Entity<Budget>()
            .HasIndex(b => new { b.FamilyId, b.Month, b.IsOverallBudget })
            .HasDatabaseName("IX_Budget_Family_Month_Overall");

        modelBuilder.Entity<SavingsGoal>()
            .HasIndex(s => new { s.FamilyId, s.IsActive })
            .HasDatabaseName("IX_SavingsGoal_Family_Active");

        // Constraints
        modelBuilder.Entity<Budget>()
            .HasCheckConstraint("CK_Budget_Limit_Positive", "[Limit] > 0");

        modelBuilder.Entity<SavingsGoal>()
            .HasCheckConstraint("CK_SavingsGoal_TargetAmount_Positive", "[TargetAmount] > 0");

        modelBuilder.Entity<SavingsGoalContribution>()
            .HasCheckConstraint("CK_SavingsGoalContribution_Amount_Positive", "[Amount] > 0");

        // Seed default transaction categories
        var defaultCategories = TransactionCategory.GetDefaultCategories();
        for (int i = 0; i < defaultCategories.Count; i++)
        {
            defaultCategories[i].Id = i + 1;
        }

        modelBuilder.Entity<TransactionCategory>().HasData(defaultCategories);
    }
}
