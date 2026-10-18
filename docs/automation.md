# Repository automation

The six GitHub Actions roles follow the examples in `V:/Aspire.Hosting.Dashboard.Railway`, `V:/Aspire.Hosting.Upstash.Redis`, `V:/RazorStyle` and `V:/CodeStyle.Analyzers`. Product-specific Aspire setup is omitted. Actions are SHA-pinned and Linux jobs use `blacksmith-2vcpu-ubuntu-2404`.

- `_run-tests.yml` builds the independently pinned samples, builds/packs the host with the caller's version, tests the installed package and repeats those scenarios in a network-disabled container. It uploads TRX results, the actual package and its SHA-256 manifest.
- `pr-validation.yml` calls that verification with `0.0.0-ci.<run-number>` and cancels superseded runs. GitHub test comments/check reporting is disabled for forks.
- `pr-labels.yml` applies the PR template's category/version labels. Its `pull_request_target` job never checks out or executes PR code.
- `release-drafter.yml` updates the draft after main changes, starting the first stable draft at `1.0.0`, then using the usual major/minor/patch labels.
- `publish.yml` gets the version from the supplied release tag, calls the same package verification against that tag, downloads and checks that exact artifact, then publishes it using NuGet OIDC. It does not rebuild or repack after verification.
- `publish-release-draft.yml` retains the agreed Sunday 18:17 UTC schedule and any-new-main-commit behavior. It requires an explicit activation variable, a prior published stable release, and one newer stable draft. Preview releases cannot unlock weekly stable publishing.

The production project declares no package version. `eng/Verify.ps1` has a local development default; Actions passes its CI/release version explicitly through build and pack.

## Owner-controlled publication setup

No publication or schedule activation was performed while implementing these workflows. Before the first public release, complete the catalogue/client/evidence gates in the [release plan](planning/release-plan.md).

Configure the `NUGET_USER` Actions secret with the nuget.org profile name, and a NuGet trusted-publishing policy for owner `PinguApps`, repository `Playbook`, workflow file `publish.yml`, with no environment. These match the [official NuGet setup](https://learn.microsoft.com/en-us/nuget/nuget-org/trusted-publishing). A live credential exchange and NuGet push have not been tested by local package checks.

After the first manual stable publication and a fresh organization schedule check, set the Actions repository variable `ENABLE_WEEKLY_PUBLISH` to `true` to activate weekly publishing. It is unset by default. Draft preparation can run before activation; it does not publish a package.

Repository settings: [Actions secrets and variables](https://github.com/PinguApps/Playbook/settings/secrets/actions), [NuGet trusted publishing](https://www.nuget.org/account/trustedpublishing), [workflows](https://github.com/PinguApps/Playbook/actions).
