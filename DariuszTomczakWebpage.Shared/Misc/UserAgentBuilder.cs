namespace DariuszTomczakWebpage.Shared.Misc;

/// <summary>
/// Single source of User-Agent values for the solution: <see cref="App"/> for public APIs that require
/// an honest client identification, and random browser User-Agents for fetching regular web pages.
/// </summary>
public static class UserAgentBuilder
{
    /// <summary>Honest app identification for public APIs (Nominatim usage policy, OSM tiles, etc.).</summary>
    public const string App = "DariuszTomczakWebPage/1.0";
}
