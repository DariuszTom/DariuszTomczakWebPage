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
            Background = "#ffffff",
            Surface = "#ffffff",
            TextPrimary = "#374151",
            TextSecondary = "#6b7280",
            Divider = "#e5e7eb",
            LinesDefault = "#e5e7eb",
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
