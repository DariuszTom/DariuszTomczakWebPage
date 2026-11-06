namespace DariuszTomczakWebPage.Services;

/// <summary>Site-wide external links, configured in the <c>Links</c> section of wwwroot/appsettings.json.</summary>
public sealed class LinksOptions
{
    public const string SectionName = "Links";

    public string? LinkedIn { get; set; }

    public string? GitHub { get; set; }

    /// <summary>Downloadable CV. When empty, "Download CV" prints the page instead.</summary>
    public string? Cv { get; set; }
}

public static class SafeUrl
{
    /// <summary>Returns the URL only if it is an absolute https URL, so config or data can't inject javascript: links.</summary>
    public static string? Https(string? url) =>
        Uri.TryCreate(url, UriKind.Absolute, out var uri) && uri.Scheme == Uri.UriSchemeHttps ? uri.AbsoluteUri : null;
}
