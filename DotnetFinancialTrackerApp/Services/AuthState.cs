using DotnetFinancialTrackerApp.Models;

namespace DotnetFinancialTrackerApp.Services;

public class AuthState
{
    public FamilyMember? CurrentUser { get; private set; }
    public bool IsAuthenticated => CurrentUser != null;
    public event Action? Changed;

    public void SignIn(FamilyMember user)
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

