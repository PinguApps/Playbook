# Select the intended SDK when executing a tool

## Goal

Use explicit `dotnet dnx` when a repository's `global.json` must choose the SDK
that executes a tool. Use the shipped `dnx`/`dnx.cmd` wrapper deliberately when
you want its newest-installed-SDK behavior, including in a repository pinned to
an SDK too old to implement `dnx`.

The wrapper behavior was serviced into **SDK 10.0.302 and 10.0.400**, and entered
.NET 11 in Preview 6. It is not a .NET 11-only feature. Older muxer-based wrappers
used `dotnet dnx` and followed ordinary SDK resolution. Check the actual wrapper
you invoke: a tool manager or stale copied script can retain that older behavior.

## Older approach

Gullhaven museum checks that accession-card identifiers are unique. It builds a
local-only package; no public or third-party package is executed. The SDK 10
comparison pins SDK 10.0.402, C# 14 and runtime 10.0.13:

{{snippet:older-project}}

## Newer approach

The .NET 11 comparison pins SDK 11.0.100-rc.2.26504.105, C# 15 and matching RC2
runtime:

{{snippet:newer-project}}

Both contain this complete checker. Argument and CoreLib output make forwarding
and the tool's own runtime observable. Duplicate cards exit with code 7.

{{snippet:checker}}

Build each version into a **different** local feed and use a different home/cache
for each. The packages intentionally share ID/version while comparing distinct
TFM assets; never put both into one feed/cache. No package dependencies are added.
Save the matching project as `MuseumCatalogue.csproj` and the checker as
`Program.cs`, alongside a `global.json` pin for the stated SDK with
`rollForward: "disable"` (and `allowPrerelease: true` for RC2). Run that
installation's absolute `dotnet` executable with
`pack ./MuseumCatalogue.csproj -c Release -o 'C:\museum proof\feed-10'`;
use a separate `feed-11` path for the newer package. Keep environment overrides
process-local and disable `DOTNET_ADD_GLOBAL_TOOLS_TO_PATH` before using a fresh
CLI home. The check manages those child settings without changing its parent.

## Pin the SDK independently from the package

In the museum working directory, use this repository pin:

{{snippet:instructions-1}}

Save a two-line text file `east wing cards.txt` containing `SHELL-004` and
`COMPASS-019`. Create `NuGet.Config` in that directory with just your absolute
local feed path:

{{snippet:instructions-2}}

Use absolute installation paths that exist on your machine. For example, with
the package already packed into that feed and the SDK pin in the current
directory:

{{snippet:instructions-3}}

`--version` reports the SDK selected by the muxer in that directory. The explicit
command uses that SDK's `dnx`. The package `@1.0.0` pins a separate decision:
which tool package to execute. `--configfile` with a cleared, local-only source
list avoids external package acquisition. `--` separates SDK options from the
tool's arguments; the path and `east wing` must arrive as single arguments.

For the independent wrapper route, invoke the shipped absolute `dnx.cmd` next
to the intended `dotnet.exe`, passing the same package/options/arguments:

The final command in the canonical block above invokes that shipped wrapper.

The serviced wrapper enumerates SDKs available to its adjacent muxer and invokes
the selected SDK's `dotnet.dll dnx` directly. `dotnet --version` under a pin does
**not** prove which SDK that wrapper selected. Use a process-local host trace to
identify the actual SDK assembly path; inspect the checker's runtime/CoreLib
separately. Do not equate SDK selection with the framework required by a tool.

## Verified selection cases

The Windows check builds an isolated layout with actual SDKs 9.0.319, 10.0.402
and 11 RC2. It copies each shipped 10.0.402 and RC2 wrapper unchanged, with its
hash recorded; the SDK/framework directories refer to the installed artifacts.
It executes both original net10 and net11 tool packages in separate caches.

| Working-directory pin | Explicit `dotnet dnx` | Either serviced wrapper in that layout |
| --- | --- | --- |
| SDK 9.0.319 | Fails: SDK has no dnx command; no tool execution | Runs with SDK 11 RC2 |
| SDK 10.0.402 | Runs the net10 package with SDK 10.0.402 | Runs with SDK 11 RC2 |
| SDK 11 RC2 | Runs with SDK 11 RC2 | Runs with SDK 11 RC2 |

SDK 11 runs the net10 package on runtime 10.0.13. SDK 10.0.402 **rejects this
net11-only package**, even with runtime 11 installed: its actual diagnostic is
`Settings file 'DotnetToolSettings.xml' was not found in the package.` Select a
compatible SDK/package asset pair as well as providing the tool runtime; an
installed runtime alone does not make every tool package consumable. The table's
successful SDK 10 route is the net10 package. The check records the SDK
assembly from host tracing and the actual tool runtime independently, rather
than inferring them from target names. It verifies paths containing spaces,
exact forwarded arguments, successful unique-card checks and duplicate exit 7.

## Migration

1. Inspect the actual absolute wrapper/muxer, installed SDK list and nearest
   `global.json` from the intended working directory. A repository pin controls
   ordinary SDK resolution, not every script named `dnx`.
2. Choose explicit `dotnet dnx` for reproducible SDK execution. Install/provide a
   compatible SDK separately when the pin predates dnx; there is no dnx command
   to enable inside SDK 9. A previously installed/local-manifest tool remains a
   valid alternative workflow, with its own runtime requirements.
3. Pin the package version and permitted feeds independently. Run only a trusted
   package. Quote the path and forwarded arguments; verify actual tool behavior
   and its nonzero failure exit.
4. If choosing the wrapper, document its bypass behavior and identify the
   actual selected SDK. Recheck after servicing or changing the installation;
   the publisher's introduction boundary is 10.0.302 / 10.0.400, while the actual
   tested serviced artifact here is 10.0.402, compared with RC2.

## Constraints

This target-11 edition describes an SDK selection capability also present in
serviced .NET 10. It preserves .NET 10 as a measured comparison, not a ninth
historical foundation or separate target-10 catalogue edition. The SDK 9 pin is
only a real negative command-availability case, not .NET 9 recipe coverage.

The wrapper algorithm is language independent. Original checker compilation
uses the C# 14/15 modes above, no workloads, native compiler, provider, browser
or rendering mode. The tool needs its own compatible CoreCLR runtime; neither
package version nor SDK number establishes that requirement. Current proof
executes Windows x64 `dnx.cmd`; the official Unix script supplies source context,
not Unix execution evidence. Nothing here installs an SDK, changes PATH or
persistent environment settings, publishes a package, or runs external tools.
Child homes disable automatic global-tools PATH integration.

## Evidence

`eng/Test-ToolSdkSelection.ps1` reproduces the actual local package/SDK cases and
its `-Record` mode saves fresh execution records after compilation capture.
Exact SDK/framework/package hashes and commands are verification baselines,
not a claim that the servicing boundary was independently binary-searched.
No API is experimental; .NET 11 RC2 proof still requires actual GA reconciliation.

## Official references

- [dnx SDK selection and servicing boundary](https://learn.microsoft.com/en-us/dotnet/core/compatibility/sdk/11/dnx-scripts-bypass-global-json)
- [dotnet dnx package execution](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-dnx)
- [global.json SDK resolution](https://learn.microsoft.com/en-us/dotnet/core/tools/global-json)
- [Shipped wrapper source](https://github.com/dotnet/sdk/blob/b370d9f8508baee1ab99f3f8aba8bf5e1dfacf41/src/Layout/redist/dnx.cmd)
- [Serviced .NET 10 wrapper source](https://github.com/dotnet/dotnet/blob/a8dd0577c5c13f43a2f57fe33b4ff11b90f6371e/src/sdk/src/Layout/redist/dnx.cmd)
