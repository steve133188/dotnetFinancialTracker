using MudBlazor;

namespace DotnetFinancialTrackerApp.Theme;

public static class AppTheme
{
    public static MudTheme Theme { get; } = new()
    {
        Palette = new PaletteLight
        {
            Primary = "#01FFFF",
            Secondary = "#000000",
            Background = "#FFFFFF",
            Surface = "#FFFFFF",
            AppbarBackground = "#FFFFFF",
            AppbarText = "#000000",
            DrawerBackground = "#FFFFFF",
            DrawerText = "#000000",
            TextPrimary = "#000000",
            TextSecondary = "#000000",
            Info = "#01FFFF",
            Success = "#01FFFF",
            Warning = "#000000",
            Error = "#000000",
            ActionDefault = "#000000",
            ActionDisabled = "rgba(0, 0, 0, 0.26)",
            ActionDisabledBackground = "rgba(0, 0, 0, 0.12)"
        },
        PaletteDark = new PaletteDark
        {
            Primary = "#01FFFF",
            Secondary = "#000000",
            Background = "#FFFFFF",
            Surface = "#FFFFFF",
            AppbarBackground = "#FFFFFF",
            AppbarText = "#000000",
            DrawerBackground = "#FFFFFF",
            DrawerText = "#000000",
            TextPrimary = "#000000",
            TextSecondary = "#000000",
            Info = "#01FFFF",
            Success = "#01FFFF",
            Warning = "#000000",
            Error = "#000000",
            ActionDefault = "#000000",
            ActionDisabled = "rgba(0, 0, 0, 0.26)",
            ActionDisabledBackground = "rgba(0, 0, 0, 0.12)"
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
