# Repository automation

The six GitHub Actions roles follow the examples in `V:/Aspire.Hosting.Dashboard.Railway`, `V:/Aspire.Hosting.Upstash.Redis`, `V:/RazorStyle` and `V:/CodeStyle.Analyzers`. Product-specific Aspire setup is omitted. Actions are SHA-pinned and Linux jobs use `blacksmith-2vcpu-ubuntu-2404`.

- `_run-tests.yml` builds the independently pinned portable samples and packs the host once with the caller's version on Blacksmith Linux. Two digest-pinned, network-disabled containers run the focused installed-package suite with only .NET 10 or only .NET 11 RC installed. Each Windows/macOS runner downloads that same package and the precompiled tests, then runs exactly one `HostSmoke` scenario; neither builds samples nor repacks. The unpublished contract fixture is separate from the shipped package.
- `pr-validation.yml` calls that verification with `0.0.0-ci.<run-number>` and cancels superseded runs. GitHub test comments/check reporting is disabled for forks.
- `pr-labels.yml` applies the PR template's category/version labels. Its `pull_request_target` job never checks out or executes PR code.
- `release-drafter.yml` updates the draft after main changes, starting the first stable draft at `1.0.0`, then using the usual major/minor/patch labels.
- `publish.yml` gets the version from the supplied release tag, calls the same package verification against that tag, downloads and checks that exact artifact, then publishes it using NuGet OIDC. It does not rebuild or repack after verification.
- `publish-release-draft.yml` retains the agreed Sunday 18:17 UTC schedule and any-new-main-commit behavior. It requires an explicit activation variable, a prior published stable release, and one newer stable draft. Preview releases cannot unlock weekly stable publishing.

The production project declares no package version. `eng/Verify.ps1` has a local development default; Actions passes its CI/release version explicitly through build and pack.

## Packed-host validation

Every consumer checks `package.sha256` before installation. Each scenario uses a fresh local feed, NuGet cache, tool store and unrelated working directory, then compares the installed assembly with the assembly in the supplied package. The desktop scenario initializes real stdio MCP, discovers both tools, retrieves discovery and complete Markdown/applicability, verifies embedded snippets, and closes cleanly. No workloads, native samples, UI, devices, emulators or client-by-OS checks run in these lanes.

`eng/Test-Package.ps1` rejects mixed-major runtime inventories. The installed server writes a host trace, and its actual CoreCLR path must match the lane's expected major. The test runner's `dotnet exec --roll-forward Major` applies only to the runner; the installed command uses the package's own `Major` policy without a `DOTNET_ROLL_FORWARD` override. The .NET 11 lane explicitly permits prerelease runtime selection with `DOTNET_ROLL_FORWARD_TO_PRERELEASE=1`; that is RC evidence, not GA evidence. [Host tracing documentation](https://learn.microsoft.com/dotnet/core/tools/dotnet-environment-variables#dotnet_host_trace).

Linux TRX files, server traces and package/runtime identity records are retained in `test-results`; desktop equivalents are in `smoke-Windows` and `smoke-macOS`. `playbook-package` contains the ordinary package and its manifest; `playbook-consumer-suite` contains the compiled tests, not another product. The final job summarizes all four TRX files and reports elapsed validation time alongside the sum of all three validation runners' full job durations. Its own reporting-job duration is explicitly excluded from that in-run subtotal; include it when auditing the completed workflow through the [jobs API](https://docs.github.com/rest/actions/workflow-jobs). Execution seconds do not establish billed cost: provider rates, discounts and rounding need separate billing evidence. See [PIN-673 verification](verification/PIN-673.md) for actual measurements and copyable commands.

## Owner-controlled publication setup

No publication or schedule activation was performed while implementing these workflows. Before the first public release, complete the catalogue/client/evidence gates in the [release plan](planning/release-plan.md).

Configure the `NUGET_USER` Actions secret with the nuget.org profile name, and a NuGet trusted-publishing policy for owner `PinguApps`, repository `Playbook`, workflow file `publish.yml`, with no environment. These match the [official NuGet setup](https://learn.microsoft.com/en-us/nuget/nuget-org/trusted-publishing). A live credential exchange and NuGet push have not been tested by local package checks.

After the first manual stable publication and a fresh organization schedule check, set the Actions repository variable `ENABLE_WEEKLY_PUBLISH` to `true` to activate weekly publishing. It is unset by default. Draft preparation can run before activation; it does not publish a package.

Repository settings: [Actions secrets and variables](https://github.com/PinguApps/Playbook/settings/secrets/actions), [NuGet trusted publishing](https://www.nuget.org/account/trustedpublishing), [workflows](https://github.com/PinguApps/Playbook/actions).
