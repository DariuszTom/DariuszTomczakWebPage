namespace DariuszTomczakWebpage.Shared.Models;

public sealed record Profile(
    Person Person,
    IReadOnlyList<Experience> Experience,
    IReadOnlyList<SkillGroup> Skills,
    IReadOnlyList<Education> Education,
    IReadOnlyList<Certification> Certifications,
    IReadOnlyList<Project> Projects,
    IReadOnlyList<Photo> Photos,
    IReadOnlyList<Language> Languages,
    Links Links);

public sealed record Person(
    string Name,
    string Title,
    string Summary,
    string? Location,
    string? PhotoUrl);

/// <summary>Dates use the "yyyy-MM" format; a null <see cref="End"/> means the role is current.</summary>
public sealed record Experience(
    string Company,
    string Role,
    string Start,
    string? End,
    string? Location,
    IReadOnlyList<string> Highlights,
    IReadOnlyList<string> Technologies);

/// <summary><see cref="Primary"/> groups are shown as highlighted chips; others as plain lists.</summary>
public sealed record SkillGroup(string Category, IReadOnlyList<string> Items, bool Primary = false);

public sealed record Education(string School, string Degree, string Start, string? End, string? Location);

public sealed record Certification(string Name, string Issuer, string? Date, string? Url);

public sealed record Project(
    string Name,
    string Description,
    IReadOnlyList<string> Technologies,
    string? RepoUrl,
    string? LiveUrl);

public sealed record Photo(string Src, string Alt, string? Caption, int Width, int Height);

public sealed record Language(string Name, string Level);

public sealed record Links(string? LinkedIn, string? GitHub);
