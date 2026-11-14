# GA SDK and package-authoring reconciliation

## PIN-967 qualification attempt

SDK11.0.100 on the current Windows machine reproduces the existing
`BuildArtifacts` failure at `TarDirectory`: MSB4018 / MSB0001,
`Cannot call Reacquire() ... when Yield() was called ...`. A second invocation
with one build node, node reuse disabled and MSBuild servers disabled also
fails. These are actual GA failures, not successful archive execution or RC
results relabelled as GA. Ordinary project compilation succeeds before the
task fails. The unchanged canonical target and its pending proof remain.

The final stable content gate now reaches `msbuild.archive-build-artifacts`
after the owner-approved DirectTLS availability disposition. The other seven
SDK editions still retain their explicit pending proof; source/artifact audit
completion did not establish their deferred behavior. PIN-967 remains In Progress
and unpublished. A supported archive correction is separately sized work under
the ticket boundary unless Pingu explicitly approves it within PIN-967.


## PIN-1322 / PIN-1323 consumer qualification

The file-access edition now has a complete SDK11 observer library and a separate
net48 consumer actually loaded by Visual Studio x64 MSBuild18.11.0.47511.
`eng/Test-BuildAccessObserver.ps1` checks real IO/errors/enumeration, previous/current
shared-field decoding, constructor defaults, two contexts, reporting-only and
capture-disabled controls, failure and active cancellation. This host omits the
project-finished callback on cancellation; end-build drains the context as
abandoned. No reusable cache state is published. Exact compiler/managed/native
identities remain separate in the execution receipt.

AffectedTests/CodeCoverage18.13.0-preview.26519.2 use their packaged builder hook
and documented default local storage. The real GA SDK11 control passes ordinary
tests and writes maps but fails the reporting handshake. The qualified conditional
route retains net11 project targets under SDK12.0.100-alpha.1.26509.101 and its
explicit preview reference pack/runtime; this is not GA11 orchestration proof.
`eng/Test-SeedBankSelection.ps1` checks two independent original modules, real
source maps, empty selection, conservative missing/stale/schema/unmapped fallbacks,
test/startup/protocol failures and active selection/collection cancellation.
For a Germination-only change all four tests run: Germination selects two while
ColdStore conservatively runs two. Pingu explicitly accepted that limit on
14 November 2026; cross-module elimination and speedup are not claimed.

Only these editions' resolved pending proof and preview exceptions are retired.
Experimental labels, historical RC records, exact eight foundations, unrelated
native exclusions and PIN-967's remaining candidate/release gates remain intact.

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

PIN-1301 retains the independent 18.11.0 package under Pingu's explicit decision: an actual independent TestHost
18.11.0 consumer retains the synthetic Password value in Message.ToString(),
whereas an isolated probe of the actual GA SDK assemblies preserves RawMessage
and redacts that Password value in ToString(). The SDK probe is artifact
diagnosis, not a supported SDK-DLL acquisition route for the recipe. No logging
guarantee for arbitrary payloads follows from either observation. The original laboratory harness now includes explicit old/current independent
package controls and a separate hashed SDK diagnostic probe. The final stable
gate remains PIN-967; this correction does not authorize release.

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

## PIN-967 telemetry qualification

`eng/Test-CliTelemetry.ps1 -Record` runs the canonical SDK10.0.403 and
SDK11.0.100 wrappers against a temporary local HTTP/protobuf collector. Both
opt-out controls return successfully without collector requests. GA export
produces nonempty `/v1/traces` and `/v1/metrics` payloads carrying `dotnet-cli`.
Microsoft trace export is disabled only in the verification process using the
actual GA TelemetryClient source contract. This measures local receipt, not
absence at every network destination, Azure Monitor behavior, retry reliability
or production collector delivery. The wrapper source is unchanged.

The shared SDK10.0.403 entry lacks its runtime configuration; the complete
portable installation supplies that comparison. Existing source-review and
fresh compilation receipts remain distinct from new execution receipts. Only
`sdk.configure-cli-telemetry` pending proof and preview exception are resolved.
Archive/glob remains PIN-1321; the other SDK gates and final stable candidate
remain open. Historical RC/source/artifact registers retain their identities.

PIN-967 container qualification: two SDK11.0.100 publishes separated by clean
produce the same image-index blob 22c084002b3f98c9d7a3e8ee06da52465cc68a1fea03804681a3503f0124c635,
21 matching regular-entry byte sequences and 19 matching content-addressed blobs.
The runtime base is pinned to index 3fe5f5495141c97986a49d3a4e76061f6225c3f93f94e8a7e522781376193863.
Both Linux x64/Arm64 manifests are retained. ContainerArchiveOutputPath avoids
engine loading; passing LocalRegistry=Docker in this route is not Docker or
Podman execution proof. Only the archive-reproducibility edition's pending proof
and preview exception are resolved. Engine selection/Podman qualification and
other SDK gates remain separate.

PIN-967 engine selection qualification: explicit Docker29.6.1 loading succeeded
for an owned linux-x64 tag. Network-disabled execution printed both expected
records and the owned image was removed. Explicit Podman on this host without
Podman failed CONTAINER1012 rather than falling back to Docker. Only the bounded
engine-selection gate is resolved; successful Podman loading/multiarchitecture
execution remains unverified in its separate recipe. No Podman acquisition or
new environment is implied.

The installed Visual Studio x64 MSBuild18.11.0.47511 accepts /reportFileAccesses
and exposes the new FileAccessData fields. A disposable complete plugin captured
real fixture reads/writes and enumeration through /graph plus /reportFileAccesses;
reporting alone did not activate that item-discovered plugin. This is host
feasibility evidence, not proof of the canonical illustrative observer's complete
context/cancellation behavior. The artifact probe also found the old experimental
project-cache aliases obsolete in favor of Microsoft.Build.ProjectCache. Keep
file-access pending until the composed canonical path is reconciled and verified.
The affected-test storage-documentation pointer still returns 404; its composed
host/registration/storage contract remains unresolved. These two reference-only
consumer gaps are not cleared by the successful decoder/rules library builds.

The current upstream affected-test rollout at TestFx commit
5ea46ecee95d6d7339c71b87d62b94524fea3609 uses SDK12.0.100-alpha.1.26509.101
and AffectedTests18.13.0-preview.26530.1, unlike this recipe's GA SDK11 and
18.13.0-preview.26519.2. Its newer handshake fixes and repository-specific Azure
DevOps configuration are not evidence for the pinned GA11 consumer. Preserve
that compatibility gap; do not retarget or upgrade the sample implicitly.

## PIN-967 final reconciliation

The completed PIN-1321/PIN-1322/PIN-1323 changes are integrated from main.
Pingu explicitly approved skipping Podman qualification. The Podman edition
retains its canonical sample identities and compilation records, with loading,
multiarchitecture inspection and execution explicitly not-tested rather than
pending or verified. No engine acquisition or successful Podman claim follows.

The current checkpoint binds all455 current row dispositions, the406 historical
rows, exact eight foundations, original source audits and six GA audit overlays.
No pending Human proof remains. Historical RC/preview candidate identities and
failed GA observations above retain their original meaning. Target11 coverage
is reconciled comprehensive guidance; target10 remains the curated eight
foundations/conditional routes. Package and platform validation must consume
one selected1.0.0 candidate; readiness does not authorize publication.
