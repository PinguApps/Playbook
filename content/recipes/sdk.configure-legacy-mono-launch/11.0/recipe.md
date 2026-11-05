# Configure an explicit legacy Mono launch

## Goal

Deliberately launch a retained Framework command-line importer on Linux using
Mono. **Microsoft no longer officially supports this scenario.** Explicit launch
properties restore a launch choice, not support or universal Framework compatibility.
Prefer migrating the importer to supported .NET when feasible.

The fictional observatory importer accepts `night survey` and an exit code. It
requires `observatory.input` in its working directory. Both comparison samples
target **net472**, using C# 7.3 and Mono's 4.7.2-api reference assemblies. This is
an SDK 10/11 comparison, not a net10/net11 runtime comparison.

## Older approach

SDK 10.0.403 on Linux evaluates `RunCommand` to `mono` for this Framework
executable and quotes the target assembly in `RunArguments` before appending
`StartArguments`. With Mono installed, the retained default works. Keep it when
using the older SDK; it does not imply Microsoft support for the runtime route.

## Newer approach

SDK 11 removes those Linux defaults. The unchanged default launch fails with
NETSDK1243. Choose the executable explicitly and construct arguments after
`TargetPath` is available. The sample sets the command early for SDK validation,
then supplies arguments and working directory before run-command computation.

{{snippet:launch}}

Enable this sample's deliberate choice with `-p:ExplicitMono=true`. The assembly
path includes `importer output`, the argument `night survey` includes a space,
and the chosen directory is `working directory`. All three need correct handling.

## Migration

1. Decide whether to retain the unsupported Mono route or migrate the application
   to supported .NET. Audit its dependencies and OS assumptions separately.
2. In an isolated Linux environment, install Mono and exact Framework reference
   assemblies. These samples use Ubuntu 24.04's `mono-devel`
   `6.8.0.105+dfsg-3.6ubuntu2`, including `/usr/lib/mono/4.7.2-api`.
   `AutomaticallyUseReferenceAssemblyPackages=false` prevents an implicit reference
   package route. The project selects those reference files explicitly.
3. Build each pinned SDK comparison with `dotnet build -c Release`. Inspect
   `RunCommand`, `RunArguments` and `RunWorkingDirectory` using `dotnet msbuild`
   with `-p:Configuration=Release -getProperty:RunCommand,RunArguments,RunWorkingDirectory`.
   For the explicit target hook, also execute `-target:ComputeRunArguments`.
4. Create `working directory/observatory.input`. Run
   `dotnet run -c Release -p:ExplicitMono=true`. Expect the importer to print
   `Observatory import: night survey`, the chosen directory, and exit **7**.
   The intentional nonzero process status must reach the caller.
5. Compare the default: with `observatory.input` in the project directory, SDK 10
   runs and exits 7; SDK 11 fails before importer execution with NETSDK1243.
6. Delete both test input files and remove the disposable container. The runner
   checks file removal and removes the container in its cleanup path.

## Constraints

This does not emulate Windows on Linux. Framework APIs, native dependencies,
desktop components and other applications need their own compatibility checks.
The host needs Mono, not a .NET 11 runtime to execute this net472 importer. SDK 11
is used to build and evaluate the new launch configuration. No package, provider,
rendering mode, workload, experimental opt-in or public deployment is involved.
No machine/user environment or PATH changes are required. SDK 11 RC3 establishes
the measured behavior only; final GA reconciliation remains separate.

## Evidence

`eng/Test-TideAndMono.ps1 -Record` compiles both SDK variants against the actual
Mono references and executes both defaults and explicit launches on Linux x64.
It checks arguments, spaces in assembly paths, working directories, exit 7 and
input cleanup. Sample evidence records contain exact commands, source hashes,
Mono reference hash, SDK, OS and installed toolchain versions.

The [official removal note](https://learn.microsoft.com/en-us/dotnet/core/compatibility/sdk/11/mono-launch-target-removed)
states the unsupported boundary and explicit property alternative. Source audit:
`docs/planning/audits/sdk-package-hosts.md`, proposal B. No extra historical
foundation is added. PIN-959 and actual-GA reconciliation remain open.
