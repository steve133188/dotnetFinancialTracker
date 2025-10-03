using MudBlazor;

namespace DotnetFinancialTrackerApp.Services;

public class UiState
{
    public bool IsKiosk { get; private set; }
    public Breakpoint CurrentBreakpoint { get; private set; } = Breakpoint.Md;
    public event Action? Changed;

    public void SetKiosk(bool kiosk)
    {
        if (IsKiosk == kiosk) return;
        IsKiosk = kiosk;
        Changed?.Invoke();
    }

    public void SetBreakpoint(Breakpoint bp)
    {
        if (CurrentBreakpoint == bp) return;
        CurrentBreakpoint = bp;
        Changed?.Invoke();
    }
}

