namespace DariuszTomczakWebpage.Shared.Models;

/// <remarks>Optional parameters are site-only extras that the LinkedIn import does not touch.</remarks>
public sealed record Profile(
    Person Person,
    IReadOnlyList<Experience> Experience,
    IReadOnlyList<SkillGroup> Skills,
    IReadOnlyList<Education> Education,
    IReadOnlyList<Certification> Certifications,
    IReadOnlyList<Project> Projects,
    IReadOnlyList<Photo> Photos,
    IReadOnlyList<Language> Languages,
    IReadOnlyList<FocusArea>? Focus = null,
    IReadOnlyList<Company>? Companies = null);

/// <param name="Tagline">Short hero text; falls back to <see cref="Summary"/>.</param>
/// <param name="About">Paragraphs of the About section; falls back to <see cref="Summary"/>.</param>
/// <param name="Stack">Short list of main technologies shown next to the name.</param>
public sealed record Person(
    string Name,
    string Title,
    string Summary,
    string? Location,
    string? PhotoUrl,
    string? Tagline = null,
    IReadOnlyList<string>? About = null,
    IReadOnlyList<string>? Stack = null);

/// <summary>Dates use the "yyyy-MM" format; a null <see cref="End"/> means the role is current.</summary>
public sealed record Experience(
    string Company,
    string Role,
    string Start,
    string? End,
    string? Location,
    IReadOnlyList<string> Highlights,
    IReadOnlyList<string> Technologies);

/// <summary>Extra per-company details shown above the roles.</summary>
public sealed record Company(string Name, string? Summary = null);

/// <summary><see cref="Primary"/> groups are shown as highlighted cards; others as secondary lists.</summary>
public sealed record SkillGroup(string Category, IReadOnlyList<string> Items, bool Primary = false);

public sealed record Education(string School, string Degree, string? Start, string? End, string? Location);

public sealed record Certification(string Name, string Issuer, string? Date, string? Url);

/// <param name="Icon">Icon key understood by the site (e.g. "book", "globe", "bot").</param>
/// <param name="Featured">Featured projects get a large card with <see cref="Features"/>.</param>
public sealed record Project(
    string Name,
    string Description,
    IReadOnlyList<string> Technologies,
    string? RepoUrl,
    string? LiveUrl,
    string? Subtitle = null,
    string? Status = null,
    IReadOnlyList<string>? Features = null,
    bool Featured = false,
    string? Icon = null);

/// <summary>A short "what I do" card in the About section.</summary>
public sealed record FocusArea(string Title, string Description, string? Icon = null);

public sealed record Photo(string Src, string Alt, string? Caption, int Width, int Height);

public sealed record Language(string Name, string Level);