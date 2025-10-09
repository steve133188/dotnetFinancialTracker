using MudBlazor;

namespace DotnetFinancialTrackerApp.Theme;

public static class AppTheme
{
    public static MudTheme Theme { get; } = new()
    {
        Palette = new PaletteLight
        {
            Primary = "#01FFFF",
            Secondary = "#66D1DB",
            Background = "#F4FEFF",
            Surface = "#FFFFFF",
            AppbarBackground = "#D9FDFF",
            AppbarText = "#000000",
            DrawerBackground = "#FFFFFF",
            DrawerText = "#000000",
            TextPrimary = "#000000",
            TextSecondary = "#1F1F1F",
            Info = "#48FBFF",
            Success = "#16A34A",
            Warning = "#F59E0B",
            Error = "#E11D48"
        },
        PaletteDark = new PaletteDark
        {
            Primary = "#01FFFF",
            Secondary = "#55C7D6",
            Background = "#F2FDFF",
            Surface = "#FFFFFF",
            AppbarBackground = "#D3FBFF",
            AppbarText = "#000000",
            DrawerBackground = "#FFFFFF",
            DrawerText = "#000000",
            TextPrimary = "#000000",
            TextSecondary = "#1F1F1F",
            Info = "#48FBFF",
            Success = "#22C55E",
            Warning = "#F59E0B",
            Error = "#FB7185"
        },
        LayoutProperties = new LayoutProperties
        {
            DefaultBorderRadius = "18px",
            AppbarHeight = "64px",
            DrawerWidthLeft = "320px",
            DrawerWidthRight = "320px"
        },
        Typography = new Typography
        {
            Default = new()
            {
                FontFamily = new[] { "Airbnb Cereal", "Inter", "Circular", "SF Pro Text", "Segoe UI", "Roboto", "system-ui", "-apple-system", "sans-serif" },
                FontWeight = 500
            },
            Button = new()
            {
                FontFamily = new[] { "Airbnb Cereal", "system-ui", "-apple-system", "sans-serif" },
                FontWeight = 600,
                LetterSpacing = ".2px"
            },
            H4 = new()
            {
                FontFamily = new[] { "Airbnb Cereal", "system-ui", "-apple-system", "sans-serif" },
                FontWeight = 700,
                LetterSpacing = "-0.5px"
            },
            H6 = new()
            {
                FontFamily = new[] { "Airbnb Cereal", "system-ui", "-apple-system", "sans-serif" },
                FontWeight = 650,
                LetterSpacing = "0.3px"
            },
            Subtitle1 = new()
            {
                FontFamily = new[] { "Airbnb Cereal", "system-ui", "-apple-system", "sans-serif" },
                FontWeight = 600
            },
            Body1 = new()
            {
                FontFamily = new[] { "Airbnb Cereal", "system-ui", "-apple-system", "sans-serif" },
                FontWeight = 450,
                LineHeight = 1.6
            }
        }
    };
}
