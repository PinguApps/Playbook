# Reference repository findings

Read-only local inspection on 15 October 2026. These observations describe the checked-out files, including several branches other than `main`. No remote fetch, build, package install, test execution, or publishing was performed.

The later [GitHub scheduling audit](publishing-schedule.md) covers the current default branches of all 35 organization repositories. Use that audit for publishing times; the local branch observations below are not the current schedule authority.

## Proposed source map

- **Structure and shared MSBuild configuration:** `V:\CodeStyle.Analyzers\AGENTS.md`, `Directory.Build.props`, `Directory.Build.targets`, and `Directory.Packages.props`.
- **Current PinguApps style and analyzer consumption:** `V:\Template\content\PinguApps.Template\Directory.Build.props`, `.editorconfig`, and `BannedSymbols.txt`. Use the generated application as the reference; Template's root packages a project template.
- **NuGet workflows and packed-consumer verification:** `V:\BlazorSitemap\.github\workflows\` and `eng\Verify.ps1`.
- **PinguApps .NET-tool packaging:** `V:\RazorStyle\src\PinguApps.RazorStyle.Cli\PinguApps.RazorStyle.Cli.csproj`.
- **Behavior scenario organization:** `V:\Aspire.Hosting.Upstash.Redis\tests\Aspire.Hosting.Upstash.Redis\README.md`.
- **MCP mechanics:** `V:\Roslynk\Source\App\Morris.Roslynk.Mcp\` and tool registrations under `Source\App\Morris.Roslynk\`.

This source map is a proposal for Pingu to approve. Matching infrastructure does not mean copying package identities, namespaces, domain-specific jobs, or runtime requirements blindly. No changes to the reference repositories are in scope.

## Shared PinguApps package conventions

The five package repositories are Dashboard.Railway, Upstash.Redis, BlazorSitemap, CodeStyle.Analyzers, and RazorStyle.

- `.slnx` solution, `src/`, `tests/`, and shared root MSBuild files.
- Central package management with versionless `PackageReference` declarations and `Directory.Packages.props`; package-level version overrides disabled.
- Nullable reference types, implicit usings, deterministic builds, code-style enforcement, and warnings as errors.
- Small project files; common test dependencies selected using `IsTestProject`.
- Reqnroll/Gherkin with xUnit v3. Template specifically requires feature scenarios rather than handwritten Fact/Theory tests.
- UTF-8, LF, four-space C#/Razor, two-space configuration/XML, file-scoped namespaces, and explicit variable types.
- PinguApps authors/company, MIT metadata, packaged README, GitHub source/project metadata, and usually SourceLink.
- `NuGet.Config` clears inherited sources and adds nuget.org.
- Most use `renovate.json` extending `local>PinguApps/renovate-config`.
- PR template categories: feature, bug, docs, security, meta, patch, minor, and major, followed by a summary.

## GitHub Actions

All five package repositories have the same six workflow roles:

1. `_run-tests.yml`: reusable/manual Release test execution, TRX results, summaries, and uploaded artifacts.
2. `pr-validation.yml`: PR-triggered validation calling reusable tests, with concurrency cancellation for superseded runs.
3. `pr-labels.yml`: labels from selected PR template categories.
4. `release-drafter.yml`: update release drafts on `main` pushes or manual dispatch.
5. `publish.yml`: publish NuGet artifacts from a GitHub release/tag.
6. `publish-release-draft.yml`: scheduled/manual publication of an existing release draft, followed by package publication.

Shared infrastructure includes `blacksmith-2vcpu-ubuntu-2404`, actions pinned to commit SHAs with major-version comments, and setup-dotnet reading `global.json`.

Release Drafter resolves `v$RESOLVED_VERSION` from major/minor/patch labels, defaulting to patch. Release categories group security, features, bugs, documentation, dependencies, and maintenance. Publishing checks out the release tag, derives the package version, packs/verifies, and pushes to nuget.org with duplicate skipping.

The weekly workflow is actual publication, not just draft preparation. It checks for new `main` commits after an existing published release, publishes a draft, and explicitly invokes reusable NuGet publishing. It does nothing before an initial published release or without an available draft. This matters for the release-policy decision.

### Modern publishing reference

BlazorSitemap's `publish.yml` validates the release tag, runs `eng/Verify.ps1 -PackageVersion <version>`, uploads package artifacts, obtains temporary credentials through `NuGet/login` with `secrets.NUGET_USER` and `id-token: write`, then publishes packages and symbols.

Its verification script packs first, creates an isolated local package feed and cache, and restores real consumers against the newly built artifacts before running behavior scenarios. That is the useful verification boundary for Playbook: eventually install and launch the packed tool, then exercise MCP over its real process transport. Project-reference and in-process tests alone do not prove the delivered package works.

### Differences requiring deliberate selection

- **Credentials:** BlazorSitemap and Dashboard use NuGet OIDC trusted publishing. Upstash, CodeStyle, and RazorStyle use a persistent `NUGET_API_KEY`.
- **Publishing checks:** BlazorSitemap and Aspire package checks exercise packed consumers. CodeStyle/RazorStyle's publishing jobs build and pack without running tests in that job.
- **Scheduled publication:** cron times differ. Newer implementations also reject multiple drafts and non-increasing stable tags.
- **PR labeling:** BlazorSitemap/Aspire use `pull_request_target` without checking out PR code; CodeStyle/RazorStyle use `pull_request`.
- **Fork reporting:** newer workflows disable GitHub checks/comment reporting for fork PRs. Test/report permissions differ accordingly.
- **Style authority:** BlazorSitemap has a minimal `.editorconfig` and some class primary constructors. It is the packaging reference, not the complete style reference.
- **Analyzer delivery:** Aspire repositories embed `build/PinguApps.CodeAnalysis`; Template consumes published `PinguApps.CodeStyle.Analyzers` (1.3.1 in this checkout).
- **Analyzer level:** Template uses `latest-all`, MSBuild warnings as errors, and banned-API analysis. The package repositories generally use `latest-recommended`.
- **Constructors and expressions:** Template/CodeStyle/RazorStyle forbid class primary constructors and prefer switch expressions. Aspire references disable that preference and prohibit certain directly returned/tuple switch expressions.
- **Test runner:** most select Microsoft.Testing.Platform. RazorStyle uses `xunit.v3.mtp-off` and VSTest-style arguments.
- **Transitive pinning:** explicitly enabled in the other four package repositories, not in BlazorSitemap.
- **SDK/runtime:** references currently pin SDK 10.0.401 or 10.0.402 and predominantly target `net10.0`; these are reference facts, not a Playbook compatibility decision. BlazorSitemap explicitly selects C# 14; several others use `latest`.

Template's root CI includes application/deployment-specific suites. Those jobs do not establish a requirement for this MCP package.

## Roslynk mechanics

### Useful implementation reference

- `Source/App/Morris.Roslynk.Mcp/Morris.Roslynk.Mcp.csproj`: `PackAsTool`, explicit tool command/package identity, packaged README and metadata, and a local version overridden by CI.
- `Source/Directory.Packages.props`: central package versions; `ModelContextProtocol` and `ModelContextProtocol.AspNetCore` are 2.2.0 in this checkout. This does not establish the latest or required Playbook SDK version.
- `Source/App/Morris.Roslynk/McpToolsRegistration.cs`: SDK assembly discovery and registration.
- `Source/App/Morris.Roslynk/Features/Solutions/OpenSolution/OpenSolutionTool.cs`: stable tool names, descriptions, argument descriptions, annotations, and constructor injection.
- `Source/App/Morris.Roslynk.Mcp/McpServerRegistration.cs`: server identity/version and shared server instructions.
- `Source/App/Morris.Roslynk.Mcp/Hosting/StdioBridge.cs`: stdio host with protocol-clean stdout. Logs and startup banners must not corrupt JSON-RPC output.

### Architecture to evaluate rather than inherit automatically

Roslynk is a `net10.0` Web SDK tool. Its default process is a persistent HTTP MCP daemon. `roslynk stdio` starts a bridge that ensures the daemon exists, connects over HTTP, and forwards tool listing and calls over stdio. The daemon preserves expensive shared Roslyn workspaces.

Its bridge forwards tools, not arbitrary MCP resources/prompts. Its launcher probes a configured loopback port, but a listening port alone does not establish server identity/version. Detached startup, idle workspace eviction, HTTP hosting, Windows-service integration, Roslyn/MSBuild dependencies, custom response budgets, and workspace snapshot machinery address Roslynk's domain. Their value for bundled guidance must be established separately.

Roslynk specifies `RollForward=LatestMajor` and has no `global.json` in the inspected tree. Do not silently inherit that runtime policy. Its stdio example uses an empty application builder with no logging providers.

The README documents global installation, `roslynk stdio`, and `dnx Roslynk --yes -- stdio`. The essential UX requirement is agreement between documented command arguments and the actual transport started.

### Verification evidence

Roslynk's MCP tests check the real DI registrations, published tool names, argument/schema shape, descriptions, and SDK-bound tool calls. The invocation tests use an in-process server with `NullTransport`.

No packed-tool installation, launched stdio initialize/list/call handshake, daemon lifecycle, or real HTTP integration test was found. No tests were run during this inspection. Preserve useful tests and add verification at the eventual Playbook package boundary rather than claiming Roslynk proves that boundary already.

### Source reuse

Roslynk's `LICENCE` is MIT, copyright 2026 Peter Morris. It requires preserving the copyright and permission notice with copies or substantial portions. If source is adopted, retain the relevant notice and record provenance. The reference is not permission to remove its attribution or assume dependencies share the same terms.

## Local provenance

- Dashboard: branch `agent/readme-nuget-badges`, commit `2a75d15fa467b0651ca01ffdd08ad7ebcae391b7`.
- Upstash: branch `main`, commit `d7bc56b79f24461b4e4a0e8fb9f64c544450c32f`.
- BlazorSitemap: branch `feature/blazor-sitemap`, commit `88cdd5bdc15ff5cb73de7356479786ab7c221376`.
- CodeStyle: branch `agent/weekly-publish-release-draft`, commit `473c632fcaf24d49a1c4623c8e45e6dd1c0db163`.
- RazorStyle: branch `main`, commit `144cf2969807e09c8ed378edbd3cca1417593421`.
- Template: branch `agent/trim-pr-test-suite`, commit `8d295fd9a1b9f3dc25b500595ac32053fed4d020`; an untracked generated-app Media.Processor `Properties/` directory was present.
- Roslynk: commit `788735a12c91d8a603c5de50474b96a7424a2007`; checkout clean at inspection.

The other listed checkouts were clean. No correspondence with current remote `main` was established.
