# Select the hosted generator in an authored Razor build task

## Goal

An exhibit compiler owns an MSBuild target that turns Razor markup into C#. Choose the actual generator deliberately, keep dependency discovery aligned, then compile and execute the emitted component. This is advanced task authoring: ordinary Razor applications should retain their normal SDK build.

## Migration

1. Keep each toolset together. The previous consumer uses SDK 10.0.402, `net10.0` and C#14. The current consumer uses SDK 11.0.100-rc.2.26504.105, `net11.0` and C#15. Resolve tasks, `rzc`, compiler, MSBuild and Roslyn from the selected SDK, with its matching runtime/reference packs. Do not copy just one task DLL into another SDK.
2. Create the complete files below in separate previous/current directories. The empty directory build/package boundaries prevent unrelated application settings from silently changing the example. `ExhibitParts` is an original project dependency containing a reusable badge. There is no compiler package, external provider, native asset or workload.
3. Restore/build the project dependency before discovery. `ResolveReferences` supplies absolute `ReferencePath` entries, including the actual built badge assembly and framework references. `SdkRazorTagHelper` consumes that full set. An assembly name or relative file name is insufficient. The target always regenerates; add dependency-aware incremental inputs/outputs only when your real host can track every source, reference and configuration change.
4. Preserve the valid older engine route. On SDK10, call discovery with `Assemblies`, then generation with its manifest. Do not pass `UseSourceGenerator` or generation `Assemblies`: those task parameters are unavailable there. SDK10 rejects the new flag with MSB4064, rather than silently choosing a hosted path.
5. On RC2, pass `UseSourceGenerator="true"` to both tasks and `Assemblies` to both. Generation hosts the Razor source generator and discovers from the supplied compilation references; it does not use the legacy manifest as its source of descriptors. Keep the manifest argument because it is still required by the public task contract. This is distinct from enabling the ordinary application's Razor generator.
6. To retain the current engine, set both flags to `false`. Omitting the two flags also selects the engine in these actual artifacts. In that mode generation uses the discovered manifest and ignores its `Assemblies` argument. The SDK's internal `_RazorToolUseSourceGenerator` property also defaults to false, but it is a private forwarding detail, **not a stable application setting**. This recipe sets public task parameters directly.
7. Run `dotnet build ExhibitCompiler.csproj -c Release -bl:exhibit.binlog` with the absolute matching SDK executable from each consumer directory, then run `dotnet bin/Release/net10.0/ExhibitCompiler.dll` or its `net11.0` counterpart with that executable. Fresh restore creates the project-only `packages.lock.json`; retain it for subsequent `-p:RestoreLockedMode=true` builds. Expected HTML is `<p>Lantern collection: 42</p>` followed by `<strong>Lantern &lt;Archive&gt;</strong>`. The harness checks exact output and prints the actual runtime.
8. Inspect `obj/Release/<target>/Exhibit.g.cs`, the adjacent `exhibit.taghelpers.json`, and the binlog. The generated source must call `OpenComponent<global::ExhibitParts.ExhibitBadge>`. Remove only ExhibitParts from the discovery/generation reference set as a negative control: the descriptor and component call disappear. A completed generation task alone does not establish complete discovery. The missing-reference controls pass when generation exits zero but both `discoveredBadge` and `manifestHasBadge` are false. An empty `[]` manifest produces the same missing component call in engine mode; hosted mode still discovers and renders the badge from its references. Add an unclosed `@if (true) {` to the Razor source: the real task must fail with RZ1006. Remove GeneratedOutput metadata or use a relative discovery assembly path: task validation must fail before compilation.

## Constraints

These public parameters are available in the pinned task assemblies and exercised through real `dotnet` MSBuild, not a task imitation or reflection host. Exact SDK pins qualify this advanced consumer; they are separate from the general .NET runtime families and do not establish support for every SDK release. The measured framework/runtime baselines are .NET/ASP.NET Core 10.0.13 and 11.0.0-rc.2.26504.105. The execution records pin the task/tool closure, MSBuild and Roslyn by product version and SHA-256. Only rows marked `matchesAudit: true` overlap and reconcile with the source audit. The rzc configuration files, MSBuild and Roslyn rows absent from that audit are hash-pinned from the actual SDK, not separately source-audit reconciled. No Razor.Language 6 metadata helper is used for execution.

Use the managed `tasks/net10.0` or `tasks/net11.0` assembly with dotnet MSBuild; this does not qualify the net472/Visual Studio task host. `UseServer=false` isolates these checks from a stale Razor build server. The `MVC-3.0` configuration name plus `component` document kind is the actual working task input; it is not an MVC HTTP rendering claim. `CreateItem` adds the emitted file to compilation after generation. The Razor HTML snippet-marker comments produce no component output.

No experimental diagnostic opt-in is needed in these artifacts. The edition has **RC2 evidence only and is not GA-qualified**; public task parameters do not promise a stable SDK disk layout, cross-version ABI or support for the private forwarding property. Requalify the complete toolset on upgrade. No older-target hosted-generator route, arbitrary extensions, IDE/Hot Reload, browser interaction, trimming, NativeAOT or deployment claim is made. The older engine remains valid. The exact eight approved .NET 10 foundations and all existing recipe identities are unchanged. PIN-959 qualifying coverage and PIN-964 actual-GA reconciliation remain open.

## Older approach

Keep the authored engine task target and manifest-based discovery with the SDK10 toolset. The unavailable new parameters are deliberately absent.

### .NET 10: `global.json`

{{snippet:n10-global-json}}

### .NET 10: `Directory.Build.props`

{{snippet:n10-directory-build-props}}

### .NET 10: `Directory.Build.targets`

{{snippet:n10-directory-build-targets}}

### .NET 10: `Directory.Packages.props`

{{snippet:n10-directory-packages-props}}

### .NET 10: `ExhibitParts/ExhibitParts.csproj`

{{snippet:n10-exhibitparts-exhibitparts-csproj}}

### .NET 10: `ExhibitParts/ExhibitBadge.cs`

{{snippet:n10-exhibitparts-exhibitbadge-cs}}

### .NET 10: `Exhibit.razor`

{{snippet:n10-exhibit-razor}}

### .NET 10: `Program.cs`

{{snippet:n10-program-cs}}

### .NET 10: `ExhibitCompiler.csproj`

{{snippet:n10-exhibitcompiler-csproj}}

## Newer approach

Choose the hosted generator explicitly, forwarding the same complete reference set to discovery and generation. To compare the current default, omit both `UseSourceGenerator` attributes; for an explicit engine selection, set both to false.

### .NET 11: `global.json`

{{snippet:n11-global-json}}

### .NET 11: `Directory.Build.props`

{{snippet:n11-directory-build-props}}

### .NET 11: `Directory.Build.targets`

{{snippet:n11-directory-build-targets}}

### .NET 11: `Directory.Packages.props`

{{snippet:n11-directory-packages-props}}

### .NET 11: `ExhibitParts/ExhibitParts.csproj`

{{snippet:n11-exhibitparts-exhibitparts-csproj}}

### .NET 11: `ExhibitParts/ExhibitBadge.cs`

{{snippet:n11-exhibitparts-exhibitbadge-cs}}

### .NET 11: `Exhibit.razor`

{{snippet:n11-exhibit-razor}}

### .NET 11: `Program.cs`

{{snippet:n11-program-cs}}

### .NET 11: `ExhibitCompiler.csproj`

{{snippet:n11-exhibitcompiler-csproj}}

## Evidence

`eng/Record-ExhibitCompiler.ps1` uses the existing sample-evidence workflow for both exact consumers, then executes the real task controls. Saved compilation/execution records bind exact inputs, commands, resolved project dependency, tool identities, generated C# and observed runtime output. Pass `-Net10Dotnet` and `-Net11Dotnet` with the absolute paths of your matching installed SDK hosts when rerunning capture; saved commands retain the authoring machine's observed locations. Compilation freshness compares exact input bytes, including the committed sample and helper inputs; capture commit/dirty metadata describes when those bytes were recorded, not a requirement to recapture after unrelated commits. Disposable binlogs, full manifests, generated files and failure logs remain under `artifacts/exhibit-compiler`; rerunning refreshes them. They are build evidence, not a new recurring native/OS matrix.

The previous default, current omitted-flag default, current explicit false and current hosted true all compile and execute the same real generated component. Missing-reference checks distinguish reference-driven discovery from mere task success; invalid syntax, required metadata, relative discovery paths and the unavailable older flag exercise actual diagnostics. The current generator's manifest-independence is implementation-specific; do not use an old manifest to compensate for incomplete hosted references. Official pinned task, tool and SDK-target sources accompany this offline body. Installed local-feed stdio inspection separately verifies ID/title/goal, full canonical source, target/edition, applicability and current evidence. No actual-GA or general application-support assertion follows.
