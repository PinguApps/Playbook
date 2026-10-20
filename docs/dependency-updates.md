# Dependency updates

`renovate.json` extends `local>PinguApps/renovate-config`. Keep that shared
preset: it supplies reviewers, labels, release-age checks, recommended package
groups and SHA pinning. Repository rules add the following boundaries.

- Routine updates can open on Mondays from 08:00 to 18:00, Europe/London time.
  At most two routine PRs can be open, with at most two created per hour.
  Existing PRs can still be refreshed outside that window.
- Updates require manual merging. Major updates also require approval in the
  Dependency Dashboard before Renovate creates a PR.
- Root NuGet package versions can advance. `Microsoft.Extensions.Hosting` stays
  below version 11 while the tool targets .NET 10. Related test packages receive
  grouped minor and patch updates.
- Routine GitHub Action updates are grouped and retain SHA pins. Runner and
  setup-tool versions stay under manual control.
- .NET SDK/runtime versions and every dependency under `samples/` stay under
  manual control. Lock-file maintenance is disabled because the current lock
  files belong to version-specific sample baselines.

Schedules and ordinary PR limits are not security-update guarantees; Renovate's
vulnerability-alert updates and explicit dashboard actions can bypass them.
Excluded dependencies still need deliberate maintenance.

Root `Directory.Packages.props` updates also set `skipArtifactsUpdate`. Renovate's
NuGet artifact updater otherwise restores every descendant project, including
samples with their own package declarations and SDK pins. Disabling sample
updates and scheduled lock-file maintenance does not prevent that restore.
The tool and tests have no committed lock files, so CI performs their normal
restore/build checks instead. If lock files are added under `src/` or `tests/`,
revisit this exemption and arrange lock-file updates for those consumers.

## Updating a baseline

Samples compare specific target frameworks, language versions, SDKs and
FSharp.Core packages. A version change can alter the behaviour being demonstrated
and invalidate `verification-evidence.json`, even when it is only a patch.

Update the relevant sample pins, package lock files, `verification.json`, recipe
requirements and verification declarations together. Coordinate SDK installation
versions in `.github/workflows/_run-tests.yml` and offline image digests in that
workflow and `eng/Test-Offline.ps1` when those baselines change. Run the affected
samples with the intended SDK, refresh their evidence using
`eng/Record-Evidence.ps1`, then run `eng/Verify.ps1` before proposing the change.

When upgrading the tool to a new .NET major, also change the root SDK pin,
`Directory.Build.props`, the Hosting version constraint in `renovate.json`, and
the relevant CI/runtime checks. Keep older comparison samples on their intended
versions unless their verified scope is deliberately changing.

## Configuration checks

Validate changes with Renovate's `renovate-config-validator --strict`, resolving
the shared preset as well as this file. Inspect the effective package rules and
dependency extraction before merging: sample packages, SDKs and runner versions
must be disabled; root NuGet packages and Action references must remain enabled.
Do not validate a flattened copy as a substitute for resolving the real preset.

After configuration changes reach the default branch, inspect the next hosted
Renovate run and Dependency Dashboard. Proposed updates must not include a .NET 9
sample upgrade to .NET 10 or sample lock-file maintenance. Local extraction and
rule checks do not exercise hosted scheduling, branch creation or PR creation.
