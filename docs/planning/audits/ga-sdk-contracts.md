# GA SDK and package-authoring reconciliation

## Catalogue dispositions after PIN-1302

The [per-contract register](ga-sdk-dispositions.json) records all 56 original
edition IDs, previous/final stability, official contract sources, selected
independent packages, existing consumer evidence and current content hashes.
The original PIN-963 register remains historical evidence, rather than being
rewritten to suggest that it inspected these corrections.

Final classification is 50 stable, three preview and three experimental.
Twelve of the fourteen original preview labels describe GA SDK contracts or
independently stable packages. Azure Linux4 retains its Beta qualification;
TemplateEngine retains preview qualification because its actual selected NuGet
dependencies are 8.0.0-rc.605. The previously stable-labelled NuGet client host
also becomes preview for that independently distributed RC package. SDK GA
does not graduate those packages. The affected-test preview package pins and
both experimental MSBuild contracts remain unchanged.

Measured current SDK identities are Roslyn 5.12.0-1.26529.118, MSBuild
18.12.2-1.26529.118 and bundled NuGet 8.0.0-rc.53118 at GA VMR 297acf93.
These come from the saved actual artifact pins and GA tooling register; the
portable GA compiler also prints that exact Roslyn version. SDK-bundled NuGet
is distinct from the independently restored 8.0.0-rc.605 package. Requirements
tables now distinguish these identities. This is a content correction, not a
recapture or relabelling of historical execution. Unchanged sample inputs retain
their existing proof; native exclusions, pending Human checks, eight historical
foundations and PIN-967 remain intact.

PIN-1301 remains a separate behavior decision: an actual independent TestHost
18.11.0 consumer retains the synthetic Password value in Message.ToString(),
whereas an isolated probe of the actual GA SDK assemblies preserves RawMessage
and redacts that Password value in ToString(). The SDK probe is artifact
diagnosis, not a supported SDK-DLL acquisition route for the recipe. No logging
guarantee for arbitrary payloads follows from either observation. The combined
change remains unpublished pending resolution and complete behavior proof.

PIN-963, 11 November 2026. Bounded source/artifact release decision register.
This does not close PIN-967 or authorize publication.

## Authority and actual artifacts

Scope follows SPEC.md at `0a8989814ea11346354464b845c79c15bb71c5e9`,
stories 41–44, 47, 93–95, 130, 132–133, 146 and 158–160; approved proposal
`b3778cc` (`GA_BUILD`), ADRs 0001–0004 and the release plan. Root PLAN.md,
CONTEXT.md and CONTEXT-MAP.md are absent. Explicit source pointers resolve.
Native delivery inputs are Done or deliberately Canceled; cancellation does
not establish execution. Follow-ons PIN-1301 and PIN-1302 are created in the same team/project.

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
by ID, title, stability and metadata/Markdown hashes. Requirements and measured
versions are read from the hashed recipe metadata's applicability routes and
Markdown; they are not extracted into a separate register field. It compares
nine unique retained VMR source files from the SDK-build and package-host
registers, checking their original hashes before fetching GA. Eight are identical.
VSTest Message.cs changes its string representation to redact secret properties.

A fresh metadata replay inspected 52 tooling assemblies per SDK, 230/232 build
files, and assigned all 2,689 old/current API/XML differences to existing rules.
The generated register retains the RC3-to-GA change in that comparison, rather
than committing the full disposable metadata dump. Whole-file hashes and API
declarations are different kinds of evidence; neither establishes execution.

| Delta | Disposition |
| --- | --- |
| VSTest Message.ToString redacts JSON properties | Existing `testing.migrate-vstest-message-readers` says the output is unchanged raw text. Prepare the narrow correction below. RawMessage remains a separate API. |
| IVsTestConsoleWrapperAsync obsolete | Existing session sample uses IVsTestConsoleWrapper. Do not introduce the obsolete alias. |
| EqtHash.GuidFromStringXxHash128 marked VSTEST001 | Retain experimental status; public visibility alone does not establish an intended new developer goal. No documented external authoring scenario was established; no new recipe is inferred from visibility. |
| ResolveAssemblyReference cancellation interface | SDK task implementation capability; not by itself a new application recipe. |
| Optimized publishing defaults false and hook changes | Current `sdk.migrate-optimized-publish-hooks` already explicitly describes the GA default and opts in for its failure control. Preserve actual native receipts and their limits. |
| PackTool shim directory and publish integration | Existing package-tool and custom native-RID samples use public SDK contracts rather than the changed private paths. Private target names alone are not supported authoring hooks. |
| Bundled versions, quoted VSTest conditions, guarded ProjectData import | Retain exact version/input identities; implementation changes are not independently executed consumer evidence. |
| Static web assets/browser Hot Reload targets | Forwarded to the verified PIN-964 web-family owner; do not silently claim framework behavior from XML inspection. |

Canonical [SDK notes](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-11/sdk)
and [compatibility index](https://learn.microsoft.com/en-us/dotnet/core/compatibility/11)
remain discovery sources. The latter expressly disclaims completeness. Package
dependency replay is recorded below. This bounded reconciliation does not prove
every recipe's behavior; the two tracked corrections retain the stable gate.

## Tracked follow-ons

As a developer reading VSTest messages, I want the guidance to distinguish raw
protocol data from redacted diagnostic text so that logging does not depend on
an incorrect promise of unchanged output.

PIN-1301 outcome: correct `testing.migrate-vstest-message-readers` and add a
secret-bearing envelope control to its existing real-package harness. Verify
RawMessage preserves the wire envelope, ToString redacts the relevant properties,
ordinary typed payload decoding remains correct, and the old/current controls
are explicitly distinguished. Refresh affected evidence and inspect installed
MCP guidance. Preserve ID, fictional domain and existing protocol boundaries.

Estimate **S (2)**: bounded wording/test/evidence refresh with actual package
execution. Executor Agent; build/deployment milestone; Current Cycle 5
(9–16 November), explicitly approved by Pingu. Source: PIN-963 and the immutable
Message.cs comparison in the adjacent register. Saved as **03-051 / PIN-1301**,
Todo, S (2), with a native relation blocking the final stable candidate gate;
do not reopen or close the completed original delivery merely from this finding.

## Verification and limits

The actual package-host replay also passed: 32 immutable source hashes, seven
full notes and 23 package ZIPs. Experimental affected-test packages remain
18.13.0-preview.26519.2; package GA is not inferred from SDK GA. All three
experimental SDK editions retain their labels. Fourteen other SDK editions
still carry preview stability metadata and require a narrow final classification
refresh against their specific GA or independent-package contracts. The native
RID recipe also names GA SDK11 while retaining an RC3 compiler build in its
requirements table; reconcile that exact statement with its saved artifact.

PIN-1302 (03-052, Todo, M (3)): reconcile stability and measured-version statements for
the complete 56-edition SDK catalogue. Estimate **M (3)** for per-contract source
review, surgical metadata/prose corrections, affected evidence freshness checks
and real installed discovery/applicability inspection. Keep independently
versioned previews and deliberately experimental APIs labelled. Do not rerun
unaffected native/platform matrices. Agent, build/deployment milestone, Current
Cycle 5 (9–16 November); source PIN-963 and this catalogue register. Its native
relation also blocks PIN-967. This is
separate from the focused VSTest redaction behavior proof above.

PIN-964 is verified in the same team/project and owns the web-family
reconciliation, including the static-web-assets/Hot Reload delta forwarded
here. Its title alone is not proof of delivered web behavior. Packaging recipes
use public SDK tool contracts rather than the changed private shim paths;
no additional public packaging goal follows from those private path changes.
The experimental hashing API remains excluded pending a documented external
authoring scenario, preserving the earlier cancellation policy rather than
inventing a consumer solely from API visibility.

`Inspect-GaSdkContracts.ps1` passed: 9 sources, 56 SDK editions, 52 tooling assets.
`Inspect-SdkBuildApis.ps1` passed against isolated fresh archives: 52/52
assemblies, 230/232 build files, 2,689 assigned differences. Installed .NET 10
SDK inspection failed on missing inspection-library/return metadata; the fresh
archive replay replaces that failed scan, without modifying the shared SDK.
No package execution, native platform execution or global stable check is claimed.
