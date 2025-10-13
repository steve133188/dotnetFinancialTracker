using DotnetFinancialTrackerApp.Models;

namespace DotnetFinancialTrackerApp.Services;

public interface IFamilyMemberService
{
    Task<List<FamilyMember>> GetAsync(string? familyId = null);
    Task<FamilyMember?> GetByIdAsync(string memberId);
    Task<FamilyMember> AddAsync(FamilyMember familyMember);
    Task UpdateAsync(FamilyMember familyMember);
    Task DeleteAsync(string memberId);
    Task<FamilyMember?> GetByNameAsync(string name, string? familyId = null);
    Task<FamilyMember> GetOrCreateMemberAsync(string name, string? familyId = null);
    Task<List<FamilyMember>> GetFamilyMembersWithSpendingAsync(string? familyId = null, DateTime? month = null);
    Task<string> GetDefaultFamilyIdAsync();
}
