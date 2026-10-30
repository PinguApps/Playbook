# Inspect declared type layout through compiler symbols

## Goal

Inspect the declared layout of fictional seed and sensor packets while analysing their source. Read layout kind, packing and explicit size from a public compiler symbol. These values describe source/metadata declarations; they do not compute the runtime size, managed object representation, field offsets or a safe wire/interop format.

## Older approach

The actual Microsoft.CodeAnalysis5.9.0 closure has no `INamedTypeSymbol.TypeLayout`. For these three declarations, each of which has an explicit `StructLayoutAttribute`, a bounded public-attribute reader extracts the constructor's kind and named `Pack`/`Size`. Omitted named values remain zero. This is not a general decoder for metadata pseudo-custom attributes or compiler defaults when no source attribute is available.

Create `PacketLayout.csproj`, `Directory.Packages.props`, `NuGet.Config` and `Packets.cs` in one directory. Pin SDK10.0.402 with `sdk.version` and `sdk.rollForward: disable` in `global.json`. The actual comparison uses runtime/reference10.0.13 and C#14.

{{snippet:project-10-packetlayout}}

{{snippet:packages-10-packetlayout}}

{{snippet:feeds-10-packetlayout}}

{{snippet:source-10-packetlayout}}

The older project does not define `CURRENT`. Its real public-attribute fallback returns Sequential/2/24, Explicit/1/32 and Auto/0/0. Calling `symbol.TypeLayout` against that package produces CS1061. A successful old attribute reader does not prove the new property existed.

## Newer approach

Use a separate directory with the same four filenames. Pin SDK11.0.100-rc.2.26504.105 in `global.json`; actual execution uses matching11 RC2 reference/runtime assets and C#15. Restore the pinned Microsoft.CodeAnalysis.CSharp5.12.0-1.26504.105 from Microsoft's `dotnet-tools` feed using this mapping. It resolves Common5.12.0-1.26504.105 and Analyzers5.12.0-1.26465.4. Package version support is independent of the host target number.

{{snippet:project-11-packetlayout}}

{{snippet:packages-11-packetlayout}}

{{snippet:feeds-11-packetlayout}}

{{snippet:source-11-packetlayout}}

`INamedTypeSymbol.TypeLayout` returns public `Microsoft.CodeAnalysis.TypeLayout`. `Kind` has type `System.Runtime.InteropServices.LayoutKind`, `PackingSize` is `ushort`, and `Size` is `int`. The property exposes declared metadata/source layout, including compiler-emitted defaults where appropriate. The examples deliberately provide explicit attributes and assert their exact values. Sequential ordering does not turn24 into a measured managed object size; explicit size32 does not validate a serializer, platform ABI or marshalling operation. No `sizeof` or `Marshal.SizeOf` guarantee follows.

## Migration

1. Check the actual package closure. Preserve the bounded attribute reader on5.9 if that is sufficient for the declarations you own; do not use internal Roslyn symbols to manufacture a replacement property.
2. When upgrading to the qualified5.12 package, obtain an `INamedTypeSymbol` from a valid compilation or metadata reference. Check compilation diagnostics and failed symbol lookup before reading layout.
3. Replace source-attribute decoding with `symbol.TypeLayout` when you need its public declared-layout result. Read all three values; zero packing/size is a declared/default value, not a measured pointer-size calculation.
4. Keep any runtime interoperability validation separate. Test the actual runtime, architecture, unmanaged boundary and serialization contract if your application depends on them; this inspector does not establish those contracts.
5. In each project directory run `dotnet restore`, `dotnet build -c Release` and `dotnet run -c Release --no-build` through the appropriate absolute SDK executable. Retain the generated lock and use `dotnet restore --locked-mode` subsequently. Expect exactly the three packet PASS lines above; a changed value or compiler error fails.

## Constraints

Only target11/edition11 is served. The valid net10 comparison adds no foundation. `introducedIn:11.0` identifies the measured release-family route; no exact first-nightly claim is made. Selected Common/CSharp assets are `lib/net10.0`; no older-target support is inferred merely from that label or from netstandard assets. No provider, rendering mode, native dependency, optional workload or experimental opt-in is required. C# is canonical for this shared symbol API; no separate VB route is needed to explain the same property. Real execution was Windows x64, with no runtime-layout or other-platform execution claim.

The5.12 package identifies VMR820230c386db5b0106c5430b3e5aee92a7826bf5. Its public source definition agrees with the measured property and types. The earlier Roslyn-main5.13 audit is not relabelled as RC2. All artifacts here are pre-GA; PIN-959 coverage and PIN-961 actual-GA reconciliation stay open.

## Evidence

Both actual dependency sets compile/run the original declarations and check deliberate sequential/explicit/automatic layout and packing/size. `eng/Test-CompilerInspections.ps1 -Record` refreshes the paired sample-evidence records and verifies CS1061 for a direct old-package property call. Those records retain commands, dependencies, exact inputs and SDK/runtime output. Real installed stdio list/get from an unrelated directory inspects the exact ordinary built/packed artifact for discovery ID/title/goal, complete canonical body, applicability, current evidence and target selection. Target10 correctly has no applicable served edition. Proxy-independent offline retrieval is distinct from network isolation, and metadata inspection is distinct from runtime object/interop-layout proof.
