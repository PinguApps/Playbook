# Verify and extend the first slice

Build prerequisites: PowerShell 7, .NET SDK **10.0.402**, **9.0.319** and **11.0.100-rc.2.26504.105**. The .NET 9 SDK is needed only for the historical comparison, never for installing or running Playbook. The .NET 11 SDK verifies the new edition and conditional older-target route. No optional workloads are used.

```powershell
Set-Location <path-to-Playbook>
pwsh ./eng/Verify.ps1
```

If .NET 9 or 11 is installed separately, pass its executable with `-Net9Dotnet <path-to-dotnet>` or `-Net11Dotnet <path-to-dotnet>`. The script rejects the focused invalid-content fixture, compiles and executes the real .NET 9/10/11 samples, including a .NET 11 SDK build of a .NET 10 target, then builds the solution with analyzers/warnings as errors and checks real `pack --no-build` rejection of incomplete/stale content. It packs the ordinary artifact once and tests it; its SHA-256 identifies that package. A second package under `artifacts/release-fixture/` contains contract-only identities and is never published. Fixture packing restores ordinary build output afterwards.

Seven focused Reqnroll scenarios install from a local-only feed and new cache under unrelated temporary directories. They exercise bare/help/version, stdio initialization, exactly two read-only tools and output schemas, complete structured/JSON-text equality, canonical snippets, aliases, coverage, retained editions, conditional routes and actionable errors. The fixture scenario tests newest-compatible selection when editions overlap, a known ID without an applicable edition, and a pre-introduction conditional route. Generation also rejects that route without its conditional flag. Synthetic fixture introduction metadata is not API introduction or release coverage evidence. All scenarios require a clean shutdown and stderr diagnostics; none project-reference production code. Temporary installs are deleted afterwards.

Local checks set unusable HTTP proxies, which establishes proxy-independent operation but is **not** network isolation. On Linux use `pwsh ./eng/Verify.ps1 -IsolateNetwork`: after build/pack, the installed-consumer tests run under `sudo unshare --net` with no external network routes. Only the local package feed is available. SDK downloads/restores happen before network isolation.

With Docker available, run `pwsh ./eng/Test-Offline.ps1` after verification. It reruns the same seven Reqnroll scenarios against the exact package with `--network none`. Only package artifacts and test binaries are mounted; no repository content is accessible. Canonical sample text is embedded independently in the test assembly for comparison. The default image is pinned by digest (.NET SDK 10.0.401, runtime 10.0.12); pulling the image is an online preparation step. This is an offline check, not .NET 11 roll-forward or a broad OS matrix.

## Inspect the development package

From the repository root after verification:

```powershell
dotnet tool install PinguApps.Playbook --version 1.0.0-preview.1 --tool-path ./artifacts/manual-tool --add-source ./artifacts
./artifacts/manual-tool/playbook --help
./artifacts/manual-tool/playbook --version
```

Help must show `playbook mcp`; version must show `1.0.0-preview.1`. This installs locally, without publishing or changing global tools. Delete `artifacts/manual-tool` when finished. To see the actual recipe body and constraints yourself, use [the MCP Inspector walkthrough](inspect-guidance.md). The Reqnroll checks verify real MCP responses and lifecycle results; TRX evidence is under `artifacts/TestResults`.

## Author a recipe edition

Discovery filters use ordinal, case-sensitive area/language identifiers advertised by the installed catalogue. Queries require all whitespace-separated terms, case-insensitively, across title/goal/tags/API names. Areas and languages each use OR; filter types use AND. Results select the newest compatible edition before filtering, then page in ordinal stable-ID order (default 20, maximum 50), with `total`, `offset`, `limit` and nullable `nextOffset`. Keep the same filters when following `nextOffset`; an offset beyond the total succeeds empty. Unknown filters and invalid ranges are tool errors rather than ignored input.

Routes may declare `sharedAreas` (default empty): explicit relevant areas where the route's framework/provider/platform/rendering prerequisites allow this shared guidance. Do not infer relevance from free-text platform descriptions or broadly include every shared recipe. `includeShared=false` disables these extra area matches; omitted/empty areas still means all. Discovery retains only routes matching the target and the area/language combination together. Detail keeps all routes for its target so agents can inspect alternatives.

Programming recipes declare demonstrated `exampleLanguages`, independently of broader API usability. Genuinely neutral configuration/build recipes explicitly set `languageNeutral=true`, use an empty `exampleLanguages`, and declare applicable `neutralLanguages` on every route. This boolean labels discovery and detail; it is never inferred from XML/JSON alone. Language-specific settings/directives (such as C# `LangVersion`) remain non-neutral and declare their programming language. Generation rejects programming snippets mislabelled neutral and missing neutral-language declarations; author review must still establish that configuration is genuinely neutral. Filter vocabulary includes explicitly supported neutral languages as well as demonstrated languages, under the existing `supportedExampleLanguages` field.

Canonical XML snippets use `<!-- snippet:<name> -->` / `<!-- endsnippet:<name> -->` markers. Existing `// snippet:<name>` markers remain supported. The unpublished fixture uses the actual sample project's generic `TargetFramework` setting as neutral XML and its C# `LangVersion` setting as non-neutral XML. These are contract cases, not additional shipped recipes or evidence of new APIs. The 53 synthetic paging identities also stay in the fixture package.

Add `content/recipes/<stable-id>/<edition>/recipe.json` and `recipe.md`, following `eng/recipe.schema.json`. Keep IDs goal-based and release-independent, with nonempty dot-separated names. Declare every supported target explicitly; no future-target compatibility is inferred. Every target needs an applicability route with exact requirements, declared source URLs and evidence for that target. An edition cannot predate introduction. A target older than `introducedIn` also requires `conditional: true`; compilation alone does not establish supported language/runtime combinations. Declare framework, package assets, providers, platforms and rendering restrictions explicitly. Keep `content/coverage.json` honest about scope/completion. Aliases, release resolution and JSON tool errors are implemented; combined discovery filters and paging are implemented.

Experimental recipes use `stability: experimental`; every route must provide nonempty `optIns` and `stabilityLimitations`, including the actual directives/configuration and versioned package requirements where applicable. These fields reach both discovery and detail. Stable recipes may omit them; generation supplies empty arrays. Validate publisher-designated experimental status and package versions during source review: structural checks cannot infer API stability from arbitrary prose.

Add independently pinned original comparison samples under `samples/`, targeting the actual previous/current releases. Include exact SDK/language/target evidence and official HTTPS references. Mark canonical source regions with `// snippet:<name>` and `// endsnippet:<name>`, and reference them in Markdown with `{{snippet:<key>}}`. Every snippet must belong to a declared evidence sample directory and contain exactly one nonempty matching region. Do not paste fenced implementations into Markdown: code/configuration blocks must come from canonical sample references. The build validates metadata, paths, evidence baselines and snippet references; it embeds generated JSON and rendered Markdown into the tool assembly. Discovery projects that same embedded edition metadata. No runtime files or fetches are needed.

Include exactly one nonempty Goal, Older approach, Newer approach, Migration, Constraints and Evidence section (descriptive heading suffixes are allowed). Newer approach requires a canonical snippet; Migration requires numbered offline implementation steps. A missing, empty or link-only section is rejected. Explain an unavailable older equivalent honestly. Review the full recipe for essential configuration and instructions: structural validation cannot prove that arbitrary prose implements its stated goal.

Content validation also runs during `dotnet pack --no-build`. It compares the freshly generated content with the catalogue recorded during compilation and rejects changes since that build, even when the changed metadata is otherwise valid. Rebuild before packing modified content. Run `pwsh ./eng/Test-Content.ps1` for the small negative fixture, and after a Release build run `pwsh ./eng/Test-Content.ps1 -Pack` to check both incomplete and stale content through the real pack target. These use temporary copies and never edit authored recipes or publish packages.

Build metadata checks do not prove compilation or execution. Add new samples to `eng/Verify.ps1` and run their claimed checks before describing them as verified. Include essential configuration and instructions offline. Keep comparisons fair and original. Extending content with a new stable ID/edition requires no host, transport or tool-registration changes.

Shared style and analyzer configuration adapts the MIT-licensed PinguApps Template's generated-app configuration. The [reference map](planning/reference-repositories.md) records the layout, packaging and scenario conventions used. Recipe prose and samples were authored independently; no guide example implementation was reused.
