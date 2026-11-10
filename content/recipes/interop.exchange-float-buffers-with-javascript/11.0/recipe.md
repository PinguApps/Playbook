# Exchange floating-point buffers with JavaScript

## Goal

Exchange fictional Reed Bank depth samples with browser code. Choose an array
when a copied value is appropriate, a span for a synchronous bounded view, or an
ArraySegment for a shared view that must outlive the initial call. The latter
requires explicit disposal. Direct float support removes the older widening and
copy-back adapter; it does not establish a measured performance improvement.

## Older approach

.NET10 supports double buffers across the same real JavaScript boundary. Widen
float values before the call and narrow the returned values afterward. For views,
copy the selected range back explicitly; a retained segment needs staging storage
until JavaScript finishes. This serial example permits one outstanding staged
segment, not concurrent use. A reusable concurrent adapter needs per-operation
ownership. This remains a valid alternative on .NET11.

Add using System.Runtime.InteropServices.JavaScript and put the class in namespace
Playbook.Samples. The public class is required for generated exports.

{{snippet:older}}

## Newer approach

.NET11 adds float[]/Span<float>/ArraySegment<float> conversions to the actual
JSMarshalerArgument runtime and corresponding generator support. Application code
declares imports/exports; do not call generated marshaller plumbing by hand.
Use JSMarshalAs Array<Number> for arrays and MemoryView for span/segment.

{{snippet:newer}}

The numeric array route creates a typed copy (Float64Array for the older adapter,
Float32Array for the new float route in the measured runtime). Mutating that copy
does not mutate the original managed array. A MemoryView refers to bounded managed
storage; slice() makes a copy and set() writes through the view. It is not an
ordinary typed array or a view that JavaScript should construct itself.

This complete JavaScript host registers the matching imports, exercises generated
exports, and checks lifetime rules. It uses the complete BridgeChecks class below
and an HTML element with id=result. Its host imports the published dotnet.js.

{{snippet:browser}}

Copy this class into BridgeChecks.cs alongside BufferBridge.cs. For the older
adapter only, define OLDER_ADAPTER in the project so its explicit copy-back runs.

{{snippet:checks}}

For a self-contained current example, name the project FloatSoundings.csproj,
put main.js in wwwroot, add Program.cs containing
`Console.WriteLine("FloatSoundings browser host");`, and create
wwwroot/index.html with this complete body:

{{snippet:html}}

Use this minimal project with the actual tested RC2 package. Requirements above
refer to the .NET11 release family; this pin records the artifact that ran.

{{snippet:project}}

Place this matching package manifest beside the project as Directory.Packages.props:

{{snippet:packages}}

## Migration

1. Use a browser-targeted net11.0 application and a matching .NET11 SDK/runtime
   package set. The sample declares C#15.0, AllowUnsafeBlocks=true for generated
   stubs and Microsoft.NET.Sdk.BlazorWebAssembly. It uses the Mono runtime and
   matching Microsoft.AspNetCore.Components.WebAssembly host package. No Blazor
   component, server circuit, UI rendering mode or provider is required.
2. Copy the newer class with its imports and namespace, and register the soundings
   JavaScript module through runtime.setModuleImports before invoking these methods.
   Keep the existing older adapter on net10.0/C#14. Both use the actual generator
   from Microsoft.NETCore.App.Ref; no handwritten JSMarshalerArgument calls or
   broad experimental/preview opt-in is needed for these tested contracts.
3. For copied values, replace double[] conversion adapters with float[] imports
   and exports. JavaScript Number values narrow to single precision. Test normal,
   negative, empty, NaN and positive/negative infinity inputs; do not use JSON as
   a substitute for this interop path because it changes non-finite semantics.
4. Pass a span only to synchronous work. A view created for Span<float> expires
   when the call returns. Do not retain it for a promise, timer or later callback.
   Copy within the call if JavaScript needs independent longer-lived values.
5. For shared storage that must survive the call, pass an ArraySegment with the
   correct offset/count. Its MemoryView pins the managed array. Use try/finally
   to dispose the view when finished, including on failure; do not access it
   after disposal. Keep mutations within the segment. Coordinate ownership when
   multiple consumers can access the buffer; this example is single-threaded.
6. Build then publish the full browser application, for example from its directory:
   <absolute-dotnet> build -c Release -p:RestoreLockedMode=true, then
   <absolute-dotnet> publish -c Release --no-restore -p:PublishTrimmed=false.
   Serve bin/Release/net11.0/publish/wwwroot over HTTP(S) and open index.html in
   a real browser. The included local Node fixture chooses an ephemeral loopback
   port and closes it afterward. Do not open the files directly with file://.
7. Inspect the generated imports and exports under obj and execute both actual
   dependency sets. Check sentinel elements around slices, retained segment writes,
   expired span rejection and disposed segment rejection. The complete samples
   include BridgeChecks and index.html, and the runner captures all these checks.

## Constraints and alternatives

The interpreted Mono route here restores Microsoft.NET.Sdk.WebAssembly.Pack and
Microsoft.NETCore.App.Runtime.Mono.browser-wasm through the matching SDK/package
set. Actual builds and publish succeed without an installed wasm-tools workload;
the SDK recommends it for optimization. Native relinking/AOT needs its own matching
workload/Emscripten evidence and is outside this route. Do not infer CoreCLR-browser
support from Mono execution. No manual DLL execution establishes browser behavior.

JSExport span/segment inputs in the proof receive runtime-created views from a
reentrant synchronous import, not a fabricated JavaScript MemoryView. A span cannot
be returned for use after its call. A segment's backing array remains pinned while
its JS proxy lives. Copy when sharing/ownership would be unclear. The old adapter
has deliberate float/double conversion and copy-back costs and does not promise
zero-copy behavior. No benchmark, universal precision, cross-thread synchronization,
other-browser/OS, CoreCLR-browser, AOT or final-GA claim is made.

## Evidence

The .NET 11 SDK baseline is now 11.0.100 GA with preview selection disabled. The .NET 10 comparisons retain their declared SDKs. Earlier RC audit details below are historical; use the current sample pins, dependency locks and GA verification records for current coverage. Existing native exclusions and pending Human gates still apply.

eng/Test-InteropSoundings.ps1 -Record -VerifySources captures exact builds,
resolved dependency sets, canonical inputs, compiler/reference/generator/generated
stub hashes and deployed JS/wasm identities. Compilation uses SDK10.0.403 with
10.0.13 packages/runtime and SDK11.0.100 with matching RC2 packages/runtime.
The framework reference hashes match the original runtime-security asset audit.
Paired official contracts and generator source are pinned in
interop-soundings.sources.json; source alone is not browser proof.

eng/Test-FloatSoundings.mjs runs both actual published applications in Chromium.
Separate browser.evidence.json records cover normal/empty/special arrays, copy
semantics, generated import/export round trips, span/segment offsets and surrounding
sentinels, span expiry, retained segment mutation, disposal and rejected access.
The canonical classes and host execute; the newer typed views are Float32Array,
the older adapter's views are Float64Array. No mock JS runtime is used.

The ordinary packed tool is installed through an isolated local feed/cache and
inspected offline from an unrelated directory using real stdio list/get for
ID/title/goal, full canonical Markdown, target11/edition11, applicability and fresh
evidence. There is no ninth .NET10 foundation or recurring browser/native matrix.
PIN-959/PIN-962 and existing Human/actual-GA gates stay open.
