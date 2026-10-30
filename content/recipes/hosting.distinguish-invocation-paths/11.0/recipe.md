# Distinguish invocation names from application file locations

## Goal

A quarry maintenance CLI has a normal command and a quarry-audit alias that selects different help text. Keep the invocation identity separate from the running executable and managed assembly, including single-file deployments where there is no separate managed file.

## Older approach

On .NET 10, GetCommandLineArgs()[0] under apphost reports a managed assembly path or resolved executable path. It is not a reliable way to recover the caller's alias. A fixed help name or an explicit command-line option remains a valid older choice. ProcessPath and entry-assembly Location already exist; choosing the correct physical-location API does not require .NET 11.

Use a net10.0 / C# 14 console project with implicit usings and System.Reflection. The same method also compiles on net11.0 / C# 15. Place it inside a class:

{{snippet:locations}}

Invocation is retained as supplied by the runtime; Executable can be null. ManagedFile is null when the entry assembly is unavailable or Location is empty. The narrow IL3000 suppression acknowledges the single-file warning exactly where the empty value is handled; it does not make a bundled assembly a physical file.

## Newer approach

Use a net11.0 / C# 15 console project. With .NET 11 apphosts, GetCommandLineArgs()[0] preserves the invocation name, allowing an alias to select help without guessing where application files live:

{{snippet:alias}}

Call Inspect(), then SelectHelpName(report.Invocation). The allow-list returns a fixed known help name; it never starts a process or opens a file using the invocation. Under a real quarry-audit symlink, the measured RC2 apphost selects quarry-audit; the ordinary older apphost selects the normal help name. The older single-file apphost reports its resolved executable name, which can also retain the alias basename on Windows; do not describe all older launch forms as losing every alias. Handle other commands through an explicit argument parser if launch spelling is not a dependable input for your product.

Launching with dotnet application.dll retains its older behavior: argv[0] identifies the managed file, while ProcessPath identifies dotnet. It does not give the app an apphost alias. A framework-dependent single-file apphost has an executable path but its bundled entry assembly's Location is empty; the helper reports that as unavailable.

## Migration

1. Classify every use of argv[0]: invocation/help selection, executable diagnostics or managed-file diagnostics. Keep it for the first; use ProcessPath or entry-assembly Location for the other two.
2. Handle a null executable and a null/empty assembly location explicitly. Display an unavailable value or skip an operation that requires a physical file.
3. Use a bounded alias mapping for help/dispatch. Never execute a reconstructed argv[0] path or treat an invocation as trusted filesystem input.
4. Launch real apphosts by absolute and relative paths and through a real symlink on a filesystem/account supporting it. Capture the exact launcher, spelling, working directory and all three child values; repeat the dotnet DLL launch.
5. Publish and run a single-file apphost. Expect empty bundled Location. Choose storage through an explicit application setting or separate platform storage policy; none of these three values establishes a data directory.

## Constraints and alternatives

The launcher determines what spelling reaches the native process. Shell resolution, quoting, command aliases and launch APIs can normalize relative names differently. The measured Windows route supplies absolute apphosts directly with ProcessStartInfo and relative/symlink names through cmd.exe /d /c in the publish directory. RC2 preserves those exact inputs; this is not a promise that every launcher preserves a user's original spelling. A shell alias is not necessarily a filesystem symlink.

The samples use real NTFS symbolic links and verified Windows symlink capability. Creating a symlink can require Developer Mode or privilege; without it, obtain actual evidence on a suitable Unix host instead of simulating an alias. No Unix execution is claimed here. The recipe APIs themselves require no native package, provider, workload, rendering mode or experimental opt-in. Desktop/server processes are the bounded route; browser/WASI are outside it.

Executable and assembly paths can differ and can be unavailable. Bundled Location is empty, not the apphost path. This helper does not use ProcessPath as a fallback managed-file location. It does not open any reported path, infer trust, choose a writable data directory or promise physical-location availability under every custom host. Native AOT/custom-loader metadata belongs to the existing separate assembly-location recipes.

Exact test artifacts are SDK 10.0.402 / 11.0.100-rc.2.26504.105, C# 14 / 15, framework reference/runtime 10.0.13 / 11.0.0-rc.2.26504.105 and Windows x64 apphosts. Single-file publication is framework-dependent with the exact runtime selected explicitly, not self-contained or Native AOT. Release-family runtime/SDK prerequisites are separate from these reproduction pins. No added package or experimental opt-in is needed; the changed runtime behavior is not a new ProcessPath API. RC2 results remain GA-pending evidence.

## Evidence

QuarryMaintenance compiles/runs on both actual dependency sets. Separate deployment records capture fourteen real child launches: ordinary absolute, relative and symlink apphosts plus dotnet DLL, then absolute, relative and symlink single-file apphosts, for each runtime. Assertions check exact RC2 invocation spelling, older resolved identity, real alias help selection, unchanged dotnet DLL behavior, existing executable/ordinary assembly files and empty bundled Location normalized to unavailable. Commands include working directory, runtime selection, publication assets and apphost SHA256; compiler/ref/runtime hashes accompany ordinary compilation.

These are real apphost/publish/OS executions with a real symlink, not mocks or cross-compilation. Installed ordinary-tool inspection checks complete offline canonical Markdown, explanatory discovery, target11/edition11 and evidence freshness; target10 remains comparison-only. The existing hosting.report-assembly-location ID and the eight .NET 10 foundations are unchanged. PIN-959 comprehensive coverage and PIN-962 actual-GA reconciliation remain open.
