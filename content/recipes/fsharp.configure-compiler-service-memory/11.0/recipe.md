# Choose imported-assembly sharing in an F# compiler host (preview package)

## Goal

Choose a compiler host's imported-assembly sharing policy while preserving the symbols and diagnostics each project should see.

## Older approach

The previous-release host reuses a checker for a workspace. Its FCS 43.10.101 dependency requires exactly FSharp.Core 10.0.101; it has no shareImportedAssemblies parameter. This existing host is valid and is not presented as broken or devoid of caches.

{{snippet:older}}

## Newer approach

Default sharing is already enabled. A host can deliberately opt an individual checker out:

{{snippet:newer}}

Use FSharp.Compiler.Service 43.13.101-rc2.26504.105 and its **exact** FSharp.Core 11.0.101-rc2.26504.105 dependency for this initially verified preview-package route. Pin both in the host's package declarations and retain a packages.lock.json. The FCS implementation asset is `lib/netstandard2.0/FSharp.Compiler.Service.dll`; the .NET 10/11 hosts select Core's `lib/net10.0/FSharp.Core.dll`. The actual net10/net11 restore resolves these two packages; the other dependencies in the package's netstandard group are not automatically separate packages for these framework groups. Do not hand-select an incompatible older Core based on the FCS assembly's target label.

A small actual checker entry point can take the project's reference closure and source file:

{{snippet:host}}

Add `open System`, `open FSharp.Compiler.CodeAnalysis` and `open FSharp.Compiler.Diagnostics`. For a minimal managed host, build the references from `(AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") |> string).Split(System.IO.Path.PathSeparator)` and include the referenced contract binary plus the loaded FSharp.Core assembly location if absent. Pass a stable workspace load timestamp, unique project-file identity, full reference paths and a real source-file path. Inspect the returned Diagnostics and AssemblySignature; project options must match the real project. Production tooling should obtain its actual build-derived options rather than transplant this small fixture's reference set.

## Migration

1. Restore/lock the exact FCS/Core pair and compile the host. Set `DisableImplicitFSharpCoreReference=true`, add explicit PackageReference declarations for both packages and enable `RestorePackagesWithLockFile=true`. The samples set `DisableImplicitLibraryPacksFolder=true` to verify actual NuGet assets. This is an FCS **hosting API**, not a compiler command-line switch or MSBuild property. The new package does not itself require a .NET 11 host: this recipe executes it on net10.0 with SDK 10.0.402 and separately on net11.0 with SDK 11.0.100-rc.2.26504.105. F# 10/11 ordinary language modes suffice; no runtime-async or preview-language flag is required.
2. Keep one checker for the appropriate workspace lifetime. Retain default sharing unless the host deliberately needs that checker excluded. Setting false is not a hard memory cap, process sandbox or substitute for correct project references.
3. Match file identities, compiler configuration and complete reference closures. Shared metadata is safe only when those inputs agree. Distinct project outputs, type-provider assemblies, multi-module imports and unaccountable closures are excluded from this sharing mechanism.
4. Validate projects sharing a binary and projects with different reference closures. Compare intended symbols and diagnostics under default and false configurations. After replacing a referenced binary, ensure updated source and metadata are observed and a project without an extra reference still rejects its symbols. Notify FCS of real project-option changes through its supported invalidation/options lifecycle; do not change timestamps to conceal a stale reference in a real host.
5. Release obsolete checker/project results as their workspace closes. Imported entries are weakly retained while consuming closures remain live; retained check results can extend their lifetime. Do not advertise a public ClearCaches method: it is absent from this observed public surface. Internal cache-clearing implementation details are not a public host API.

## Constraints and alternatives

Sharing reduces duplicate imports for compatible workloads; benchmark savings are workload-specific. This recipe promises neither a percentage nor a hard upper memory bound and adds no benchmark suite. Default sharing is an automatic benefit; the authored decision here is the explicit hosted policy and correctness boundary.

The primary edition targets .NET 11; its conditional .NET 10 route uses the **new** FCS/Core pair on an older host, not the historical previous-release sample. Exact package matching is necessary because of the declared dependency contract. Exact SDK patch numbers are verification baselines. This recipe covers this first-party release change, not arbitrary FCS releases. No workload, provider, rendering mode, UI or native toolchain is needed. Managed execution was verified on Windows x64; other platforms and final GA packages require their own proof.

## Evidence

Three real restored/locked hosts compile and run: previous FCS on .NET 10, new FCS on .NET 11, and new FCS on .NET 10. They compile disposable contract binaries, check shared and differing closures, compare intended symbols/diagnostics, replace an int-valued contract with a string-valued contract at the same path and verify both checkers observe the change. Missing-reference diagnostics prove closure isolation. This is actual FCS execution, not mocked results or inspection of metadata alone. Fresh evidence and complete guidance are retrieved through the isolated installed-tool stdio harness. GA reconciliation remains separate.

Official sources: [public checker signature](https://github.com/dotnet/dotnet/blob/820230c386db5b0106c5430b3e5aee92a7826bf5/src/fsharp/src/Compiler/Service/service.fsi), [default setting](https://github.com/dotnet/dotnet/blob/820230c386db5b0106c5430b3e5aee92a7826bf5/src/fsharp/src/Compiler/Service/service.fs), [import matching and weak retention](https://github.com/dotnet/dotnet/blob/820230c386db5b0106c5430b3e5aee92a7826bf5/src/fsharp/src/Compiler/Driver/CompilerImports.fs).
