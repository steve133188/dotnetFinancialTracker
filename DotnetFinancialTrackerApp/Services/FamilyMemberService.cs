using DotnetFinancialTrackerApp.Data;
using DotnetFinancialTrackerApp.Models;
using Microsoft.EntityFrameworkCore;

namespace DotnetFinancialTrackerApp.Services;

public class FamilyMemberService : IFamilyMemberService
{
    private readonly AppDbContext _db;
    private readonly ITransactionsService _transactionsService;

    public FamilyMemberService(AppDbContext db, ITransactionsService transactionsService)
    {
        _db = db;
        _transactionsService = transactionsService;
    }

    public async Task<List<FamilyMember>> GetAsync(string? familyId = null)
    {
        var query = _db.FamilyMembers.AsQueryable();

        if (!string.IsNullOrEmpty(familyId))
        {
            query = query.Where(fm => fm.FamilyId == familyId);
        }

        return await query
            .Where(fm => fm.IsActive)
            .OrderBy(fm => fm.Name)
            .ToListAsync();
    }

    public async Task<FamilyMember?> GetByIdAsync(string memberId)
    {
        return await _db.FamilyMembers
            .FirstOrDefaultAsync(fm => fm.MemberId == memberId && fm.IsActive);
    }

    public async Task<FamilyMember> AddAsync(FamilyMember familyMember)
    {
        familyMember.CreatedAt = DateTime.UtcNow;
        familyMember.UpdatedAt = DateTime.UtcNow;

        _db.FamilyMembers.Add(familyMember);
        await _db.SaveChangesAsync();
        return familyMember;
    }

    public async Task UpdateAsync(FamilyMember familyMember)
    {
        familyMember.UpdatedAt = DateTime.UtcNow;
        _db.FamilyMembers.Update(familyMember);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(string memberId)
    {
        var member = await GetByIdAsync(memberId);
        if (member != null)
        {
            member.IsActive = false;
            member.UpdatedAt = DateTime.UtcNow;
            await UpdateAsync(member);
        }
    }

    public async Task<FamilyMember?> GetByNameAsync(string name, string familyId)
    {
        return await _db.FamilyMembers
            .FirstOrDefaultAsync(fm => fm.Name.ToLower() == name.ToLower()
                                    && fm.FamilyId == familyId
                                    && fm.IsActive);
    }

    public async Task<FamilyMember> GetOrCreateMemberAsync(string name, string familyId, string role = "Parent")
    {
        var existingMember = await GetByNameAsync(name, familyId);
        if (existingMember != null)
        {
            return existingMember;
        }

        var newMember = new FamilyMember
        {
            MemberId = Guid.NewGuid().ToString(),
            Name = name,
            FamilyId = familyId,
            Role = role,
            SpendingLimit = role.ToLower() switch
            {
                "parent" => 2000m,
                "teen" => 500m,
                "child" => 100m,
                _ => 1000m
            },
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        return await AddAsync(newMember);
    }

    public async Task<List<FamilyMember>> GetFamilyMembersWithSpendingAsync(string familyId, DateTime? month = null)
    {
        var members = await GetAsync(familyId);

        // Calculate current month spending for each member
        var currentMonth = month ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        var nextMonth = currentMonth.AddMonths(1);

        var transactions = await _transactionsService.GetAsync(from: currentMonth, to: nextMonth);

        foreach (var member in members)
        {
            var memberSpending = transactions
                .Where(t => !t.IsIncome && (t.User == member.Name || t.FamilyMemberId == member.MemberId))
                .Sum(t => t.Amount);

            member.SpentThisMonth = memberSpending;
            member.TransactionsThisMonth = transactions
                .Count(t => t.User == member.Name || t.FamilyMemberId == member.MemberId);
        }

        return members;
    }

    public async Task<string?> GetDefaultFamilyIdAsync()
    {
        var familyAccount = await _db.FamilyAccounts.FirstOrDefaultAsync();
        return familyAccount?.FamilyId;
    }
}