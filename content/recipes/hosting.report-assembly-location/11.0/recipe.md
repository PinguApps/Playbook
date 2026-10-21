# Report logical assembly locations and handle missing physical files

## Goal

Give diagnostics for a fictional harbour module a stable logical label, while preserving a clear distinction between a label and an available assembly file.

Use file-based, untrimmed CoreCLR console samples targeting net10.0 / C# 14 and net11.0 / C# 15, with implicit usings and nullable enabled. Put declarations in namespace Playbook.Samples and import System.Reflection and System.Runtime.Loader. The location snippet itself needs no external package.

## Older approach

A .NET 10 host can own its reporting function and return a logical URI for its module. Callers must use that function; it does not replace Assembly.Location for other libraries. This remains a reasonable choice when you do not control process-wide startup.

{{snippet:older}}

The type's assembly-name comparison identifies this small demonstration module. A real host with multiple versions or load contexts should use its own explicit module identity/mapping rather than treating a simple name as globally unique.

## Newer approach

On .NET 11, configure the framework callback once during host startup, before dependent diagnostic/resource consumers observe locations:

{{snippet:newer}}

SetAssemblyLocationOverride is static and process-wide, not a per-AssemblyLoadContext setting. A second registration throws InvalidOperationException; there is no reset operation. The callback receives the runtime's computed location. Return that original value for assemblies outside the host's mapping. Never call assembly.Location within the callback: it would recurse.

The sample label harbour://modules/parcel is a logical identifier, not a physical path. Returning it does not move the assembly, create a file, change load resolution or prove a resource exists. Existing callers that pass Assembly.Location directly to File APIs must be reviewed before enabling a process-wide override. Keep the callback deterministic, inexpensive and free of failures; location readers now depend on it.

## Unavailable locations

Use the independent deployment diagnostic on both targets:

{{snippet:caller-older}}

{{snippet:caller-newer}}

Import System.Reflection, System.Runtime.CompilerServices and System.Diagnostics.CodeAnalysis for this snippet. Location maps an empty location to null, so the caller can report "(unavailable)" rather than opening an empty path. The method-scoped IL3000 suppressions cover both compilation and native publication: this method handles that exact single-file limitation. They do not suppress warnings in other file-dependent code.

A stream-loaded assembly normally has no location. Single-file bundled assemblies and Native AOT assemblies can also lack one. Use AppContext.BaseDirectory for explicitly deployed files next to the application, or embedded resources for bundled content, with appropriate existence checks. It is the application directory, not a replacement physical assembly path. Assembly.GetName can still provide identity without a file.

The Caller method is diagnostic support, not needed to obtain a location. Its unavailable reason is independent: .NET 10 Native AOT has no caller-inspection equivalent; .NET 11 needs stack-trace support. See the metadata recipe for the full caller migration. Do not infer one availability result from the other.

## Migration

1. Identify whether each consumer needs assembly identity, a logical display label, an actual assembly file or an application content directory.
2. Keep a host-owned reporting function when a process-wide override is unnecessary or startup ownership is shared.
3. If using the .NET 11 hook, register once at a controlled startup point. Preserve the original location for unrelated assemblies and coordinate with all libraries that read it.
4. Handle empty paths independently of labels. Do not use a logical URI with File.Open.
5. Exercise the exact deployment. The custom-loader override here is executed on CoreCLR; Native AOT does not support dynamic plug-in loading and is not claimed as a route for this callback example.

## Constraints

The hook is process-wide and irreversible for the process lifetime. A logical label is not a physical file, and missing locations need a separate fallback. Custom loading is a CoreCLR example; Native AOT deployment checks below cover caller/location availability only.

## Evidence

Actual net10.0 and net11.0 MetadataInspection projects load the built module from a stream. The old runtime location is empty while the host reporting function returns the logical label. The newer Assembly.Location returns that label, unrelated core-library locations are preserved, and a second process-wide registration is rejected.

Separately labelled DeploymentInspection records publish/run Windows x64 single-file and Native AOT apps, both with caller fallbacks. Native runs cover stack traces enabled and disabled; a separate ordinary CoreCLR publication disables stack traces while retaining a physical assembly file. Each unavailable-location assertion is independent of the caller result.

For a standalone deployment probe use OutputType=Exe and the appropriate target/language. Set RuntimeIdentifier=win-x64, SelfContained=true, PublishSingleFile=true for the bundled case, or PublishAot=true for Native AOT. Set StackTraceSupport=true/false for the desired caller case. Native publishing requires the supported native compiler/linker and OS SDK; the actual MSVC/Windows SDK versions and per-mode restored packages are recorded in output. The caller/location code has no package, workload, provider or rendering dependency. MetadataLoadContext is used only by the companion project's independent metadata checks, not by this location hook.

Evidence is actual Windows x64 execution on the declared .NET 10 and .NET 11 RC2 artifacts, not mocks, cross-compilation or GA proof. No process-wide callback is installed in Playbook itself; this is guidance for a consumer host you control.
