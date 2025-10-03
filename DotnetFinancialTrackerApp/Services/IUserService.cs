using DotnetFinancialTrackerApp.Models;

namespace DotnetFinancialTrackerApp.Services;

public interface IUserService
{
    Task<List<UserProfile>> GetUsersAsync();
    Task<UserProfile> CreateAsync(string name, string pin);
    Task<UserProfile?> VerifyAsync(int userId, string pin);
}

