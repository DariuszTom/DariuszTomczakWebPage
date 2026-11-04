# Plan strony DariuszTomczakWebPage

Blazor WebAssembly (.NET 10) + MudBlazor, hosting: **Azure Static Web Apps (Free)**.

## Cel

Zastąpić strony z szablonu (Counter, Weather, „Hello, world!”) jednostronicową stroną osobistą:

**Hero → CV → Projects → Contact** (nawigacja kotwicami `#cv`, `#projects`, `#contact`).

Cała treść (doświadczenie, skille, edukacja, certyfikaty, projekty, linki kontaktowe) pochodzi
z jednego pliku `wwwroot/data/profile.json` w repozytorium. Aktualizacja strony = edycja tego
pliku (np. bezpośrednio na GitHubie). CI przebudowuje i wdraża stronę automatycznie.

Styl wizualny oparty na CV (`docs/Resume-Darek-Tomczak v3.10.html`), spójny ze splashem
(`Splash/Main.dc.html`, `wwwroot/css/splash.css`). Szczegóły w sekcji [Wygląd i stack UI](#wygląd-i-stack-ui).

## Decyzje

| Obszar | Decyzja |
|---|---|
| Układ | Jedna długa strona z kotwicami |
| Źródło danych | `wwwroot/data/profile.json` w repo, wczytywany przez `HttpClient` |
| Konfigurowalny URL | `wwwroot/appsettings.json` → `Profile:Url`; późniejsze przejście na Azure Blob Storage = zmiana jednej linijki |
| Zdjęcia | W repo: `wwwroot/img/`, format **WebP** (max ~1600 px, ~100–300 KB), `loading="lazy"`, `width`/`height` i `alt`. Bez Blob Storage, bo publiczny Blob = brak limitu kosztów przy ataku (patrz [Koszty i ryzyko DDoS](#koszty-i-ryzyko-ddos)). Usunięcie metadanych EXIF (GPS) przed commitem |
| LinkedIn | Brak bezpośredniej integracji (API nie udostępnia doświadczenia/skilli, scraping łamie regulamin). Opcjonalny import z eksportu danych LinkedIn (ZIP z CSV) |
| Kontakt | Same linki: LinkedIn, GitHub. Bez e-maila, telefonu, formularza i backendu |
| Projekty | Ręcznie wybrane, w `profile.json` |
| Azure SQL | **Nieużywana w v1.** Statyczny JSON jest darmowy, wersjonowany w git i nie ma endpointu do ataku. SWA *Database connections* (Data API builder) zostały wycofane 30.11.2025, więc baza wymagałaby własnego API (Azure Function), czyli dodatkowej powierzchni ataku. Istniejąca baza `receptyoksdb` należy do projektu ReceptyOKS i nie jest współdzielona. Ewentualnie później: osobna baza, rate limiting w API i Managed Identity |
| Projekt `DariuszTomczakWebpage.Shared` | Biblioteka klas i modeli (`Models/Profile.cs`), referencjonowana przez WASM. Zawiera też `MailSender`/`AnthropicAgent` na przyszłe API. Sekrety (klucze API, SMTP) nigdy nie trafiają do WASM, tylko do API po stronie serwera. Do rozważenia: wydzielenie AI/Mail do osobnego projektu, żeby WASM nie pobierał tych paczek |

## Wygląd i stack UI

### Motyw (z CV)

| Token | Wartość | Użycie |
|---|---|---|
| Primary (navy) | `#1e3a5f` → `#0f2744` (gradient) | AppBar, nagłówki, panel boczny / tło hero |
| Secondary (amber) | `#f59e0b` / `#d97706` | akcenty: podkreślenia `h2`, pigułki z datami, markery list, hover linków |
| Tekst | `#374151` / `#4b5563` / `#6b7280` | treść / opisy / metadane |
| Tło | `#ffffff`, karty `#f9fafb`, obramowania `#e5e7eb` | sekcje, karty edukacji i projektów |
| Font | **Inter** 400/500/600/700 (self-hosted, już w `wwwroot/fonts`) | cała strona |
| Promienie | 4–8 px | chipy, karty |

Elementy charakterystyczne do odtworzenia:
- nagłówki sekcji: uppercase, `letter-spacing: 1.5px`, amberowa belka 4 px po lewej + amberowe podkreślenie,
- doświadczenie: oś czasu z lewą kreską (`#e5e7eb`, na hover amber), okres w amberowej "pigułce",
- skille: chipy na navy z amberową ramką (`MudChip` + własna klasa),
- kontakt: kwadratowe ikony z amberowym gradientem,
- układ desktop: treść + navy sidebar (skille, narzędzia, języki) → na mobile jedna kolumna.

Motyw definiowany raz w `MudTheme` (`PaletteLight`, opcjonalnie `PaletteDark`, `Typography` z Inter),
drobne elementy w CSS isolation (`*.razor.css`).

### Stack: czy MudBlazor wystarczy?

Tak. **MudBlazor pokrywa wszystko**, czego potrzebujemy. Dodatkowe biblioteki UI nie są potrzebne.

| Potrzeba | Rozwiązanie | Dodatkowa paczka? |
|---|---|---|
| Layout, AppBar, siatka, karty, chipy, timeline | MudBlazor (`MudAppBar`, `MudGrid`, `MudCard`, `MudChip`, `MudTimeline`) | nie |
| Ikony (w tym GitHub, LinkedIn) | `Icons.Material.*`, `Icons.Custom.Brands.*` z MudBlazor | nie |
| Font | Inter self-hosted (bez Google Fonts: lepsze CSP i prywatność) | nie |
| Wydruk / PDF CV | `@media print` + przycisk "Pobierz PDF" (`window.print()`) lub statyczny PDF w `wwwroot` | nie |
| SEO (WASM nie renderuje HTML na serwerze) | statyczne `<meta>` description, Open Graph, `JSON-LD` (`schema.org/Person`) w `index.html` | nie |
| Testy komponentów *(opcjonalnie)* | **bUnit** + xUnit w osobnym projekcie testowym (workflow już ma `dotnet test`) | tak, tylko w testach |

**Usunięte** (✅):
- **Bootstrap** (`wwwroot/lib/bootstrap`, link w `index.html`) – dubluje MudBlazor i koliduje ze stylami,
- `NavMenu`, `MainLayout.razor.css` z szablonu, `Counter`, `Weather`, `sample-data`.

### Prywatność danych kontaktowych

- **Numer telefonu z CV nie trafia na publiczną stronę** (boty zbierają numery) – zostaje tylko w PDF.
- **E-mail również nie trafia na stronę.** Kontakt wyłącznie przez LinkedIn i GitHub.

## Bezpieczeństwo

- **Brak backendu = brak endpointów do nadużycia.** Rate limiter na poziomie aplikacji nie jest
  potrzebny.
- **`staticwebapp.config.json`**:
  - `Content-Security-Policy` (z uwzględnieniem wymagań Blazor WASM: `'wasm-unsafe-eval'`),
  - `X-Content-Type-Options: nosniff`,
  - `Referrer-Policy: strict-origin-when-cross-origin`,
  - `Permissions-Policy` (wyłączenie kamery, mikrofonu, geolokalizacji),
  - `frame-ancestors 'none'` (ochrona przed clickjackingiem),
  - `navigationFallback` dla routingu SPA,
  - nagłówki cache (krótki cache dla `profile.json`).
- **Ruch wolumetryczny / DDoS**: patrz [Koszty i ryzyko DDoS](#koszty-i-ryzyko-ddos).
- **Na przyszłość** (formularz kontaktowy / czat AI): Azure Function (SWA managed API) z sekretami
  po stronie serwera (app settings / Key Vault), rate limiting (ASP.NET Core `RateLimiter` lub
  Front Door WAF) oraz captcha (np. Cloudflare Turnstile).

### Koszty i ryzyko DDoS

**Strona może być w 100% darmowa i atak nie wygeneruje rachunku.**

| Usługa | Koszt | Co się dzieje przy zalewie ruchu |
|---|---|---|
| **SWA Free** | 0 zł | 100 GB transferu / miesiąc, **nadwyżki niedostępne** → po przekroczeniu strona przestaje odpowiadać do końca miesiąca, **brak dopłat** |
| SWA Standard | ~9 USD / mies. | nadwyżka 0,20 USD / GB → **atak = rachunek** |
| Azure Blob Storage (publiczny) | grosze | płatność za każdy GB i każde żądanie, **bez limitu** → atak = rachunek |
| Azure Front Door Standard + WAF | ~35 USD / mies. + ruch + reguły | rate limiting działa, ale opłata stała i za ruch |
| Cloudflare Free (przed własną domeną) | 0 zł | darmowa ochrona DDoS, cache, opcjonalne reguły rate limit |

Decyzje:
- **Zostajemy na SWA Free**, bez Blob Storage i bez Front Door. Najgorszy scenariusz ataku to
  niedostępność strony, a nie rachunek.
- Cała treść (JSON + zdjęcia) serwowana z SWA, żadne zasoby nie są pobierane z płatnych usług.
- **Ochrona subskrypcji**: budżet w Azure Cost Management (np. 5 USD) z alertem e-mail,
  jako siatka bezpieczeństwa na wypadek pomyłkowo utworzonych płatnych zasobów.
- *(opcjonalnie, przy własnej domenie)* Cloudflare Free jako proxy przed SWA.
- Lekka strona = większy zapas: 100 GB przy ~3 MB na wizytę to ~30 000 wizyt miesięcznie.

### CI/CD bez sekretów (OIDC / workload identity federation)

W GitHubie **nie przechowujemy** długożyjącego deployment tokenu SWA ani hasła service principala.

```text
GitHub Actions ──(krótkożyjący token OIDC)──► Microsoft Entra ID
        │                                        │ federated credential
        │                                        ▼
        │                     User-assigned managed identity (id-dtomczak-web-deploy)
        │                                        │ RBAC: rola tylko na zasobie SWA
        ▼                                        ▼
 azure/login ──► az staticwebapp secrets list ──► token tylko w pamięci joba (zamaskowany)
        └──► Azure/static-web-apps-deploy (skip_app_build, gotowy publish/wwwroot)
```

- **Tożsamość**: user-assigned managed identity z *federated credential* ufającą wyłącznie
  `repo:DariuszTom/DariuszTomczakWebPage:environment:Master_ENV` (nazwa environmentu w GitHubie to `Master_ENV`).
  Forki, inne repozytoria i inne gałęzie nie dostaną tokenu.
- **GitHub Environment `Master_ENV`**: ograniczenie do gałęzi `master`, opcjonalnie wymagane
  ręczne zatwierdzenie (required reviewers).
- **Najmniejsze uprawnienia**: rola przypisana tylko na scope zasobu Static Web App
  (nie na subskrypcji / resource group).
- **Brak sekretów w repo**: `AZURE_CLIENT_ID`, `AZURE_TENANT_ID`, `AZURE_SUBSCRIPTION_ID` to
  identyfikatory, nie sekrety. Trzymane jako *repository/environment variables*.
- **Token SWA** pobierany w trakcie joba, maskowany (`::add-mask::`), nigdy nie zapisywany.
  Można go rotować w dowolnym momencie (`az staticwebapp secrets reset-api-key`) bez zmian w GitHubie.
- **Uprawnienia workflow**: `permissions: { id-token: write, contents: read }` tylko w jobie deploy;
  domyślnie `contents: read`.
- **Utwardzenie akcji**: akcje przypięte do commit SHA, Dependabot dla `github-actions`.
- **Pull requesty**: tylko build + test, bez dostępu do Azure (brak `id-token` i environment).

### Analiza kodu i sekretów (GitHub Actions)

| Workflow | Narzędzie | Kiedy | Wynik |
|---|---|---|---|
| `codeql.yml` | CodeQL (`csharp`, `actions`, `javascript-typescript`), zestaw `security-and-quality`, `build-mode: none` | push/PR na `master`, co tydzień | Code scanning (repo publiczne) albo artefakt SARIF (repo prywatne) |
| `codacy.yml` | Codacy Analysis CLI | push/PR na `master`, co tydzień | jw.; z sekretem `CODACY_PROJECT_TOKEN` wyniki trafiają też do panelu Codacy |
| `secrets-scan.yml` | TruffleHog | push/PR: nowe commity; co tydzień i ręcznie: cała historia | job kończy się błędem przy znalezionym sekrecie (zweryfikowanym lub nieznanym) |
| `dependabot.yml` | Dependabot | co tydzień | grupowane PR dla GitHub Actions i NuGet |

Zasady: wszystkie akcje przypięte do SHA, `permissions: contents: read` domyślnie,
`persist-credentials: false` przy checkout. Repo jest prywatne, więc upload do Code scanning
wymaga GitHub Code Security; bez niej SARIF jest dostępny jako artefakt joba.
Konfiguracja Codacy: `.codacy.yml` (wykluczenia: `bin`, `obj`, `wwwroot/lib`, `Splash/vendor`, `*.min.*`)
oraz `.eslintrc.json` (ESLint 8 w Codacy wymaga konfiguracji w repo).
Opcjonalnie: włączyć **Secret Protection / push protection** w ustawieniach repo (blokuje push z sekretem).

## Struktura `profile.json` (szkic)

```json
{
  "person": {
	"name": "Dariusz Tomczak",
	"title": "...",
	"summary": "...",
	"location": "...",
	"photoUrl": "img/me.jpg"
  },
  "experience": [
	{
	  "company": "...",
	  "role": "...",
	  "start": "2022-01",
	  "end": null,
	  "location": "...",
	  "highlights": ["..."],
	  "technologies": [".NET", "Blazor", "Azure"]
	}
  ],
  "skills": [
	{ "category": "Backend", "items": ["C#", ".NET", "ASP.NET Core"] }
  ],
  "education": [
	{ "school": "...", "degree": "...", "start": "...", "end": "..." }
  ],
  "certifications": [
	{ "name": "...", "issuer": "...", "date": "...", "url": "..." }
  ],
  "projects": [
	{
	  "name": "...",
	  "description": "...",
	  "technologies": ["..."],
	  "repoUrl": "https://github.com/DariuszTom/...",
	  "liveUrl": null
	}
  ],
  "photos": [
    { "src": "img/hero.webp", "alt": "...", "caption": "..." }
  ],
  "links": {
	"linkedIn": "https://www.linkedin.com/in/...",
	"gitHub": "https://github.com/DariuszTom"
  }
}
```

## Zadania

| # | ID | Zadanie | Zależy od |
|---|---|---|---|
| 1 | `profile-model` | Rekordy modelu profilu (Person, Experience, SkillGroup, Education, Certification, Project, Links) | – |
| 2 | `profile-json` | `wwwroot/data/profile.json` wypełniony danymi z CV v3.10 (bez telefonu) | – |
| 3 | `profile-service` | `IProfileService`: wczytanie JSON przez `HttpClient`, cache w pamięci, URL z `Profile:Url` | 1, 2 |
| 4 | `layout-cleanup` | Usunięcie Counter/Weather/`sample-data`/NavMenu/**Bootstrap**; nowy `MainLayout` z MudBlazor AppBar i nawigacją kotwicami; `MudTheme` z paletą z CV | – |
| 5 | `section-hero` | Sekcja Hero: imię, tytuł, krótkie bio, przyciski CTA | 3, 4 |
| 6 | `section-cv` | Sekcja CV: oś czasu doświadczenia, skille jako chipy, edukacja i certyfikaty | 3, 4 |
| 7 | `section-projects` | Sekcja Projects: karty projektów (opis, technologie, linki) | 3, 4 |
| 8 | `section-contact` | Sekcja Contact: LinkedIn, GitHub | 3, 4 |
| 9 | `swa-config` | `staticwebapp.config.json`: nagłówki bezpieczeństwa, fallback SPA, cache | – |
| 10 | `deploy-workflow` | Job `deploy` w `.github/workflows/dotnet-desktop.yml`: OIDC (`azure/login`) → token SWA pobierany w locie → `static-web-apps-deploy` z artefaktu Release; environment `Master_ENV`; akcje przypięte do SHA + Dependabot | 9 |
| 11 | `linkedin-import` | *(opcjonalnie)* Skrypt: eksport LinkedIn (CSV) → `profile.json` | 1 |
| 12 | `seo-meta` | `index.html`: tytuł, meta description, Open Graph, JSON-LD `Person` | – |
| 13 | `print-cv` | Styl `@media print` dla sekcji CV + przycisk "Pobierz PDF" | 6 |
| 14 | `ui-tests` | *(opcjonalnie)* projekt testowy bUnit dla sekcji | 5–8 |
| 15 | `photos` | `wwwroot/img/` (WebP, bez EXIF), pole `photos` w `profile.json` (`src`, `alt`, `caption`), zdjęcie w Hero + opcjonalna galeria (`MudCarousel`/siatka) z lazy loading | 3 |
| 16 | `cost-budget` | Budżet Azure Cost Management (np. 5 USD) z alertem e-mail (część `infra/`) | – |

## Utworzona infrastruktura (03.11.2026)

| Zasób | Nazwa | Region / uwagi |
|---|---|---|
| Resource group | `rg-dtomczak-web` | westeurope (metadane), tag `project=dtomczak-web` |
| Static Web App (Free) | `swa-dtomczak-web` | centralus (treść serwowana globalnie), `https://blue-flower-035660c10.3.azurestaticapps.net` |
| Managed identity | `id-dtomczak-web-deploy` | polandcentral, clientId `50d33859-43d6-4059-a77e-db4c3cdcb574` |
| Federated credential | `github-production` | subject `repo:DariuszTom/DariuszTomczakWebPage:environment:Master_ENV` |
| Rola | Contributor | scope: tylko `swa-dtomczak-web` |
| Budżet | `dtomczak-web-monthly` | 1 USD/mies. na RG, alerty e-mail: 50% i 100% faktycznie, 100% prognoza |

Static Web Apps nie jest dostępne w Poland Central (dostępne regiony: Central US, East US 2, West US 2,
West Europe, East Asia), a West Europe odrzuca nowe zasoby (*region not accepting new customers*, ponowna próba 03.11.2026).
Region SWA określa tylko lokalizację metadanych; pliki strony są serwowane z globalnej sieci brzegowej, więc
użytkownicy z Polski dostają je z najbliższego węzła. Tożsamość jest w Poland Central. Gdy West Europe zacznie
przyjmować zasoby, można przenieść SWA (nowy zasób + przypisanie roli).

Zmienne GitHub (environment `Master_ENV`): `AZURE_CLIENT_ID=50d33859-43d6-4059-a77e-db4c3cdcb574`,
`AZURE_TENANT_ID=206881a2-87f2-4c02-a4e9-56585b0486ad`, `AZURE_SUBSCRIPTION_ID=09b0d360-f88f-4ef7-b4e7-4e5ee5efa705`,
`SWA_NAME=swa-dtomczak-web`, `SWA_RESOURCE_GROUP=rg-dtomczak-web`.

## Wymagane kroki po stronie właściciela

Kroki 1–3 i 7: **wykonane** (patrz wyżej).

1. Utworzyć zasób **Azure Static Web App (Free)** z deployment source *Other*
   (bez automatycznego workflow i bez tokenu w GitHubie).
2. Utworzyć **user-assigned managed identity** i nadać jej rolę *Contributor* **tylko na zasobie SWA**.
3. Dodać do tożsamości **federated credential** (GitHub Actions):
   organizacja `DariuszTom`, repo `DariuszTomczakWebPage`,    typ *Environment*, nazwa `Master_ENV`.
   4. W GitHubie utworzyć environment **`Master_ENV`** (deployment branch: `master`, opcjonalnie required reviewers)
   i dodać **zmienne** (nie sekrety): `AZURE_CLIENT_ID`, `AZURE_TENANT_ID`, `AZURE_SUBSCRIPTION_ID`,
   `SWA_NAME`, `SWA_RESOURCE_GROUP`.
5. Uzupełnić `profile.json` prawdziwymi danymi (lub wygenerować z eksportu LinkedIn).
6. *(opcjonalnie)* Podpiąć własną domenę w SWA (ewentualnie przez Cloudflare Free).
7. Ustawić **budżet z alertem** w Azure Cost Management.
8. Dodać zdjęcia do `wwwroot/img/` (WebP, bez EXIF).

Kroki 1–4 mogą zostać przygotowane jako skrypt `az` / Bicep (`infra/`), żeby całość była powtarzalna.
