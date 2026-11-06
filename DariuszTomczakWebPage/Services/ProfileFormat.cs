using System.Globalization;
using DariuszTomczakWebpage.Shared.Models;

namespace DariuszTomczakWebPage.Services;

/// <summary>Display helpers for profile data (dates in "yyyy-MM" or "yyyy" form).</summary>
public static class ProfileFormat
{
    private static readonly CultureInfo Culture = CultureInfo.GetCultureInfo("en-GB");

    public static string Month(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "present";
        }
        if (DateOnly.TryParseExact(value, "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
        {
            return date.ToString("MMM yyyy", Culture);
        }
        return value;
    }

    public static string Year(string? value) =>
        string.IsNullOrWhiteSpace(value) ? "present" : value.Length >= 4 ? value[..4] : value;

    public static string MonthRange(string? start, string? end) => $"{Month(start)} – {Month(end)}";

    public static string YearRange(string? start, string? end) =>
        start is null ? Year(end) : $"{Year(start)} – {Year(end)}";

    public static string Dashes(string text) => text.Replace(" - ", " – ", StringComparison.Ordinal);

    public static Experience? Current(Profile profile) =>
        profile.Experience.FirstOrDefault(e => string.IsNullOrWhiteSpace(e.End));
}
