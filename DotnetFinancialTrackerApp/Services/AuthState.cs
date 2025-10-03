using DotnetFinancialTrackerApp.Models;

namespace DotnetFinancialTrackerApp.Services;

public class AuthState
{
    public UserProfile? CurrentUser { get; private set; }
    public bool IsAuthenticated => CurrentUser != null;
    public event Action? Changed;

    public void SignIn(UserProfile user)
    {
        CurrentUser = user;
        Changed?.Invoke();
    }

    public void SignOut()
    {
        CurrentUser = null;
        Changed?.Invoke();
    }
}

