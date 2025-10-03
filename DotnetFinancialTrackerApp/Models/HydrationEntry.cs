using System.ComponentModel.DataAnnotations;

namespace DotnetFinancialTrackerApp.Models;

public class HydrationEntry : WellbeingItem
{
    [Range(1, 10)]
    public int GlassesTarget { get; set; } = 8;

    [Range(0, 20)]
    public int GlassesConsumed { get; set; }

    public DateTime Date { get; set; } = DateTime.Today;

    public override WellbeingItemType GetItemType() => WellbeingItemType.Hydration;

    public override void MarkCompleted()
    {
        if (GlassesConsumed >= GlassesTarget)
        {
            base.MarkCompleted();
        }
    }

    public double GetCompletionPercentage()
    {
        return GlassesTarget > 0 ? Math.Min(100.0, (GlassesConsumed * 100.0) / GlassesTarget) : 0.0;
    }

    public void AddGlass()
    {
        if (GlassesConsumed < 20)
        {
            GlassesConsumed++;
            if (GlassesConsumed >= GlassesTarget)
            {
                MarkCompleted();
            }
        }
    }
}