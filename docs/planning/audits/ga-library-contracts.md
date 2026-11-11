# GA runtime and shared-library reconciliation

PIN-962, 11 November 2026. This is a bounded release decision register, not
permission to publish or a claim of execution on every supported platform.

## Authority and coverage

Scope follows SPEC.md at `0a8989814ea11346354464b845c79c15bb71c5e9`, stories
41–44, 47, 93–95, 130, 132–133, 146 and 158–160; approved proposal `b3778cc`
(`GA_LIB`); ADRs 0001–0004 and the release plan. Root PLAN.md, CONTEXT.md and
CONTEXT-MAP.md are absent. No explicit source pointer is unavailable.

All 48 native delivery inputs on PIN-962 are Done in PinguApps/Playbook.
Their completion permits reconciliation; their historical RC observations do
not become GA proof. PIN-1050 and PIN-1051 explicitly transferred final tensor
execution here under Pingu's 30 October preparation decision. Their original
recipes and older comparison identities remain intact.

The [official release feed](https://builds.dotnet.microsoft.com/dotnet/release-metadata/11.0/releases.json)
and [GA notes](https://github.com/dotnet/core/blob/main/release-notes/11.0/11.0.0/11.0.0.md)
report SDK11.0.100/runtime11.0.0, released 10 November. Installed SDK source
identity and the tensor package nuspec identify VMR
`297acf930fb0f5c07fd4e51d78e521b4ad2e5d1f`. This clears the availability wait.
The retained RC2 VMR is `820230c386db5b0106c5430b3e5aee92a7826bf5`.

The adjacent generated source register binds 143 prepared edition11 library
recipes by ID, title, stability and metadata/Markdown hashes. It compares all
92 distinct RC2 VMR sources in the data/IO, serialization, Extensions,
runtime/security, numeric/parsing and cryptography-platform audit manifests,
validating their retained hashes before comparison. These include reference
declarations, implementation contracts, generator diagnostics and project inputs.
Original disposition registers remain the goal/grouping/exclusion authority.

## Findings and disposition

| Surface | GA comparison | Required action |
| --- | --- | --- |
| 91 retained source files | Byte-identical RC2/GA sources. | No correction or additional authoring goal established by this comparison. Keep existing experimental labels, platform limits and independently versioned package requirements. |
| System.IO.Compression reference declarations | ZstandardDictionary.Train returns byte[] rather than ZstandardDictionary. | Existing `compression.use-zstandard` explicitly excludes dictionary/prefix interoperability and never calls Train. No broken taught call or new reusable goal follows merely from the return-type correction. |
| Tensor package/backport | Actual System.Numerics.Tensors11.0.0 nuspec identifies GA VMR; selected asset hash is recorded. Four backport sources are separately compared against the merged release branch. | Execute actual package consumers; merged source alone is insufficient. Results below satisfy the transferred checks. |
| Existing saved consumers | Library net11 receipts use SDK11.0.100, including compile-only platform routes. | Preserve declared not-tested exclusions. Neither compilation nor SDK identity establishes native behavior. No new platform matrix or hardware requirement. |

The cumulative [library notes](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-11/libraries),
[runtime notes](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-11/runtime)
and [compatibility index](https://learn.microsoft.com/en-us/dotnet/core/compatibility/11)
remain the release-family discovery entry points. The compatibility index itself
disclaims completeness; this bounded comparison is not proof that no future
breaking-change note can appear. Automatic performance improvements remain
excluded unless they change an authored decision. SDK/web/data/native framework
reconciliation retains its own owners rather than being silently absorbed here.

## Actual final-artifact execution

Windows x64 CoreCLR, SDK11.0.100, runtime/reference11.0.0, locked
System.Numerics.Tensors11.0.0/lib/net11.0 asset SHA256
`d792726013d402cf22c0b24a73c04251b38bd8a97fb556b3eedf0e599ce96da6`:

- SignalPipeline: complex finite/nonfinite classification, signed zero and named
  methods/operators pass; older SDK10.0.403/runtime10.0.14 controls also pass.
- InstrumentBatch: default/ranked empties, leading/internal/zero dimensions,
  shape equality versus broadcasting, first-input stack/concatenate axes,
  destinations/views/extrema and all five scalar/pairwise Any families pass.
- Its opt-in native proof passes at **2,147,483,656 elements** after memory
  preflight: 6,442,450,968 allocated bytes plus 2 GiB headroom; observed available
  physical/commit memory exceeds 198/204 GiB. Elapsed 2m52s, peak working set
  6,477,611,008 bytes. Copy/equality, late mismatch, reversal, offset view,
  multidimensional indexing and all extrema also pass beyond int.MaxValue.
- GridTransform: all copy/flatten wrappers, Try false versus layout exceptions,
  rejection before writes, permitted dense/identical operations, resize/reversal,
  repeated/singleton/empty outputs and Array/covariance controls pass. Snapshot
  and independent-output logical values pass, including 42,76,108,144.

Two test-fixture corrections were necessary: zero-length dimensions require zero
strides in the constructed expected empty view; destination concatenation requires
at least two inputs, so singleton/empty output controls now include a compatible
empty second input. These are verified against actual GA, not inferred API fixes.
Historical RC receipts remain in Git at the task base `9abc66c7`.

Recipe-specific pending tensor proof can clear after these results. PIN-967
remains the global stable candidate gate. This audit identifies **no additional
authoring follow-on** from the inspected delta; no empty or speculative tickets
are created. Independent platform exclusions and all other Human/release gates
remain in force. No release or deployment is authorized.

## Reproduce

PowerShell7, HTTPS and the two existing SDK hosts below are required. The .NET10
portable host avoids an incomplete shared SDK10.0.403 installation; no PATH or
persistent environment changes are needed. The tensor recorder compares PATH
before/after and performs its own native-memory preflight.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-74ea50d7-47a2e70aac883dab'
$ga = 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe'
$older = 'V:\_worktrees\Playbook\t3-1ead8027\artifacts\dotnet10\dotnet.exe'
& ./eng/Inspect-GaLibraryContracts.ps1 -OutputPath docs/planning/audits/ga-library-contracts.sources.json
& ./eng/Record-ComplexSignals.ps1 -Net10Dotnet $older -Net11Dotnet $ga -VerifySources
& ./eng/Record-InstrumentBatch.ps1 -Net10Dotnet $older -Net11Dotnet $ga -RunReleaseChecks -VerifySources
& ./eng/Record-GridTransform.ps1 -Net10Dotnet $older -Net11Dotnet $ga -RunReleaseChecks -VerifySources
```

Expect source-comparison PASS, successful paired compilation/execution and native
preflight/PASS output. Saved sample records retain commands, dependencies, inputs
and artifact hashes. Other operating systems, architectures, AOT and Mono remain
unverified by these runs. Source/artifact comparison does not rerun unrelated
consumer behavior or replace its existing evidence limitations.

Installed MCP inspection (setup builds/packs the real server, installs that local
artifact and calls stdio list/get from an unrelated directory):

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-74ea50d7-47a2e70aac883dab'
$env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 `
  -RecipeId @('numerics.align-tensor-shapes','numerics.copy-tensor-views-safely','numerics.handle-complex-special-values') `
  -Query '' -RecipeEdition '11.0' -TargetVersion '11.0' `
  -PackageVersion '0.0.0-pin962' -BuildAndPack `
  -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory './artifacts/pin962/inspection'
```

Expect all three discovery IDs/titles/goals, full canonical Markdown and target11,
edition11 applicability with verified tensor proof. This checks the agent's
served guidance; other-platform runtime behavior remains outside its proof.
