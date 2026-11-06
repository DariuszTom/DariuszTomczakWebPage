using MudBlazor;

namespace DariuszTomczakWebPage.Theme;

/// <summary>Palette and typography taken from the CV (navy + amber, Inter).</summary>
public static class SiteTheme
{
    public const string Navy = "#1e3a5f";
    public const string NavyDark = "#0f2744";
    public const string Amber = "#f59e0b";
    public const string AmberDark = "#d97706";

    private static readonly string[] FontFamily = ["Inter", "-apple-system", "BlinkMacSystemFont", "Segoe UI", "sans-serif"];

    public static MudTheme Create() => new()
    {
        PaletteLight = new PaletteLight
        {
            Primary = Navy,
            PrimaryDarken = NavyDark,
            Secondary = Amber,
            SecondaryDarken = AmberDark,
            SecondaryContrastText = NavyDark,
            AppbarBackground = NavyDark,
            AppbarText = "#ffffff",
            Background = "#f5f7fa",
            Surface = "#ffffff",
            TextPrimary = "#1a2433",
            TextSecondary = "#4a5567",
            Divider = "#e1e6ed",
            LinesDefault = "#e5e7eb",
        },
        PaletteDark = new PaletteDark
        {
            Primary = "#8db4e8",
            Secondary = Amber,
            SecondaryContrastText = NavyDark,
            AppbarBackground = "#0b1220",
            AppbarText = "#ffffff",
            Background = "#0b1220",
            Surface = "#131c2e",
            TextPrimary = "#e6ecf5",
            TextSecondary = "#a9b6c9",
            Divider = "#26324a",
            LinesDefault = "#26324a",
        },
        Typography = new Typography
        {
            Default = new DefaultTypography { FontFamily = FontFamily },
            H1 = new H1Typography { FontFamily = FontFamily, FontWeight = "700" },
            H2 = new H2Typography { FontFamily = FontFamily, FontWeight = "700" },
            H3 = new H3Typography { FontFamily = FontFamily, FontWeight = "700" },
            H4 = new H4Typography { FontFamily = FontFamily, FontWeight = "700" },
            H5 = new H5Typography { FontFamily = FontFamily, FontWeight = "600" },
            H6 = new H6Typography { FontFamily = FontFamily, FontWeight = "600" },
            Button = new ButtonTypography { FontFamily = FontFamily, FontWeight = "600", TextTransform = "none" },
        },
        LayoutProperties = new LayoutProperties { DefaultBorderRadius = "8px" },
    };
}
