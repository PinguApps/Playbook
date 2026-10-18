# Verify and extend the first slice

Build prerequisites: PowerShell 7, .NET SDK **10.0.402** and **9.0.319**. The .NET 9 SDK is needed only for the historical comparison, never for installing or running Playbook. No optional workloads are used.

```powershell
Set-Location <path-to-Playbook>
pwsh ./eng/Verify.ps1
```

If .NET 9 is installed in a separate directory, pass its executable with `-Net9Dotnet <path-to-dotnet>`. The script compiles and executes both real target-framework samples, builds the solution with analyzers/warnings as errors, packs once, then tests that artifact. Its SHA-256 identifies the tested package.

The two focused Reqnroll scenarios install from a local-only feed with a new cache under an unrelated temporary directory, exercise bare/help/version, initialize stdio, check exactly two read-only tools and schemas, retrieve complete detail, compare the structured response to its JSON fallback and generated snippets to the canonical samples, reject an incompatible target and close stdin. They require a clean exit and stderr diagnostics. They never project-reference the production assembly. Temporary installs are deleted after each scenario.

Local checks set unusable HTTP proxies, which establishes proxy-independent operation but is **not** network isolation. On Linux use `pwsh ./eng/Verify.ps1 -IsolateNetwork`: after build/pack, the installed-consumer tests run under `sudo unshare --net` with no external network routes. Only the local package feed is available. SDK downloads/restores happen before network isolation.

With Docker available, run `pwsh ./eng/Test-Offline.ps1` after verification. It reruns the same two Reqnroll scenarios against the exact package with `--network none`. Only package artifacts and test binaries are mounted; no repository content is accessible. Canonical sample text is embedded independently in the test assembly for comparison. The default image is pinned by digest (.NET SDK 10.0.401, runtime 10.0.12); pulling the image is an online preparation step. This is an offline check, not .NET 11 roll-forward or a broad OS matrix.

## Inspect the development package

From the repository root after verification:

```powershell
dotnet tool install PinguApps.Playbook --version 1.0.0-preview.1 --tool-path ./artifacts/manual-tool --add-source ./artifacts
./artifacts/manual-tool/playbook --help
./artifacts/manual-tool/playbook --version
```

Help must show `playbook mcp`; version must show `1.0.0-preview.1`. This installs locally, without publishing or changing global tools. Delete `artifacts/manual-tool` when finished. To see the actual recipe body and constraints yourself, use [the MCP Inspector walkthrough](inspect-guidance.md). The Reqnroll checks verify real MCP responses and lifecycle results; TRX evidence is under `artifacts/TestResults`.

## Author a recipe edition

Add `content/recipes/<stable-id>/<edition>/recipe.json` and `recipe.md`, following `eng/recipe.schema.json`. Keep IDs goal-based and release-independent. Declare every supported target explicitly; no future-target compatibility is inferred. The initial slice supports canonical declared targets only; aliases, conditional routes, richer errors and discovery filters belong to subsequent issues.

Add independently pinned original comparison samples under `samples/`, targeting the actual previous/current releases. Include exact SDK/language/target evidence and official HTTPS references. Mark canonical source regions with `// snippet:<name>` and `// endsnippet:<name>`, and reference them in Markdown with `{{snippet:<key>}}`. The build validates metadata, paths, evidence baselines and snippet references; it embeds generated JSON and rendered Markdown into the tool assembly. No runtime files or fetches are needed.

Build metadata checks do not prove compilation or execution. Add new samples to `eng/Verify.ps1` and run their claimed checks before describing them as verified. Include essential configuration and instructions offline. Keep comparisons fair and original. Extending content with a new stable ID/edition requires no host, transport or tool-registration changes.

Shared style and analyzer configuration adapts the MIT-licensed PinguApps Template's generated-app configuration. The [reference map](planning/reference-repositories.md) records the layout, packaging and scenario conventions used. Recipe prose and samples were authored independently; no guide example implementation was reused.
