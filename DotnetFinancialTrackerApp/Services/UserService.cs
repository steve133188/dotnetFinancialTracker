using DotnetFinancialTrackerApp.Data;
using DotnetFinancialTrackerApp.Models;
using Microsoft.EntityFrameworkCore;

namespace DotnetFinancialTrackerApp.Services;

public class UserService : IUserService
{
    private readonly AppDbContext _db;

    public UserService(AppDbContext db) => _db = db;

    public Task<List<FamilyMember>> GetUsersAsync() => _db.FamilyMembers.OrderBy(m => m.Name).ToListAsync();

    public Task<FamilyMember?> GetByIdAsync(string memberId) => _db.FamilyMembers.FirstOrDefaultAsync(m => m.Id == memberId);

    public async Task<FamilyMember> CreateAsync(string name, string pin)
    {
        var member = new FamilyMember
        {
            Name = name.Trim(),
            Pin = pin
        };
        _db.FamilyMembers.Add(member);
        await _db.SaveChangesAsync();
        return member;
    }

    public async Task<FamilyMember?> VerifyAsync(string memberId, string pin)
    {
        var member = await _db.FamilyMembers.FirstOrDefaultAsync(m => m.Id == memberId);
        if (member is null) return null;
        return string.Equals(pin, member.Pin, StringComparison.Ordinal) ? member : null;
    }

    public async Task<FamilyMember?> UpdateNameAsync(string memberId, string name)
    {
        var trimmed = name.Trim();
        if (string.IsNullOrWhiteSpace(trimmed)) return null;

        var member = await _db.FamilyMembers.FirstOrDefaultAsync(m => m.Id == memberId);
        if (member is null) return null;

        member.Name = trimmed;
        await _db.SaveChangesAsync();
        return member;
    }

    public async Task<bool> UpdatePinAsync(string memberId, string currentPin, string newPin)
    {
        var member = await _db.FamilyMembers.FirstOrDefaultAsync(m => m.Id == memberId);
        if (member is null) return false;

        if (!string.Equals(currentPin, member.Pin, StringComparison.Ordinal)) return false;

        member.Pin = newPin;
        await _db.SaveChangesAsync();
        return true;
    }

    // Calculated data methods
    public async Task<decimal> GetUserSpentThisMonthAsync(string memberId)
    {
        var currentMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        var nextMonth = currentMonth.AddMonths(1);

        return await _db.Transactions
            .Where(t => t.FamilyMemberId == memberId &&
                       t.Date >= currentMonth &&
                       t.Date < nextMonth &&
                       t.Type == TransactionType.Expense)
            .SumAsync(t => t.Amount);
    }

    public async Task<int> GetUserTransactionCountThisMonthAsync(string memberId)
    {
        var currentMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        var nextMonth = currentMonth.AddMonths(1);

        return await _db.Transactions
            .Where(t => t.FamilyMemberId == memberId &&
                       t.Date >= currentMonth &&
                       t.Date < nextMonth)
            .CountAsync();
    }

    public async Task<decimal> GetUserIncomeThisMonthAsync(string memberId)
    {
        var currentMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        var nextMonth = currentMonth.AddMonths(1);

        return await _db.Transactions
            .Where(t => t.FamilyMemberId == memberId &&
                       t.Date >= currentMonth &&
                       t.Date < nextMonth &&
                       t.Type == TransactionType.Income)
            .SumAsync(t => t.Amount);
    }
}