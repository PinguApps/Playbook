# GA SDK and package-authoring reconciliation

PIN-963, 11 November 2026. Bounded audit; draft disposition pending review.
This does not close PIN-967 or authorize publication.

## Authority and actual artifacts

Scope follows SPEC.md at `0a8989814ea11346354464b845c79c15bb71c5e9`,
stories 41–44, 47, 93–95, 130, 132–133, 146 and 158–160; approved proposal
`b3778cc` (`GA_BUILD`), ADRs 0001–0004 and the release plan. Root PLAN.md,
CONTEXT.md and CONTEXT-MAP.md are absent. Explicit source pointers resolve.
Native delivery inputs are Done or deliberately Canceled; cancellation does
not establish execution. No related ticket has been changed by this audit.

The [release feed](https://builds.dotnet.microsoft.com/dotnet/release-metadata/11.0/releases.json)
and [GA notes](https://github.com/dotnet/core/blob/main/release-notes/11.0/11.0.0/11.0.0.md)
identify SDK 11.0.100/runtime 11.0.0, released 10 November. A fresh win-x64
SDK archive was downloaded and its SHA512 checked against that feed. The
10.0.403 comparison archive was separately checked against its release feed.
Archives were extracted into ignored artifacts without installing tools.

The GA source identity is `297acf930fb0f5c07fd4e51d78e521b4ad2e5d1f`.
Historical RC2 source identity is `820230c386db5b0106c5430b3e5aee92a7826bf5`;
the existing SDK metadata register retains RC3 rather than RC2 tooling.
These distinct baselines are intentional and must not be relabelled.

## Observed comparison

The adjacent generated register binds all 56 prepared edition11 SDK recipes
by ID, title, stability, requirements and metadata/Markdown hashes. It compares
nine unique retained VMR source files from the SDK-build and package-host
registers, checking their original hashes before fetching GA. Eight are identical.
VSTest Message.cs changes its string representation to redact secret properties.

A fresh metadata replay inspected 52 tooling assemblies per SDK, 230/232 build
files, and assigned all 2,689 old/current API/XML differences to existing rules.
The generated register retains the RC3-to-GA change in that comparison, rather
than committing the full disposable metadata dump. Whole-file hashes and API
declarations are different kinds of evidence; neither establishes execution.

| Delta | Draft disposition |
| --- | --- |
| VSTest Message.ToString redacts JSON properties | Existing `testing.migrate-vstest-message-readers` says the output is unchanged raw text. Prepare the narrow correction below. RawMessage remains a separate API. |
| IVsTestConsoleWrapperAsync obsolete | Existing session sample uses IVsTestConsoleWrapper. Do not introduce the obsolete alias. |
| EqtHash.GuidFromStringXxHash128 marked VSTEST001 | Retain experimental status; public visibility alone does not establish an intended new developer goal. Needs final documented-scenario assessment before audit completion. |
| ResolveAssemblyReference cancellation interface | SDK task implementation capability; not by itself a new application recipe. |
| Optimized publishing defaults false and hook changes | Current `sdk.migrate-optimized-publish-hooks` already explicitly describes the GA default and opts in for its failure control. Preserve actual native receipts and their limits. |
| PackTool shim directory and publish integration | Requires final assessment against existing package-tool and custom native-RID samples. Private target names alone are not supported authoring hooks. |
| Bundled versions, quoted VSTest conditions, guarded ProjectData import | Retain exact version/input identities; implementation changes are not independently executed consumer evidence. |
| Static web assets/browser Hot Reload targets | Requires reconciliation with web-family ownership; do not silently claim framework behavior from XML inspection. |

Canonical [SDK notes](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-11/sdk)
and [compatibility index](https://learn.microsoft.com/en-us/dotnet/core/compatibility/11)
remain discovery sources. The latter expressly disclaims completeness. Final
note/package dependency reconciliation and dispositions remain pending; this
draft is not a completed family audit or proof of every recipe's behavior.

## Prepared follow-on draft

As a developer reading VSTest messages, I want the guidance to distinguish raw
protocol data from redacted diagnostic text so that logging does not depend on
an incorrect promise of unchanged output.

Proposed outcome: correct `testing.migrate-vstest-message-readers` and add a
secret-bearing envelope control to its existing real-package harness. Verify
RawMessage preserves the wire envelope, ToString redacts the relevant properties,
ordinary typed payload decoding remains correct, and the old/current controls
are explicitly distinguished. Refresh affected evidence and inspect installed
MCP guidance. Preserve ID, fictional domain and existing protocol boundaries.

Estimate **S (2)**: bounded wording/test/evidence refresh with actual package
execution. Executor Agent; existing build/deployment milestone; cycle pending
Pingu's explicit choice. Source: PIN-963 and the immutable Message.cs comparison
in the adjacent register. Native ticket allocation, overlap refresh and creation
remain pending. Add it as a prerequisite to the final stable candidate gate;
do not reopen or close the completed original delivery merely from this finding.

## Verification so far

`Inspect-GaSdkContracts.ps1` passed: 9 sources, 56 SDK editions, 52 tooling assets.
`Inspect-SdkBuildApis.ps1` passed against isolated fresh archives: 52/52
assemblies, 230/232 build files, 2,689 assigned differences. Installed .NET 10
SDK inspection failed on missing inspection-library/return metadata; the fresh
archive replay replaces that failed scan, without modifying the shared SDK.
No package execution, native platform execution or global stable check is claimed.
