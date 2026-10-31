# Link native code into experimental CoreCLR WebAssembly apps

## Goal

Call a small C calibration function from a browser-hosted .NET app. The fictional instrument's accepted integer readings are 0..10000; input 60 must produce 42. This demonstrates a native integration boundary rather than a claim that this calculation needs native code.

**Experimental route; not tested locally.** The reviewed .NET 11RC2 CoreCLR browser targets implement native relinking. Matching CoreCLR browser runtime/WebAssembly SDK packs and Emscripten are unavailable in this Windows installation and existing Docker authoring image. A wasm-tools workload label or .NET 10 Mono pack does not establish that route. The canonical C/C#/project/JavaScript below are source-reviewed intended inputs, not compiled or executed native evidence. Do not adopt them as a production-verified toolchain.

## Older approach: .NET 10 / C# 14

No supported equivalent CoreCLR native-relink route is established for the older baseline: the newer pipeline replaces earlier stub targets. A managed implementation can preserve this bounded calculation when reusing a native library is optional. It is an honest functional alternative, **not** native-library reuse or CoreCLR WebAssembly proof. Existing Mono WebAssembly native integration is a different runtime choice and is not invalidated by this change; no unverified Mono recipe is advertised here.

{{snippet:older}}

## Newer approach: intended route, .NET 11 / C# 15

The current CoreCLR pipeline compiles NativeFileReference .c/.cpp sources, generates managed-to-native call helpers from P/Invoke metadata, and relinks dotnet.native.wasm against the runtime pack. The P/Invoke module name below matches the native source filename without extension. A prebuilt .a/.o must target the same wasm ABI/toolchain; a Windows DLL or ordinary Linux library is not interchangeable.

{{snippet:native}}

The managed boundary validates the domain before crossing it and uses only a blittable int signature. Put it in Calibration.cs with `using System.Runtime.InteropServices;`. No complex string/object marshalling is claimed.

{{snippet:newer}}

Use the source-qualified matching WebAssembly SDK configuration, rather than merely adding a native item to an arbitrary console or Blazor project:

{{snippet:project}}

Program.cs contains:

{{snippet:program}}

The canonical main.js starts the app using the produced runtime loader:

{{snippet:startup}}

Serve the produced browser bundle over a local HTTP origin; its index.html uses a module script pointing to main.js. Keep the generated _framework tree alongside it, with the runtime loader and relinked native assets from the same build. This is a plain browser application, not a Blazor rendering-mode integration. No server or browser was started during authoring.

## Migration and toolchain qualification

1. Inventory the native ABI, exported symbol, argument widths and memory ownership. Keep the minimal integer contract above while first validating the build route.
2. Use matching .NET 11 CoreCLR browser runtime and WebAssembly SDK packs. The intended baseline is SDK/runtime11.0.100-rc.2.26504.105 /11.0.0-rc.2.26504.105, C# 15, browser-wasm. Pin any resolved package closure and inspect the actual SDK's _WebAssemblyTargetsFile/import graph. It must supply the CoreCLR relink targets, not Mono-only targets. Merely setting UseMonoRuntime=false is insufficient.
3. Qualify Emscripten 6.0.2 from that runtime source's Versions.props together with matching workload/native pack assets and JS host. Record absolute compiler/node paths and versions. No compatible installed combination is asserted here. Do not activate emsdk shell/PATH integration as setup.
4. On that qualified installation, build/publish ShoreCalibration.csproj in Release. Confirm the native source is compiled, the CoreCLR managed-to-native helpers include shore_calibration, and the link output is dotnet.native.wasm. Retain actual resolved SDK/runtime/native artifacts and logs; an unevaluated NativeFileReference item is not evidence.
5. Run the produced browser bundle. Require the native call's exact 42 result, plus invalid managed input rejection, and verify the runtime is CoreCLR. A managed fallback, desktop DllImport or successful Mono build cannot satisfy this check.
6. Requalify against final GA artifacts before claiming GA support. Until the above can run, keep this exact sample as not-tested rather than a pending obligation to obtain another native environment.

## Constraints and alternatives

The route is experimental even though it targets the .NET 11 release family. Explicit choices are UseMonoRuntime=false and WasmBuildNative=true; no blanket C# preview opt-in is needed. The reviewed CoreCLR targets require SIMD and wasm exception handling; disabling either is rejected. Browser support for those features and the exact generated loader must be established on the real host. Emscripten/runtime ABIs, workload versions and runtime flavor must match. Existing .NET 10 Emscripten 3.1.56/Mono assets do not qualify the intended 6.0.2 CoreCLR route. No custom provider, service, account, server infrastructure or rendering mode is required by the calibration itself. Consumer dependencies are not bundled by Playbook.

## Evidence

The actual .NET 10 managed comparison compiles/runs with SDK 10.0.402/C# 14/runtime10.0.13. `eng/Record-NativeBoundaries.ps1 -VerifySources` verifies immutable runtime/SDK/relink/version sources and exact source/input hashes. The current project is evaluated only: target net11.0, browser-wasm, UseMonoRuntime=false and WasmBuildNative=true. This installation's _WebAssemblyTargetsFile is empty, so it has no qualified CoreCLR app-bundle integration. No native build is attempted or claimed.

**Not tested:** samples/net11.0/ShoreCalibration/ShoreCalibration.csproj native compilation, Emscripten invocation, CoreCLR relinking, complete browser bundling and native-function execution. The matching native/browser packs are absent locally and from the existing Docker route. Intended versions are not measured toolchain versions. This sample-specific PIN-699 exclusion preserves source-reviewed guidance without requiring a new environment or claiming the expected 42 was observed. The actual older managed result does not change that classification. Pingu owns feasible approvals and refresh coordination; comprehensive and GA gates remain open.
