using System.Security.Cryptography;
using System.Text;
using DotnetFinancialTrackerApp.Data;
using DotnetFinancialTrackerApp.Models;
using Microsoft.EntityFrameworkCore;

namespace DotnetFinancialTrackerApp.Services;

public class UserService : IUserService
{
    private readonly AppDbContext _db;

    public UserService(AppDbContext db) => _db = db;

    public Task<List<UserProfile>> GetUsersAsync() => _db.Users.OrderBy(u => u.Name).ToListAsync();

    public async Task<UserProfile> CreateAsync(string name, string pin)
    {
        var salt = Convert.ToBase64String(RandomNumberGenerator.GetBytes(16));
        var hash = HashPin(pin, salt);
        var user = new UserProfile { Name = name.Trim(), Salt = salt, PinHash = hash };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return user;
    }

    public async Task<UserProfile?> VerifyAsync(int userId, string pin)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user is null) return null;
        var hash = HashPin(pin, user.Salt);
        return string.Equals(hash, user.PinHash, StringComparison.Ordinal) ? user : null;
    }

    private static string HashPin(string pin, string salt)
    {
        using var sha = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(pin + ":" + salt);
        var hash = sha.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }
}

