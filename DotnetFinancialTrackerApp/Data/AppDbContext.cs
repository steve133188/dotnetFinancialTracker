using DotnetFinancialTrackerApp.Models;
using Microsoft.EntityFrameworkCore;

namespace DotnetFinancialTrackerApp.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<TransactionTemplate> TransactionTemplates => Set<TransactionTemplate>();
    public DbSet<Budget> Budgets => Set<Budget>();
    public DbSet<Achievement> Achievements => Set<Achievement>();
    public DbSet<GamificationState> GamificationStates => Set<GamificationState>();
    public DbSet<UserProfile> Users => Set<UserProfile>();
    public DbSet<HydrationEntry> HydrationEntries => Set<HydrationEntry>();
    public DbSet<MedicationReminder> MedicationReminders => Set<MedicationReminder>();
    public DbSet<HouseholdTask> HouseholdTasks => Set<HouseholdTask>();

    // Family Banking DbSets
    public DbSet<FamilyAccount> FamilyAccounts => Set<FamilyAccount>();
    public DbSet<FamilyMember> FamilyMembers => Set<FamilyMember>();
    public DbSet<FamilyGoal> FamilyGoals => Set<FamilyGoal>();
    public DbSet<FamilyInsights> FamilyInsights => Set<FamilyInsights>();
    public DbSet<SpendingLimit> SpendingLimits => Set<SpendingLimit>();
    public DbSet<FamilyMemberGoal> FamilyMemberGoals => Set<FamilyMemberGoal>();
    public DbSet<GoalContribution> GoalContributions => Set<GoalContribution>();
}
