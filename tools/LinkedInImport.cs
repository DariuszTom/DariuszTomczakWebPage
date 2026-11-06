// Merges a LinkedIn data export (unzipped folder with CSV files) into wwwroot/data/profile.json.
//
// Usage (from the repo root):
//   dotnet run tools/LinkedInImport.cs -- <export-folder> [profile.json]
//
// Merge rules (re-running is safe):
//   - facts from LinkedIn win: roles, dates, locations, headline, languages, certifications;
//   - curated content stays: highlights, technologies, skill categories, summary, degrees, projects;
//   - new LinkedIn items are added (positions, schools, projects); new skills go to the "More" group.
// Only CV-related files are read. Messages, contacts, e-mail, phone numbers etc. are never touched.

#:property PublishAot=false

using System.Globalization;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

if (args.Length < 1)
{
    Console.Error.WriteLine("Usage: dotnet run tools/LinkedInImport.cs -- <export-folder> [profile.json]");
    return 1;
}

var exportDir = args[0];
var profilePath = args.Length > 1 ? args[1] : Path.Combine("DariuszTomczakWebPage", "wwwroot", "data", "profile.json");
if (!Directory.Exists(exportDir)) return Fail($"Export folder not found: {exportDir}");
if (!File.Exists(profilePath)) return Fail($"profile.json not found: {profilePath}");

var profile = JsonNode.Parse(File.ReadAllText(profilePath))!.AsObject();
var log = new List<string>();

MergePerson();
MergeExperience();
MergeSkills();
MergeEducation();
ReplaceCertifications();
ReplaceLanguages();
MergeProjects();

var json = profile.ToJsonString(new JsonSerializerOptions
{
    WriteIndented = true,
    IndentSize = 2,
    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
});
File.WriteAllText(profilePath, json + Environment.NewLine, new UTF8Encoding(false));

log.ForEach(Console.WriteLine);
Console.WriteLine($"Updated {profilePath}");
return 0;

void MergePerson()
{
    var row = Read("Profile.csv").FirstOrDefault();
    if (row is null) return;
    var person = profile["person"]!.AsObject();

    if (Value(row, "Headline") is { } headline) person["title"] = headline;
    // The LinkedIn summary is only a fallback: the curated one is usually better worded.
    if (string.IsNullOrWhiteSpace((string?)person["summary"]) && Value(row, "Summary") is { } summary)
        person["summary"] = summary;

    var links = profile["links"]!.AsObject();
    var gitHub = Regex.Match(row.GetValueOrDefault("Websites") ?? "", @"https://github\.com/[^\]\s,]+");
    if (string.IsNullOrWhiteSpace((string?)links["gitHub"]) && gitHub.Success) links["gitHub"] = gitHub.Value;
}

void MergeExperience()
{
    var rows = Read("Positions.csv");
    if (rows.Count == 0) return;
    var items = profile["experience"]!.AsArray();

    foreach (var row in rows)
    {
        var company = Value(row, "Company Name");
        var start = Month(Value(row, "Started On"));
        if (company is null || start is null) continue;

        var item = items.OfType<JsonObject>().FirstOrDefault(e =>
            Same((string?)e["company"], company) && (string?)e["start"] == start);
        if (item is null)
        {
            item = new JsonObject
            {
                ["company"] = company,
                ["highlights"] = new JsonArray(),
                ["technologies"] = new JsonArray(),
            };
            items.Add(item);
            log.Add($"+ experience: {company} / {Title(Value(row, "Title"))}");
        }

        item["role"] = Title(Value(row, "Title"));
        item["start"] = start;
        item["end"] = Month(Value(row, "Finished On"));
        item["location"] = Location(Value(row, "Location"));

        var linkedInHighlights = Value(row, "Description") is { } description ? SplitHighlights(description).ToList() : [];
        var current = (item["highlights"] as JsonArray)?.Count ?? 0;
        if (current == 0 && linkedInHighlights.Count > 0)
        {
            item["highlights"] = new JsonArray(linkedInHighlights.Select(h => (JsonNode?)h).ToArray());
            log.Add($"~ experience: {company} / {item["role"]} - highlights generated from LinkedIn, review them");
        }
        else if (linkedInHighlights.Count > current)
        {
            log.Add($"? experience: {company} / {item["role"]} - LinkedIn has {linkedInHighlights.Count} points, profile.json has {current}");
        }
    }

    Sort(items, e => (string?)e["start"]);
}

void MergeSkills()
{
    var names = Read("Skills.csv").Select(r => Value(r, "Name")).OfType<string>().ToList();
    if (names.Count == 0) return;
    var groups = profile["skills"]!.AsArray();

    var existing = groups.OfType<JsonObject>()
        .SelectMany(g => g["items"]!.AsArray().Select(i => (string?)i))
        .OfType<string>()
        .ToList();
    var known = existing.SelectMany(SkillKeys).ToHashSet();
    var knownWords = existing.Select(Words).ToList();
    var ignored = ReadIgnoreList();

    var more = groups.OfType<JsonObject>().FirstOrDefault(g => (string?)g["category"] == "More");
    foreach (var name in names)
    {
        var keys = SkillKeys(name).ToList();
        if (ignored.Contains(name) || keys.Any(known.Contains)) continue;
        // "Kali Linux" is covered by "Linux (Ubuntu, Kali)", "Visio" by "Visio and Lucidchart".
        var words = Words(name);
        if (knownWords.Any(w => words.IsSubsetOf(w))) continue;
        if (more is null)
        {
            more = new JsonObject { ["category"] = "More", ["items"] = new JsonArray() };
            groups.Add(more);
        }
        more["items"]!.AsArray().Add(name);
        keys.ForEach(k => known.Add(k));
        knownWords.Add(words);
        log.Add($"+ skill (More): {name}");
    }
}

void MergeEducation()
{
    var rows = Read("Education.csv");
    if (rows.Count == 0) return;
    var items = profile["education"]!.AsArray();

    foreach (var row in rows)
    {
        var school = Value(row, "School Name");
        var start = Year(Value(row, "Start Date"));
        var end = Year(Value(row, "End Date"));
        if (school is null) continue;

        // LinkedIn mixes Polish and English school names, so entries are matched by years.
        var item = items.OfType<JsonObject>().FirstOrDefault(e =>
            (string?)e["end"] == end && (start is null || (string?)e["start"] is null || (string?)e["start"] == start));
        if (item is null)
        {
            item = new JsonObject { ["school"] = School(school), ["degree"] = Degree(Value(row, "Degree Name")) };
            items.Add(item);
            log.Add($"+ education: {item["school"]} ({end})");
        }

        item["start"] = start;
        item["end"] = end;
        item["location"] ??= null;
    }

    Sort(items, e => (string?)e["end"] ?? (string?)e["start"]);
}

void ReplaceCertifications()
{
    var rows = Read("Certifications.csv");
    if (rows.Count == 0) return;

    var certs = rows
        .Where(r => Value(r, "Name") is not null)
        .Select(r => new JsonObject
        {
            ["name"] = Value(r, "Name"),
            ["issuer"] = Issuer(Value(r, "Authority")),
            ["date"] = Month(Value(r, "Started On")),
            ["url"] = CleanUrl(Value(r, "Url")),
        })
        .OrderByDescending(c => (string?)c["date"], StringComparer.Ordinal)
        .ToArray<JsonNode?>();

    profile["certifications"] = new JsonArray(certs);
    log.Add($"= certifications: {certs.Length}");
}

void ReplaceLanguages()
{
    var rows = Read("Languages.csv");
    if (rows.Count == 0) return;

    var languages = rows
        .Where(r => Value(r, "Name") is not null)
        .Select(r => (Name: Value(r, "Name")!, Level: Level(Value(r, "Proficiency"))))
        .OrderBy(l => l.Level.Rank)
        .Select(l => new JsonObject { ["name"] = l.Name, ["level"] = l.Level.Text })
        .ToArray<JsonNode?>();

    profile["languages"] = new JsonArray(languages);
    log.Add($"= languages: {languages.Length}");
}

void MergeProjects()
{
    var items = profile["projects"]!.AsArray();
    foreach (var row in Read("Projects.csv"))
    {
        var name = Value(row, "Title");
        if (name is null || items.OfType<JsonObject>().Any(p => Same((string?)p["name"], name))) continue;

        // Links pasted into the text ("Steam:https://...") are dropped; the project URL has its own field.
        var description = Value(row, "Description") is { } raw
            ? Regex.Replace(raw, @"\s*(\w+:\s*)?https?://\S+", "").Trim()
            : null;
        if (string.IsNullOrEmpty(description))
        {
            log.Add($"! project skipped (no description on LinkedIn): {name}");
            continue;
        }

        items.Add(new JsonObject
        {
            ["name"] = name,
            ["description"] = description,
            ["technologies"] = new JsonArray(),
            ["repoUrl"] = null,
            ["liveUrl"] = CleanUrl(Value(row, "Url")),
        });
        log.Add($"+ project: {name} - review the description and technologies");
    }
}

// ---------- helpers ----------

int Fail(string message)
{
    Console.Error.WriteLine(message);
    return 1;
}

List<Dictionary<string, string>> Read(string file)
{
    var path = Path.Combine(exportDir, file);
    if (!File.Exists(path))
    {
        log.Add($"! missing {file}, skipped");
        return [];
    }

    var records = ParseCsv(File.ReadAllText(path, Encoding.UTF8));
    if (records.Count == 0) return [];
    var header = records[0];
    return records.Skip(1)
        .Where(r => r.Any(c => c.Length > 0))
        .Select(r => header.Select((h, i) => (h, v: i < r.Count ? r[i] : "")).ToDictionary(x => x.h, x => x.v))
        .ToList();
}

static List<List<string>> ParseCsv(string text)
{
    var rows = new List<List<string>>();
    var row = new List<string>();
    var field = new StringBuilder();
    var quoted = false;

    for (var i = 0; i < text.Length; i++)
    {
        var c = text[i];
        if (quoted)
        {
            if (c == '"' && i + 1 < text.Length && text[i + 1] == '"') { field.Append('"'); i++; }
            else if (c == '"') quoted = false;
            else field.Append(c);
        }
        else if (c == '"') quoted = true;
        else if (c == ',') { row.Add(field.ToString()); field.Clear(); }
        else if (c is '\r' or '\n')
        {
            if (c == '\r' && i + 1 < text.Length && text[i + 1] == '\n') i++;
            row.Add(field.ToString()); field.Clear();
            rows.Add(row); row = [];
        }
        else field.Append(c);
    }

    if (field.Length > 0 || row.Count > 0) { row.Add(field.ToString()); rows.Add(row); }
    return rows;
}

static string? Value(Dictionary<string, string> row, string column) =>
    row.TryGetValue(column, out var v) && !string.IsNullOrWhiteSpace(v) ? Regex.Replace(v.Trim(), @"\s+", " ") : null;

static string? Title(string? value) => value is null ? null : Regex.Replace(value, @"\s*-\s+", " - ");

static HashSet<string> Words(string value) =>
    Regex.Split(value.ToLowerInvariant(), @"[^a-z0-9#+]+").Where(w => w.Length > 0 && w is not "and").ToHashSet();

// Optional tools/LinkedInImport.ignore: one skill per line that should never be added.
static HashSet<string> ReadIgnoreList()
{
    var path = Path.Combine(AppContext.GetData("EntryPointFileDirectoryPath") as string ?? "tools", "LinkedInImport.ignore");
    return File.Exists(path)
        ? File.ReadAllLines(path).Select(l => l.Trim()).Where(l => l.Length > 0 && !l.StartsWith('#')).ToHashSet(StringComparer.OrdinalIgnoreCase)
        : new HashSet<string>(StringComparer.OrdinalIgnoreCase);
}

// LinkedIn Learning courses accredited for CPE credits are exported with NASBA as the authority.
static string Issuer(string? value) => value switch
{
    null => "",
    "LinkedIn" or "National Association of State Boards of Accountancy" => "LinkedIn Learning",
    _ => value,
};

static bool Same(string? a, string? b) =>
    string.Equals(a?.Trim(), b?.Trim(), StringComparison.OrdinalIgnoreCase);

static string? Month(string? value)
{
    if (value is null) return null;
    if (DateTime.TryParseExact(value, "MMM yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d)) return d.ToString("yyyy-MM", CultureInfo.InvariantCulture);
    return Year(value);
}

static string? Year(string? value) => value is null ? null : Regex.Match(value, @"\d{4}") is { Success: true } m ? m.Value : null;

static string? Location(string? value) => value switch
{
    null => null,
    _ when Regex.IsMatch(value, "Krak|Crac", RegexOptions.IgnoreCase) => "Kraków, Poland",
    _ => value,
};

static string School(string value) =>
    value.Contains("National Louis University", StringComparison.OrdinalIgnoreCase)
        ? "Wyższa Szkoła Biznesu - National Louis University"
        : value;

static string Degree(string? value) => value switch
{
    null => "",
    _ when value.StartsWith("Magister", StringComparison.OrdinalIgnoreCase) => "Master's degree",
    _ when value.StartsWith("Licencjat", StringComparison.OrdinalIgnoreCase) => "Bachelor's degree",
    _ when value.StartsWith("Inżynier", StringComparison.OrdinalIgnoreCase) => "Engineer's degree",
    _ => value,
};

static (int Rank, string Text) Level(string? value) => value?.ToLowerInvariant() switch
{
    { } v when v.StartsWith("native") => (0, "Native"),
    { } v when v.StartsWith("full professional") => (1, "Fluent"),
    { } v when v.StartsWith("professional working") => (2, "Professional working"),
    { } v when v.StartsWith("limited working") => (3, "Limited working"),
    { } v when v.StartsWith("elementary") => (4, "Elementary"),
    _ => (5, value ?? ""),
};

// Only https links; query strings (tracking parameters) are dropped.
static string? CleanUrl(string? url)
{
    if (url is null || !Uri.TryCreate(url, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps) return null;
    return uri.GetLeftPart(UriPartial.Path);
}

// "Amazon Web Services (AWS)" -> "amazonwebservicesaws", "amazonwebservices", "aws".
static IEnumerable<string> SkillKeys(string name)
{
    static string Norm(string s) => Regex.Replace(s.ToLowerInvariant(), @"[^a-z0-9#+]", "");
    yield return Norm(name);
    var m = Regex.Match(name, @"^(.*?)\s*\((.*?)\)\s*$");
    if (m.Success)
    {
        yield return Norm(m.Groups[1].Value);
        yield return Norm(m.Groups[2].Value);
    }
    foreach (var part in name.Split([',', '/'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        yield return Norm(part);
}

// LinkedIn flattens bullet points into one line; split before capitalised verbs ("... architecture Conducting ...").
static IEnumerable<string> SplitHighlights(string description) =>
    Regex.Split(description, @"(?<=[\w).])\s+(?=[A-Z][a-z]+ing\b)")
        .Select(s => s.Trim().TrimEnd('.').Trim())
        .Where(s => s.Length > 0)
        .Select(s => s + ".");

static void Sort(JsonArray array, Func<JsonObject, string?> key)
{
    var sorted = array.OfType<JsonObject>().OrderByDescending(key, StringComparer.Ordinal).ToList();
    array.Clear();
    sorted.ForEach(array.Add);
}
