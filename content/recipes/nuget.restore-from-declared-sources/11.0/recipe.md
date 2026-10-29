# Restore dependencies independently of next-best cache entries

## Goal

Make a nonfloating package request resolve from declared sources when its
requested minimum is unavailable. Understand why an intermediate version found
only in a machine's global packages folder no longer repairs that request.
An exact requested cached version still works.

Cwmglas research station shares a small reading-scale library. The controlled
comparison uses original packages at **2.0.0**, **2.3.0**, **2.5.0** and **2.8.0**.
The active source has 2.0.0/2.8.0; 2.3.0 is absent there, while an isolated
cache has only 2.5.0. These intentionally incomplete feeds demonstrate a failure
mode; the final migration uses a version actually present on the source.

## Older approach

Save this project as `StationReadings.csproj` in an empty `library-10` directory:

{{snippet:older-project}}

SDK 10 can select the cache-only intermediate version when the minimum is absent.
That historical behavior is useful to diagnose machine differences, but does
not supply a repeatable dependency declaration for a fresh consumer.

## Newer approach

In a separate `library-11` directory, save this project instead:

{{snippet:newer-project}}

Both libraries contain the same original `ReadingScale.cs`:

{{snippet:library}}

Use exact SDK **10.0.402 / 11.0.100-rc.2.26504.105** pins in `global.json`
beside the matching project: `sdk.version` is the chosen version,
`rollForward` is `disable`, and `allowPrerelease` is true only for RC2.
Language modes are C# **14 / 15**; the restore change itself is language
independent and was introduced in **.NET 11 RC1**. It is not enabled/disabled
by lowering `SdkAnalysisLevel`.

## Construct the controlled sources and caches

Use separate directories for each SDK comparison, with absolute paths such as
`C:\station-proof\route-11\seed-feed` and `C:\station-proof\route-11\feed`.
Do not combine same-ID/version packages containing different framework assets.
With the matching absolute `dotnet` executable in the library directory:

1. Pack four actual packages using
   `pack StationReadings.csproj -c Release -o <seed-feed> -p:Version=<version>`
   for 2.0.0, 2.3.0, 2.5.0 and 2.8.0. This builds each version, including its
   version-bearing assembly. Keep all four in the seed feed.
2. Copy only the **2.0.0 and 2.8.0 nupkg files** into the active feed.
3. Create separate `NuGet.Config` files for the seed and active feeds using this
   template, with the corresponding absolute path:

{{snippet:config}}

Save the following as `Consumer.csproj` in a fresh consumer directory:

{{snippet:newer-consumer-project}}

For the SDK-10 route use this complete project instead:

{{snippet:older-consumer-project}}

Replace `PACKAGE_ID` with `Cwmglas.Station.Readings` and `PACKAGE_VERSION` with
the scenario's request. Save this as `Program.cs` and copy the matching SDK pin:

{{snippet:consumer}}

For each scenario use a new child-process CLI home, a separate absolute
`NUGET_PACKAGES` cache and `DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=0` before first use.
Keep these overrides child-local; leave shared caches, PATH and persistent
environment settings untouched. The seed consumer and tested consumer share
**only that scenario's cache**. They have different project/obj directories.

Run the matching absolute SDK executable from each consumer directory:

1. To seed the intermediate package, set the seed consumer's version to
   **`[2.5.0]`** and run
   `restore Consumer.csproj --configfile <seed-config> --force --no-http-cache -p:NuGetAudit=false`.
   `--no-http-cache` does not disable the global packages folder.
2. In a different tested consumer, set the version to **`2.3.0`** (an inclusive
   nonfloating minimum, not `[2.3.0]`). Restore with the same command and cache,
   replacing the config with the **active** source configuration.
3. Inspect `obj/project.assets.json` library/target keys, compile/runtime paths
   and `packages.lock.json`. Build with
   `build Consumer.csproj -c Release --no-restore`, then execute
   `bin/Release/net11.0/Consumer.dll` (or `net10.0`). It prints `reading=12.5`
   and the actually loaded library package version.

SDK 10 selects **2.5.0**; SDK 11 selects **2.8.0**. On the actual tested
artifacts, these default **direct** requests emit **NU1601**, rather than the
compatibility note's NU1603. Check selected assets as well as diagnostic text.
The newer algorithm searches configured sources for the next-best version,
while retaining ordinary exact cache hits. This does not disable all caching.

## Verify failure, exact-hit and fresh-consumer cases

Create independent copies of the active feed and new consumers/caches for each
case, avoiding incremental assets, lock reuse and contamination by earlier runs:

- **No higher source version:** copy only 2.0.0 into this case's feed. Seed only
  2.5.0 as above, then request 2.3.0 from the active config. SDK 10 still chooses
  2.5.0/NU1601; SDK 11 fails **NU1102**, even though that intermediate package
  is cached. Do not compile using the failed restore's stale assets.
- **Exact cached minimum:** seed **`[2.3.0]`** from the seed feed, then request
  **`2.3.0`** in another consumer using an active feed containing only 2.0.0.
  Both SDKs succeed at **2.3.0**, without NU1603, and execute that library.
- **Uncached consumer:** start with an empty cache and a separate consumer
  requesting 2.3.0 against the active 2.0.0/2.8.0 feed. Both SDKs select
  **2.8.0/NU1601** and execute it. A seeded-machine success was never evidence
  that the missing minimum existed on the source.

Repeat these cases for a **transitive** request. Create a copy of the consumer
project referencing Readings/2.3.0, retaining the program. Pack it using the
seed config as `Cwmglas.Station.Survey/1.0.0`, passing
`-p:AssemblyName=StationSurvey` to keep its assembly identity distinct from the
consumer. The seed feed contains the actual
2.3.0 library, so packing the parent does not require an approximate restore.
The parent nuspec declares Readings minimum **2.3.0**; inspect it before use.
Copy that Survey package into every case's active feed. Fresh tested consumers
now reference **Survey/1.0.0**, without a direct Readings override. Seed only
Readings/2.5.0 or Readings/2.3.0 as specified above. The selected Readings assets
and loaded versions match the direct cases. The default resolver emits **no
NU1601/NU1603** for these transitive approximations on either tested SDK; the
new missing-source failure still emits NU1102. This proves
package dependencies are affected too, rather than merely direct references.

For a separate **NU1603 diagnostic control**, create a fresh transitive consumer
and empty cache, keep the active 2.0.0/2.8.0 feed and Survey/1.0.0, and add
`-p:RestoreUseLegacyDependencyResolver=true -p:RestorePackagesWithLockFile=false`
to the restore command. This fresh diagnostic case deliberately has no lock
file; the default cases above generate and inspect lock files. Both tested
SDKs select 2.8.0, emit NU1603 and execute that library. This documented resolver
switch diagnoses the source note's warning claim; it is **not** the migration
remedy, and the default cache-selection checks above do not use it. The source
note's version-selection rule is verified; its universal warning wording is
qualified by actual resolver/direct-versus-transitive behavior.

## Migration

1. Identify missing nonfloating minimum versions in direct references and
   dependency nuspecs. Record the actual SDK, sources and selected assets;
   compare against a genuinely empty cache in a separate consumer.
2. Publish/retain the intended version on an approved source, or change the
   declaration to a reviewed version already present there. For this local
   exercise, request **`[2.8.0]`** against the active 2.0.0/2.8.0 feed and verify
   a clean restore. No public publication is performed.
3. Repack dependencies that declare unavailable minima, rather than relying on
   a developer's cached intermediate version. Verify direct and transitive graphs.
4. Rebuild/run the actual selected assets. Review and commit the real application's
   lock file under its existing policy, using locked restore for reproducibility.
   Do not call a diagnostic-suppressed approximate restore a dependency repair.

## Constraints

This comparison concerns **inclusive, nonfloating minimum requests**. It does
not establish floating-version/range behavior, source authentication/mapping,
fallback-folder execution, package vulnerability cooldown or all cache policies.
The exact-hit case also does not prove the cached bytes match a source's bytes.
The separate cooldown/vulnerability recipes retain their own goals.

Original net10/net11 libraries and consumers execute on runtimes
10.0.13 / 11.0.0-rc.2.26504.105. No external package dependency, database/provider,
workload, native compiler, browser or rendering mode is required. PowerShell 7
and both SDK/runtime installations reproduce the local checks.

## Evidence

`eng/Test-NuGetMigration.ps1` captures fresh compilation/execution records with actual SDK NuGet assembly
versions/hashes, exact commands, local package hashes, selected compile/runtime
assets and all direct/transitive failure cases. Only disposable scenario feeds
are changed; no shared cache is cleared. Windows x64 execution establishes this
bounded proof, not other OS execution. The behavior is not experimental, while
actual GA reconciliation and PIN-959/PIN-963 publication gates remain open.
These target-11 editions add no ninth .NET 10 foundation.

## Official references

- [Next-best search and unchanged exact cache hits](https://learn.microsoft.com/en-us/dotnet/core/compatibility/sdk/11/restore-global-packages-folder-version-search)
- [Actual source-provider candidate boundary](https://github.com/dotnet/dotnet/blob/820230c386db5b0106c5430b3e5aee92a7826bf5/src/nuget-client/src/NuGet.Core/NuGet.Commands/RestoreCommand/SourceRepositoryDependencyProvider.cs)
- [NU1601 direct dependency bump](https://learn.microsoft.com/en-us/nuget/reference/errors-and-warnings/nu1601)
- [NU1603 approximate match](https://learn.microsoft.com/en-us/nuget/reference/errors-and-warnings/nu1603)
- [Documented resolver diagnostic switch](https://learn.microsoft.com/en-us/nuget/consume-packages/package-references-in-project-files#nuget-dependency-resolver)
- [NU1102 unavailable version](https://learn.microsoft.com/en-us/nuget/reference/errors-and-warnings/nu1102)
