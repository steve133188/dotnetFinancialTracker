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
        // Simplified - no family groups, all members are active
        return await _db.FamilyMembers
            .OrderBy(fm => fm.Name)
            .ToListAsync();
    }

    public async Task<FamilyMember?> GetByIdAsync(string memberId)
    {
        return await _db.FamilyMembers
            .FirstOrDefaultAsync(fm => fm.Id == memberId);
    }

    public async Task<FamilyMember> AddAsync(FamilyMember familyMember)
    {
        familyMember.CreatedAt = DateTime.UtcNow;

        _db.FamilyMembers.Add(familyMember);
        await _db.SaveChangesAsync();
        return familyMember;
    }

    public async Task UpdateAsync(FamilyMember familyMember)
    {
        _db.FamilyMembers.Update(familyMember);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(string memberId)
    {
        var member = await GetByIdAsync(memberId);
        if (member != null)
        {
            _db.FamilyMembers.Remove(member);
            await _db.SaveChangesAsync();
        }
    }

    public async Task<FamilyMember?> GetByNameAsync(string name, string? familyId = null)
    {
        return await _db.FamilyMembers
            .FirstOrDefaultAsync(fm => fm.Name.ToLower() == name.ToLower());
    }

    public async Task<FamilyMember> GetOrCreateMemberAsync(string name, string? familyId = null)
    {
        var existingMember = await GetByNameAsync(name);
        if (existingMember != null)
        {
            return existingMember;
        }

        var newMember = new FamilyMember
        {
            Name = name,
            Pin = "1234" // Default PIN - should be changed on first login
        };

        return await AddAsync(newMember);
    }

    public async Task<List<FamilyMember>> GetFamilyMembersWithSpendingAsync(string? familyId = null, DateTime? month = null)
    {
        // Simplified - just return all members
        // Spending calculations are now handled by UserService
        return await GetAsync();
    }

    public async Task<string> GetDefaultFamilyIdAsync()
    {
        var familyAccount = await _db.FamilyAccounts.FirstOrDefaultAsync();
        if (familyAccount is not null)
        {
            return familyAccount.FamilyId;
        }

        var defaultAccount = new FamilyAccount
        {
            FamilyId = "family-default",
            FamilyName = "Family"
        };

        _db.FamilyAccounts.Add(defaultAccount);
        await _db.SaveChangesAsync();

        return defaultAccount.FamilyId;
    }
}
