# Await asynchronous resource cleanup in F# Interactive

## Goal

Wait for a script's asynchronous resource cleanup before reporting completion, including when its work fails. Select an FSI toolset with the required FSharp.Core asset.

## Shared original resource and checks

Save this complete support as `FsiCleanupSupport.fsx`. `DisposeAsync` waits on a gate before recording completion; the checks prove suspension, exactly one cleanup, body-error identity and the measured cleanup-error behaviour.

{{snippet:support}}

## Older approach

The actual SDK 10.0.402 FSI loads **netstandard2.0** FSharp.Core. A `task { use owned = resource }` binding for IAsyncDisposable fails with **FS0001**, because that asset exposes the IDisposable binding. The asynchronous Using overload already exists in the older NuGet **netstandard2.1** asset: the overload itself is not new. A compiled .NET 10 application and this FSI host select different assets.

Save this definition in `Cleanup.fsx`, preceded by `#load "FsiCleanupSupport.fsx"`, `open System`, `open System.Runtime.ExceptionServices`, `open System.Threading.Tasks` and `open FsiCleanupSupport`:

{{snippet:older}}

Append `showCore ()` and `Task.Run(fun () -> checkCases true inspectBeacon).GetAwaiter().GetResult()`. This explicit cleanup path awaits disposal outside the protected body and preserves its exception dispatch. A cleanup failure takes precedence over the body error. It remains a valid alternative on the newer toolset too.

## Newer approach

The .NET 11 SDK ships a **net10.0** FSharp.Core in its FSI tools. It supports the ordinary task binding below without `LangVersion=preview`. Replace the older definition; retain the same support/imports.

{{snippet:newer}}

Append `showCore ()` and `Task.Run(fun () -> checkCases false inspectBeacon).GetAwaiter().GetResult()`. The final GetResult waits for the complete work; starting a task and exiting does not establish cleanup. The harness runs the checks on the thread pool and awaits them before script completion.

## Migration

1. Select the intended SDK using `global.json` in the script directory. Exact tested pins are `10.0.402` and `11.0.100-rc.2.26504.105`, with `rollForward: "disable"`. Run `dotnet --version` from that directory to confirm selection. A separately installed SDK may require its absolute dotnet host path.
2. Run `dotnet fsi --exec Cleanup.fsx` from that directory. The .NET 11 FSI host requires its corresponding .NET 11 runtime; a NuGet package's target label does not retarget FSI. No workload or external package restore is needed for these scripts. Do not replace files in the SDK, add a conflicting Core reference to an already initialized FSI session, or infer selection from `fsi.deps.json` alone.
3. Inspect `showCore` output: location, TargetFrameworkAttribute and SHA-256 identify the actually loaded DLL. The previous SDK copy reports `.NETStandard,Version=v2.0`, hash `7c1574193332ea24678d32103df140c094c85cf1b08708ffda88ef44540b03a6`. The RC2 tools copy reports `.NETCoreApp,Version=v10.0`, hash `cc9140abff3283005b2418dd270c6a097e70abcb90c22af3473fafac0d2687ca`. The newer SDK DLL is not byte-identical to the NuGet net10.0 asset.
4. Require each case to remain incomplete while disposal is gated, then observe `closing,closed` exactly once and script completion afterward. Both normal and body-failure cases await cleanup.
5. Preserve the **observed RC2 limitation**: with this genuinely pending DisposeAsync, the newer ordinary task binding loses a cleanup exception. A successful body returns 23; a failed body retains its body exception, even when cleanup subsequently fails. This is measured behaviour, not a recommended exception policy or a claim about every F# release. If cleanup failures must propagate, use the explicit older path and run it with `checkCases true`; do not silently depend on the lost exception.

## Constraints and alternatives

- This is `task { use ... }` followed by awaiting the task. It is not top-level asynchronous `use`, a new `async {}` disposal rule, interactive Native AOT, or the experimental runtime-async builder feature.
- The SDK/runtime release family and selected SDK toolset asset are consumer requirements. Exact hashes and patch/RC versions reproduce this evidence; they do not establish GA behaviour or a universal minimum patch.
- Keep compiled tasks as an alternative when interactive asset selection is unsuitable. Preserve the valid .NET 10 explicit cleanup path instead of calling all older approaches broken.
- Cancellation and simultaneous use/disposal need their own application policy. These tests assert four bounded lifetime/error cases, not resource-specific correctness or debugger integration.

## Evidence

Both scripts ran through the actual declared `dotnet fsi --exec` processes, with loaded asset identity and all four cases recorded in the embedded compilation/execution evidence. The older binding rejection is also recorded. See `docs/verification/fsharp-lifetimes.md` for the tested commands. Official task signatures, FSI toolset source and release notes are listed in the structured sources.
