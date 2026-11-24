# Repository automation

The six GitHub Actions roles follow the examples in `V:/Aspire.Hosting.Dashboard.Railway`, `V:/Aspire.Hosting.Upstash.Redis`, `V:/RazorStyle` and `V:/CodeStyle.Analyzers`. Product-specific Aspire setup is omitted. Actions are SHA-pinned. Blacksmith runs the build, Windows x64, macOS Arm64 and Linux Arm64 jobs. GitHub-hosted runners cover Windows Arm64 and macOS x64. The Linux build uses `blacksmith-2vcpu-ubuntu-2404`, Windows x64 smoke uses `blacksmith-2vcpu-windows-2025`, and macOS Arm64 smoke uses `blacksmith-6vcpu-macos-15`. Windows Server 2025 is currently a public-beta runner; macOS 15 uses Apple Silicon M4 (ARM64). These are the smallest documented sizes for each selected OS, and runner versions remain manually maintained. See [Blacksmith's runner inventory](https://docs.blacksmith.sh/blacksmith-runners/overview).

- On pull requests, `pr-validation.yml` cancels superseded runs and uses one Linux job with a ten-minute limit. Package-affecting changes build and pack `0.0.0-ci.<run-number>` and run one `HostSmoke` scenario against the installed package. Allowlisted docs changes run guards and inventory, with their result reported in the check on the PR head. GitHub test comments and check reporting are disabled for forks. Manual dispatch runs the full suite.
- `_run-tests.yml` defaults to full validation for manual runs and release callers. It builds the independently pinned portable samples and packs the host once with the caller's version on Blacksmith Linux. A digest-pinned, network-disabled container runs the focused installed-package suite with SDK 11.0.100 and the .NET 11 GA runtime. The same package set is used for bundled smoke checks across all six targets, plus a portable fallback smoke on Alpine Linux. The unpublished contract fixture is separate from the shipped package. `release_ready` always forces this full path even if a caller requests smoke validation.
- `pr-labels.yml` applies the PR template's category/version labels. Its `pull_request_target` job skips bodies without a possible checkbox and never checks out or executes PR code.
- `release-drafter.yml` updates the draft after main changes, starting the first stable draft at `1.0.0`, then using the usual major/minor/patch labels.
- `publish.yml` gets the version from the supplied release tag, validates against that tag, then downloads and checks the exact package set. It publishes the six platform payloads and the portable fallback using NuGet OIDC, then publishes the pointer package.
- `publish-release-draft.yml` retains the agreed Sunday 18:17 UTC schedule and any-new-main-commit behavior. It requires an explicit activation variable, a prior published stable release, and one newer stable draft. Preview releases cannot unlock weekly stable publishing.

The production project declares no package version. `eng/Test-Smoke.ps1` and `eng/Verify.ps1` have local development defaults; Actions passes its CI/release version explicitly through build and pack.

## Categories and package versions

Select a category and a version checkbox in the PR template. The automatic labeller
reads those checkboxes; Release Drafter groups merged PRs using those labels.
Dependency PRs receive `dependencies` from Renovate rather than a template checkbox.

| Example change | Labels | Draft section | Version after 1.2.3 |
| --- | --- | --- | --- |
| Correct recipe guidance | `docs`, `patch` | Documentation | 1.2.4 |
| Add a compatible recipe or capability | `feature`, `minor` | Features | 1.3.0 |
| Break a public tool contract or change a stable recipe ID | `feature`, `major` | Features | 2.0.0 |
| Fix a defect | `bug`, `patch` | Bug Fixes | 1.2.4 |
| Security correction | `security`, `patch` | Security | 1.2.4 |
| Dependency maintenance | `dependencies`, `patch` | Dependencies | 1.2.4 |
| Repository maintenance | `meta`, `patch` | Meta | 1.2.4 |

The highest version label across merged PRs wins; an absent version label defaults
to patch. Categories do not automatically imply a version increment. Package SemVer
is independent of .NET coverage: adding .NET 11 guidance does not imply package 11.
Before the first stable release, the workflow explicitly requests `1.0.0` for the
stable draft. Early previews are manually chosen `1.0.0-preview.N` prereleases,
subject to the [release readiness gates](planning/release-plan.md); neither a preview
nor a draft authorizes stable publication or activates weekly publishing.

`eng/Test-ReleaseDraft.mjs` exercises the actual pinned Release Drafter parser,
category matcher and version resolver against this repository's configuration.
It uses synthetic merged PRs and makes no GitHub writes. Run the PowerShell wrapper
with Node.js 24 and its bundled npm; it downloads the pinned upstream source and
installs its fixture dependencies with lifecycle scripts disabled.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-747466ca'
& ./eng/Test-ReleaseDraft.ps1
```

Expect seven category/version examples and a final `PASS` summary. The fixture's
npm lock file and downloaded source remain under ignored `artifacts/`; these are
verification dependencies, outside the production package and automatic update
permissions. This checks draft content/version logic locally; hosted labelling
permissions and GitHub release writes require the normal hosted runs.

## Verification output

Use [the contribution check-selection and retention policy](contributing.md#choose-the-check-and-retain-its-result). Routine local output defaults to ignored `artifacts/`; CI keeps its existing artifact uploads. Report commands/results/limits in the PR rather than adding a ticket report. Required sample evidence and exceptional release/product proof remain durable and cannot depend solely on expiring CI artifacts.

## Pull request smoke coverage

`eng/Test-Smoke.ps1` builds both maintained root consumers with analyzers and warnings as errors. Normal build/pack content generation still checks all recipe metadata, canonical snippets and saved evidence freshness. It installs the actual package from a fresh local feed/cache outside the checkout, checks help/version, initializes MCP, discovers all three tools, lists recipes, retrieves one known ID with complete Markdown/applicability, checks canonical snippets, then verifies clean shutdown. Exactly one passing scenario is required; failed, skipped or empty test selections fail the job. Recipe-specific scenarios are reserved for full validation, so adding a recipe does not add another PR test.

PRs that only change the unserved paths listed in `eng/Get-PrValidationScope.ps1` run publication guards and the dependency inventory on the same job, without SDK setup, packing or installation. The PR results check reports those guards directly. The packed README, served recipes, samples, and every other path use installed smoke.

The dependency inventory runs on every PR. The real Renovate rule checks run for workflow, engineering, dependency policy/configuration, project/solution and shared build/package/SDK/tool changes. The classifier verifies the event's merge and parents against the checkout and compares both sides of a rename as separate paths. Uncertain classification runs installed smoke and the Renovate checks. Both manual validation entry points and release validation run the full suite.

The policy check also parses the publishing and validation YAML to check reusable-workflow permission contracts. The release-draft caller must grant `actions: read` through `publish.yml` to verification, even when publication is gated off: GitHub validates the composed workflow before evaluating job conditions. OIDC remains scoped to the publishing job and its reusable caller. See [PIN-1023 verification](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-1023.md) for the reproduced failure and safe hosted validation.

Run the same smoke locally with PowerShell 7 and SDK 11.0.100:

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-4d84c6cb'
& ./eng/Test-Smoke.ps1
```

Expect the final `PASS: built, packed and installed the actual tool` message and one passed scenario in `artifacts/TestResults/smoke.trx`. PR smoke uploads that result and server runtime evidence. Full validation uploads all eight packages and their SHA-256 manifest for the native smoke and publication jobs.

## Packed-host validation

Full-validation artifact consumers check `package.sha256` before installation. Every installed scenario, including PR smoke, uses a fresh local feed, NuGet cache, tool store and unrelated working directory, then compares the installed assembly with the assembly in the supplied package. The smoke scenario initializes real stdio MCP, discovers all three tools, retrieves discovery and complete Markdown/applicability, verifies embedded snippets, and closes cleanly. No workloads, native samples, UI, devices, emulators or client-by-OS checks run in these lanes.

`eng/Test-Package.ps1` rejects mixed-major runtime inventories. The installed server writes a host trace, and its actual CoreCLR path must match the lane's expected major. The test runner's `dotnet exec --roll-forward Major` applies only to the runner; the installed command uses the package's own `Major` policy without a `DOTNET_ROLL_FORWARD` override. The Alpine lane verifies that the portable payload uses the shared .NET 11 runtime. The six bundled smoke checks use an isolated .NET 10 SDK for installation and assert that CoreCLR loads from the expected tool payload with the `DOTNET_ROOT*` overrides cleared. [Host tracing documentation](https://learn.microsoft.com/dotnet/core/tools/dotnet-environment-variables#dotnet_host_trace).

Linux TRX files, server traces and package/runtime identity records are retained in `test-results`; the remaining five targets record the selected package and CoreCLR path in `smoke-<rid>`. `playbook-package` carries the eight packages and their manifest. The final job reports test results, elapsed time and total runner time. Its own reporting-job duration is explicitly excluded from that in-run subtotal; include it when auditing the completed workflow through the [jobs API](https://docs.github.com/rest/actions/workflow-jobs). Execution seconds do not establish billed cost: provider rates, discounts and rounding need separate billing evidence. See [PIN-673 verification](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-673.md) for actual measurements and copyable commands.

## Owner-controlled publication setup

No publication or schedule activation was performed while implementing these workflows. Before the first public release, complete the catalogue/client/evidence gates in the [release plan](planning/release-plan.md).

Configure the `NUGET_USER` Actions secret with the nuget.org profile name, and a NuGet trusted-publishing policy for owner `PinguApps`, repository `Playbook`, workflow file `publish.yml`, with no environment. Grant the policy permission to publish `PinguApps.Playbook` and the seven payload IDs under `PinguApps.Playbook.*`, including first-time package creation. See the [official NuGet setup](https://learn.microsoft.com/en-us/nuget/nuget-org/trusted-publishing).

After the first manual stable publication and a fresh organization schedule check, set the Actions repository variable `ENABLE_WEEKLY_PUBLISH` to `true` to activate weekly publishing. It is unset by default. Draft preparation can run before activation; it does not publish a package.

Repository settings: [Actions secrets and variables](https://github.com/PinguApps/Playbook/settings/secrets/actions), [NuGet trusted publishing](https://www.nuget.org/account/trustedpublishing), [workflows](https://github.com/PinguApps/Playbook/actions).

## Validate package publication safely

Manual `publish.yml` dispatch defaults `validate_only` to `true`. It resolves the
selected tag, runs the full publication checks (including the native smoke lanes),
downloads `playbook-package` from this same run, and checks each of the eight
filenames, SHA-256 hashes and embedded NuGet IDs and versions. NuGet login and push
  are skipped. The selected `preview.N` version uses only the named, input-bound
  preview exceptions; stable versions reject pending proof. Missing required
  catalogue/evidence checks still fail validation. `validate_only` does not change
  that version-specific boundary. Explicitly disabling it is actual
publication and requires separate owner authorization.

Preview tags use `1.0.0-preview.N` and are accepted only by manual dispatch.
Published stable releases must have a stable tag and must not be marked prerelease.
Release Drafter continues to choose independent package SemVer from PR labels.
Publishing uses the current run's verified package set. The pointer is pushed
after all seven payload pushes succeed. A duplicate or failed push stops
publication. If a run partially uploads a version, prepare a new version for
the next release attempt so its pointer cannot expose a mixed set.

A successful reusable validation call requires the Linux suite, Alpine fallback
and all six bundled smoke checks.

Run local negative checks without accounts or publication:

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\agent-788b14ab-a700d1594145d2ef'
& ./eng/Test-Publishing.ps1
# After full verification, inspect the same package set:
& ./eng/Test-Publishing.ps1 -Artifacts ./artifacts/packages/1.0.0-preview.1
```

The default check uses synthetic ZIPs to exercise the guards. Supplying
`-Artifacts` checks the real package set too. These checks do not establish NuGet login,
push, hosted runner success or release readiness. The existing package checks
remain responsible for installed tool behavior. Dispatch the **Publish NuGet
Packages** workflow on the reviewed branch with a release-ready tag containing
these publication checks and `validate_only: true` for safe hosted composition/artifact
validation. Tags predating these scripts cannot use this validation path; their
checkout fails closed rather than skipping the new checks.

For the separate Human trusted-publisher action, retain repository owner
`PinguApps`, repository `Playbook`, workflow filename `publish.yml` and **empty
environment** (the publishing job declares none). Package scope is
`PinguApps.Playbook`; `NUGET_USER` is the nuget.org profile that owns the policy,
not a persistent API key. The nested weekly caller uses this same publishing
workflow and publishes live when its owner-controlled activation and stable-draft
gates pass. `validate_only` applies only to direct manual dispatch; it does not
turn enabled weekly calls into validation runs. Check [NuGet's setup instructions](https://learn.microsoft.com/en-us/nuget/nuget-org/trusted-publishing)
when creating the policy. No account or publisher configuration is performed by
these checks. Weekly activation and first public/stable release gates remain
owner-controlled.
