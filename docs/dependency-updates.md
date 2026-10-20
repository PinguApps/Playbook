# Dependency updates

`renovate.json` extends `local>PinguApps/renovate-config`. Keep the shared preset
for reviewers, labels, release-age checks, package groups and SHA pinning. Local
rules permit only explicitly reviewed updates; new packages, locations and
package managers default to manual maintenance.

## Automatic suggestions

- Only the listed root NuGet packages may advance, within their explicit major
  release ranges. These are dependency-specific constraints, not a claim that
  package major numbers always equal target-framework versions.
- Only the listed Action references in the listed workflows may advance. SHA
  pins are retained, and routine Action updates are grouped. Floating branch refs,
  including the current `harupy/auto-labeling` `master` ref, stay manual.
- Root test packages receive grouped minor and patch updates. Root NuGet updates
  skip artifact regeneration because the tool and tests have no committed lock
  files; normal CI still restores, builds and exercises those consumers.
- Routine branches can open on Mondays from 08:00 to 18:00, Europe/London time,
  with two open PRs and two new PRs per hour. Existing PRs can be refreshed outside
  that window; explicit dashboard actions can bypass ordinary schedules/limits.
- Merging remains manual. A separate Renovate Update Assistant may have its own
  merge policy; `automerge: false` controls Renovate itself.

Major upgrades, replacements, rollbacks, pre-1.0/preview dependencies, SDKs,
runtimes, runner/setup-tool versions, sample dependencies, lock-file maintenance
and configuration migrations require deliberate changes. Adding a package to
the root central file does not automatically grant it update permission. Moving
a known dependency to another package file also removes its permission.
Explicit NuGet pins, ranges and wildcard versions also stay manual; the root
permissions apply only to the reviewed bare stable package versions.

Renovate's vulnerability-fix PR mode and OSV mode are disabled because their
security-specific version/rule overrides can supersede ordinary bounds. This
does not change GitHub's security-alert settings. Review security alerts and
perform urgent or out-of-range fixes deliberately, including for pinned samples.
Permitted ordinary updates can still include security fixes within their bounds.

## Changing projects and their dependencies

For a new, moved, retargeted or otherwise modified project:

1. Decide whether it is a maintained tool/test consumer or a version-specific
   example. Inspect its effective shared build files, central packages, SDK pin,
   tools, workloads, runtime/container assets and lock-file requirements.
2. Keep examples inside `samples/` with a package boundary isolated from the root.
   Their packages, SDKs, providers and experimental/native toolchains remain
   author-maintained even if an update is described as compatible or secure.
3. For an automated consumer, add narrow package/file permissions and explicit
   compatible version ranges to `renovate.json`. Update the independently checked
   release expectations in `eng/Test-Renovate.mjs` and the reviewed root-consumer
   inventory in `eng/Test-DependencyPolicy.ps1`. Define artifact handling before
   adding tool/test lock files. Revisit framework-specific bounds when retargeting.
4. Run both checks below. Verify intended updates are enabled, protected versions
   stay disabled, and actual affected consumers restore/build/run with the proposed
   versions. Record the policy decision and verification in the project-change PR.

An unknown project outside `samples/`, a changed root consumer/package boundary,
a root target other than .NET 10, or a tool/test lock file fails the inventory
check until its policy is reviewed. This guards conventional project inheritance;
custom MSBuild imports/conditions still require actual evaluated consumer checks.

The remaining [Playbook backlog](https://linear.app/pinguapps/project/playbook-18c41f341c73)
includes EF/providers, Aspire/Gateway, file-based apps, containers, desktop/mobile
workloads and RC-to-GA reconciliation. These stay manual by default as they arrive.
For example, [PIN-863](https://linear.app/pinguapps/issue/PIN-863) explicitly
distinguishes Microsoft.Data.Sqlite 11's .NET 10 consumer from EF Core 11's .NET 11
requirement. [PIN-695](https://linear.app/pinguapps/issue/PIN-695) requires exact
mobile composition, and [PIN-963](https://linear.app/pinguapps/issue/PIN-963) requires
new GA evidence rather than relabelling RC proof. Broad Microsoft/EF/Aspire/native
package wildcards would undermine these distinctions.

## Updating a sample baseline

Update sample pins, lock files, `verification.json`, recipe requirements and
verification declarations together. Coordinate SDK installation versions in
`.github/workflows/_run-tests.yml` and offline image digests in that workflow and
`eng/Test-Offline.ps1` when those baselines change. Run the affected samples with
the intended SDK, refresh evidence using `eng/Record-Evidence.ps1`, and run
`eng/Verify.ps1`. Preserve older comparison editions unless their scope changes.

Renovate's NuGet artifact updater can restore every descendant of the root central
package file, including samples with separate package declarations and SDK pins.
Disabling sample updates or scheduled maintenance alone does not stop that path;
the root `skipArtifactsUpdate` exemption prevents it. Revisit the exemption if
tool/test lock files are introduced.

## Policy checks

CI runs the real Renovate rule checks before the package build and the inventory
check through `eng/Verify.ps1`. The rule check resolves the current shared preset,
extracts actual dependencies and checks synthetic future-project, dependency,
release-line and update-type cases. It uses Renovate's own version matching,
rule evaluator and branch generation, not a substitute implementation.

From PowerShell 7, with Git installed:

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a44d23d2'
& ./eng/Test-DependencyPolicy.ps1
& ./eng/Test-Renovate.ps1
```

Expect two `PASS` summaries and strict configuration validation without errors.
The wrapper reuses Node.js 24.11+ in the Node.js 24 release line when available;
otherwise it downloads portable Node.js 24.21.0 and verifies the archive against
the official SHA-256 manifest. CI passes `-IsolatedNode` to exercise this setup
regardless of the runner's installed runtime. No system installation is required.
The wrapper installs pinned Renovate 44.172.1 in a unique temporary directory,
disables npm lifecycle scripts, installs the published RE2 artifact without a
compiler fallback, and invokes tools by absolute path without changing PATH.
An existing 44.172.1 installation can be supplied with `-RenovateRoot` to avoid setup.
Network access is needed for setup and shared-preset resolution.

After a policy change reaches the default branch, inspect the next hosted run
and dashboard for unintended proposals. These checks constrain suggestions; they
cannot prove that a dependency publisher's patch has no behaviour changes, or
that untested custom build logic is compatible. Keep consumer CI meaningful.
