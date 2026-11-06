using System.Net.Http.Json;
using System.Text.Json;
using DariuszTomczakWebpage.Shared.Models;
using Microsoft.Extensions.Options;

namespace DariuszTomczakWebPage.Services;

public sealed class ProfileOptions
{
    public const string SectionName = "Profile";

    /// <summary>Relative (to the app base) or absolute URL of profile.json.</summary>
    public string Url { get; set; } = "data/profile.json";

    /// <summary>Absolute URL of the downloadable CV. When empty, "Download CV" prints the page instead.</summary>
    public string? CvUrl { get; set; }
}

public interface IProfileService
{
    Task<Profile> GetAsync(CancellationToken cancellationToken = default);
}

public sealed class ProfileService(HttpClient http, IOptions<ProfileOptions> options) : IProfileService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private Task<Profile>? _cached;

    public Task<Profile> GetAsync(CancellationToken cancellationToken = default)
    {
        // Share one in-flight request between callers; a caller's token only cancels its own wait.
        var task = _cached ??= LoadAsync();
        if (task.IsFaulted)
        {
            _cached = null;
        }
        return task.WaitAsync(cancellationToken);
    }

    private async Task<Profile> LoadAsync()
    {
        try
        {
            return await http.GetFromJsonAsync<Profile>(options.Value.Url, JsonOptions)
                ?? throw new InvalidOperationException("Profile data is empty.");
        }
        catch
        {
            _cached = null;
            throw;
        }
    }
}
