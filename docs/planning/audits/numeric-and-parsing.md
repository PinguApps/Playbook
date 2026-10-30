# Numeric and parsing compatibility: PIN-683

Reviewed **21 October 2026**. The original audit below is source-only. Its nine proposals were subsequently approved and tracked by PIN-986. The **30 October delivery reconciliations** below record actual A–D delivery and qualify the original source claims; other proposals retain their own owners. Existing recipe/edition identities remain intact.

## Scope, provenance and evidence boundary

[PIN-683](https://linear.app/pinguapps/issue/PIN-683), `SPEC.md` at `0a8989814ea11346354464b845c79c15bb71c5e9`, stories 47, 101, 158 and 159; approved proposal `b3778cc`, key `SH_Q03`; inventory `migration.numeric-and-parsing`. Read the target and comments, the cited spec/proposal, current planning README, implementation/coverage/verification plans, glossary, ADRs 0001–0004, contribution/evidence rules, [PIN-703's API audit](data-io.md) and [delivered numeric evidence](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-736-738.md). Root `PLAN.md`, `CONTEXT.md` and `CONTEXT-MAP.md` are absent. No explicit provenance pointer is unavailable. Historical planning claims that implementation has not begun are stale; the current package and evidence harness exist.

The target belongs to PinguApps team `62f80270-b641-4fa3-b60c-46be4c4df2c8`, Playbook project `b22380c7-6fde-49f4-8826-e5631dc5df7a`. It has no native blocker. A paginated, archived-inclusive scoped query returned **317** issues; every team/project boundary was verified. Scoped numeric, parsing, tensor, calendar and rounding searches and the full project titles found no existing separate delivery owner for the proposals. PIN-736/737/738 are Done after PR #43; their full descriptions/comments were read. PIN-703 explicitly retains this semantic audit, including tensor Clamp/Round. These are the only related full-issue reads that affect the decisions here. Completed tickets are not reopened or re-estimated.

The [source manifest](numeric-and-parsing.sources.json) pins **24** complete public files with SHA-256 values and headings:

- dotnet/docs `a109e77898150ddb66e3000ce315d23fcc2700e9`: evolving .NET 11 compatibility index, all eight named notes, and the tensor storage/NumPy article. Each full note's old/new behavior, version, rationale, mitigation and affected APIs was reviewed, rather than inferred from its title.
- dotnet/dotnet `4099f83977f4d29604a9aeb971fb0a7bc1d5f699` / `820230c386db5b0106c5430b3e5aee92a7826bf5`: package-matched .NET 10.0.13 / .NET 11 RC2 reference declarations and tensor project composition. Current Math, JapaneseCalendar, TensorPrimitives.Clamp and TensorPrimitives.Round implementations qualify the notes.
- dotnet/core `ffed591b0fb8f9aab50ecc28f2c765049fb28d8f`: full Preview 2 and Preview 7 library notes, covering inherited tensor Round/Clamp findings.

Reference-source presence is API evidence only. PIN-703's actual asset manifest supplies the separately recorded SDK/package identity and metadata comparison; this audit does not rerun it or execute its assemblies. Source review cannot establish that every source implementation ships unchanged in a binary. No new SDK, package, project, workload, dependency permission, MCP tool or served recipe changes here. The compatibility index explicitly remains incomplete; this bounded audit is not an absence-of-change claim for all numerics/globalization APIs. RC evidence remains RC evidence; final artifact reconciliation stays with the existing GA work and comprehensive publication gate.

## Full mitigation dispositions

### A and B delivery reconciliation — 30 October 2026

PIN-1045 delivers `globalization.validate-date-parsing-options`; PIN-1046 extends
`numerics.preserve-conversion-intent` edition11.0. Original source manifest IDs
date-parsing, conversions, previous/current-runtime-api and previous/current-numeric-api
were fetched again and matched all six saved SHA-256 values. The declarations and
package-matched implementation sources were reconciled with **actual** SDK10.0.402 /
runtime/ref10.0.13 and SDK11.0.100-rc.2.26504.105 / runtime/ref11RC2. No added NuGet,
native, provider, workload or experimental opt-in is required. The samples remain
isolated author-maintained dependencies with no new Renovate permission.

The [paired booking consumers](../../../samples/net11.0/BookingImport/BookingChecks.cs)
qualify the note: invalid styles change false→ArgumentException; null formats arrays
also change false→ArgumentException, but null/empty array entries change
false→**FormatException**. Unsupported Q already throws FormatException on .NET10;
malformed custom formats can still return false, null string input can short-circuit
styles, and matching earlier alternatives can skip invalid later Q. Both raw APIs
and the same explicit allow-list importer are executed through string/span,
single/multiple-format, valid and malformed-row controls. Do not teach the draft's
universal ArgumentException interpretation.

The [instrument consumers](../../../samples/InstrumentArchive/ArchiveChecks.cs)
execute original G7/G15 policy, signs, significant-digit ties, exponent/underflow,
finite range and nonfinite rejection; Decimal/BigInteger destination-width and
advertised constructor/Convert/IConvertible/generic routes. Representative Decimal
double/float/Half and BigInteger double/float corrections are observed. Finite
BigInteger-to-Half integer inputs need no invented changed result: exact-double
intermediate values already round correctly in this bounded range. BFloat16 is
executed only on net11, without an invented net10 equivalent. RC2 still converts
Decimal-to-Half through double: the very-close 5.001953125000000000000000001 control
rounds down on both artifacts. This unavailable general single-rounding subgoal
remains explicit in served guidance and for PIN-962's actual-GA reconciliation.

Two actual **net10/C#14** assemblies built with the different SDKs contain different
decimal/double constants, read separately from runtime-input conversions; both run
on actual .NET10.0.13. Exact compiler, selected reference-pack and runtime hashes,
commands, inputs and outputs live in the five sample evidence records, refreshed by
`eng/Record-BookingArchive.ps1`. The ordinary installed local-feed/cache artifact is
inspected through real offline stdio list/get, complete canonical body, discovery
ID/title/goal, target/edition, applicability and freshness using the existing
[Inspector procedure](../../inspect-guidance.md#date-parser-settings-and-numeric-representation).
These claims do not close PIN-959, PIN-962, other delivery owners or Human gates;
RC2 is not actual-GA evidence and exactly eight .NET10 foundations remain.

All eight notes describe behavioral changes. **Included** means authored mitigation or changed assumptions remain delivery work. **Grouped** means a named existing goal shares responsibility; it does not imply complete recipe coverage. The extra tensor rows retain findings explicitly assigned by PIN-703.

### C and D delivery reconciliation — 30 October 2026

PIN-1047 delivers `numerics.choose-fractional-rounding`; PIN-1048 delivers
`numerics.select-small-integer-conversion`, both edition11.0. Nine source-manifest
entries (`rounding`, `small-integers`, previous/current runtime/numeric APIs,
`round-implementation`, `round-tensor-implementation`, `libraries-preview2`) were
downloaded again with all saved SHA-256 values matched. Actual paired SDK10.0.402 /
C#14 / runtime/ref10.0.13 and SDK11.0.100-rc.2.26504.105 / C#15 /
runtime/ref11RC2 consumers qualify those source claims. Small conversions need
no package. MeasurementExport locks System.Numerics.Tensors10.0.13 / 11RC2 and
selects actual lib/net10.0 / lib/net11.0 assets. These samples are isolated,
author-maintained dependencies; no Renovate permission is added. No native,
provider, workload, rendering or experimental opt-in is required for these routes.

The original observatory controls execute binary inputs just above/below decimal
midpoints, negative symmetry, large finite values, digit limits and invalid modes,
unchanged Decimal/no-digits controls and double/float/Half/NFloat delegating calls.
The explicit bounded scale/round/divide policy retains precision/range limitations;
it is not a universal former-runtime emulator. Both actual tensor packages execute
zero/nonzero digits, all modes, independent/in-place double spans, float controls,
capacity/invalid-mode failures and shifted-overlap rejection. No newer tensor
package on an older target is claimed.

The original record writer executes all five small destinations with double/float,
representative Half values and their actual precision/range, bounds/fractions,
infinities/NaN, checked overflow, generic/native APIs, deliberate int/narrowing
and native-int/narrowing policies. An actual bounded buffer write and source/
destination validation reject malformed offsets. Native and legacy out-of-range
results are observations, not portable historical values. Windows x64 CoreCLR JIT
is measured; interpreter, Native AOT, Mono, other OS/architectures and actual-GA
execution remain unverified. The official note excludes Mono from the change.

`eng/Record-NumericPolicies.ps1` captures exact commands, dependencies, inputs,
outputs and compiler/reference/runtime/package hashes in the four existing-format
sample evidence records. Ordinary pack and the isolated local-feed/cache installed
stdio Inspector verify full canonical Markdown, discovery ID/title/goal, target11 /
edition11, applicability and current evidence. Target10 returns no applicable
edition; exactly eight foundations remain. The [copyable walkthrough](../../inspect-guidance.md#fractional-rounding-and-small-integer-policies)
was run from an unrelated directory. PIN-959, PIN-962 and existing Human gates
remain open; this is bounded RC2 recipe evidence, not comprehensive readiness.

| Finding / introduction | Exact scope and mitigation | Coverage and accountable delivery |
| --- | --- | --- |
| [DateOnly/TimeOnly parsing][date-parsing], Preview 2 | `TryParse` and `TryParseExact` must receive valid `DateTimeStyles` and format arguments. Invalid options can throw `ArgumentException`; malformed user data with valid options is a different failure. Validate configuration; handle `ArgumentException` at a boundary accepting externally supplied options. The note lists string, provider/style and exact single-/multiple-format forms; reference declarations also expose span forms. A no-option overload being listed does not mean every bad date throws. | **Delivered → PIN-1045**, edition11.0; actual string/span/single/multiple checks and an explicit configuration boundary. The reconciliation above records FormatException and validation-order qualifications. Numeric-prefix parsing in PIN-737 remains separate. |
| [Decimal/BigInteger conversions][conversions], Preview 7 | Binary-to-Decimal no longer hides source precision behind 7/15 significant digits. Decimal-to-binary and BigInteger-to-double/float/Half/BFloat16 round the exact source once. Affects constructors, casts, Convert, Decimal.ToSingle/ToDouble, relevant CreateChecked/CreateSaturating/CreateTruncating and equivalent IConvertible/generic-math paths. Preserve decimal intent with decimal text/literals. For legacy binary-to-decimal results use invariant G7/G15 formatting plus decimal parsing with `NumberStyles.Float`, ties to even and symmetric signs; fixed decimal places are not significant digits. Preserve protocol bit patterns explicitly. No old-algorithm switch. New-SDK constant folding can change an older-target assembly on rebuild. | **Grouped → PIN-737**, delivered `numerics.preserve-conversion-intent` and `numerics.roundtrip-float-bits` cover decimal intent, one double/BigInteger case and raw bits. **Extended → PIN-1046:** original legacy significant-digit policy, bounded conversion directions/widths and real SDK11/net10 constant proof. The reconciliation above retains the actual RC2 Half limitation; no universal conversion audit is claimed. |
| [Math/MathF fractional-digit rounding][rounding], Preview 7 | `Math.Round(double,int[,mode])` and `MathF.Round(float,int[,mode])` now round the exact binary input; positive digit counts are uncapped (17+/9+ leave double/float unchanged). Negative digits remain invalid; this does not legalize invalid rounding modes. Delegating double/float/Half/NFloat interface entry points inherit the change. Decimal overloads and single-argument double/float overloads are unaffected. If exact former output is required, explicitly use scale/round/divide with its range/overflow limitations, or choose decimal quantities and an explicit policy. | **Delivered → PIN-1047**, `numerics.choose-fractional-rounding` edition11.0. PIN-737's Decimal.Round ties-to-even tests do not establish this migration. PIN-736's IEEE decimal choices are supporting alternatives, not Math.Round proof. |
| [Small integer casts][small-integers], Preview 7 | Unchecked float/double and corresponding Half conversions to sbyte/byte/short/ushort/char saturate at destination bounds; in-range fractions truncate toward zero and NaN becomes zero. Relevant Single/Double.ConvertToInteger calls agree. CoreCLR (including interpreter) and Native AOT are included; **Mono is excluded**. Checked overflow, integer narrowing and the older wider/vector conversion change are unaffected. Prefer saturation, or deliberately choose ConvertToIntegerNative, `(ushort)(int)x`, or native-int-then-narrowing according to the intended policy. Native conversion is platform-dependent and cannot promise historical results; validate indices/lengths separately. | **Delivered → PIN-1048**, `numerics.select-small-integer-conversion` edition11.0. Both pinned reference sets already contain Single/Double.ConvertToIntegerNative and ConvertToInteger with IBinaryInteger constraints; Char implements that interface. Do not invent a new API introduction or a universal legacy cast. |
| [Complex special values][complex], Preview 7 | Nongeneric Complex delegates to Complex<double>. Multiply/divide, Reciprocal, Abs, Pow, Sqrt, Exp, Log/Log10, trig/hyperbolic/inverse functions can preserve directed infinities and signed results rather than collapse to two NaNs. Update tests and classification logic that assumes nonfinite input always means NaN; no compatibility switch exists. Annex G leaves the sign of a zero quotient component unspecified, so do not assert a universal sign for that case. | **Grouped → PIN-736**, delivered `numerics.select-complex-precision` checks one zero divisor. **Remaining → E:** a separate special-value handling goal, not a second scalar-representation recipe. |
| [Tensor shape alignment][tensor-shapes], .NET 11 package | Default rank-zero empties compute as `[0]` without changing stored Rank/Lengths/Strides; explicit empty dimensions remain. Ignore only redundant leading singleton dimensions, not arbitrary broadcasting in shape equality. Stack/concatenate axes follow the first effective shape. `[0]` may broadcast to `[2,0]`, but `[0]` with `[0,2]` is incompatible. Preserve requested metadata; validate exact ranks/lengths explicitly if required. Native-sized traversal and all five Any comparison families now cover the full logical range, including empties; empty views retain their storage origin. | **Included → F.** Broadcast/BroadcastTo/TryBroadcastTo, elementwise/copy/destination paths, concatenate/stack, reshape/split/squeeze/unsqueeze/permute/slice/resize/reverse/SequenceEqual and index extrema belong to the same shape/traversal contract. Package upgrades can affect earlier TFMs; no recipe currently establishes such a route. |
| [Tensor storage layout][tensor-storage], .NET 11 package | Unsupported overlap throws `ArgumentException` before writing, including TryCopyTo/TryFlattenTo after capacity/shape checks succeed. Too-small destination (and incompatible shape for TryCopyTo) still returns false without copying. Dense/equal-count or identical-view copies, dense flatten/resize, identical nonrepeating elementwise views and dense/no-op in-place reversal are operation-specific permitted cases. Use separate output or snapshot **each** overlapping input; repeated zero-stride output needs independent storage. Array-backed constructors reject incompatible element types; writable covariance is rejected, compatible read-only covariance permitted. No arbitrary-overlap switch; TensorPrimitives retains its separate overlap contract. | **Included → G.** Includes IReadOnlyTensor copy/flatten and corresponding Tensor<T>/TensorSpan<T>/ReadOnlyTensorSpan<T> methods, destination elementwise/resize/reverse/concatenate and both System.Array constructors per span type. This is not a blanket ban on overlap or zero strides: singleton strides and empty destinations remain permitted. |
| [Japanese calendar minimum][japanese-calendar], Preview 1 | JapaneseCalendar.MinSupportedDateTime moves from Gregorian 1868-09-08 to **1868-10-23**, confirmed by the pinned implementation constant. Replace old assumptions and reject/route earlier Gregorian dates explicitly; use the runtime minimum rather than hardcoding the old date. The note's phrase “between” must not exclude the newly valid minimum itself. No restore switch is documented. | **Included → H.** Validate the half-open removed interval `[1868-09-08,1868-10-23)`, the new boundary and historical-data policy. This is distinct from parsing format/style validation. |
| TensorPrimitives.Clamp, Preview 7 library note | Full note says scalar remainder agrees with vector processing using Min(Max(x,min),max). **Qualified by pinned implementation:** vectorizable types (including Half's path) use that expression; other types call T.Clamp and may still throw when min > max. Explicitly validate bounds when rejection is application policy; never rely on vector width, position or the headline as a universal nonthrowing contract. | **Included → I.** Separate bound-validation goal. This source conflict remains visible until actual consumers establish the advertised type/asset routes. |
| TensorPrimitives.Round early returns, Preview 2 library note | Missing returns caused double processing, including in-place paths. **Excluded as an independent goal:** no distinct authored migration/workaround is established by that fix alone. RC2's digits overload now uses per-element T.Round fallback for correct rounding and uncapped nonnegative digits; no vectorized accuracy promise follows. | **Grouped → C** when teaching rounding across spans. Preserve overlap/capacity/mode contracts and actual result tests; do not silently assume the scalar interface rules prove every tensor call. |

No named finding is silently dropped or left with an unknown owner. The retained uncertainty is **delivery evidence**, especially type/platform/package routes and the Clamp headline qualification; source-audit completion cannot clear that obligation.

## Delivery proposals for backlog approval

**Proposals only.** Destination: the same team/project, existing **02 - Language and library guidance** milestone `c5232470-50ac-4f56-aea8-bd8ff7d13b41`, Backlog. The complete scoped search found the highest existing number **02-053**. Proposed **02-054–02-062** below are provisional, not reservations; repeat overlap/number checks immediately before approved creation. Relate approved issues to PIN-683, relevant existing numeric tickets, and the comprehensive coverage gate. No milestone, title, estimate or state on a related issue is changed here.

The native convention is S=2, M=3, L=5, confirmed by PIN-736/737/738 and the target. Estimates include source reconciliation, authoring, real evidence and installed retrieval. Each item is an independent PR boundary; shared files and numbering are not dependencies.

### Shared acceptance and prerequisites

Every proposal must deliver all of these, plus its focused checks below:

1. Reconcile the full cited mitigation against exact reference and implementation assets. Resolve any route-specific uncertainty before advertising it. Preserve existing stable IDs/editions; B extends `numerics.preserve-conversion-intent` instead of duplicating it. No ninth .NET 10 foundation is authorized.
2. Author original same-goal .NET 10/.NET 11 C# comparisons and complete offline goal, older/newer approaches, numbered migration, constraints, applicability, official references and canonical snippets. Fictional examples are proposed below; do not reuse Microsoft's demonstration code. Preserve fair older alternatives. Set explicit runtime/SDK/language/package-asset/platform/provider/rendering/stability conditions; verify experimental annotations and actual opt-ins.
3. Use the existing capture/build/pack harness. Initial measured baselines: SDK **10.0.402**, runtime/ref **10.0.13**, C# 14 / net10.0; SDK **11.0.100-rc.2.26504.105**, runtime/ref **11.0.0-rc.2.26504.105**, C# 15 / net11.0. Update pins only through the dependency policy and real recapture. These are reproducibility pins, not universal consumer requirements. No accounts, secrets, native UI workloads or cloud resources are required for the managed routes.
4. Compile and execute both actual consumers; save exact commands, resolved dependencies, outputs and input hashes. Use focused failures and corrected results below. Reconcile runtime-produced values separately from compiler-folded constants and record OS/architecture/globalization mode where material. No source review, mock or cross-compilation substitutes for execution. Missing required proof blocks that delivery PR.
5. Build/pack the ordinary artifact and use the **existing** installed stdio harness from an unrelated directory and isolated feed/cache, with offline retrieval. Check list ID/title/goal, full get Markdown, canonical snippet identity, target/edition/applicability/evidence and unsupported routes. No new recurring OS/client/native matrix. Update the disposition register; RC is not GA proof. No package publication or resource provisioning is authorized by these proposals.

F, G and I need matching **System.Numerics.Tensors 10.0.13 / 11.0.0-rc.2.26504.105** packages, reviewed lockfiles and net10/net11 assets. C additionally needs these packages for its tensor rounding subcase. Source project composition includes older assets, but does not prove multidimensional APIs on netstandard/.NET Framework. The reviewed netcore tensor declaration file has no Experimental annotation; delivery must still check actual selected package metadata. Advertise a net10-plus-RC2-package route only after a separate actual net10 consumer build/run. Its package update can change behavior without a runtime upgrade.

### A. 02-054 - Separate invalid date parsing options from bad data — S (2)

Proposed ID `globalization.validate-date-parsing-options`. A fictional booking importer validates its configured formats/styles separately from customer data. Reproduce old false/new ArgumentException for invalid styles and invalid exact-format cases on both DateOnly and TimeOnly; check string and span entry points, single/multiple formats, valid parsing and malformed input returning false with valid options. Validate supported styles/formats or handle configuration failure explicitly; never blanket-catch every bad date. S covers a small managed parser boundary and full shared delivery. Source: [date parsing][date-parsing].

### B. 02-055 - Preserve deliberate legacy numeric representations — M (3)

Extend `numerics.preserve-conversion-intent` from PIN-737. A fictional instrument archive must retain an agreed significant-digit decimal representation while a new analysis path preserves exact source intent. Implement original invariant G7/G15-to-decimal helpers; test positive/negative values, exponent notation, ties-to-even, small/large finite magnitudes and range/nonfinite rejection. Cover representative Decimal-to-double/float/Half and BigInteger-to-double/float/Half corrections, with BFloat16 only where actually available; do not invent a .NET 10 BFloat16 equivalent. Check constructor/Convert/generic routes that the recipe advertises. Compile a net10 constant-conversion consumer with each SDK, compare embedded results and run it on the actual net10 runtime separately from runtime-input casts. Raw protocol bits use the existing bit-roundtrip guidance. M covers bounded conversion families plus the SDK/target distinction. Source: [conversions][conversions].

### C. 02-056 - Choose explicit fractional rounding semantics — M (3)

Proposed ID `numerics.choose-fractional-rounding`. A fictional measurement exporter chooses exact binary-input rounding, decimal-quantity rounding, or an explicitly retained scale/round/divide policy. Check below/above-midpoint original inputs, large finite values, double digits 16/17 and float digits 7/9, negatives and invalid modes; distinguish unaffected Decimal and no-digits overloads. Execute representative double/float/Half/NFloat delegating entry points when advertised. For TensorPrimitives.Round check independent and in-place spans, digits zero/nonzero and mode/capacity failures with the actual packages. State legacy scaling's limitations rather than promising historical reproduction for every value. M includes scalar/span results and real package evidence. Sources: [rounding][rounding], manifest `round-tensor-implementation` and `libraries-preview2`.

### D. 02-057 - Select a small integer conversion policy — M (3)

Proposed ID `numerics.select-small-integer-conversion`. A fictional binary record writer chooses saturation, checked rejection or explicit intermediate/narrowing behavior before storing an offset. Check all five destinations with float/double and representative Half inputs: in-range fractions, both bounds, infinities and NaN; verify checked overflow and bounds validation. Demonstrate ConvertToInteger, ConvertToIntegerNative and explicit intermediate alternatives without asserting portable historical native values. Measure the tested CoreCLR OS/architecture. Interpreter/Native AOT/Mono claims need their own actual runs; otherwise make those execution gaps explicit and do not advertise their verification. M covers the policy matrix and real runtime-dependent behavior. Source: [small integers][small-integers].

### E. 02-058 - Handle complex nonfinite results deliberately — M (3)

Proposed ID `numerics.handle-complex-special-values`, linked to PIN-736 without changing `numerics.select-complex-precision` identity. A fictional signal pipeline classifies undefined versus directed-infinite results. Execute original multiply/divide/zero-divisor, reciprocal/magnitude/power, representative elementary/trigonometric/hyperbolic/inverse cases and signed-zero checks on nongeneric Complex on both releases. Explain the full affected function family; do not promise an unspecified quotient-zero sign. Check finite values and application classification logic alongside NaN/infinity behavior; show that changing to generic storage is not required for the migration. M covers original special-value checks and full offline delivery. Source: [Complex][complex].

### F. 02-059 - Align tensor shapes without losing metadata — L (5)

Proposed ID `numerics.align-tensor-shapes`. A fictional instrument batch compares and combines differently ranked measurements. Execute default versus explicitly ranked empties, redundant leading versus significant internal/zero dimensions, shape equality versus broadcasting, `[0]`/`[2,0]` success and `[0]`/`[0,2]` rejection. Check first-input axes for stack/concatenate, caller-destination shape retention, views/index extrema and all five Any predicates including empties and late matches. Establish native-sized traversal with a safe bounded native-backed allocation and measured resource requirements before testing beyond int.MaxValue; if that required check cannot run, retain incomplete delivery evidence rather than inferring it from a small managed array. L includes varied shape APIs and the large-logical-range verification route. Source: [tensor shapes][tensor-shapes].

### G. 02-060 - Copy tensor views with explicit storage ownership — M (3)

Proposed ID `numerics.copy-tensor-views-safely`. A fictional grid transformer snapshots each overlapping input or allocates independent output. Reproduce unsupported strided/shifted overlap and prove rejection leaves output unchanged; contrast documented dense/identical allowed cases across copy/flatten, elementwise, resize and reversal. Test Try capacity/shape false separately from layout exceptions. Check repeated zero-stride output rejection, permitted singleton/empty output, incompatible System.Array storage, writable covariance rejection and read-only compatible covariance. Verify corrected snapshots retain the intended logical values, not just that no exception occurs. M covers a single storage-ownership goal with real package evidence. Sources: [tensor storage][tensor-storage] and manifest `tensor-guide`.

### H. 02-061 - Validate dates against the Japanese calendar boundary — S (2)

Proposed ID `globalization.validate-japanese-calendar-range`. A fictional historical-record importer queries MinSupportedDateTime and routes unsupported Gregorian dates to an explicit validation result or a deliberately chosen alternative representation. Run the old boundary, a date inside the removed interval, the day before the new minimum and the minimum itself. Check a real JapaneseCalendar operation, not only a constant comparison, and preserve stored historical values instead of silently clamping them. Record culture/globalization mode and fail honestly if the environment cannot instantiate the calendar. S covers this bounded managed boundary and complete delivery. Source: [calendar][japanese-calendar].

### I. 02-062 - Validate tensor clamp bounds consistently — S (2)

Proposed ID `numerics.validate-tensor-clamp-bounds`. A fictional calibration import requires lower <= upper before modifying output. Validate each relevant scalar/per-element bound independently of TensorPrimitives.Clamp exception behavior. Execute vectorizable float and nonvectorizable decimal cases with reversed/equal/ordered bounds, short spans and spans with scalar remainders; record the actual package/type behavior rather than repeating the Preview 7 headline. Show corrected validation preserves destination contents on failure and expected clamped values on success. S covers a small validation boundary; a claimed additional unsupported type/route requires evidence before expansion. Sources: manifest `libraries-preview7`, `clamp-implementation` and the reference signatures.

## Reproduce source verification

PowerShell 7 and public network access are required. This exact block was executed from an unrelated directory. It downloads only immutable source bytes to a new temporary directory, checks all 24 hashes and the complete mitigation headings of all eight notes, and reads the saved proposal titles. It does not compile or execute inspected code, install tools or change PATH.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-42ee9364'
$ErrorActionPreference = 'Stop'
$manifest = Get-Content -LiteralPath './docs/planning/audits/numeric-and-parsing.sources.json' -Raw | ConvertFrom-Json
$auditTemp = Join-Path ([IO.Path]::GetTempPath()) ('pin683-' + [guid]::NewGuid())
New-Item -ItemType Directory -Path $auditTemp | Out-Null
try {
    if ($manifest.sources.Count -ne 24) { throw 'Expected 24 reviewed sources' }
    $noteIds = @('date-parsing', 'conversions', 'rounding', 'complex', 'small-integers', 'tensor-shapes', 'tensor-storage', 'japanese-calendar')
    foreach ($source in $manifest.sources) {
        $file = Join-Path $auditTemp ($source.id + '.txt')
        Invoke-WebRequest -Uri $source.url -OutFile $file
        if ((Get-FileHash -LiteralPath $file -Algorithm SHA256).Hash.ToLowerInvariant() -ne $source.sha256) {
            throw "Source hash mismatch: $($source.id)"
        }
        if ($source.id -in $noteIds) {
            $body = Get-Content -LiteralPath $file -Raw
            foreach ($section in @('Previous behavior', 'New behavior', 'Recommended action', 'Affected APIs')) {
                if ($body -notmatch ('(?m)^## ' + [regex]::Escape($section) + '\r?$')) {
                    throw "Missing mitigation section: $($source.id) / $section"
                }
            }
        }
    }
    Get-Content -LiteralPath './docs/planning/audits/numeric-and-parsing.md' |
        Select-String -Pattern '^### [A-I]\. '
    'PASS: 24 source hashes; eight full mitigation notes; nine approval-only delivery proposals'
}
finally {
    Remove-Item -LiteralPath $auditTemp -Recurse -Force
}
```


[date-parsing]: https://github.com/dotnet/docs/blob/a109e77898150ddb66e3000ce315d23fcc2700e9/docs/core/compatibility/core-libraries/11/dateonly-timeonly-tryparse-argumentexception.md
[conversions]: https://github.com/dotnet/docs/blob/a109e77898150ddb66e3000ce315d23fcc2700e9/docs/core/compatibility/core-libraries/11/decimal-biginteger-floating-point-conversions.md
[rounding]: https://github.com/dotnet/docs/blob/a109e77898150ddb66e3000ce315d23fcc2700e9/docs/core/compatibility/core-libraries/11/math-round-digits.md
[complex]: https://github.com/dotnet/docs/blob/a109e77898150ddb66e3000ce315d23fcc2700e9/docs/core/compatibility/core-libraries/11/complex-annex-g-special-values.md
[small-integers]: https://github.com/dotnet/docs/blob/a109e77898150ddb66e3000ce315d23fcc2700e9/docs/core/compatibility/jit/11/fp-to-small-integer.md
[tensor-shapes]: https://github.com/dotnet/docs/blob/a109e77898150ddb66e3000ce315d23fcc2700e9/docs/core/compatibility/core-libraries/11/tensor-shape-alignment.md
[tensor-storage]: https://github.com/dotnet/docs/blob/a109e77898150ddb66e3000ce315d23fcc2700e9/docs/core/compatibility/core-libraries/11/tensor-storage-layout-validation.md
[japanese-calendar]: https://github.com/dotnet/docs/blob/a109e77898150ddb66e3000ce315d23fcc2700e9/docs/core/compatibility/globalization/11/japanese-calendar-min-date.md
