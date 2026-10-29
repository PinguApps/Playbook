# Check the selected SDK for cached security and support warnings

## Goal

Skylark's dispatch application pins its SDK and wants visible warnings about that
SDK's reported vulnerabilities or end of support. Enable the SDK check alongside
an explicit SDK-selection policy. A successful build, or the absence of a warning,
is never a security verdict.

## Requirements and actual artifacts

The SDK performing the build is the subject of the check, independently of the
application's target framework and of NuGet package auditing. This edition serves
target 11 only. Its net10.0 comparison adds no historical foundation or down-level
SDK11 route. There is no provider, workload, native compiler or rendering dependency.
Windows x64 is the executed platform; other platforms need their own qualification.

| Layer | Older comparison | Current example |
| --- | --- | --- |
| SDK | 10.0.402 | 11.0.100-rc.2.26504.105 |
| C# compiler / language setting | 5.9.0-1.26505.111 / 14.0 | 5.12.0-1.26504.105 / 15.0 |
| Framework, reference pack and runtime | net10.0 / 10.0.13 | net11.0 / 11.0.0-rc.2.26504.105 |
| Build task | No SDK vulnerability target/task | Inbox Microsoft.NET.Build.Tasks and SdkVulnerabilityCheck.targets |

These exact test pins do not mean a future SDK is automatically qualified. The
supported prerequisite is an SDK shipping the documented check; use the complete
official distribution, not a copied task DLL. RC2 evidence is not GA proof.

## Older approach

Keep the selected SDK explicit in `global.json`, with version **10.0.402** and
`rollForward: "disable"`. Inspect the resolved `dotnet --version`, supported
release policy and official security advisories separately. SDK10 in this
comparison has no `CheckSdkVulnerabilities` task; setting an unknown MSBuild
property succeeds silently and adds no protection. Existing package auditing
remains useful for its own package scope.

Save this project as `SkylarkServicing.csproj` under `samples/net10.0/SkylarkServicing`:

{{snippet:older}}

## Newer approach

Use SDK **11.0.100-rc.2.26504.105**, `rollForward: "disable"`, the supported C#15
setting and the original dispatch program below. Save the project under
`samples/net11.0/SkylarkServicing`. Both projects link the same source at
`samples/net10.0/SkylarkServicing/Program.cs`.

{{snippet:newer}}

{{snippet:program}}

The opt-in is `CheckSdkVulnerabilities=true`. In the measured RC2 build it defaults
to false. Actual warnings are **NETSDK1238** for vulnerabilities and **NETSDK1239**
for end of life; the source draft's NETSDK1236/1237 numbers are superseded.
Feature-band discontinuation is a separate NETSDK1240 warning. Do not suppress
SDK warnings merely to obtain a green build; a warnings-as-errors policy can fail
the build and should be chosen deliberately.

The task reads
`<DOTNET_CLI_HOME>/.dotnet/sdk-vulnerability-cache/sdk-status-<resolved-sdk>.json`.
Without that environment override, it uses the user's .dotnet directory. The CLI
populates advisory data separately; the build task reads disk and makes no network
request. A restore entry point may also start the CLI's background updater, which
does not make the MSBuild task a live advisory client.

## Migration

1. Pin SDK selection in the application's `global.json`. Run its absolute
   `dotnet.exe --version` from the project directory and verify the selected SDK.
2. Add the shown property to the current project; keep NuGetAudit configured for
   package vulnerabilities separately. Save the linked program at the stated path.
3. Run `dotnet restore SkylarkServicing.csproj` and
   `dotnet build SkylarkServicing.csproj -c Release`. For a security decision, obtain
   current official advisory/support information and verify its coverage and time.
4. For deterministic authoring checks, use a disposable child-process
   `DOTNET_CLI_HOME` with `DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=0`. The helper's synthetic
   cache has PascalCase `IsEol`, optional `EolDate`, `Cves` entries containing `Id`
   and `Url`, `LatestSdkVersion` and `FeatureBandDiscontinued`. It deliberately uses
   fictional CVE-2099-00001, never a statement about this SDK's real vulnerability.
5. Exercise missing, clear, vulnerable, EOL, stale and corrupt files through
   `dotnet msbuild SkylarkServicing.csproj -restore -t:Build -p:CheckSdkVulnerabilities=true`.
   That entry point avoids the CLI restore command's separate background updater.
   The supplied helper also disables package audit for this isolated check and
   uses unusable proxies. It never edits a real user cache.
6. Check expected diagnostics and unchanged cache bytes. Restore normal advisory
   acquisition for real use; retain the selected SDK, data provenance and data age
   when deciding whether to upgrade. Rebuild the application after changing SDKs.

## Constraints

Missing or corrupt metadata causes a silent skip. A clear controlled file only
proves deserialization of that input. The task does **not** reject an old cache by
timestamp: a vulnerability fixture dated 2000 still warns. Thus a clean-looking,
missing or stale cache cannot establish current security or support status.
Do not create a clean cache as a substitute for advisory acquisition.

No blanket warning suppression,
network-isolation or live-advisory freshness claim is made. SDK11 GA artifacts and
PIN-959/PIN-963 remain separate gates.

## Evidence

`eng/Test-SkylarkBuild.ps1 -Record` compiles and executes both real projects with
their actual reference/runtime sets. It runs actual restore/build against the six
controlled cache states. Current vulnerable/stale cases warn NETSDK1238, EOL warns
NETSDK1239, and missing/clear/corrupt cases do not warn. The older SDK emits no
such warnings even when the property is set. Cached bytes remain unchanged with
unusable proxies. This combines source-confirmed disk-only task behavior with
proxy-independent execution; it does not establish OS-level network isolation.

Fresh sample records retain exact commands, inputs, resolved dependencies, compiler
and SDK identities. Ordinary Playbook build/pack checks freshness; installed stdio
retrieval checks full Markdown, snippets, target11/edition11 and applicability.
See the official pinned task source, global.json documentation and support-policy
references supplied with this recipe. Security advisories must be checked afresh
before making a real security decision.
