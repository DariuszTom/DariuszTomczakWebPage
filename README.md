# DariuszTomczakWebPage

Personal portfolio and CV website of **Dariusz Tomczak**, Senior Full Stack Developer.
Built as a standalone **Blazor WebAssembly** app (.NET 11) with **MudBlazor**, hosted as a static site.

## Features

- **Home** (`/`): hero with an animated "code debugger" and focus areas, then skills, experience timeline, projects and contact sections.
- **About** (`/about`): longer bio with a photo.
- Print-friendly CV: "Download CV" opens the configured CV link, or prints the page when none is set.
- **Not found** (`/not-found`) page.
- Scroll-aware navigation that highlights the current section.
- Light/dark theme that follows the system setting and can be toggled.
- Instant static splash screen in `index.html` while the WebAssembly runtime loads.
- Respects `prefers-reduced-motion`.
- Self-hosted fonts (Inter, JetBrains Mono). No third-party requests at runtime.

All content comes from a single JSON file, [`wwwroot/data/profile.json`](DariuszTomczakWebPage/wwwroot/data/profile.json), so updating the CV doesn't need any code changes.

## Solution structure

```
DariuszTomczakWebPage.slnx
├── DariuszTomczakWebPage/            Blazor WebAssembly app
│   ├── Components/                   UI sections (Hero, Skills, ExperienceTimeline, Projects, Contact, ...)
│   ├── Layout/MainLayout.razor       App bar, navigation, theme, footer
│   ├── Pages/                        Home, About, NotFound
│   ├── Services/                     ProfileService (loads + caches profile.json), options, formatting
│   ├── Theme/SiteTheme.cs            MudBlazor palette and typography
│   ├── Splash/                       Design mockup (reference only, excluded from build)
│   └── wwwroot/
│       ├── appsettings.json          Profile URL and external links
│       ├── data/profile.json         CV content
│       ├── css/ js/ fonts/ images/
│       └── staticwebapp.config.json  SPA fallback for Azure Static Web Apps
├── DariuszTomczakWebpage.Shared/     Class library: Profile models and shared helpers
├── tools/LinkedInImport.cs           Script that merges a LinkedIn export into profile.json
└── .github/                          CI, CodeQL, secrets scan, Dependabot
```

## Getting started

### Prerequisites

- [.NET 11 SDK](https://dotnet.microsoft.com/download)
- WebAssembly build tools (needed for `publish`):
  ```
  dotnet workload install wasm-tools
  ```

### Run locally

```
dotnet run --project DariuszTomczakWebPage
```

Then open http://localhost:5093 (or https://localhost:7113 with the `https` profile).
In Visual Studio, set `DariuszTomczakWebPage` as the startup project and press F5.

### Build and publish

```
dotnet build DariuszTomczakWebPage.slnx -c Release
dotnet publish DariuszTomczakWebPage/DariuszTomczakWebPage.csproj -c Release -o publish
```

The deployable static site ends up in `publish/wwwroot`. Any static host works. For Azure Static Web Apps, `staticwebapp.config.json` rewrites unknown routes to `index.html`.

## Configuration

`wwwroot/appsettings.json`:

| Key | Description |
|-----|-------------|
| `Profile:Url` | Location of the profile JSON, relative to the app base or absolute. Default: `data/profile.json`. |
| `Links:LinkedIn` | LinkedIn profile URL. |
| `Links:GitHub` | GitHub profile URL. |
| `Links:Cv` | Downloadable CV. When empty, "Download CV" prints the page instead. |

Only absolute `https` URLs are rendered as links (see `SafeUrl`).

## Editing content

The schema of `profile.json` is defined by the records in [`DariuszTomczakWebpage.Shared/Models/Profile.cs`](DariuszTomczakWebpage.Shared/Models/Profile.cs):
person, experience, companies, skills, education, certifications, projects (with awards), focus areas, photos and languages.
Dates use the `yyyy-MM` format. An empty `end` date means the role is current.

### Importing from LinkedIn

Download your data export from LinkedIn, unzip it, and run from the repo root:

```
dotnet run tools/LinkedInImport.cs -- <export-folder> [path/to/profile.json]
```

The script is safe to run more than once:

- **Taken from LinkedIn:** roles, dates, locations, headline, languages and certifications.
- **Kept from the current file:** highlights, technologies, skill categories, summary, degrees and projects.
- **Added:** new items from LinkedIn.

Skills listed in `tools/LinkedInImport.ignore` are never imported.

## CI/CD

| Workflow | Purpose |
|----------|---------|
| `dotnet-desktop.yml` | Restores, builds, tests and publishes on pushes and PRs to `master`. Uploads `wwwroot` as an artifact. |
| `deploy-azure.yml` | Manual (**Run workflow**) deploy to Azure Static Web Apps. Logs in with OIDC using the `Master_ENV` environment variables `AZURE_CLIENT_ID`, `AZURE_TENANT_ID`, `AZURE_SUBSCRIPTION_ID`, `SWA_NAME` and `SWA_RESOURCE_GROUP`. |
| `codeql.yml` | CodeQL analysis of C#, JavaScript/TypeScript and Actions workflows. |
| `secrets-scan.yml` | TruffleHog scan of new commits, plus a weekly full-history scan. |
| `dependabot.yml` | Weekly updates for NuGet packages and GitHub Actions. |

The workflows pin actions to commit SHAs and give the job token read-only access by default.

## Tech stack

C# · .NET 11 · Blazor WebAssembly · MudBlazor · HTML/CSS · JavaScript

## License

Released into the public domain under the [Unlicense](LICENSE.txt).