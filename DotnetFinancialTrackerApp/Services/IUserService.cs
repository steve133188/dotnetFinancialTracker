using DotnetFinancialTrackerApp.Models;

namespace DotnetFinancialTrackerApp.Services;

public interface IUserService
{
    Task<List<FamilyMember>> GetUsersAsync();
    Task<FamilyMember> CreateAsync(string name, string pin);
    Task<FamilyMember?> VerifyAsync(string memberId, string pin);
    Task<FamilyMember?> UpdateNameAsync(string memberId, string name);
    Task<bool> UpdatePinAsync(string memberId, string currentPin, string newPin);
    Task<FamilyMember?> GetByIdAsync(string memberId);
}
