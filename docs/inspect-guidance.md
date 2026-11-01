# Inspect the guidance an agent receives

`playbook mcp` speaks protocol over stdin/stdout. Use MCP Inspector as a client; it initializes and closes each connection automatically. `eng/Inspect-Guidance.ps1` builds the real server, installs pinned Inspector 2.11.0 under ignored `artifacts/` if needed, calls both tools, and prints **ID/title/goal, full expanded Markdown and applicability**. Responses default to `artifacts/inspection/<id>.json`.

Requires PowerShell 7, the repository's .NET SDK 10.0.402 and Node 22.19+. Setup may restore/download packages online; guidance itself is embedded and offline. The helper invokes the resolved executables by absolute path without changing PATH or global tools. It disables .NET first-run PATH integration. No native workloads or account are required for server inspection.

## Dual-stack connections and endpoint buffers

Requires PowerShell7, Node22.19+, SDK10.0.402/runtime10.0.13 and the separate
SDK11.0.100-rc.2.26504.105/runtime11RC2 installation. The samples have no package
dependencies or workloads. RelayDial requires OS IPv4/IPv6, localhost resolving
only to both loopback families and available ephemeral TCP ports; it owns all
listeners/attempts. EndpointLedger performs no network I/O. No system DNS/routes,
global tools, PATH or persistent environment changes are required.

Refresh the four sample records and verify nine immutable official sources:

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-ec011ee0'
& ./eng/Test-NetworkBuffers.ps1 -Record -VerifySources
```

Expect Default/Parallel to pass both/either/neither-family listener cases,
selected-peer payload, event completion and cancellation/draining. The older
scheduler checks its own success/fallback, payload and drained-cancellation paths.
A saturated owned accept queue holds current TCP connections pending for real
cancellation. All sockets are disposed. Endpoint checks cover numeric IPv4/IPv6,
ports/defaults, malformed/UTF8 failures, exact/large/short output and unchanged
buffer boundaries. Current char/generic interfaces execute too. PATH/persistent
environment snapshots must agree; saved records include actual loaded runtime and
audit-matching references/compiler/native identities. No fixed latency, failed-write
interior rollback, other-platform, allocation benchmark or GA claim is made.

This block is tested from an unrelated starting directory. It builds/packs the
ordinary tool, installs that exact artifact using the existing isolated local-only
feed/cache, then calls actual installed stdio list/get from an unrelated temporary
directory. It prints both discovery IDs/titles/goals, full canonical Markdown,
explicit target11/edition11, applicability and fresh paired evidence. Setup can
restore online; unusable proxies during retrieval prove proxy independence, not
OS-level network isolation.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-ec011ee0'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('sockets.race-dual-stack-connections','networking.parse-endpoint-buffers') `
  -Query 'network-buffers' -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
  -PackageVersion '0.0.0-pin1090-1091' -BuildAndPack `
  -Dotnet 'C:\Program Files\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory './artifacts/network-buffers/inspection'
```

Expect two complete entries and final exact-artifact PASS. Target10 returns
no_applicable_edition. Missing/stale evidence, snippet/body drift or applicability
changes fail. Existing eight foundations, Human gates, PIN-959 and PIN-962 remain
unchanged/open. RC2 preparation is not final-GA verification.

## Registered certificate identifiers and typed DNS records

Requires PowerShell 7, Node 22.19+, SDK10.0.402/runtime10.0.13 and the separate
SDK11.0.100-rc.2.26504.105/runtime11RC2 installation. The older DNS consumer pins
DnsClient1.8.0 and executes its net8.0 asset; other consumers use framework APIs.
Certificate proof needs the available Windows ECDSA P-256 provider. DNS proof
needs free loopback UDP53 on127.0.0.1/127.0.0.2 and the existing Linux Docker
runtime images. No account, workload, public zone or system DNS edit is required.

Refresh affected sample/source evidence separately:

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-30a070b6'
& ./eng/Test-IdentityRecords.ps1 -Record -VerifySources
```

Expect four passing actual consumers and nine immutable source hashes. Certificate
checks export/reload SAN bytes, primitive tags, multiple/large OIDs, unrelated DNS,
manual/new equality, malformed/null inputs, failed append and explicit criticality.
DNS checks execute seven record kinds, fields/TTLs, NODATA/NXDOMAIN, SOA120/minimum45
and absent SOA, two-server override and pre/in-flight cancellation. Windows RC2
returns negative TTL0 and rejects custom ports/mixed families; separate network-none
Linux runs return45 and use custom ports. Read-only mounts/private tmpfs disappear
with the containers. PATH and persistent settings are checked unchanged.

This block is exercised from an unrelated starting directory. It builds/packs
the ordinary tool, installs that exact artifact through an isolated local-only
feed/cache, then calls real installed stdio list/get from a temporary unrelated
directory. Setup may restore online; unusable proxies during retrieval prove
proxy independence, not OS network isolation.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-30a070b6'
& ./eng/Test-Smoke.ps1 -Dotnet 'C:\Program Files\dotnet\dotnet.exe' `
  -PackageVersion '0.0.0-pin1088-1089'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('certificates.encode-registered-identifiers','dns.inspect-typed-records') `
  -Query 'identity-records' -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
  -PackageVersion '0.0.0-pin1088-1089' `
  -Dotnet 'C:\Program Files\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory './artifacts/identity-records/inspection'
& ./eng/Test-IdentityRecordSnippets.ps1
```

Expect one passing installed smoke, both discovery IDs/titles/explanatory goals,
full canonical Markdown, target11/edition11, applicability and fresh paired
compilation plus separate Linux execution evidence. Structured/text results agree;
target10 returns no_applicable_edition. Missing/stale evidence, a changed snippet/body
or a nonzero exit fails. Inspection consumes saved proof; it does not rerun DNS or
certificate checks. No trust, network identity validation, public resolver,
other-platform/native matrix or GA verification is claimed. The string signature
corrects the audit draft's Oid spelling. SRV selection stays in the existing recipe;
eight foundations, PIN-959/PIN-962 and existing Human gates remain unchanged/open.

The final snippet check copies only the installed Markdown code blocks into four
fresh pinned console consumers, one source file per block, and runs certificate
export/read-back or a real local DNS query. It supplies no missing imports to the
copied snippets; all required imports and namespaces are inside their canonical
regions. The temporary projects/sockets are removed. Source evidence separately
prints the loaded consumer CoreLib path/version/hash and rejects runtime baseline
drift before saving; verification pins are not additional application requirements.

## Value-based JSON classification and message authenticators

This builds/packs the ordinary tool, installs that exact artifact using the
existing isolated local-only feed/cache and calls real stdio list/get from an
unrelated temporary directory. It displays ID/title/goal, complete expanded
canonical Markdown, target11/edition11, applicability and fresh paired evidence.
Tested from an unrelated starting directory:

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-aa90caa3'
& ./eng/Test-ClassifierAuthentication.ps1
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('json.classify-payload-values','crypto.verify-message-authenticators') `
  -Query 'classifier-authentication' -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
  -PackageVersion '0.0.0-pin1086-1087' -BuildAndPack `
  -Dotnet 'C:\Program Files\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory './artifacts/classifier-auth/inspection'
```

Requires PowerShell7, Node22.19+, SDK10.0.402/ref-runtime10.0.13 and complete
SDK11.0.100-rc.2.26504.105/ref-runtime11RC2. No extra sample packages or workloads.
Positive KMAC checks require a supported provider (measured Windows x64 CNG);
the sample records native DLL versions/hashes. No account or external service is
used. Setup can restore tool/Inspector2.11.0 online; unusable proxies during
installed retrieval prove proxy independence, not OS network isolation.

Expect four successful builds/runs, complete selected JSON payloads in either
property order, ten rejected JSON cases, reference null, registration precedence,
global no-match/unregistered failure contracts and generated metadata without
reflection fallback. Authenticators verify independent known answers, failures,
actual cancellation after12 bytes, incremental preserve/reset and KMAC
customization/support. Raw HMAC invalid lengths throw; raw KMAC has variable
output length, while the protocol enforces32 bytes. Final exact-artifact inspection
reports PASS; target10 returns no_applicable_edition. `-Record -VerifySources`
refreshes affected proof and verifies immutable source hashes. Stale inputs,
canonical-content/target/applicability drift or nonzero exit fail. Union custom
null, AOT, timing, other-platform/provider and final-GA claims remain unverified;
eight foundations and PIN-959/PIN-962/Human gates stay open. No new matrix.

## Text cancellation, IDN buffers and directory diagnostics

This builds/packs the ordinary tool, installs that exact artifact through the
existing isolated local feed/cache, and calls real stdio list/get from an unrelated
temporary directory. It prints ID/title/goal, complete canonical Markdown,
target11/edition11, applicability and fresh paired evidence. Tested from an
unrelated starting directory:

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-01d67885'
& ./eng/Test-TextBoundaries.ps1
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('io.cancel-text-output','text.convert-domain-labels','io.report-directory-failures') `
  -Query 'text-boundaries' -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
  -PackageVersion '0.0.0-pin1081-1083' -BuildAndPack `
  -Dotnet 'C:\Program Files\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory './artifacts/text-boundaries/inspection'
```

Requires PowerShell7, Node22.19+, SDK10.0.402/ref-runtime10.0.13 and complete
SDK11.0.100-rc.2.26504.105/ref-runtime11RC2. Samples execute on Windows x64;
IDN explicitly uses non-invariant system ICU (measured 72.1.0.4), with the loaded
DLL hash/data identity captured. No account, DNS/service, global tools or native
workload. Setup can restore tool/Inspector2.11.0 online; retrieval with unusable
proxies establishes proxy independence, not OS network isolation.

Expect six successful sample builds/runs: real StreamWriter cancellation retains
a 128-byte prefix in the controlled 8192-character case; IDN round trips and
exact/short/invalid/policy checks pass; actual Windows missing-directory operations
populate the current property. Constructor/fallback checks do not replace those
operations. Target10 returns no_applicable_edition and the final exact-artifact
inspection reports PASS. `-Record -VerifySources` on Test-TextBoundaries refreshes
affected proof and rechecks pinned official sources; ordinary runs preserve it.
Stale evidence, canonical-content drift or a nonzero exit is failure. Other-platform,
NLS/invariant IDNA, arbitrary-writer cancellation, atomic rollback, performance and
final-GA claims remain unverified; exactly eight foundations and PIN-959/PIN-962
and existing Human gates remain intact. No native/client matrix is added.

## Pipe handles, mobile locking, native WebAssembly and HKDF inputs

This block builds/packs the ordinary tool, installs that exact package through
the existing isolated local feed/cache, and calls real stdio list/get from an
unrelated temporary directory. It prints ID/title/goal, full canonical Markdown,
explicit target11/edition11, applicability and fresh evidence. Tested from an
unrelated starting directory:

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-07530b3c'
& ./eng/Test-Smoke.ps1 -PackageVersion '0.0.0-pin741-758-1059' `
  -Dotnet 'C:/Program Files/dotnet/dotnet.exe'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('io.use-pipe-handles','io.migrate-mobile-locking',
    'runtime.link-native-wasm-code','crypto.bound-hkdf-inputs') `
  -Query 'native-boundaries' -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
  -PackageVersion '0.0.0-pin741-758-1059' `
  -Dotnet 'C:/Program Files/dotnet/dotnet.exe' `
  -Node 'C:/Program Files/nodejs/node.exe' `
  -OutputDirectory './artifacts/native-boundaries/inspection'
```

Requires PowerShell7, Node22.19+ and SDK10.0.402. Setup can restore the tool and
Inspector2.11.0 online; installed calls use unusable proxies, establishing proxy
independence rather than OS network isolation. Expect four entries and final
exact-artifact PASS; target10 returns no_applicable_edition. Missing canonical
content, stale evidence or a changed target/applicability fails. Apple mobile
behavior and the experimental CoreCLR WebAssembly native sample are explicitly
sample-specific not-tested; source review and intended Emscripten6.0.2 are not
compiled/linked/invoked proof. No new native environment, CI matrix or ninth
foundation is added; comprehensive and GA gates remain open.

To refresh the affected sample evidence, run `& ./eng/Record-NativeBoundaries.ps1
-VerifySources` after selecting this same worktree. That separate command needs
the installed paired SDK10.0.402/runtime10.0.13 and SDK11RC2/runtime11RC2, plus
the existing Docker runtime images. It runs actual Windows HKDF boundaries,
Windows/Linux pipe contracts and portable journal checks, records exact native
assets/input hashes, and retains unavailable-native source-review classifications.
It changes no global tools, PATH or persistent environment variables.

## Integer division, magnitude and format checksums

This block builds/packs the ordinary tool once, installs that exact artifact
through the existing isolated local feed/cache and runs actual stdio list/get
from an unrelated temporary directory. It prints all three IDs/titles/goals,
complete canonical Markdown, explicit target11/edition11, applicability and fresh
paired evidence. Target10 returns `no_applicable_edition`; the older comparisons
add no ninth foundation. Tested from an unrelated starting directory:

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-ffe3f7cb'
& ./eng/Test-IntegerFormats.ps1
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('numerics.choose-integer-division','numerics.measure-integer-magnitude','io.compute-format-checksums') `
  -Query '' -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
  -PackageVersion '0.0.0-pin1076-1078' -BuildAndPack `
  -Dotnet 'C:/Program Files/dotnet/dotnet.exe' `
  -Node 'C:/Program Files/nodejs/node.exe' `
  -OutputDirectory './artifacts/integer-formats/inspection'
```

Requires PowerShell7, Node22.19+, SDK10.0.402 and complete
SDK11.0.100-rc.2.26504.105 with matching runtime/reference10.0.13/11RC2 for the
paired checks. Locked Hashing packages are10.0.13/11RC2, lib/net10/lib/net11.
Setup can restore packages/Inspector online; installed retrieval with unusable
proxies establishes proxy independence, not OS network isolation. No service,
account, native workload, global tool install or persistent environment change.

Expect five signed division modes and invalid/zero/overflow checks, exact integer
magnitude boundaries through10^38 and both 128-bit maxima, four independent
checksum vectors with wire bytes, segmented/reset/short-buffer checks and final
exact-artifact PASS. `-Record -VerifySources` on Test-IntegerFormats refreshes
affected evidence and rechecks pinned official sources; ordinary runs preserve
saved records. Missing/stale evidence, wrong assets, changed canonical content,
wrong target or a nonzero exit fails. RC2, other-platform execution and optional
RC2-package-on-net10 are not GA/compatibility claims; PIN-959/PIN-962 remain open.

## Explicit untested native-platform checks

The [native testing policy](contributing.md#native-testing-policy) keeps existing
Windows/Linux proof while disclosing unavailable Mac, device and CPU checks as
`humanProof.status: not-tested`, with exact `untestedSamples` and scope. These
declared exclusions do not block publication. Other genuinely pending Human
actions and missing/stale locally required compilation remain gated.

This block builds/packs the ordinary tool, installs that exact package using the
existing isolated local feed/cache, and calls real stdio list/get from an unrelated
directory. It prints each ID/title/goal, complete canonical Markdown, applicability
and evidence. It needs PowerShell 7, SDK 10.0.402 and Node 22.19+; setup may restore
packages and Inspector 2.11.0 online. It changes no global tool, PATH or native
environment. The block was exercised from an unrelated starting directory.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-3a9b9778'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('console.color-piped-output','process.capture-output',
    'process.control-lifetime','process.control-native-handles',
    'process.inspect-termination','process.prepare-before-start',
    'sdk.watch-app-hosts-and-devices','simd.select-supported-instructions') `
  -Query '' -TargetVersion '11.0' -PackageVersion '0.0.0-pin699' -BuildAndPack `
  -Dotnet 'C:\Program Files\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory './artifacts/native-testing/inspection'
```

Expect all eight entries, `Not tested:` selection warnings, sample-specific
`not-tested` detail and a final exact-artifact `PASS`. Actual compilation/execution
records retain their kinds and input freshness. An absent warning, lost sample
identity, stale record, changed canonical body or nonzero command is a failure.
This does not execute the excluded platforms, establish source-only compilation,
or assert full-catalogue release readiness.

## Replace positional identity in embedded JSON sequences

This block builds/packs the ordinary tool, then installs that exact artifact
through the existing isolated local feed/cache. Real stdio list/get runs from an
unrelated temporary directory and prints ID/title/goal, complete canonical
Markdown, applicability, target11/edition11 and fresh paired evidence. Target10
returns `no_applicable_edition`; the older comparison adds no ninth foundation.

Requires PowerShell7, Node22.19+, SDK10.0.402 and runtime10.0.13 for inspection.
Consumer recapture also requires SDK11.0.100-rc.2.26504.105/runtime11RC2 and
Windows x64 native SQLite assets. Restoring dependencies and Inspector can use
the network. Retrieval with unusable proxies proves proxy independence, not
OS-level network isolation. No account, external resource, global tool install
or persistent environment change is required. Tested from an unrelated directory:

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-bf76c8b2'
& ./eng/Test-TrailCards.ps1
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId 'ef.replace-ordinal-json-ownership' `
  -Query '' -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
  -PackageVersion '0.0.0-pin847' -BuildAndPack `
  -Dotnet 'C:\Program Files\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory ./artifacts/pin847/inspection
```

Expect EF10/11 builds and provider PASS results, then discovery title
"Preserve an embedded sequence while replacing positional ownership", its
explanatory goal and the final exact-artifact inspection PASS. The actual EF10
SQLite file is read/edited by EF11 and read back by EF10. Original order,
duplicates and an empty array survive; removal/edit/append reload successfully.
The old mapping triggers the RC2 relational diagnostic. The old table remains
nullable while the new model generates NOT NULL: production schema/data
migration still needs separate review. Use `Test-TrailCards.ps1 -Record` only
to recapture changed sample proof. No other-provider, production migration or GA
proof follows. Missing/stale evidence, body/applicability drift or nonzero exit
is failure; disposable database cleanup is automatic.

## Compiler conversions, declared layout and F# callback adaptation

This builds/packs the ordinary tool once, installs that exact artifact through
the existing isolated local feed/cache and calls real stdio list/get from an
unrelated temporary directory. It displays every ID/title/goal, complete canonical
Markdown, target11/edition11, applicability and fresh paired evidence. Target10
returns `no_applicable_edition`; the original older consumers remain comparisons,
preserving exactly eight foundations. The VB-native source also verifies VB
comment-marker expansion and canonical identity.

Prerequisites: PowerShell7, Node22.19+, SDK10.0.402. Recapturing consumer proof
also needs complete SDK11.0.100-rc.2.26504.105 and runtime/reference10.0.13/11RC2.
Roslyn5.12.0-1.26504.105 restores from Microsoft's public dotnet-tools feed;
FSharp.Core pins are10.1.402/11.0.101-rc2.26504.105. Setup may restore online;
retrieval with unusable proxies proves proxy independence, not network isolation.
No global install, persistent environment change, provider or native workload.
This literal block was tested from an unrelated starting directory:

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-8780a32c'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('languages.inspect-coalescing-conversions','languages.inspect-type-layout','fsharp.adapt-opaque-callbacks') `
  -Query '' -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
  -PackageVersion '0.0.0-pin1035-1037' -BuildAndPack -CheckDemonstratedLanguages `
  -Dotnet 'C:\Program Files\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory ./artifacts/compiler-inspection/installed
```

Expect "Inspect null-coalescing conversions in C# and VB", "Inspect declared
type layout through compiler symbols", "Adapt an opaque curried callback once"
and their explanatory goals, with complete expanded files and current evidence.
`eng/Test-CompilerInspections.ps1 -Record` refreshes changed consumer proof;
without `-Record` it runs the paired actual consumers, IL and diagnostic checks
without replacing saved records. C#/VB check numeric/reference/identity flags
and null/wrong-language failures; layout checks Sequential/2/24, Explicit/1/32,
Auto/0/0 as declarations, not runtime object layout. F# separate consumers check
results, ordered effects, exception propagation and a hoisted Adapt versus the
InvokeFast control; actual FS3350/FS3916 and newer-compiler F#8-mode imports are
distinct from old-compiler or older-target compatibility. RC2 is not GA proof;
coverage, GA and Human gates stay open. A stale/missing record, changed body,
unexpanded snippet, wrong target or nonzero exit fails the check.

## ZIP initialization and invocation locations

This block was verified from an unrelated directory. It builds/packs the ordinary
tool and installs that exact artifact through a fresh local-only feed/cache.
Real stdio list/get displays both discovery IDs, titles and explanatory goals,
complete canonical Markdown, target11/edition11, applicability and fresh evidence.
Target10 returns `no_applicable_edition`; the comparisons add no ninth foundation.

Use PowerShell7, Node22.19+, SDK10.0.402 and runtime10.0.13 for tool inspection.
Sample recapture also needs complete SDK11.0.100-rc.2.26504.105/ref/runtime11RC2,
Windows x64/NTFS and actual symlink permission. Source replay/setup can use the
network; retrieval with unusable proxies establishes proxy independence, not
OS-level network isolation. No account, global tool installation or PATH change.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-40fcde33'
& ./eng/Record-WeatherMaintenance.ps1 -VerifySources
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('archive.handle-async-initialization','hosting.distinguish-invocation-paths') `
  -Query '' -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
  -PackageVersion '0.0.0-pin1042-1043' -BuildAndPack `
  -Dotnet 'C:/Program Files/dotnet/dotnet.exe' -Node 'C:/Program Files/nodejs/node.exe' `
  -OutputDirectory ./artifacts/weather-maintenance/inspection
```

Expect paired ZIP PASS results, fourteen actual ordinary/single-file child launches,
and the final exact-artifact installed inspection PASS. Both malformed central
directories fail at Entries on .NET10 and at the factory on RC2; missing end records
fail at both factories. RC2 Entries performs zero reads after async initialization.
Successful directory discovery is separate from damaged-payload EOF/CRC rejection.
Successful leaveOpen and caller cleanup after failed initialization are checked.
Launch records show exact invocation/ProcessPath/entry Location; real single-file
Location is empty. Older single-file resolved executable names may retain an alias
basename. No Unix/every-launcher/AOT/GA claim follows; PIN-959/PIN-962 stay open.
A stale record, missing body/snippet/applicability or nonzero command is a failure.

## Date parser settings and numeric representation

This block builds/packs the ordinary tool and installs that exact artifact from a
local-only feed with a fresh cache. Real stdio list/get runs from an unrelated
temporary directory and prints ID/title/goal, complete canonical Markdown,
target11/edition11, applicability and current evidence. Target10 returns
`no_applicable_edition`: the .NET10 comparisons and SDK11/net10 rebuild experiment
remain inside the target11 edition, preserving exactly eight foundations.

Prerequisites: PowerShell7, Node22.19+, SDK10.0.402 and, for recapture, a complete
SDK11.0.100-rc.2.26504.105 installation plus runtime/ref10.0.13 and11RC2. The source hash replay requires internet access; setup can
restore online. Retrieval with unusable proxies proves proxy independence, not
network isolation. No global tool install, PATH change, credential or provider.
The literal block was tested from an unrelated starting directory:

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-6d85858c'
& ./eng/Record-BookingArchive.ps1 -VerifySources `
  -Net10Dotnet 'C:\Program Files\dotnet\dotnet.exe' `
  -Net11Dotnet 'C:\Users\pingu\.dotnet-playbook-11\dotnet.exe'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('globalization.validate-date-parsing-options','numerics.preserve-conversion-intent') `
  -Query '' -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
  -PackageVersion '0.0.0-pin1045-1046' -BuildAndPack `
  -Dotnet 'C:\Program Files\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory ./artifacts/booking-archive/inspection
```

Expect both explanatory discovery entries and the final exact-artifact PASS.
The recorder verifies six pinned official source hashes, both booking executions,
both numeric executions and the SDK11/net10 constant consumer on actual runtime10;
it compares embedded constants separately from runtime-input casts. Compiler,
selected targeting pack and actual runtime hashes accompany each saved record.
Date styles throw ArgumentException on RC2, but the measured format cases require
FormatException/false qualifications. The original allow-list reader rejects
configuration before rows; malformed data remains false. G7/G15 policy checks
cover signs, ties, exponents, small/large values, range/nonfinite rejection and
representative conversion families. RC2's extremely close Decimal-to-Half midpoint
limitation remains explicit; BigInteger-to-Half controls do not invent a changed
finite result, and BFloat16 runs only on net11. No universal conversion, native,
browser or actual-GA claim follows. PIN-959/PIN-962 and Human gates stay open.

A nonzero exit, stale evidence, absent ID, unexpanded snippet or changed
structured/text response is a failed check.

## Span storage and dynamic logical binding

This block selects the actual task worktree and was checked starting from an
unrelated directory. It builds/packs once, installs that exact ordinary artifact
through the existing isolated local feed/cache, then uses real installed stdio
list/get to display both IDs, titles, explanatory goals, complete canonical
Markdown, explicit target11/edition11, applicability and fresh evidence.
Each target10 request must return `no_applicable_edition`; direct retrieval of the
excluded readonly-metadata goal must return `unknown_recipe`.

Run this block in **PowerShell 7 (`pwsh`)**. Windows PowerShell 5.1 changes native
argument/error handling and is outside this helper's prerequisites.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-6b360ce7'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('csharp.preserve-span-storage','csharp.bind-dynamic-conditions') `
  -RemovedRecipeId @('csharp.resolve-readonly-metadata') -Query '' `
  -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
  -PackageVersion '0.0.0-pin1032-1034' -BuildAndPack `
  -Dotnet 'C:/Program Files/dotnet/dotnet.exe' -Node 'C:/Program Files/nodejs/node.exe' `
  -OutputDirectory ./artifacts/compiler-migrations/inspection
```

Expect **Preserve span storage across nested scopes** and **Preserve dynamic
logical operator binding** with their discovery descriptions and actual paired
evidence. Setup requires PowerShell7, SDK10.0.402/runtime10.0.13 and Node22.19+;
package/Inspector setup may restore online. Retrieval with unusable proxies proves
proxy independence, not OS-level network isolation. The exact eight foundations
and comprehensive/actual-GA/Human gates remain unchanged.

For sample reproduction, also provide SDK11.0.100-rc.2.26504.105/ref/runtime11 RC2
and run `eng/Test-CompilerMigrations.ps1 -SetupBaseline -Record`; supply
`-Net11Dotnet` with the absolute `dotnet` executable path when installed elsewhere. On Windows x64
that helper can download/extract the official SHA512-checked portable SDK10.0.100
historical reproducer under ignored `artifacts/`; no installer or PATH integration
runs. The helper checks real Span/ReadOnlySpan old acceptance versus CS8352,
both old dynamic binder exceptions versus CS7083, paired corrected storage/aliasing
and all16 logical truth-table/invocation-count cases. Ordinary readonly
method-group/lambda/local calls and emitted InAttribute modifiers succeed in both
supported sets. PIN-1033 uses its explicit evidenced-exclusion allowance: no
supported deficient reference closure/CS0518 repair is claimed or taught.
See the [bounded source reconciliation](planning/audits/compiler-contracts.md#compiler-migration-delivery-reconciliation).

## Selected base-image trust, Azure Linux migration and collector observation

This block was checked starting outside the checkout. It builds/packs the ordinary
tool and installs that exact artifact in
the Inspector's isolated local feed/cache. Real stdio list/get display each ID,
title, **goal**, complete canonical Markdown, target11/edition11, applicability and
fresh evidence. Both target10 gets must return `no_applicable_edition`; the paired
older workers add no ninth foundation.

Setup requires PowerShell7, SDK10.0.402/runtime10.0.13, Node 22.19+ and online package
setup. For sample reproduction, also supply SDK11.0.100-rc.2.26504.105 and a working
Linux amd64 Docker engine. Notation1.3.2/ORAS1.3.6 archives, exact Microsoft root
certificates and image digests are checked by the authoring helpers. Signature
verification needs public registry/certificate access; embedded guidance retrieval
uses unusable proxies, establishing proxy independence rather than OS-level network
isolation. Worker runs use Docker `--network none`. No system trust store, production
registry or persistent environment setting is changed by the sample checks.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-713eb23e'
$previousCertificateSetup = $env:DOTNET_GENERATE_ASPNET_CERTIFICATE
$env:DOTNET_GENERATE_ASPNET_CERTIFICATE = 'false'
try {
    & ./eng/Inspect-BlazorMigrationGuidance.ps1 `
        -RecipeId @('containers.verify-base-image-signatures','containers.migrate-azurelinux-base-images','testing.migrate-collector-event-inputs') `
        -Query 'quay' -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
        -PackageVersion '0.0.0-pin1137-1139' -BuildAndPack `
        -Dotnet 'C:/Program Files/dotnet/dotnet.exe' -Node 'C:/Program Files/nodejs/node.exe' `
        -OutputDirectory ./artifacts/quay-containers/inspection
}
finally { $env:DOTNET_GENERATE_ASPNET_CERTIFICATE = $previousCertificateSetup }
```

Expect **Verify the signature of a selected .NET base image**, **Migrate an
authored Azure Linux base-image selection** and **Observe VSTest collector events
without rewriting outcomes**, with their explanatory discovery goals.
The first records real strict index/platform verification, existing .NET10 signing,
wrong-root/scope and missing/tampered envelope rejection. The second records actual
managed worker execution on digest-pinned Azure Linux3/4, OS/runtime/architecture,
zlib export/call, ICU fr-FR formatting and absent configured-library exit23.
`eng/Test-QuayWorker.ps1 -Record` then `eng/Test-QuaySignatures.ps1 -Record`
reproduce those checks and refresh the existing sample records. Azure Linux4 stays
**Beta**. NativeAOT, other variants/architectures, performance, arbitrary ABI and GA
support remain unqualified. `eng/Test-QuayCollector.ps1 -Record` additionally
restores/builds paired locked18.7/18.11 package closures and loads the real
collector in matching test runs. Four real start/end events match TRX identity,
name, IsChild=false and outcome; collection preserves the disabled run's one
passing and one deliberate failing assertion. Separate constructor fixtures and
a compile-only assignment control confirm public setters were added, not removed.
No child=true/nested-framework transport, MTP or arbitrary collector compatibility
is claimed. PIN-959,
PIN-963 and unrelated Human gates remain open. No recurring native matrix is added.

## Disposable restore graphs, referenced symbols and evaluated imports

This block was checked from an unrelated directory. It builds/packs the ordinary
tool once, installs that exact artifact through the existing isolated local
feed/cache and calls real installed stdio list/get. It prints each ID/title/goal,
complete canonical Markdown, target11/edition11, applicability and current paired
evidence. Every target10 get must return `no_applicable_edition`; the real older
comparisons add no ninth foundation.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-c5612d78'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('nuget.skip-disposable-restore-graph','sdk.publish-referenced-symbols','msbuild.inspect-import-tree') `
  -Query 'beacon-build' -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
  -PackageVersion '0.0.0-pin1134-1136' -BuildAndPack `
  -Dotnet 'C:/Program Files/dotnet/dotnet.exe' -Node 'C:/Program Files/nodejs/node.exe' `
  -OutputDirectory ./artifacts/beacon/inspection
```

Expect the reusable titles **Avoid writing a disposable CI restore graph**,
**Include referenced symbols in published output** and **Inspect the actual build
import tree**, with explanatory goals and fresh actual evidence. Separately,
`eng/Test-BeaconBuild.ps1 -Record` recreates the paired local-package/locked-restore,
publish and evaluated-import controls with the exact SDK10/11 executable defaults
declared in that helper. It builds/runs both actual consumers and every publish;
checks true/false/default dgspec outputs, exact package hashes/equal selected assets,
normal local repeat and missing-package failure; verifies plural versus ineffective
singular symbol policy and own/reference/snupkg boundaries; and evaluates nested,
conditional, SDK, unset/false, ordering and duplicate/parent import controls.

Setup needs PowerShell7, SDK10.0.402/runtime/ref10.0.13, Node22.19+ and, to reproduce
samples, complete SDK11.0.100-rc.2.26504.105/runtime/ref11RC2. Tool/Inspector setup
may restore online; sample dependencies are original and local-only. Retrieval
with unusable proxies proves proxy independence, not network isolation. Actual
SDK10 already honours graph suppression/import items and includes the reference
PDB. SDK11's plural symbol property defaults true; false excludes that PDB, while
the app's own symbol setting remains separate. No redundant older copy workaround,
cache/no-op equivalence, speed saving, single-file/debugger, arbitrary import order,
other-platform or GA claim is made. PATH/persistent settings, the exact eight
foundations and PIN-959/PIN-963/Human gates remain unchanged.

## Token-binding removal and published F# MVC views

These two 11.0 editions use the existing actual-package Inspector with an isolated
local feed/cache and unrelated working directory. Requires PowerShell7,
SDK10.0.402/runtime10.0.13, SDK11.0.100-rc.2.26504.105/runtime11 RC2 and Node22.19+.
Sample restore and Inspector setup may use the network; retrieved guidance is
embedded. No trust-store change, production identity provider or native workload.

The block was exercised starting outside the checkout. The first command runs
paired real HTTPS/cookie checks, reproduces ASPDEPR010, compiles/renders F# views,
publishes to a clean source-free directory and exercises the missing-view control.
It refreshes the existing sample records. The second command builds/packs once,
installs that exact ordinary artifact and prints what an agent receives.

Use PowerShell 7 (`pwsh`): Windows PowerShell 5.1 changes native JSON-argument
quoting. The fixture retains `artifacts/web-retirement-<guid>` as disposable
published-consumer output for inspection.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-61e6bfa1'
& ./eng/Test-WebRetirement.ps1 -RecordEvidence
& ./eng/Inspect-BlazorMigrationGuidance.ps1 -BuildAndPack `
  -Dotnet 'C:/Program Files/dotnet/dotnet.exe' `
  -PackageVersion '0.0.0-pin1185-1186' -Query 'web-retirement' `
  -RecipeId @('authentication.remove-token-binding-assumption','mvc.compile-fsharp-views') `
  -UnsupportedTargetVersion '10.0' -OutputDirectory ./artifacts/web-retirement/inspection
```

Expect both discovery IDs, titles and explanatory goals, complete expanded
Markdown, explicit11.0 applicability and current evidence, with text/structured
equality and unsupported10.0 rejection. The two comparisons add no .NET10
foundation. The F# current consumer prints the actual .Views.dll path and matching
owner load context; its negative publish builds but returns the specific missing
view500. Token binding is RFC8471, not RFC5929 channel binding; removal preserves
the fixture's real authorization/HTTPS policy and supplies no replacement binding
security. Proxy-blocked retrieval is not network-isolation proof. RC2 Windows x64
execution is not actual-GA, arbitrary loader/trimming, browser or Hot Reload proof.
PIN-959/PIN-964 and unrelated Human gates stay open.

## HTTP/3 budgets, JSON Patch intent and early integration-test setup

This block builds/packs the ordinary tool once and installs that exact artifact
through the existing isolated local feed/cache. Real installed stdio list/get run
from an unrelated directory and display ID/title/goal, complete canonical Markdown,
target11/edition11, full applicability and current paired sample evidence. Each
target10 get must return `no_applicable_edition`; these comparisons do not add
another .NET10 foundation. The last command reconstructs all six consumers solely
from the returned Markdown in a temporary directory, without remote NuGet feeds.

Setup needs PowerShell7, Node22.19+, both exact SDK/runtime sets below, Windows
x64 with TLS1.3 and working MsQuic2.5.11.159415436. The first command restores
the locked packages, compares audited ZIP/reference/compile assets and records
compiler/runtime/native identities plus actual requests. No certificate trust,
production resource or persistent environment change is made. Prepared packages
are reused for the returned-body consumers. Retrieval uses unusable proxies;
loopback is bypassed for actual requests. This proves proxy independence, not
network isolation. No recurring native/client/OS matrix is added.

The complete block was checked starting from an unrelated directory:

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-5b4d9f99'
& ./eng/Test-WebResourceRecipes.ps1 `
  -Dotnet 'C:/Program Files/dotnet/dotnet.exe' `
  -Net11Dotnet 'C:/Users/pingu/.dotnet-playbook-11/dotnet.exe' -RecordEvidence
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('hosting.configure-http3-budgets', 'json-patch.serialize-operation-intent', 'testing.configure-web-application-builder') `
  -Query 'web-resource-recipes' -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
  -PackageVersion '0.0.0-pin1182-batch' -BuildAndPack `
  -Dotnet 'C:/Program Files/dotnet/dotnet.exe' -Node 'C:/Program Files/nodejs/node.exe' `
  -OutputDirectory ./artifacts/web-resources/inspection
& ./eng/Test-WebResourceConsumers.ps1 `
  -Dotnet 'C:/Program Files/dotnet/dotnet.exe' `
  -Net11Dotnet 'C:/Users/pingu/.dotnet-playbook-11/dotnet.exe'
```

Expect these discoveries and observations:

- `hosting.configure-http3-budgets`: **Configure HTTP/3 connection budgets without
  preview scaffolding**. Goal: set per-connection stream/buffer budgets and retain
  backpressure. Both real HTTP/3 consumers queue a third request behind two active
  requests on one connection, exhaust inbound unidirectional credit and pause/
  resume 16MiB uploads/downloads. The old CA2252 control warns; newer builds need
  no preview acknowledgement. No exact process-memory or application quota claim.
- `json-patch.serialize-operation-intent`: **Preserve JSON Patch operation
  serialization intent**. Goal: preserve required from/value intent and nulls,
  checking actual updates before removing custom rules. Six-operation round trips,
  wire members and HTTP failures pass. The current direct method call warns CS0618.
  New from metadata omits null, but retains a spurious non-null from. Both readers
  accept missing replace value as null; stricter validation remains app policy.
- `testing.configure-web-application-builder`: **Configure a web integration test
  through the application builder**. Goal: isolate early startup configuration/
  services and await fixture disposal. Actual separate entry assemblies/TestServer
  requests verify early/late order, production controls and async disposal; the
  newer hook rejects explicit IHostBuilder/IWebHostBuilder paths. Content-root
  attributes make the separate consumers independent of solution discovery; explicit
  service completion handles overlapping teardown. .NET10 keeps
  ConfigureWebHost; only older .NET11 preview fixtures used the renamed spelling.

Actual Windows x64 RC2 proof does not establish other OS/native lanes, strict
JSON Patch validation/security, physical TestServer network behavior or GA.
PIN-959/PIN-964, the eight foundations and unrelated Human gates stay unchanged.

## SDK servicing, partial ReadyToRun and native tool RIDs

The following block was verified starting from an unrelated directory. Setup
requires PowerShell7, Node22.19+, both exact SDK/runtime distributions below,
public NuGet access, MSVC14.51.36231/link14.51.36260.0 and Windows SDK10.0.26100.0
libraries. The focused check runs controlled advisory-cache cases (not a live
security verdict), full/selected PE publishing with JIT fallback, and real native
packing/install/run plus unsupported/missing-toolchain failures. It preserves
process/user/machine PATH and uses disposable homes/caches.

The Inspector builds/packs the ordinary tool once, installs that exact artifact
through an isolated local-only feed/cache, then calls real installed stdio list/get
from an unrelated directory. It displays ID/title/**goal**, full expanded Markdown,
target11/edition11, applicability and current paired sample evidence. Target10 must
return no_applicable_edition; these comparisons add no historical foundations.
Unusable proxies prove proxy independence, not OS-level network isolation.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-3263bd3f'
$env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
& 'C:/Program Files/dotnet/dotnet.exe' build PinguApps.Playbook.slnx -c Release -p:Version=0.0.0-pin1131-batch
& 'C:/Program Files/dotnet/dotnet.exe' pack ./src/PinguApps.Playbook -c Release --no-build -p:PackageVersion=0.0.0-pin1131-batch --output ./artifacts
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('sdk.check-sdk-servicing-status', 'sdk.choose-partial-readytorun-inputs', 'sdk.customize-native-tool-package-rids') `
  -Query 'skylark-build' -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
  -PackageVersion '0.0.0-pin1131-batch' `
  -Dotnet 'C:/Program Files/dotnet/dotnet.exe' -Node 'C:/Program Files/nodejs/node.exe' `
  -OutputDirectory ./artifacts/skylark-build/inspection
```

Expect all six consumers to pass. Current controlled vulnerability/stale and EOL
metadata warn NETSDK1238 and NETSDK1239; missing/corrupt data silently skips.
ReadyToRun checks inspect actual headers, partial selection and exclusions, including
zero unprofiled partial native methods versus a nonempty full control, then
execute direct=21/late=35/cold=SK-7 normally and with forced JIT. Native commands
create outer plus one win-x64 inner package, install locally and report crates=21,
architecture=X64, dynamic=False from a real native payload; unsupported/missing
toolchains fail. Candidate Arm64 defaults are evaluated separately, never advertised
as a delivered payload. No live advisory freshness, performance, other-platform,
GA, production resource or public-publication claim is made. PIN-959/PIN-963 and
unrelated Human gates remain open.

## Read a recipe

### VSTest host, resolver and custom task migrations

This block was exercised from an unrelated directory. Prerequisites: PowerShell7,
the two exact SDK/runtime installations below, Node22.19+ and public NuGet access
for setup. The first command captures the real old/current platform consumers,
compiler controls, test counts/cancellation/cleanup, extension loading and effective
settings, including the scalar semicolon failure and XML alternative. It preserves
process/user/machine PATH and uses isolated homes/caches.

The Inspector then builds/packs the ordinary tool, installs that exact artifact
through a fresh local-only feed/cache and calls real installed stdio list/get from
an unrelated directory. Both targets are explicit. It displays ID/title/**goal**,
complete canonical Markdown, edition11, applicability and current sample evidence;
target10 is a separately executed conditional public-package route. Unusable
proxies qualify proxy independence, not network isolation.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-d4b8d1f7'
& ./eng/Test-TernTesting.ps1 `
  -Net10Dotnet 'C:/Program Files/dotnet/dotnet.exe' `
  -Net11Dotnet 'C:/Users/pingu/.dotnet-playbook-11/dotnet.exe' -Record
$recipes = @(
  'testing.migrate-vstest-session-host',
  'testing.migrate-assembly-resolution-context',
  'testing.pass-custom-task-runsettings'
)
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId $recipes -Query 'tern-testing' -TargetVersion '11.0' `
  -PackageVersion '0.0.0-pin1128-vstest' -BuildAndPack `
  -Dotnet 'C:/Program Files/dotnet/dotnet.exe' -Node 'C:/Program Files/nodejs/node.exe' `
  -OutputDirectory ./artifacts/tern-testing/inspection11
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId $recipes -Query 'tern-testing' -TargetVersion '10.0' `
  -PackageVersion '0.0.0-pin1128-vstest' `
  -Dotnet 'C:/Program Files/dotnet/dotnet.exe' -Node 'C:/Program Files/nodejs/node.exe' `
  -OutputDirectory ./artifacts/tern-testing/inspection10
```

Expect four distinct discoveries; each ordinary/post-cancel host run has two
passes and one deliberate failure, cancellation has zero results, and observed
owned testhosts exit. Actual plugin resolution returns west:13 with its real
requestor, while the platform's .NET ALC path has a null requestor. Each custom
task selects/passes exactly one settings test with spaces, backslashes, separators
and switch-looking values unchanged. Old escaped literal semicolons survive;
the current scalar rejects them and its XML-file alternative passes.
Missing Tern settings must fail one selected test; XML verification also checks
the effective literal-semicolon value in the real TRX.

The resolver's old setter belongs to the audited SDK10 binary, **not** public18.7;
public18.9 already has the constructor. The current package consumer works on10/11.
No pooled-lifetime equivalence, MTP drop-in, general plugin security, VSTestTask2
runtime, Unix backslash behavior, arbitrary adapters/collectors or actual-GA claim
is made. PIN-959/PIN-963/Human gates and all eight foundations remain open.

The following example was checked from an unrelated directory. Change `-RecipeId` and `-TargetVersion` for the affected guidance; both list and get receive the same explicit target.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-4d84c6cb'
& ./eng/Inspect-Guidance.ps1 `
  -Dotnet 'C:\Program Files\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -TargetVersion '11.0' `
  -RecipeId 'blazor.configure-browser-from-server'
```

Expect `blazor.configure-browser-from-server`, its reusable title and goal, complete original .NET 10/11 TideDesk setup, and full target-11 requirements. There must be no unresolved `{{snippet:...}}` token. Compilation, browser execution and historical RC2 limits remain separate. Use `-NoBuild` only after a current Release build. Use `-OutputDirectory` for another disposable output location.

To inspect several changed recipes, pass an array to `-RecipeId`. Discovery follows all returned pages with unchanged filters before selecting those IDs. A missing ID, failed command or incomplete body fails the helper. The file output is the actual MCP response, including `result.structuredContent.recipe`; it can feed existing returned-body consumer checks:

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-4d84c6cb'
& ./eng/Test-TideDeskConsumer.ps1
```

That consumer uses the inspection response above, reconstructs both actual project pairs outside the checkout, compiles them with their declared SDKs, and checks real RC2 HTTP options. It requires both declared SDK installations. The report defaults to ignored `artifacts/tide-desk/consumer.json`; `-ResponsePath` and `-OutputPath` select alternatives. It does not repeat native browser reconnection.

## Identity UI, encoded routes and gRPC documents

These three original 11.0 editions share one ordinary package inspection. Setup
requires both exact sample SDK/runtime sets, PowerShell7, Node22.19+ and authenticated
GitHub CLI access to the public pinned generator release. The first command restores
the actual locked packages, verifies/acquires the Windows x64 generator, exercises
the paired samples/publishes and refreshes saved sample records. It does not create
fresh browser evidence. Existing browser observations remain valid only while their
relevant page/layout/CSS/package inputs remain unchanged.

The block was exercised from an unrelated directory. The Inspector builds/packs
once, installs that exact artifact through a fresh local feed/cache, and performs
real installed stdio list/get for **each** ID. It displays ID/title/goal, complete
canonical Markdown, target11/edition11, applicability and fresh evidence, compares
text/structured results, and checks target10's `no_applicable_edition` response.
The final command reconstructs all six original consumers solely from those
returned bodies. Prepared packages are reused with no remote NuGet feeds; remote
proxies are unusable and loopback is explicitly bypassed for the real requests.
This establishes proxy independence, not network isolation.

```powershell
Set-Location -LiteralPath 'V:/_worktrees/Playbook/agent-pin-1178-04-103-move-customized-identity-ui-off-bootstrap-4'
& ./eng/Test-WebMigrationBatch.ps1 `
  -Dotnet 'C:/Program Files/dotnet/dotnet.exe' `
  -Net11Dotnet 'C:/Users/pingu/.dotnet-playbook-11/dotnet.exe' `
  -OutputDirectory ./artifacts/batch04/verified -RecordEvidence
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('identity.migrate-ui-framework-selection', 'routing.preserve-encoded-segment', 'grpc.replace-swagger-adapter') `
  -Query 'web-migration-batch' -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
  -PackageVersion '0.0.0-pin1178-batch' -BuildAndPack `
  -Dotnet 'C:/Program Files/dotnet/dotnet.exe' -Node 'C:/Program Files/nodejs/node.exe' `
  -OutputDirectory ./artifacts/batch04/inspection
& ./eng/Test-WebMigrationConsumers.ps1 `
  -Dotnet 'C:/Program Files/dotnet/dotnet.exe' `
  -Net11Dotnet 'C:/Users/pingu/.dotnet-playbook-11/dotnet.exe'
```

Expect these discoveries:

- `identity.migrate-ui-framework-selection`: **Move customized Identity UI off
  Bootstrap 4**. Goal: keep customized sign-in pages rendering by selecting
  Bootstrap5 and migrating affected markup, styles and published assets.
- `routing.preserve-encoded-segment`: **Preserve encoded route segments in
  proxy-style requests**. Goal: retain encoded slashes inside identifiers and
  remove compensation only after actual wire-level comparison.
- `grpc.replace-swagger-adapter`: **Replace the retired gRPC Swagger integration**.
  Goal: keep the transcoded JSON interface discoverable using a generated,
  protobuf-derived build contract after adapter retirement.

The Identity check uses published compiled pages/assets and retains antiforgery,
confirmation and local-return behavior. The separate retained Chromium screenshots
qualify login/validation rendering for the older, explicit5 and default publishes;
the command block does not repeat a browser session. Routing proves the measured
old absolute-form decoding difference and current preservation, with raw parser,
middleware and handler observations. gRPC proves actual old adapter/new generator,
gRPC and JSON values, nonempty operations/schemas/comments and negative requests.
The replacement document is **Swagger2.0/OpenAPIv2**, not a runtime drop-in adapter
or an automatic AddOpenApi integration. Neither generic provider compatibility,
all Google annotation patterns, all browser/platforms nor actual-GA proof follows.
Eight foundations and PIN-959/PIN-963/PIN-964/Human gates remain unchanged.

## Generated static endpoint order

This builds/packs the ordinary tool and installs that exact artifact through the
existing isolated local feed/cache. Real installed stdio list/get execute from an
unrelated directory with unusable proxies, displaying ID/title/goal, full canonical
Markdown, target11/edition11, applicability and fresh paired sample evidence.
The extra get checks target10's `no_applicable_edition` response. Inspection needs
PowerShell7, SDK10.0.402 and Node22.19+. The reconstructed consumer also requires
SDK11.0.100-rc.2.26504.105 and its matching runtime, a preview RC2 baseline, not GA.
The block passes both installed SDK executables explicitly; adjust those arguments
for another installation. Setup may restore online. Retrieval proves proxy
independence, not network isolation. No global installation is required.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\agent-pin-1176-04-101-give-a-generated-static-endpoint-deliberate-route'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId 'web.order-static-asset-routes' -Query 'static-assets' `
  -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
  -PackageVersion '0.0.0-pin1176' -BuildAndPack `
  -Dotnet 'C:\Program Files\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory ./artifacts/pin1176/inspection
& ./eng/Test-AmberGalleryConsumer.ps1 `
  -Dotnet 'C:\Program Files\dotnet\dotnet.exe' `
  -Net11Dotnet 'C:\Users\pingu\.dotnet-playbook-11\dotnet.exe'
```

Expect `web.order-static-asset-routes`, title "Give a generated static endpoint
deliberate route precedence" and goal "Choose which static resource wins when
generated asset routes overlap, using explicit endpoint order and verifying the
resource actually served." Complete Markdown includes both original projects,
physical assets, boundary files, manifest acquisition/projection and numbered
migration. The final helper reconstructs only returned Markdown in a temporary
unrelated directory, builds with both exact SDKs and no remote NuGet feeds, then
executes actual manifest/HTTP/failure checks. It writes routine results under
ignored `artifacts/pin1176/consumer`.

Current results qualify default/explicit/invalid string order, numeric JSON
failure, equal-order ambiguity and frozen-descriptor rejection; .NET10 retains
its working convention alternative. Only the bounded generated-manifest consumer
and Windows x64 managed HTTP lane are qualified, with no general arbitrary-schema,
browser, published-layout or GA guarantee. The eight .NET10 foundations and
PIN-959/PIN-963/PIN-964/Human gates remain unchanged.

## Dynamic OpenAPI document names

This builds/packs the ordinary tool and installs that exact artifact through an
isolated local feed/cache. Real installed stdio list/get run from an unrelated
directory with unusable proxies and display ID/title/goal, complete canonical
Markdown, target11/edition11, applicability and fresh sample evidence. The extra
get checks target10's explicit `no_applicable_edition` response. Setup requires
PowerShell7, SDK10.0.402 and Node22.19+ and may restore online; retrieval proves
proxy independence, not network isolation. No global installation is needed.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\agent-pin-1174-04-099-generate-contracts-for-dynamically-named-api'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId 'openapi.discover-dynamic-documents' -Query 'dynamic-documents' `
  -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
  -PackageVersion '0.0.0-pin1174' -BuildAndPack `
  -Dotnet 'C:\Program Files\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory ./artifacts/pin1174/inspection
```

Expect title "Generate contracts for dynamically named API documents" and goal
"Discover all OpenAPI document names offered by an application when names come
from configuration, and generate matching runtime and build-time contracts."
The complete body contains both original PlanetariumContracts projects, named
options, resolver, runtime checks, generation commands and numbered migration.
Both public/staff documents have distinct paths/titles. Current controls establish
exact duplicate elimination, explicit plus additional names and missing/empty/null
behavior through the real built-in provider/GetDocument host. Older explicit
registrations remain valid; no downlevel resolver, arbitrary-provider, client,
browser or actual-GA claim is made. PIN-959/PIN-963/PIN-964 and Human gates stay open.

To refresh the bounded proof with both actual SDK/runtime/package sets:

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\agent-pin-1174-04-099-generate-contracts-for-dynamically-named-api'
& ./eng/Test-Planetarium.ps1 `
  -Dotnet 'C:\Program Files\dotnet\dotnet.exe' `
  -Net11Dotnet 'C:\Users\pingu\.dotnet-playbook-11\dotnet.exe' -RecordEvidence
```

Expect real runtime and build-time PASS lines on both targets, plus expected
empty/null-name generation rejection on .NET11. The helper restores its process
settings and checks all PATH scopes. Generated JSON and routine logs stay under
ignored artifacts; named ongoing sample records retain hashes and observations.

## Closed native build extensions

This block builds/packs the ordinary tool, installs that exact artifact through
the existing isolated local feed/cache, then calls real installed stdio list/get
from an unrelated directory. It displays ID/title/goal, complete canonical
Markdown, target11/edition11, applicability and both managed plus actual native
evidence. PowerShell7, SDK10.0.402 and Node22.19+ are required for inspection.
Setup may restore online; unusable proxies during retrieval establish proxy
independence, not network isolation. The inspection itself needs no native tools.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\agent-pin-1120-03-025-register-a-closed-set-of-build-extensions-in-an-aot'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId 'msbuild.register-aot-build-extensions' -Query 'native-build-extensions' `
  -TargetVersion '11.0' -PackageVersion '0.0.0-pin1120' -BuildAndPack `
  -Dotnet 'C:\Program Files\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory ./artifacts/pin1120/inspection
```

Expect "Register a closed set of build extensions in an AOT host", its reusable
goal, all project/SDK/task/resolver/logger/bootstrap/package snippets, numbered
migration, exact package/toolchain conditions and three fresh evidence records.
SDK10 retains the verified managed alternative. SDK11 uses public package18.11.0,
distinct from bundled18.12; native execution verifies the fixed set and missing
task/resolver MSB4283/MSB4282. IL2026 remains visible at the public build boundary.
No analyzer-clean/general AOT, down-level native, arbitrary plugin or actual-GA
claim is made. Target10 remains unsupported for this new registration recipe.

To refresh the actual paired/native proof with the exact prerequisites recorded
in the recipe and sample pins:

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\agent-pin-1120-03-025-register-a-closed-set-of-build-extensions-in-an-aot'
& ./eng/Record-NorthQuayBuildHost.ps1 `
  -Net10Dotnet 'C:\Program Files\dotnet\dotnet.exe' `
  -Net11Dotnet 'C:\Users\pingu\.dotnet-playbook-11\dotnet.exe'
```

This first restores both locked package closures, then invokes the existing
sample evidence workflow and real ILC/platform-linker publish/native execution.
An empty `-Packages <absolute-cache-directory>` is supported; no incidental older
native compiler cache is required. It uses absolute executables and child-only
native include/library inputs; PATH and persistent environment must stay unchanged.
The public Framework package supplies the eight trim feature settings, checked
in the actual ILC response. IL2026 is retained, not suppressed. The eight approved
foundations and PIN-959/PIN-963/actual-GA/Human gates remain open.

## Partial project evaluation

This block builds/packs the ordinary tool, installs that exact artifact through
an isolated local feed/cache, and calls real installed stdio list/get from an
unrelated directory. It displays ID/title/goal, complete canonical Markdown,
target11/edition11, applicability and both current museum host evidence records.
PowerShell7, SDK10.0.402 and Node22.19+ are required for inspection. Setup may
restore online; retrieval with unusable proxies proves proxy independence, not
network isolation. No global tool or PATH change is needed.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\agent-pin-1119-03-024-evaluate-only-the-project-information-a-tool-needs'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId 'msbuild.evaluate-project-stages' -Query 'partial-evaluation' `
  -TargetVersion '11.0' -PackageVersion '0.0.0-pin1119' -BuildAndPack `
  -Dotnet 'C:\Program Files\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory ./artifacts/pin1119/inspection
```

Expect title "Evaluate only the project information a tool needs", the reusable
inspection goal, complete museum project/import/readers/bootstrap and numbered
migration. SDK10 retains full evaluation; SDK11 proves all stages and guard
failures, default/explicit Full, real CLI property/item values and separate target
execution. In actual RC2 partial `Targets.Count` throws, superseding Preview7's
zero-count example. The host uses Locator1.11.2 with complete installed SDK closure;
no down-level partial, universal speed, other-platform or actual-GA claim is made.
Target10 remains unsupported for this new partial-evaluation recipe.

To refresh the bounded managed host proof with both complete SDK/runtime installs:

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\agent-pin-1119-03-024-evaluate-only-the-project-information-a-tool-needs'
$previousIntegration = $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH
try {
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
  & ./eng/Record-Evidence.ps1 -Sample 'samples/net10.0/MuseumInventory/MuseumInventory.csproj' `
    -Dotnet 'C:\Program Files\dotnet\dotnet.exe'
  & ./eng/Record-Evidence.ps1 -Sample 'samples/net11.0/MuseumInventory/MuseumInventory.csproj' `
    -Dotnet 'C:\Users\pingu\.dotnet-playbook-11\dotnet.exe'
}
finally { $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = $previousIntegration }
```

These sample records pin compiler/reference/native-host/runtime/MSBuild/SDK
resolver artifacts and record all loaded assemblies and actual SDK/import hashes.
The target-count guard and mutable factory failures are real execution evidence,
not zero counts fabricated for unavailable state. The exact eight foundations,
PIN-959/PIN-963/actual-GA and unrelated Human gates remain open.

## Typed custom build-task inputs

This block builds/packs the ordinary artifact and uses the existing isolated
local-only feed/cache Inspector harness. It invokes real installed stdio list/get
from an unrelated directory, displaying ID/title/goal, full canonical Markdown,
explicit target/edition, applicability and both fresh host evidence records.
Setup requires PowerShell 7, SDK 10.0.402 and Node 22.19+; setup can restore online.
Retrieval uses unusable proxies, proving proxy independence, not network isolation.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\agent-pin-1118-03-023-bind-typed-parameters-in-a-safe-custom-build-task'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId 'msbuild.bind-typed-task-inputs' -Query 'typed-task-inputs' `
  -TargetVersion '11.0' -PackageVersion '0.0.0-pin1118' -BuildAndPack `
  -Dotnet 'C:\Program Files\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory ./artifacts/pin1118/inspection
```

Expect title "Bind typed file inputs in a custom MSBuild task", edition 11.0,
target 11.0, experimental `-mt` conditions and two current compilation/execution
records. Markdown retains the explicit-path older task, typed newer task,
environment-dependent defaults, same-binary constructor fallback, numbered
migration and exact-host limits. The saved runtime proof covers actual concurrent
same-process/different-thread task execution, metadata, unchanged task process
state, ordinary/sidecar controls and real missing-file failures. No down-level
typed-task, server-reuse/performance or actual-GA route is claimed.

To refresh the task evidence, run SDK 10 first because the SDK 11 constructor
check executes that same older compatibility DLL. Neither command installs a
global tool or changes PATH; SDK 11 and its matching runtime must already exist:

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\agent-pin-1118-03-023-bind-typed-parameters-in-a-safe-custom-build-task'
$previousIntegration = $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH
try {
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
  & ./eng/Record-Evidence.ps1 -Sample 'samples/net10.0/ObservatoryTasks/ObservatoryTasks.csproj' `
    -Dotnet 'C:\Program Files\dotnet\dotnet.exe'
  & ./eng/Record-Evidence.ps1 -Sample 'samples/net11.0/ObservatoryTasks/ObservatoryTasks.csproj' `
    -Dotnet 'C:\Users\pingu\.dotnet-playbook-11\dotnet.exe'
}
finally { $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = $previousIntegration }
```

PIN-959/PIN-963 and unrelated Human gates remain open. There is no recurring
native/client/OS matrix for this managed Windows x64 recipe.

## Typed VSTest message readers

This tested block builds/packs the ordinary tool, installs the exact package in
an isolated local feed/cache, then calls installed stdio list/get from an
unrelated directory. It prints ID/title/goal, complete canonical Markdown,
explicit target/edition, applicability and all three current evidence records.
PowerShell7, SDK 10.0.402 and Node22.19+ are required; setup may use the network.
Retrieval uses unusable proxies, proving proxy independence, not network isolation.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\agent-pin-1073-03-021-read-vstest-messages-through-the-supported-typed'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId 'testing.migrate-vstest-message-readers' -Query 'vstest-messages' `
  -TargetVersion '11.0' -PackageVersion '0.0.0-pin1073' -BuildAndPack `
  -Dotnet 'C:\Program Files\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory ./artifacts/pin1073/inspection
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId 'testing.migrate-vstest-message-readers' -Query 'vstest-messages' `
  -TargetVersion '10.0' -PackageVersion '0.0.0-pin1073' `
  -Dotnet 'C:\Program Files\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory ./artifacts/pin1073/inspection10
```

Expect title "Read VSTest messages through the supported typed contract",
edition 11.0 and a conditional target10 route. The laboratory examples retain
actual 18.7/18.11 package and SDK/runtime distinctions, typed TestCase/traits/
TestResult/ITestRunStatistics checks, protocol/header/failure semantics and the
concrete-statistics limitation. Custom resolver/converter extensions have no
replacement promised. `eng/Test-VSTestMessages.ps1 -Record` captures the separate
real published-consumer, foreign-wire, removed-symbol and ABI proof with both
healthy pinned SDK executables. Retrieval is not actual-GA or arbitrary test-host
integration evidence; PIN-959/PIN-963 and independent Human gates remain open.

## Deliberate database-provider replacement

This tested example selects the PIN-1190 worktree from an unrelated directory,
builds/packs the ordinary tool and installs that exact package through an isolated
local feed/cache. The helper calls actual installed stdio `list_recipes` and
`get_recipe`, showing ID/title/goal, complete canonical Markdown, target/edition,
applicability and fresh evidence. Setup requires PowerShell7, SDK10.0.402 and
Node22.19+ and may restore packages online. Retrieval with unusable proxies proves
proxy independence, not network isolation. No global tool or PATH change is needed.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\agent-pin-1190-05-049-replace-a-configured-database-provider-deliberately'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId 'ef.replace-context-provider' -Query 'provider-replacement' `
  -TargetVersion '11.0' -PackageVersion '0.0.0-pin1190' -BuildAndPack `
  -Dotnet 'C:\Program Files\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory ./artifacts/pin1190/inspection
```

Expect `ef.replace-context-provider`, title "Replace a configured database provider
deliberately", its test-configuration goal, target11/edition11 and two current
ReedPermits evidence records. Full Markdown includes both complete original
projects. Provider proof covers actual SQLite/InMemory CRUD, retained/mixed
configuration failures and resolved lifetimes; InMemory is not relational proof.
The options helpers are separate infrastructure operations. Other providers,
platforms, factories/pooling, trimming/AOT and actual GA remain unverified.

## Custom client rules and component cache contracts

This block builds/packs the ordinary tool and uses the existing isolated local-feed/cache
Inspector harness. It changes to an unrelated temporary directory before calling
the exact installed executable over stdio, and displays both recipes' ID, title,
goal, complete Markdown, applicability and current compilation/execution evidence.
Setup requires PowerShell 7, SDK 10.0.402 and Node 22.19+; Inspector 2.11.0 is
installed locally when absent. Setup may use the network; retrieval uses unusable
proxies, which demonstrates proxy independence rather than network isolation.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-8fca8201'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('blazor.export-custom-client-validation-rules','blazor.declare-component-cache-safety') `
  -Query 'garden-library' -PackageVersion '0.0.0-garden-library' `
  -Dotnet 'C:\Program Files\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' -BuildAndPack `
  -OutputDirectory ./artifacts/garden-library/inspection
```

Expect two explicit `11.0` editions, full canonical-source equality, complete
static-SSR requirements and four current evidence records per recipe. The custom
rule body includes the .NET 10 duplicate handler and .NET 11 provider/JS mapping;
the cache body includes live parameter replay and refusal after initialization.
These checks deliver the guidance; the paired real Chromium/HTTP evidence comes
from `eng/Record-GardenLibrary.ps1`. RC2 is not GA proof. No native or distributed
provider behavior is claimed. The helper retains its unique temporary install/feed
for inspection and restores process settings; it does not install a global tool.

## JavaScript recovery and InputFile cleanup

For JavaScript failure recovery and derived InputFile cleanup, this tested block
builds/packs the ordinary tool, installs that exact package through the isolated
local feed/cache, and calls real stdio list/get from an unrelated temporary
directory. PowerShell 7, SDK 10.0.402 and Node 22.19+ are required for inspection;
setup may use the network. Retrieval uses unusable proxies, which proves proxy
independence rather than network isolation.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-e039c0ab'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('blazor.handle-js-invocation-failures','blazor.dispose-input-file-subclass-resources') `
  -Query 'owned-lifetimes' -PackageVersion '0.0.0-owned-lifetimes' `
  -Dotnet 'C:\Program Files\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' -BuildAndPack `
  -OutputDirectory ./artifacts/owned-lifetimes/inspection
```

Expect both IDs/titles/goals, explicit 11.0 editions, complete expanded canonical
.NET 10/11 files, applicability and four fresh compilation/execution records per
recipe. Recovery preserves application errors; teardown distinguishes RC2's
internally guarded reference disposal from an actual disconnected module call.
InputFile cleanup preserves ordinary selection and its framework callback relay.
The paired browser proof is separate from retrieval, and neither establishes GA.
With both SDKs and the measured Chromium/Playwright installations available,
`eng/Test-OwnedLifetimesConsumer.ps1` reconstructs all four projects solely from
these responses and reruns their real browser/renderer checks. The script's
`-Playwright` and `-Chromium` parameters take the absolute installed paths.

## Live identity tools

For the live identity recipes, this block builds/packs the ordinary tool and
uses the existing isolated local-feed/cache installation. It calls real stdio
list/get from an unrelated directory and prints discovery ID/title/goal, complete
canonical Markdown, applicability and fresh sample evidence. The walkthrough was
launched from an unrelated directory. Setup may restore/download packages;
retrieval uses unusable proxies, which establishes proxy independence rather
than network isolation.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-2784b7c8'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('signalr.refresh-live-credentials','blazor.react-to-refreshed-identity') `
  -Query '' -PackageVersion '0.0.0-live-tool-contracts' `
  -Dotnet 'V:\_worktrees\Playbook\t3-2784b7c8\artifacts\identity-tooling\pinned-dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' -BuildAndPack `
  -OutputDirectory ./artifacts/identity-tooling/inspection
```

Expect two explicit 11.0 editions, original .NET 10/RC2 consumers, the complete
TypeScript development source-build prerequisite, local identity/provider limits
and current compilation/execution records. `eng/Test-LiveIdentityConsumer.ps1`
reconstructs the .NET and TypeScript consumers solely from these installed
responses and executes their real hub and Interactive Server checks; supply
absolute `-Playwright`, `-Chromium`, `-Dotnet` and `-Node` paths. This is bounded
local execution, not Azure SignalR Service, production authentication or GA proof.
The task-local SDK root above preserves the recorded 10.0.402/RC2 baselines
after the machine's installation advanced to 10.0.403/RC3. An equivalent installed
root containing those exact SDKs and runtimes can be supplied instead.

## Runtime overrides and SDK selection for tool execution

From PowerShell 7 with SDK 10.0.402 and Node 22.19+, this builds/packs the
ordinary tool, then reuses the isolated local-feed/cache harness to inspect that
exact artifact over real stdio from an unrelated temporary directory. Inspector
2.11.0 is installed locally if missing. Setup may restore packages online;
retrieval uses unusable proxies (proxy independence, not network isolation).

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-72e14ee7'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('deployment.control-runtime-overrides','sdk.select-tool-execution-sdk') `
  -Query 'host-selection' -PackageVersion '0.0.0-host-selection' `
  -Dotnet 'C:\Program Files\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' -BuildAndPack `
  -OutputDirectory ./artifacts/host-selection/inspection
```

Expect both IDs/titles/goals, explicit edition
11.0, complete expanded canonical Markdown, applicability and four current
compilation/execution records per recipe. The second recipe correctly reports
introduction 10.0: wrapper bypass was serviced into 10.0.302/10.0.400 and was
executed here on 10.0.402 and RC2. This adds no target-10 foundation. The existing
helper's name is historical; its recipe-ID/query parameters accept both recipes.

Actual behavior is separate from retrieval. `eng/Test-RuntimeOverrides.ps1`
executes the original .NET 10/11 samples with isolated matching hostfxr/runtime,
duplicate and nonduplicate properties, source regeneration, Release dev-file
generation/copy and a fresh production artifact allowlist.
`eng/Test-ToolSdkSelection.ps1` executes both original local packages, three SDK
pins, both shipped Windows wrappers, exact quoted arguments and failure exits.
These require the declared Windows x64 SDK/runtime installations; parameters
accept alternate absolute installation paths. SDK 9 is a negative command case
only. Unix wrapper execution and actual .NET 11 GA reconciliation remain unverified.
## Nested EF scalars and primitive sequences

This block builds/packs once, runs the existing installed MCP smoke scenario,
then uses that same artifact through the existing local-feed/cache Inspector
harness. It was checked starting from an unrelated directory. Requires PowerShell
7, SDK 10.0.402 and Node 22.19+; the standalone consumer checks additionally need
the declared .NET 10 and .NET 11 RC2 SDKs/packages. Setup may restore packages
online. Retrieval uses unusable proxies, demonstrating proxy independence rather
than network isolation. No external database or account is needed.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-3101a2fc'
& ./eng/Test-Smoke.ps1 -PackageVersion '0.0.0-ef-models'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('ef.configure-nested-scalar','ef.configure-primitive-sequence') `
  -Query 'ef-models' -PackageVersion '0.0.0-ef-models' `
  -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory ./artifacts/ef-models/inspection
```

Despite the helper's historical name, its recipe/query parameters reuse the same
packed stdio path for these EF recipes. Expect both IDs/titles/goals, explicit
11.0 editions, full expanded original .NET 10/11 projects, applicability and two
current compile-and-run records per recipe. EstuaryMarkers configures the nested
`marker_code`; KilnReadings distinguishes primitive JSON sequences from a binary
scalar. The inspection asserts full canonical body equality and matching
applicability; it does not rerun SQLite. The sample records establish real Windows
x64 SQLite model/schema/round-trips using EF 10.0.13 and EF11 RC2. Other providers,
native platforms, production data conversion and actual GA remain unverified.

## Package identity migration and restore sources

From PowerShell 7 with SDK 10.0.402 and Node 22.19+, this builds/packs the
ordinary tool and reuses the isolated local-feed/cache Inspector harness to call
that exact installed artifact from an unrelated temporary directory. Inspector
2.11.0 is installed locally if absent. Setup may use the network; retrieval uses
unusable proxies, demonstrating proxy independence rather than network isolation.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-fa570270'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('nuget.migrate-package-identities','nuget.restore-from-declared-sources') `
  -Query 'nuget-migration' -PackageVersion '0.0.0-nuget-migration' `
  -Dotnet 'C:\Program Files\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' -BuildAndPack `
  -OutputDirectory ./artifacts/nuget-migration/inspection
```

Expect both discovery IDs/titles/goals, explicit 11.0 editions, full original
.NET 10/11 canonical projects/code, applicability and four current compilation/
execution records per recipe. The helper's historical name does not restrict its
recipe parameters. Identity guidance distinguishes legacy-invalid punctuation
from NU5052 and proves direct/transitive migration. Restore guidance records
selected and executed versions for direct/transitive, source/cache/failure cases.
`eng/Test-NuGetMigration.ps1` reproduces the paired local-feed behavior using the
declared SDK/runtime installations; its parameters accept alternate absolute paths.
No public feed operation, other OS execution or actual GA verification is claimed.
Sample SDKs/packages remain author-maintained under the dependency policy.

## EF query adapters and UInt128 parameters

This checked example starts from an unrelated directory, selects this worktree,
builds/packs the ordinary tool and uses the existing isolated feed/cache harness.
It displays ID/title/goal, complete canonical Markdown, applicability and current
sample evidence through real installed stdio list/get. Requires PowerShell 7,
SDK 10.0.402 and Node 22.19+; the separate EF1004 reproduction additionally needs
SDK 11.0.100-rc.2.26504.105. Setup can restore online. Retrieval uses unusable
proxies, demonstrating proxy independence rather than network isolation.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-5b34b81d'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('ef.stream-query-without-reexecution','sqlite.bind-large-unsigned-value') `
  -Query 'data-adapters' -PackageVersion '0.0.0-data-adapters' -BuildAndPack `
  -Dotnet 'C:\Program Files\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory ./artifacts/data-adapters/inspection11
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId 'sqlite.bind-large-unsigned-value' -TargetVersion '10.0' `
  -Query 'data-adapters' -PackageVersion '0.0.0-data-adapters' `
  -Dotnet 'C:\Program Files\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory ./artifacts/data-adapters/inspection10
```

Expect both stable IDs at edition 11.0 and the conditional direct-Sqlite11 route
on target 10.0; no EF11-on-.NET10 route is advertised. Five saved real consumers
cover EF command/disposal and UInt128 padded TEXT/order checks. EF1004 is separately
reproduced by `eng/Test-EfQueryAdapter.ps1`. The EF body explains runtime11's
adapter fast path and SQLite's synchronous native I/O. The UInt128 body retains
explicit string reading/application parsing. Neither retrieval nor these Windows
x64 RC2 consumers establishes other native platforms/providers or actual GA.

## Embedded template-engine hosts

From an unrelated directory, this selects the exact PIN-1071 worktree, builds and
packs the ordinary tool, installs that artifact in a fresh local-only feed/cache,
then displays real stdio discovery ID/title/goal, full canonical Markdown,
applicability, edition and fresh evidence for both advertised targets. Requires
PowerShell 7, SDK 10.0.402 and Node; sample reproduction also needs SDK/runtime 11
RC2 and the public package feeds. Retrieval uses unusable proxies, proving proxy
independence rather than network isolation.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\agent-pin-1071-03-019-retarget-an-embedded-template-engine-host'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId 'sdk.retarget-template-engine-hosts' -Query 'template-engine' `
  -PackageVersion '0.0.0-template-host' -BuildAndPack `
  -Dotnet 'C:\Program Files\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory ./artifacts/template-host/inspection11
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId 'sdk.retarget-template-engine-hosts' -Query 'template-engine' `
  -TargetVersion '10.0' -PackageVersion '0.0.0-template-host' `
  -Dotnet 'C:\Program Files\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory ./artifacts/template-host/inspection10
```

Expect edition 11.0 on both targets, the conditional newer-package net10 route,
and four current compilation/execution records. The body distinguishes NU1701
implicit fallback from eight strict NU1202 failures, preserves the old netstandard
host library, and demonstrates generated net10 applications from both modern host
TFMs. `eng/Test-TemplateEngineHost.ps1` reproduces the real package/host/generated
consumer checks with absolute SDK parameters. Framework, other OS execution and
actual GA remain unverified; PIN-959/PIN-963 remain open. All new sample dependencies
are author-maintained, outside automated root-package update permissions.

## SQLite target and retained dependency

This checked block starts from an unrelated directory, selects the assigned
worktree, builds/packs the ordinary tool and reuses the existing isolated
local-feed/cache harness. Setup needs PowerShell7, SDK10.0.402 and Node22.19+;
Inspector2.11.0 is installed locally if absent. Setup can restore online;
retrieval uses unusable proxies (proxy independence, not network isolation).

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\agent-pin-863-05-021-keep-low-level-sqlite-access-on-a-supported-framework'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId 'sqlite.retain-supported-runtime' -TargetVersion '11.0' `
  -Query 'sqlite-runtime' -PackageVersion '0.0.0-sqlite-runtime' -BuildAndPack `
  -Dotnet 'C:\Program Files\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory ./artifacts/sqlite-runtime/inspection11
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId 'sqlite.retain-supported-runtime' -TargetVersion '10.0' `
  -Query 'sqlite-runtime' -PackageVersion '0.0.0-sqlite-runtime' `
  -Dotnet 'C:\Program Files\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory ./artifacts/sqlite-runtime/inspection10
```

Expect the ID/title/goal, retained edition11.0 on both explicit targets, full
canonical projects and Markdown, exact applicability and five current evidence
records. Target10 includes the conditional Sqlite11 route and retained Sqlite10
comparison. Sqlite11 uses net10.0 assets; EF11 still requires net11.0. The net48
alternative has real Windows Framework EXE execution, not a selectable catalogue
target. No UWP/classic Xamarin native execution or actual-GA proof is claimed.
`eng/Record-SqliteRuntime.ps1` separately recaptures the four real consumers with
the declared SDK10/SDK11 and Windows Framework runtime. Retrieval does not rerun SQL.

## Test and extension JSON dependencies

This verified block selects the assigned worktree from an unrelated directory,
builds/packs the ordinary tool, installs that exact package in an isolated local
feed/cache and calls real stdio list/get. Setup requires PowerShell7,
SDK10.0.402 and Node22.19+; the helper installs Inspector2.11.0 locally if absent.
Restores/setup use public packages; retrieval uses unusable proxies (proxy
independence, not network isolation).

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\agent-pin-1072-03-020-own-json-dependencies-in-test-projects-and-extensions'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId 'testing.declare-json-dependencies' -TargetVersion '11.0' `
  -Query 'json-dependencies' -PackageVersion '0.0.0-json-dependencies' -BuildAndPack `
  -Dotnet 'C:\Program Files\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory ./artifacts/json-dependencies/inspection11
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId 'testing.declare-json-dependencies' -TargetVersion '10.0' `
  -Query 'json-dependencies' -PackageVersion '0.0.0-json-dependencies' `
  -Dotnet 'C:\Program Files\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory ./artifacts/json-dependencies/inspection10
```

Expect ID/title/goal, edition11.0 on both explicit targets, full original
Markdown/canonical snippets, the conditional newer-package target10 route and
seven current compilation/execution records. The body distinguishes old
transitive JSON, explicit runtime ownership, actual adapter/collector loading,
missing-direct/runtime failures, SDK-root copies and VSTest/MTP selection.
`eng/Test-JsonDependencies.ps1 -Record` separately captures the actual managed
consumer checks with its explicit SDK10/SDK11 parameters. Native CodeCoverage,
other operating systems and actual GA remain unverified; PIN-959/PIN-963 stay open.
The seven sample projects and their exact locks remain author-maintained under
the existing sample dependency policy; no new automatic update permission is added.

## Retired SQLite native bundle initialization

This block was executed from an unrelated directory. It builds/packs the ordinary
tool and reuses the isolated local-feed/cache harness for real installed stdio
list/get. Requires PowerShell7, SDK10.0.402 and Node22.19+; Inspector2.11.0 is
installed locally if absent. Setup may restore online. Retrieval uses unusable
proxies, proving proxy independence rather than network isolation.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\agent-pin-864-05-022-restore-native-sqlite-initialization-after-bundle'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId 'sqlite.replace-retired-native-bundle' -TargetVersion '11.0' `
  -Query 'sqlite-initialization' -PackageVersion '0.0.0-sqlite-initialization' -BuildAndPack `
  -Dotnet 'C:\Program Files\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory ./artifacts/sqlite-initialization/inspection
```

Expect ID/title/goal, edition11.0, full canonical .NET10/.NET11 files and prose,
Windows system-engine applicability and three current sample evidence records.
The older bundle comparison is not an additional .NET10 foundation or selectable
current target route. `eng/Record-SqliteInitialization.ps1` separately recaptures
actual Windows x64 native initialization, committed debit/reopen, password refusal
and omitted-initialization failure using SDK10.0.402 and SDK11 RC2. That helper
records the loaded DLL/version/hash; retrieval does not execute SQL. Encryption,
other platforms and actual GA remain unverified. No paid provider is selected.

## Protect data into owned storage

This checked block starts from an unrelated directory, selects the PIN-1171
worktree, builds/packs the ordinary tool and installs that exact artifact in the
existing isolated local feed/cache. It calls real stdio list/get and displays the
ID/title/goal, full canonical Markdown, explicit target/edition, applicability and
fresh evidence. Setup requires PowerShell7, SDK10.0.402 and Node22.19+; paired
sample reproduction also requires SDK11.0.100-rc.2.26504.105 at its declared path.
Setup may restore online; retrieval uses unusable proxies, demonstrating proxy
independence rather than network isolation. Inspector2.11.0 is installed locally.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\agent-pin-1171-04-096-protect-data-directly-into-an-owned-buffer'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId 'security.protect-into-buffer' -TargetVersion '11.0' `
  -Query 'owned-buffer' -PackageVersion '0.0.0-museum-buffer' -BuildAndPack `
  -Dotnet 'C:/Program Files/dotnet/dotnet.exe' `
  -Node 'C:/Program Files/nodejs/node.exe' `
  -OutputDirectory ./artifacts/museum-buffer/inspection
```

Expect edition11.0, two current real compilation/execution records, complete
original .NET10 array/.NET11 span/fallback source and advanced AAD checks. The
actual Windows x64 framework CNG key encryptor handles token/empty/1MiB recovery;
wrong purpose/key ring, tampering and changed AAD reject and clear failed output.
Target10 has no applicable span edition; the old array comparison remains in the
11.0 body. No universal allocation count, other backend/platform or actual-GA
claim is made. PIN-959/PIN-964 and existing Human gates remain open. Samples stay
author-maintained under the existing dependency boundary.

## SQLite time components

This block was checked starting from an unrelated directory. It selects the exact
assigned worktree, builds/packs the ordinary tool, then uses the existing isolated
local-feed/cache harness for real installed stdio list/get. Requires PowerShell7,
SDK10.0.402 and Node22.19+; Inspector2.11.0 installs locally if absent. Setup may
restore online. Retrieval uses unusable proxies, proving proxy independence rather
than network isolation. Separate sample execution also needs SDK/runtime11 RC2.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\agent-pin-1210-05-069-filter-sqlite-rows-by-time-components'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId 'sqlite.query-time-components' -TargetVersion '11.0' `
  -Query 'time-components' -PackageVersion '0.0.0-sqlite-components' -BuildAndPack `
  -Dotnet 'C:/Program Files/dotnet/dotnet.exe' `
  -Node 'C:/Program Files/nodejs/node.exe' `
  -OutputDirectory ./artifacts/sqlite-components/inspection
```

Expect ID/title/goal, explicit target11/edition11, complete canonical EF10/EF11
Markdown, applicability and two current compilation/execution records. The older
parameterized SQL remains server-side; the current predicates execute `%H`/`%M`/`%S`
through SQLite strftime. Native versions3.53.3/3.53.4, stored fractions/boundaries,
exact results and translation failures are captured by the separate HeronSignals
sample records. Inspection does not rerun those databases. No EF11-on-net10,
additional foundation, arbitrary precision/timezone, other-platform or actual-GA
claim is added; PIN-959/PIN-965 and existing Human gates remain open.

## Supply an application-specific cross-origin verdict

This checked block starts from an unrelated directory, selects the exact assigned
worktree, builds/packs the ordinary tool and installs that artifact in the existing
isolated local feed/cache. Real stdio list/get displays discovery ID/title/goal,
full canonical Markdown, explicit edition/target, applicability and fresh evidence.
Setup requires PowerShell7, SDK10.0.402 and Node22.19+; Inspector2.11.0 installs
locally when absent. Setup can restore online; retrieval uses unusable proxies
(proxy independence, not network isolation). Browser/paired consumer reproduction
additionally needs the declared SDK11 RC2 and Chrome; retrieval does not run them.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\agent-pin-1172-04-097-supply-an-application-specific-cross-origin-verdict'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId 'web.supply-origin-protection-policy' -TargetVersion '11.0' `
  -Query 'seed-exchange' -PackageVersion '0.0.0-seed-exchange' -BuildAndPack `
  -Dotnet 'C:/Program Files/dotnet/dotnet.exe' `
  -Node 'C:/Program Files/nodejs/node.exe' `
  -OutputDirectory ./artifacts/seed-exchange/inspection
```

Expect edition11.0, complete original .NET10 token+origin/.NET11 custom/default/token
source, exact applicability and two current compile-and-run records. The helper
asserts canonical body and structured/JSON-text equality. The custom rule trusts
operator-owned exact origins independently of CORS reads; JSON and token-required
clients remain separate. `eng/Record-SeedExchange.ps1` requires explicit absolute
`-Net10Dotnet` and `-Net11Dotnet` executable paths and recaptures actual matching
consumer/HTTP proof; the existing form evidence document retains actual Chrome156
loopback controls. No net10 ICsrfProtection route is advertised: explicit target10
discovery has no matching edition and get returns an actionable no-applicable-edition
error. No production authentication, other browser/OS, proxy or actual-GA claim is
made; PIN-959/PIN-964 and existing Human gates remain open. These isolated samples
remain author-maintained; no additional automatic dependency permission is granted.

## Inspect a response directly

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-4d84c6cb'
$response = Get-Content -LiteralPath './artifacts/inspection/blazor.configure-browser-from-server.json' -Raw | ConvertFrom-Json
$recipe = $response.result.structuredContent.recipe
$recipe.markdown
$recipe.applicability | ConvertTo-Json -Depth 40
$recipe.evidence | Select-Object sample, kind, target, sdk, freshness | Format-Table
$recipe.humanProof | Format-List
```

Discovery is a compact selection summary: primary/optional routes, version conditions, restrictions, notes, opt-ins and stability warnings. `get_recipe` supplies full reasons, prerequisites, constraints, sources and evidence. Missing summary fields do not mean unrestricted compatibility. Requirements describe the consumer; exact SDK/package/toolchain pins describe recorded verification, not mandatory upgrades.

Both tools require `targetVersion` independently. Inspect the application's target frameworks and project/`global.json` context first. Supported aliases include `10`, `10.0`, `net10.0`, `11`, `11.0` and `net11.0`. Deliberate `latest` is for exploration/unknown targets; it selects the newest installed catalogue release and proves no consumer compatibility. The helper preserves the target as a JSON string.

For a direct Inspector call, use the same absolute Node, Inspector entry point and .NET/server paths established by the helper. For an installed artifact, substitute its absolute `playbook.exe mcp` command for `dotnet <server.dll> mcp`; see [local package installation](contributing.md#inspect-the-development-package). Direct Node launch avoids the Windows npx `inspector is not recognized` wrapper failure. Inspector exits nonzero for a tool error; do not treat an empty response as success.

## Choose further checks

Use the [contribution policy](contributing.md#choose-the-check-and-retain-its-result) for capture/refresh and retention. Inspection verifies delivery, not browser/provider/native execution or release readiness. Saved `freshness: current` means inputs matched at package build time; it does not inspect the consumer or current upstream APIs. Pending Human proof stays visible separately from compilation.

`eng/Test-Content.ps1`, `eng/Test-Evidence.ps1` and their `-Pack` variants exercise content and freshness rejection. The existing installed-package scenarios cover filter combinations, paging, release selection, complete structured/text equality, recovery and errors. Run those when changing the corresponding contract; do not copy their test catalogue into a ticket walkthrough or repeat a platform matrix for a link edit.

## Parsed API explorer route contracts

This tested block builds/packs the ordinary tool and installs that exact artifact
through the existing isolated local-feed/cache harness. Real installed stdio
list/get runs from an unrelated temporary directory and displays ID/title/goal,
complete canonical Markdown, target/edition, applicability and both current
compilation/execution records. Setup requires PowerShell7, SDK10.0.402 and
Node22.19+ and can restore online. Retrieval uses unusable proxies: this proves
proxy independence, not network isolation. No global tool or PATH change occurs.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\agent-pin-1173-04-098-read-parsed-route-contracts-in-an-api-explorer'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId 'web.inspect-api-route-pattern' -Query 'route-pattern' `
  -TargetVersion '11.0' -PackageVersion '0.0.0-pin1173' -BuildAndPack `
  -Dotnet 'C:\Program Files\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory ./artifacts/pin1173/inspection
```

Expect "Read parsed route contracts in an API explorer extension", edition11.0,
target11.0, complete .NET10/11 MarineRoutes files and fresh evidence. Each actual
consumer discovers eight MVC/Minimal routes and checks constraints/defaults/
optional/catch-all, nullable/legacy controls and 18 real HTTP outcomes. .NET10
retains bounded source-template parsing; the new parsed property is unavailable
there. Other providers can leave it null. No native/browser matrix, universal
provider support or actual-GA claim; PIN-959/PIN-964 and Human gates stay open.

## Registration confirmation policy

This builds/packs the ordinary tool and installs that exact artifact through the
existing isolated local feed/cache. Real installed stdio list/get from an unrelated
directory display ID/title/goal, complete canonical Markdown, target11/edition11,
applicability and current paired compile/execution evidence. Target10 correctly has
no applicable edition; its working original pages are an executed comparison in
edition11, preserving exactly eight foundations. Retrieval uses unusable proxies:
proxy independence, not network isolation.

The block was executed from an unrelated directory. Setup needs PowerShell7,
SDK10.0.402 and Node22.19+; the final reconstruction also needs SDK11 RC2 and the
matching restored locked packages. Online package setup precedes offline retrieval.
No global tool, production account, messages or third-party provider is required.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\agent-pin-1177-04-102-keep-registration-aligned-with-confirmation-policy'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId 'identity.confirm-before-registration-signin' -Query 'confirmation' `
  -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
  -PackageVersion '0.0.0-pin1177' -BuildAndPack `
  -Dotnet 'C:/Program Files/dotnet/dotnet.exe' `
  -Node 'C:/Program Files/nodejs/node.exe' `
  -OutputDirectory ./artifacts/pin1177/inspection
& ./eng/Test-AquariumVolunteersConsumer.ps1 `
  -Dotnet 'C:/Program Files/dotnet/dotnet.exe' `
  -Net11Dotnet 'C:/Users/pingu/.dotnet-playbook-11/dotnet.exe'
```

Expect `identity.confirm-before-registration-signin`, title "Keep registration
aligned with confirmation policy" and goal "Require the configured email, phone
or account confirmation before granting a new registrant a session, across
password and external registration." Both original full projects, lock files and
numbered migration steps must be returned without unresolved snippet tokens.
The final helper recreates only returned files in an unrelated temporary directory,
builds both exact closures without remote feeds and runs 32 actual new-registration
checks plus later confirmation/sign-in and antiforgery/provider/return controls.
It observes the .NET10.0.13 packaged email/phone bypass honestly; the original
CanSignInAsync-gated pages and .NET11 packaged UI defer sessions correctly.
Controlled local external authentication is not third-party interoperability;
RC2 and local SQLite/HTTP evidence are not production, browser or actual-GA proof.

## Structured assembly-conflict and server events

From an unrelated directory, run the existing installed-package Inspector with the
exact SDK10 executable (the SDK11 fixture is a separate authoring prerequisite):

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\agent-pin-1121-03-026-consume-structured-assembly-conflict-and-server'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId 'msbuild.inspect-structured-build-events' -Query 'structured assembly' `
  -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
  -PackageVersion '0.0.0-pin1121' -BuildAndPack `
  -Dotnet 'C:/Program Files/dotnet/dotnet.exe' `
  -Node 'C:/Program Files/nodejs/node.exe' `
  -OutputDirectory ./artifacts/pin1121/inspection
```

Expect ID `msbuild.inspect-structured-build-events`, title "Consume structured
assembly-conflict and server events", its explanatory goal, edition11.0 and target11.0,
complete canonical Markdown, applicability and current SDK10/11 sample evidence.
The helper builds/packs the ordinary tool, installs that exact artifact through a
local-only feed/fresh cache, then runs real stdio list/get from an unrelated temp
directory with unusable proxies. This proves proxy-independent offline retrieval,
not network isolation. No target10 typed route, unsupported server embedding,
short-lived experimental server execution, universal reuse/performance or actual-GA
claim is made. PIN-959/PIN-963/Human gates remain open.

## NuGet staging uploads

This builds/packs the ordinary tool, installs that exact package through an
isolated local feed/cache and calls real stdio list/get from an unrelated
directory. It prints ID/title/goal, the complete canonical Markdown, target11 /
edition11, applicability and fresh paired evidence. The target10 request must
return no_applicable_edition: the older valid ordinary-publication comparison
stays in the target11 body and is not staging support.

The block was executed from an unrelated directory. Refresh paired evidence with
`eng/Record-FoundryStaging.ps1` on both complete pinned Windows x64 SDK/runtime/ref
installations when sample inputs change. Inspection needs PowerShell7, SDK10.0.402
and Node22.19+. Setup may download tool/test dependencies;
retrieval uses unusable proxies, establishing proxy independence, not network
isolation. The evidence fixture uses only disposable literal127.0.0.1 HTTP and a
generated noncredential marker. No public package upload or real API key is needed.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\agent-pin-1124-03-029-upload-packages-to-a-v3-staging-endpoint'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId 'nuget.stage-package-uploads' -Query 'staging' `
  -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
  -PackageVersion '0.0.0-pin1124' -BuildAndPack `
  -Dotnet 'C:\Program Files\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory ./artifacts/pin1124/inspection
```

Expect title "Upload packages to a V3 staging endpoint" and goal "Stage a package
and its symbols through a service-advertised endpoint when a publisher needs an
upload step before final publication, with explicit group, transport and failure
handling." Saved paired proof records original SDK pack/PDB validation and real
wire checks. Current staging verifies advertised endpoint/PUT/chunked multipart bytes,
original filenames, route/query, group present/null/empty and key present/absent
without printing its value, HTTP201/400, missing supported resource, default HTTP
refusal, same-origin authorization with default/fixture flags, loopback-only insecure opt-in, received cancellation and request timeout. URI-only policy checks are separate from HTTPS transport proof; malformed header/chunk/length connections also close with complete owned cleanup.
SDK10 verifies actual ordinary publication, fixed package.nupkg filename and its
HTTP400 failure, with no staging API/group equivalent. The fixture stops its own
listener/clients and removes temporary packages. Any nonzero exit, stale evidence,
missing recipe, wrong applicability or unexpanded snippet is a failure.

This qualifies the pinned SDK-bundled public API and faithful local protocol only.
No independently distributed NuGet.Protocol8 package, older-target staging route,
hosted service interoperability, TLS/certificate flow, final publication, atomicity
or GA is claimed. Exact eight foundations and PIN-959/PIN-963/Human gates stay open.

## Reused NuGet hosts, analyzer inventory and launch diagnostics

The following block builds/packs one ordinary artifact and inspects that exact
installed local-feed/cache artifact for all three recipes. Real stdio list/get
from an unrelated directory prints each ID/title/goal, complete canonical
Markdown, target11/edition11, applicability and fresh paired evidence; target10
gets `no_applicable_edition`. The final consumer reconstructs the default
net10/net11 projects and sources from returned Markdown, supplies standard SDK,
import and local-feed scaffolding, then runs actual same-process
MSBuild/configuration and benign-child behaviour outside the checkout.

Prerequisites: PowerShell7, Node22.19+, complete SDK10.0.402 and
SDK11.0.100-rc.2.26504.105 installations, runtime/ref10.0.13 and11RC2. Setup may
restore online; installed retrieval with unusable proxies proves proxy
independence, not network isolation. No global install, provider or real secret.
From an unrelated directory this literal worktree block was tested:

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\agent-pin-1125-03-030-invalidate-custom-nuget-caches-between-reused-builds'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('nuget.reset-reused-host-state','nuget.inspect-analyzer-assets','nuget.own-plugin-launch-diagnostics') `
  -Query 'nuget' -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
  -PackageVersion '0.0.0-pin1125-batch03' -BuildAndPack `
  -Dotnet 'C:\Program Files\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory ./artifacts/batch03/inspection
& ./eng/Test-PackageWorkshopConsumer.ps1 `
  -Net10Dotnet 'C:\Program Files\dotnet\dotnet.exe' `
  -Net11Dotnet 'C:\Users\pingu\.dotnet-playbook-11\dotnet.exe'
```

Expect all three discovery entries, full sources/requirements and current
evidence, then the returned-consumer PASS. The complete negative/lifecycle/asset
suite is separately reproducible with `eng/Record-PackageWorkshop.ps1` using the
same two absolute SDK parameters. It measures real same-host stale/reset/lazy
behaviour and owned open-file/unsubscription/failure controls; actual original
analyzer pack/locked restore/old-current-mixed compiler paths and diagnostic
failures; visible current CS0618 versus zero-warning fixed consumers, and real
missing/valid benign OS child launches with redaction. Expected negative compiler
errors and deliberate task failure are asserted passing controls, not suppressed
production warnings. The pinned SDK does not consume AnalyzerAssets: `_._` is
not enforcement. No third-party automatic reset, provider credential exchange,
other-platform, performance or GA guarantee is made. Eight approved foundations,
PIN-959 qualifying coverage/evidence, PIN-963 actual-GA and Human gates remain open.

## Verify withdrawn compiler guidance

Requires PowerShell 7, SDK 10.0.402 and Node 22.19+. This builds/packs the ordinary
tool and installs the exact artifact from an isolated local-only feed/cache.
Setup can restore online. Installed stdio calls run from an unrelated temporary
directory with unusable proxies (proxy independence, not network isolation).

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-5ba443cc'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId 'razor.replace-runtime-compilation-registration' `
  -RemovedRecipeId @('razor.migrate-generated-document-contracts','razor.copy-intermediate-nodes','razor.choose-task-generation-path') `
  -Query '' -TargetVersion '11.0' -PackageVersion '0.0.0-scope-removal' `
  -Dotnet 'C:\Program Files\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' -BuildAndPack `
  -OutputDirectory ./artifacts/scope-removal/inspection
```

Expect three `PASS` lines confirming each removed ID is absent from discovery and
returns `unknown_recipe` on a direct request. The retained recipe prints its ID,
title, goal, full expanded Markdown and applicability, with current sample evidence
and equal structured/text responses. A remaining ID, any other error or stale/
changed retained content fails the check. No new compiler-consumer or GA claim follows.

## Registry challenges and native publish hooks

Requires PowerShell7, SDK10.0.402 and Node22.19+. This builds/packs the ordinary
tool, installs that exact artifact through a local-only feed and fresh cache,
then exercises real stdio list/get from an unrelated temporary directory with
unusable proxies. Setup can restore online; retrieval proves proxy independence,
not network isolation. The block was tested from an unrelated starting directory.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-d3fc93fb'
& ./eng/Test-Smoke.ps1 -Dotnet 'C:\Program Files\dotnet\dotnet.exe' `
  -PackageVersion '0.0.0-pin1142-batch04'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('sdk.configure-redirected-output-encoding','containers.migrate-registry-auth-challenges','sdk.migrate-optimized-publish-hooks') `
  -Query 'estuary' -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
  -PackageVersion '0.0.0-pin1142-batch04' `
  -Dotnet 'C:\Program Files\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory ./artifacts/estuary/inspection-final
```

Expect all three IDs, titles and explanatory goals; full canonical Markdown;
target11/edition11 applicability; and current paired compilation/execution
evidence. Target10 requests must return `no_applicable_edition`: the valid older
examples remain comparisons inside each target11 edition. A missing recipe,
unexpanded snippet, stale evidence or changed structured/text response fails.

The registry proof uses original workers, real Distribution3.1.2 TLS/token
exchanges and image push/run, seven SDK11 refusals before token requests/writes,
private child trust and an internal SDK network. Native proof uses actual Windows
x64 ILC/MSVC-link/PE execution and matched binlogs for old/current legacy versus
migrated hooks, default/false/true, NoBuild refresh/repeat and missing/wrong
sidecars. Refresh changed inputs with `eng/Test-EstuaryRegistry.ps1 -Record` or
`eng/Test-EstuaryWindows.ps1 -Record` on their declared authoring environments.
Inspection itself consumes saved evidence; it does not repeat those native/service
checks. Beta/RC2 limits, eight foundations and comprehensive/GA/Human gates remain.
Encoding proof captures actual paired SDK build diagnostics through pipes under
owned private CP1252/65001 consoles. An explicit UTF-8 reader preserves text and
real failed-build exits; the proposed and source-level opt-outs retain UTF-8 on
these artifacts and are disclosed as unavailable. No legacy producer encoding
route or newer-only reader API is claimed.

## Package streams and TAR hard-link policy

Requires PowerShell 7, SDK 10.0.402 with runtime 10.0.13, SDK/runtime
11.0.100-rc.2.26504.105 / 11.0.0-rc.2.26504.105, Node 22.19+ and
Windows NTFS for the real TAR identity proof. The exact packaging versions are
locked in the samples; no Open XML SDK or separate TAR package is needed.

This block was tested from an unrelated starting directory. Setup may restore
online. It builds/packs the ordinary tool, installs that exact artifact through a
local-only feed and fresh cache, and runs real stdio list/get from an unrelated
temporary directory with unusable proxies. This proves proxy independence, not
network isolation. No public package publication or global tool install occurs.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-d402c791'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('archive.adapt-package-part-streams','archive.choose-hard-link-policy') `
  -Query 'archive' -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
  -PackageVersion '0.0.0-pin1040-1041' -BuildAndPack `
  -Dotnet 'C:\Program Files\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory ./artifacts/archive-policy/inspection
```

Expect both discovery IDs, titles and explanatory goals, full expanded canonical
Markdown, target11/edition11 applicability, and fresh compilation/execution
records. Both target10 requests must return `no_applicable_edition`: the valid
older code and RC2 package's measured net10 asset remain comparison evidence.
The script asserts canonical snippet identity and structured/text equality.

Refresh changed consumer evidence on the declared Windows environment:

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-d402c791'
$previousIntegration = $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH
try {
  $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
  foreach ($sample in @(
    'samples/net10.0/MuseumPackage/MuseumPackage.csproj',
    'samples/net10.0/MuseumPackageRc2/MuseumPackageRc2.csproj',
    'samples/net10.0/ObservatoryTar/ObservatoryTar.csproj',
    'samples/net11.0/MuseumPackage/MuseumPackage.csproj',
    'samples/net11.0/ObservatoryTar/ObservatoryTar.csproj'
  )) {
    $runtime = if ($sample.StartsWith('samples/net11.0/')) {
      'C:\Users\pingu\.dotnet-playbook-11\dotnet.exe'
    } else { 'C:\Program Files\dotnet\dotnet.exe' }
    & ./eng/Record-Evidence.ps1 -Sample $sample -Dotnet $runtime
  }
}
finally { $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = $previousIntegration }
```

Expect the four real package access cases for all three dependency combinations,
sequential/bounded random access with size failures, and actual NTFS IDs plus
TAR writer/extractor policies, sync/async path/stream extraction, overwrite and
cancellation failures, path/link-target rejection, partial extraction and cleanup.
The separate growth-control stream only instruments the buffering guard; package
and TAR behavior use actual library/filesystem operations. No link-incapable
receiver, other-platform or GA execution is claimed. Comprehensive and GA gates
remain open; no recurring platform matrix is added.

## Fractional rounding and small integer policies

Requires PowerShell7, SDK10.0.402/runtime10.0.13, SDK11.0.100-rc.2.26504.105 /
runtime11.0.0-rc.2.26504.105 and Node22.19+. The tensor samples lock matching
System.Numerics.Tensors10.0.13/11RC2 with actual net10/net11 assets. Scalar rounding
and small conversions need no package. No provider, workload, service or global
installation is needed.

This block was tested from an unrelated starting directory. It builds/packs the
ordinary tool, installs that exact artifact through an isolated local-only feed
and fresh cache, then runs real stdio list/get from an unrelated temporary directory.
Setup may restore online; unusable proxies during retrieval prove proxy independence,
not network isolation. The output displays ID/title/goal, complete canonical Markdown,
target11/edition11 applicability and fresh compilation/execution records.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-9d85709e'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('numerics.choose-fractional-rounding','numerics.select-small-integer-conversion') `
  -Query 'numeric-policy' -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
  -PackageVersion '0.0.0-pin1047-1048' -BuildAndPack `
  -Dotnet 'C:\Program Files\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory ./artifacts/numeric-policy/inspection
```

Expect both discovery IDs with their reusable titles/goals, full expanded snippets,
current paired evidence, matching structured/text results and no_applicable_edition
for target10. The .NET10 code is a working comparison, preserving eight foundations.
Refresh changed sample inputs using `eng/Record-NumericPolicies.ps1` with the two
absolute SDK executables (`-Net10Dotnet` / `-Net11Dotnet`); `-VerifySources` also
replays nine immutable audit-source hashes. Exact commands, inputs, dependency closure,
compiler/reference/runtime/package hashes and OS/architecture are saved in sample
records. Output includes original midpoint/large/digit/mode controls, real tensor
independent/in-place/capacity/overlap checks and all-five-destination small conversion
plus bounded writer checks. Native results are observations. Windows x64 CoreCLR JIT
is measured; no interpreter/Native AOT/Mono, newer tensor package on net10, other
platform or actual-GA proof. PIN-959/PIN-962 and Human gates remain open.

## Historical dates and tensor ownership preparation

Requires PowerShell7, Node22.19+, SDK10.0.402/runtime10.0.13 and
SDK11.0.100-rc.2.26504.105/runtime11RC2, plus outbound HTTPS to
raw.githubusercontent.com for the recorders' -VerifySources checks. The paired calendar consumers need
no package or workload and require non-invariant globalization with usable ja-JP.
Measured execution is Windows x64 CoreCLR JIT/ICU; NLS and other platforms are
not verified. Tensor examples lock10.0.13/11RC2 with net10/net11 assets. The
full-release storage contract requires a final package containing backport#135281;
successful current execution remains pending under PIN-962. Refresh changed
inputs with the two recorders below. No new platform/native matrix is required.

The following literal worktree block is exercised from an unrelated starting
directory. It builds/packs the ordinary tool, installs that exact artifact from
an isolated local-only feed/cache and calls real stdio list/get from an unrelated
temporary directory. Setup may restore online. Retrieval with unusable proxies
proves proxy independence, not network isolation.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-34e74034'
& ./eng/Record-HistoryImport.ps1 -VerifySources
& ./eng/Record-GridTransform.ps1 -VerifySources
& ./eng/Test-Smoke.ps1 -Dotnet 'C:\Program Files\dotnet\dotnet.exe' `
  -PackageVersion '0.0.0-pin1051-1052'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('globalization.validate-japanese-calendar-range','numerics.copy-tensor-views-safely') `
  -Query 'data-validation' `
  -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
  -PackageVersion '0.0.0-pin1051-1052' `
  -Dotnet 'C:\Program Files\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory ./artifacts/calendar-storage/inspection
```

Expect discovery ID/title/goal, complete canonical Markdown, target11/edition11,
applicability, fresh paired evidence and equal structured/text responses. Target10
must return `no_applicable_edition`: the older code remains a comparison within
the target11 guide and preserves the eight foundations. The recorder executes all
four historical boundaries with GetYear/GetEra, retains source Gregorian values,
and checks that invariant-mode children fail visibly. Any nonzero script exit,
stale evidence, missing recipe or unexpanded snippet is a failure. Tensor discovery
and detail must disclose pending final-package execution. Its current record is
compile with supplementary bounded ownership/Try false policy execution; the
expected-release overlap/output/constructor suite is not claimed to pass RC2/RC3.
After PIN-962 updates final pins, locks and metadata, run Record-GridTransform.ps1
with -RunReleaseChecks -VerifySources and retain actual results before clearing
pending proof. Inspection validates served content and freshness, not the missing
package behavior. RC2 execution does not close actual-GA, release-ready,
comprehensive coverage or existing Human gates.

## Complex signal classification and tensor shape preparation

Requires PowerShell7, SDK10.0.402/runtime10.0.13, SDK11.0.100-rc.2.26504.105 /
runtime11.0.0-rc.2.26504.105 and Node22.19+. The signal samples use framework
Complex. InstrumentBatch locks tensor10.0.13/11RC2 for previous execution/current
compilation; the intended tensor route requires the final11 package with backport
#135281. No provider, workload or experimental opt-in is required.

This block was tested from an unrelated starting directory. It builds/packs the ordinary
tool, installs that exact artifact through an isolated local-only feed and fresh
cache, then calls real stdio list/get from an unrelated temporary directory.
Setup may restore online; unusable proxies during retrieval establish proxy
independence, not network isolation.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-ade7763e'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('numerics.handle-complex-special-values','numerics.align-tensor-shapes') `
  -Query 'numeric-shapes' -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
  -PackageVersion '0.0.0-pin1049-1050' -BuildAndPack `
  -Dotnet 'C:\Program Files\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory ./artifacts/numeric-shapes/inspection
```

Expect both discovery IDs/titles/goals, complete expanded canonical Markdown,
target11/edition11 applicability, fresh paired compilation/execution evidence,
matching structured/text results and `no_applicable_edition` for target10.
Refresh changed signal inputs using `eng/Record-ComplexSignals.ps1`; add
`-VerifySources` to check the three immutable audit-source hashes. Exact commands,
compiler/reference/runtime identities, inputs, component bits and classifications
are saved in sample evidence. RC2 is not GA proof; only Windows x64 CoreCLR JIT
execution is measured.

Expect the tensor discovery goal and detail to disclose final-package execution
pending. Its previous record is compile-and-run; the current record is compile,
and humanProof.status is pending. Freshness means recorded inputs match, not that
new shape behavior ran. Published RC2/RC3 packages lack the behavior; inspection
checks served guidance and metadata only. The native-width suite remains unexecuted.
PIN-962 owns final SDK/runtime/package, all small and native checks and evidence
refresh. Under Pingu's preparation decision, this PR may deliver the guide while
release-ready packaging, comprehensive, GA and existing Human gates stay open.

Refresh affected tensor preparation evidence with:

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-ade7763e'
& ./eng/Record-InstrumentBatch.ps1 -VerifySources
```

Expect previous-policy execution, current compilation and four pinned source hash
checks. This command intentionally does not execute the current expected-release
checks. After PIN-962 updates pins, locks, dependency declarations and metadata for
the actual final artifacts, use -RunReleaseChecks with the final SDK paths. That
explicit option also runs the large native proof, requiring Windows x64 and about
8GiB measured free physical/commit memory. A refused/failed native run leaves the
proof pending; do not clear it based on the smaller checks.
# Calibration bounds and host failure boundaries

Requires PowerShell7, Node22.19+, SDK10.0.402/runtime10.0.13 and
SDK11.0.100-rc.2.26504.105/runtime11RC2. CalibrationImport locks tensors10.0.13 /
11RC2; AquariumImport locks full Hosting10.0.13/11RC2 with its actual transitive
closure. No provider, native library/workload, account or service is needed.

The following block is tested from an unrelated starting directory. It builds/
packs the ordinary tool, installs that exact artifact from an isolated local-only
feed/cache, then calls real stdio list/get from an unrelated temporary directory.
Setup may restore online. Unusable proxies during retrieval prove proxy independence,
not network isolation. No public publication or global installation occurs.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-76b55631'
& ./eng/Test-Smoke.ps1 -Dotnet 'C:\Program Files\dotnet\dotnet.exe' `
  -PackageVersion '0.0.0-pin1053-1054'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('numerics.validate-tensor-clamp-bounds','hosting.observe-background-failures') `
  -Query 'calibration-host' -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
  -PackageVersion '0.0.0-pin1053-1054' `
  -Dotnet 'C:\Program Files\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory ./artifacts/calibration-host/inspection
```

Expect one passing installed smoke, then both discovery IDs/titles/explanatory
goals, complete expanded canonical Markdown, target11/edition11 applicability,
fresh paired compilation/execution records, equal structured/text responses and
`no_applicable_edition` for both target10 requests. Missing/stale evidence or an
unexpanded/different snippet fails. The older code remains a comparison, preserving
exactly eight foundations. Inspection consumes saved evidence; it does not repeat
consumer behavior. To refresh changed consumer inputs and the eight immutable
source hashes on the declared environment:

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-76b55631'
& ./eng/Record-CalibrationHost.ps1 -VerifySources
```

Expect float/decimal raw-versus-validated scalar/pairwise cases at lengths1/3/33,
reversed/equal/ordered and late-invalid bounds, capacity/length/NaN and unchanged
failed output/correct values; then35 lifecycle children plus the canonical demo
per runtime. Host checks separately assert signalled startup, single/multiple/
shutdown exception identities, logs, chosen exit17 versus0, normal cancellation,
Ignore still running, disposal and child reaping. The explicit older failure
journal and current boundary both preserve failure exit17. PATH and persistent
environment values are checked unchanged. Expected diagnostic failure logs from
the demonstration are passing controls. The recorder ends successfully; a nonzero
script exit is a failure. Windows x64 CoreCLR JIT is measured; browser/custom-host,
other-platform/engine, newer-package-on-net10 and actual-GA claims are unverified.
PIN-959/PIN-962 and existing Human gates remain open; no recurring matrix is added.

## File load, watch root and certificate issuer boundaries

Requires PowerShell7, Node22.19+, SDK10.0.402/runtime10.0.13 and the separate
SDK11.0.100-rc.2.26504.105/runtime11RC2 installation. File-provider samples lock
Configuration.Json/FileProviders.Physical10.0.13/11RC2 with actual net10/net11
assets. Certificate consumers use framework X509 APIs and existing Linux Docker.
No workload, account, global install or production service is needed.

The following literal worktree block is tested from an unrelated starting directory.
It builds/packs the ordinary tool, installs the exact artifact through an isolated
local-only feed/cache, then calls real stdio list/get from an unrelated temporary
directory. Setup can restore online; retrieval with unusable proxies proves proxy
independence, not network isolation. The Linux behavior fixture separately uses
network-none containers with loopback available.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-15728aaa'
& ./eng/Test-Smoke.ps1 -Dotnet 'C:\Program Files\dotnet\dotnet.exe' `
  -PackageVersion '0.0.0-pin1055-1056-1060'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('configuration.observe-file-load-failures','io.compose-file-watcher-roots','crypto.supply-chain-intermediates') `
  -Query 'file-chain' -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
  -PackageVersion '0.0.0-pin1055-1056-1060' `
  -Dotnet 'C:\Program Files\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory ./artifacts/file-chain/inspection
```

Expect one passing installed smoke, then all three discovery IDs, reusable titles
and explanatory goals, complete expanded canonical Markdown, target11/edition11,
applicability, fresh paired evidence and identical structured/text responses.
Target10 must return `no_applicable_edition`; the valid older approaches remain
comparisons within target11 and preserve exactly eight foundations. A nonzero exit,
missing recipe/snippet, changed body or stale evidence fails the check.

Refresh changed sample inputs and eleven immutable source hashes on the declared
Windows NTFS/Docker environment:

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-15728aaa'
& ./eng/Record-FileChain.ps1 -VerifySources
```

Expect actual JSON initial/explicit/reload/missing/deletion/recreation and Windows
exclusive-lock failure/recovery, callback type/Ignore/data/token results, separately
labelled synthetic stream/GetFileInfo failures and bounded unignored-reload children.
Expect both watcher constructors, null/NUL/unrelated/prefix siblings and current
real equal/ancestor/descendant/empty/relative/deferred tokens, recursion, arming,
canonical factories and cleanup. Older ancestor/relative construction acceptance
is not advertised as reliable monitoring; the older factory uses equal normalized
paths. There is no polling/browser/symlink-security or universal event-count claim.

ArchiveManifest compilation records are distinct from `linux.evidence.json`.
Each runtime executes four fresh, removed network-none/read-only containers with
private HOME/cache tmpfs and random CA material. Expect old three-fetch success,
current two-fetch PartialChain, supplied-chain success with zero downloads,
UntrustedRoot/NotTimeValid rejection and real manifest signature/tamper controls.
Loaded OpenSSL3.3.7/3.5.4 library versions/hashes, exact image digests and cleanup
are saved. CustomRootTrust/NoCheck are private offline-fixture policy, not production
trust or online-revocation/TLS proof. No shared roots/caches are edited. Process,
user and machine PATH and persistent user/machine environment are checked unchanged.

Windows x64 NTFS and Linux x64 OpenSSL are measured. Other-platform/filesystem,
newer-package-on-net10, TLS, online revocation and actual-GA behavior remain
unverified. Inspection consumes saved proof rather than rerunning these behaviors.
PIN-959/PIN-962 and existing Human gates stay open; no native CI matrix is added.

## Certificate extension state, client TLS chains and file aliases

Requires PowerShell7, Node22.19+, SDK10.0.402/runtime10.0.13 and the separate
SDK11.0.100-rc.2.26504.105/runtime11RC2 installation. All six consumers use framework
APIs with no NuGet packages/workloads. Authoring uses Windows x64 local NTFS and
the existing digest-pinned Linux x64 OpenSSL runtime containers.

This block was tested from an unrelated starting directory. It builds/packs the
ordinary tool, installs that exact artifact using an isolated local-only feed/cache,
then calls real stdio list/get from an unrelated temporary directory. Setup may
restore online; retrieval with unusable proxies proves proxy independence rather
than network isolation. The separate TLS fixtures use network-none containers.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-1fc43630'
& ./eng/Test-Smoke.ps1 -Dotnet 'C:\Program Files\dotnet\dotnet.exe' `
  -PackageVersion '0.0.0-pin1061-1063-1075'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('crypto.replace-encoded-extension-data','tls.supply-client-certificate-chain','io.create-hard-links') `
  -Query 'certificate-links' -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
  -PackageVersion '0.0.0-pin1061-1063-1075' `
  -Dotnet 'C:\Program Files\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory ./artifacts/certificate-links/inspection
```

Expect one passing installed smoke and all three discovery IDs, reusable titles
and explanatory goals, complete expanded canonical Markdown, target11/edition11
applicability, fresh paired evidence, structured/text equality and
`no_applicable_edition` for target10. The old consumers are comparisons within the
target11 guide; exactly eight foundations remain. A nonzero exit, changed/missing
snippet, mismatched body or stale evidence fails. Inspection consumes saved evidence;
it does not rerun native consumer behavior.

Refresh affected samples and fourteen immutable selected/supplemental source hashes:

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-1fc43630'
& ./eng/Record-CertificateLinks.ps1 -VerifySources
```

Expect the original .NET10 RawData stale-cache failure and both supported update
choices on both releases, all decoded fields/raw bytes/OID/Critical true/false,
malformed lazy decode, getter-only use and the isolated current-target old-setter
SYSLIB0065 diagnostic without suppression. Current supported code builds with
warnings as errors. This covers one Basic Constraints extension type.

Expect Windows local NTFS native/File/FileInfo aliases to share writes in both
directions and retain data when either name is deleted first. Destination-exists,
absent-target and directory-target attempts reject without clobbering data or
creating the missing alias. Native error/current exception values are observations,
not an all-platform taxonomy. Cleanup verifies and removes only the created tree.
No Unix/network-share/unsupported-filesystem/cross-volume result is claimed.

Expect four fresh private containers per TLS runtime. The old no-custom-policy
flow fetches one missing client issuer and exchanges booking/ACK data; the current
flow fails PartialChain without fetching. Explicit server ExtraStore/downloads-off
succeeds on both, while unrelated-root and expired-client controls reject without
fetching. Clients send only their leaf; callbacks require strict validation. TLS1.3,
OpenSSL3.3.7/3.5.4, loaded native/managed hashes, certificate inputs, exact images,
commands and cleanup are recorded separately from Windows compilation. Private
HOME/cache/tmpfs and listeners/keys disappear; no shared trust store is modified.
NoCheck is for this offline fixture, not proof of online revocation. Compatibility
switches, client-supplied full chains, Windows TLS and final-GA behavior are untested.

Process/user/machine PATH and persistent user/machine environment remain unchanged.
No production resources, public publication or recurring native matrix is added.
PIN-959/PIN-962, other delivery owners and existing Human gates remain open.

## Legacy signatures and RSA padding

Requires PowerShell7, Node22.19+, SDK10.0.402/runtime10.0.13 and the separate
SDK11.0.100-rc.2.26504.105/runtime11RC2 installation. All four consumers use
framework cryptography APIs without packages/workloads. Windows x64 CAPI provider24
and disposable RSA2048 keys provide the real interoperability path. The public-only
DSA fixture was generated on supporting Windows; no Mac key generation is needed.

This literal worktree block is tested from an unrelated starting directory. It
builds/packs the ordinary tool, installs that exact artifact through an isolated
local-only feed and new cache, then calls real stdio list/get from an unrelated
temporary directory. Setup may restore online; unusable proxies during retrieval
prove proxy independence, not network isolation. No public/global installation,
production container access, native installation or persistent setting change occurs.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-dfde5574'
& ./eng/Test-Smoke.ps1 -Dotnet 'C:\Program Files\dotnet\dotnet.exe' `
  -PackageVersion '0.0.0-pin1057-1062'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('crypto.migrate-dsa-signatures','crypto.make-rsa-padding-explicit') `
  -Query 'signature-padding' -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
  -PackageVersion '0.0.0-pin1057-1062' `
  -Dotnet 'C:\Program Files\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory ./artifacts/signature-padding/inspection
```

Expect one passing installed smoke, then both discovery IDs, reusable titles and
explanatory goals, complete expanded canonical Markdown, target11/edition11,
applicability, fresh paired evidence and equal structured/text responses. Target10
returns no_applicable_edition; old code remains a working comparison and the eight
foundations remain unchanged. Missing/stale evidence, snippet/body mismatch or any
nonzero exit is a failure. Inspection consumes saved evidence; it does not repeat
provider behavior or establish the excluded macOS outcomes.

Refresh affected exact inputs, compilation/execution and nine immutable sources:

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-dfde5574'
& ./eng/Record-SignaturePadding.ps1 -VerifySources
```

Expect paired Windows DSA fixture/ECDSA P256/SHA256/DER checks: imported public
legacy signature, independent certificates and SPKI peer, tamper/encoding/key/
algorithm rejection and explicit unauthenticated UnsupportedLegacy handling.
Mac .NET10 DSA verification, .NET11 DSA factory/provider/certificate failures and
both releases' ECDSA certificate/signature/routing checks remain sample-specific
not-tested under PIN-699; no Apple native asset inspection is claimed.

Expect three real CAPI processes: old-send, new-exchange and old-receive. Both
false/Pkcs1 and true/OaepSHA1 cross-decrypt in both directions; explicit/canonical
roundtrips succeed, wrong padding/zeroed ciphertext and OAEP-SHA256 reject. An
isolated unchanged old-source/current-target build fails specifically SYSLIB0064,
without suppression. Supported consumers build with zero warnings/errors.
Ciphertext hashes differ across fresh randomized encryptions; recovered plaintext
matches. The private disposable key/ciphertext/diagnostic directory is removed and
its absence checked; no named/production container is used. No stronger-provider
redesign or authentication guarantee follows. Process/user/machine PATH and
persistent user/machine environment are checked unchanged. RC2 evidence is not GA;
PIN-959/PIN-962 and existing Human gates stay open, with no recurring native matrix.

## Colour channels and span extrema

The original SignalLegend and ObservatoryBatch consumers require only the two
pinned shared SDKs. No package, native workload, provider or device is needed.
Refresh affected sample records and verify the eleven selected immutable sources:

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a43ec602'
& ./eng/Test-ChannelExtrema.ps1 -Record -VerifySources
```

Expect four passing real consumers: distinct RGBA/ARGB known words and endian
round trips, byte/float preservation, span length/copy and equality/hash checks;
value/reference/nullable extrema, explicit/default comparers, ignored/all-null
entries and distinct empty contracts. Reference SHA256 values match the source
audit; output records compiler/CoreLib identities. No colour processing, NaN,
performance, other-platform or final-GA claim follows.

This block is exercised from an unrelated starting directory. It builds/packs the
ordinary tool once and inspects that exact package through an isolated local-only
feed/cache and real installed stdio calls from a temporary unrelated directory.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a43ec602'
& ./eng/Test-Smoke.ps1 -Dotnet 'C:\Program Files\dotnet\dotnet.exe' `
  -PackageVersion '0.0.0-pin1079-1080'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('numerics.preserve-color-channels','collections.find-span-extrema') `
  -Query 'channel-extrema' -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
  -PackageVersion '0.0.0-pin1079-1080' `
  -Dotnet 'C:\Program Files\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory ./artifacts/channel-extrema/inspection
```

Expect a passing installed smoke and both discovery IDs, reusable titles and goals,
full expanded canonical Markdown, target11/edition11, applicability and fresh paired
records. Structured/text fallback must agree. Target10 returns no_applicable_edition;
these .NET10 consumers compare older approaches within target11 guidance and add no
foundation. The Inspector prints exactly what an agent receives and saves JSON plus
the package hash. Retrieval with unusable proxies establishes proxy independence,
not network isolation. Setup may restore online; no new native/client/OS matrix.
PIN-959 comprehensive coverage, PIN-962 GA and existing Human gates remain open.

## Reduced-precision values and finite CBOR nesting

Requires PowerShell7, Node22.19+, SDK10.0.402/runtime10.0.13 and the separate
SDK11.0.100-rc.2.26504.105/runtime11RC2 installation. SensorArchive uses framework
APIs only. CalibrationGroups pins System.Formats.Cbor10.0.13/11RC2 with actual
lib/net10.0/lib/net11.0 assets. No account, provider, workload or native setup.

Refresh affected samples through the existing evidence workflow:

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-65a77b17'
& ./eng/Test-PrecisionDepth.ps1 -Record -VerifySources
```

Expect four real passing consumers and ten matching immutable sources. Numeric
checks measure reading/mean error budgets, exact/rounded values, range versus
Half, signed zero/infinity/NaN, underflow/overflow and actual generic math. CBOR
checks compare accepted values/bytes, depth8/9, arrays/maps and definite/indefinite
containers, truncation/extra/trailing data, current zero/-1/below-1 and reader64/65
versus writer1000/1001. The actual old10.0.13 reader already enforces64; its writer
completes1001. Commands, locks, compiler/CoreLib and audit-matching references,
package archives and executed assemblies are recorded. PATH/persistent settings
are checked unchanged. A nonzero exit is failure.

This literal block was exercised from an unrelated starting directory. It builds
and packs the ordinary tool, installs that exact artifact through an isolated
local-only feed/cache, then calls real installed stdio list/get from an unrelated
temporary directory. Setup may restore online. Unusable proxies during retrieval
prove proxy independence, not network isolation.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-65a77b17'
& ./eng/Test-Smoke.ps1 -Dotnet 'C:\Program Files\dotnet\dotnet.exe' `
  -PackageVersion '0.0.0-pin1084-1085'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('numerics.choose-bfloat16','cbor.bound-nesting') `
  -Query 'precision-depth' -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
  -PackageVersion '0.0.0-pin1084-1085' `
  -Dotnet 'C:\Program Files\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory ./artifacts/precision-depth/inspection
```

Expect one passing installed smoke and both discovery IDs, reusable titles and
explanatory goals, complete expanded canonical Markdown, target11/edition11,
applicability and fresh paired evidence. Structured/text responses agree; target10
returns no_applicable_edition. Missing/stale evidence, changed snippets/body or
a nonzero exit fails. Inspection consumes saved consumer proof. Eight foundations
remain unchanged. Windows x64 RC2 is measured, not other-platform or final-GA
behavior; no BFloat16 serialization/performance or unlimited-depth claim follows.
PIN-959/PIN-962 and existing Human gates remain open; no recurring matrix is added.

## Atomic flags and typed function-pointer calls

Requires PowerShell7, Node22.19+, SDK10.0.402/runtime10.0.13 and the separate
SDK11.0.100-rc.2.26504.105/runtime11RC2 installation. Both StationFlags and
QuarryCalls use framework APIs only, with no packages/workloads. QuarryCalls
requires Windows x64 CoreCLR dynamic code and the system MulDiv export; unsafe
C# is enabled. No native compiler, account or external service is needed.

Refresh affected samples through the existing evidence workflow:

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-2f8c93ea'
& ./eng/Test-RuntimeCalls.ps1 -Record -VerifySources
```

Expect four passing consumers and eight matching immutable sources. Flags checks
cover 128 synchronized prior-value chains and 40,000 mixed updates, unrelated-bit
preservation, current integer/enum backing sizes, sign/high bits and unsupported
struct/floating-point rejection. Call checks execute real managed and native MulDiv
calls, signed rounding, zero-divisor/overflow sentinels, direct pointer comparison,
injected false dynamic-code guard and current invalid-signature rejection. Saved
records retain exact commands/inputs, compiler/CoreLib/native identities and
audit-matching references. PATH/persistent settings must remain unchanged.

This literal block is tested from an unrelated starting directory. It builds and
packs the ordinary tool, installs that exact artifact through an isolated local-only
feed/cache, then calls installed stdio list/get from an unrelated temporary directory.
Setup may restore online; unusable proxies during retrieval prove proxy independence,
not network isolation.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-2f8c93ea'
& ./eng/Test-Smoke.ps1 -Dotnet 'C:\Program Files\dotnet\dotnet.exe' `
  -PackageVersion '0.0.0-pin1094-1096'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('threading.update-enum-flags-atomically','reflection.emit-function-pointer-calls') `
  -Query 'runtime-calls' -TargetVersion '11.0' -UnsupportedTargetVersion '10.0' `
  -PackageVersion '0.0.0-pin1094-1096' `
  -Dotnet 'C:\Program Files\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory ./artifacts/runtime-calls/inspection
```

Expect one passing installed smoke and both discovery IDs, reusable titles and
explanatory goals, complete expanded canonical Markdown, target11/edition11,
applicability and fresh paired evidence. Structured/text responses must agree;
target10 returns no_applicable_edition. Missing/stale evidence, changed snippets/body
or a nonzero exit fails. Inspection consumes saved behaviour proof, without rerunning
consumers. Eight foundations remain unchanged. Windows x64 RC2 is measured; no
multi-field atomicity, performance, x86/other ABI, Native AOT, arbitrary ILGenerator,
marshalling or final-GA execution is established. The unsupported-dynamic-code
branch is injected, not an execution on an unavailable runtime. PIN-959/PIN-962
and existing Human gates remain open; no recurring native/client/OS matrix is added.
