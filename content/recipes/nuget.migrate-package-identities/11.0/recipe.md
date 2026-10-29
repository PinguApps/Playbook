# Choose a compliant package identity during migration

## Goal

Replace a legacy-valid package ID that now raises NU5052, while deliberately
moving direct and transitive consumers to the replacement. A new ID creates a
different NuGet identity; it does not redirect existing references.

The Berwyn wildlife charity shares a tern intake rule with field teams. Its old
package is `Bérwyn.Wildlife.Tags`; its replacement is `Berwyn.Wildlife.Tags`.
The code contract and assembly name remain the same so that the local checks
can isolate the package-identity change.

## Older approach

Save this as `WildlifeTags.csproj` in an empty `library-10` directory:

{{snippet:older-project}}

SDK 10.0.402 accepts this Unicode ID without NU5052. Existing private/local
consumption remains a valid older workflow; pack acceptance says nothing about
whether a public feed accepts a new upload under that name.

## Newer approach

In a separate `library-11` directory, save this replacement project:

{{snippet:newer-project}}

Save this complete `TagRules.cs` in each library directory:

{{snippet:library}}

Use SDK **10.0.402 / 11.0.100-rc.2.26504.105**, respectively, with C# **14 / 15**.
Save a `global.json` beside each project with `sdk.version` set to that exact
SDK, `rollForward` set to `disable`, and `allowPrerelease` true only for RC2.
Those are the verification pins, separate from the supported SDK-11 feature
requirement. Pack each library with its installation's absolute `dotnet`
executable, arguments `pack WildlifeTags.csproj -c Release -o <absolute-feed>`.
Use a separate feed/cache per framework comparison. Do not mix same-ID/version
packages containing different target assets in one feed or cache.

SDK 11 introduces NU5052 at **SdkAnalysisLevel >= 11.0.100**. To reproduce the
warning, pack the newer project with `-p:PackageId=Bérwyn.Wildlife.Tags` and
`-p:TreatWarningsAsErrors=false`. This still creates the local package. Repeat
into a fresh output directory with `-p:TreatWarningsAsErrors=true`: packing fails
with NU5052 and creates no package. Pack with `-p:SdkAnalysisLevel=10.0.300` to
observe the older analysis policy on SDK 11. It hides this warning; it does not
rename the package or change a feed's rules. Prefer the replacement identity.

## Test the consumers and dependency identities

For each comparison, pack both IDs at version 1.0.0 into that route's local feed.
For SDK 11's legacy fixture only, pass `-p:TreatWarningsAsErrors=false`; retain
strict warnings for the compliant replacement. Inside a new consumer directory,
save this project as `Consumer.csproj`:

{{snippet:newer-consumer-project}}

For the SDK 10 comparison use this complete project instead:

{{snippet:older-consumer-project}}

Replace `PACKAGE_ID` with the old or new ID and `PACKAGE_VERSION` with `1.0.0`.
Save the following as `Program.cs`, and copy the matching `global.json`:

{{snippet:consumer}}

Create `NuGet.Config` in the consumer directory, replacing `C:\wildlife\feed`
with the absolute route-specific feed:

{{snippet:config}}

Before using a new CLI home, set **DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=0** in the
child process. Use fresh child-process `DOTNET_CLI_HOME` and `NUGET_PACKAGES`
directories for each route; no PATH or persistent settings changes are needed.
Run the absolute matching SDK executable from the consumer directory with:

1. `restore Consumer.csproj --configfile NuGet.Config --force --no-http-cache -p:NuGetAudit=false`
2. `build Consumer.csproj -c Release --no-restore`
3. `bin/Release/net11.0/Consumer.dll` (use `net10.0` for the older project).

The program prints `admitted=TERN-042;rejected=OTTER-007`. Inspect
`obj/project.assets.json` and the generated `packages.lock.json`: the library
key must be the requested old/new ID at 1.0.0, with real `lib/net11.0/WildlifeTags.dll`
compile/runtime assets (or `net10.0`). Both package archives contain that library;
their nuspec IDs differ. SDK numbers do not determine package identity.

To verify a transitive migration, create another copy of the consumer project,
initially referencing the old ID at 1.0.0. Replace its `Program.cs` with
`public static class IntakeMarker {}` and change `OutputType` to `Library`.
Pack it as `Berwyn.Wildlife.Intake/1.0.0`, using the same local config and
`-p:AssemblyName=WildlifeIntake` to give it a distinct assembly identity. A fresh
consumer referencing **Intake/1.0.0** must still restore the old Tags ID. Merely
adding the replacement nupkg to the feed cannot change this dependency.

Edit that dependent project's reference to the new Tags ID, then pack it as
**Intake/2.0.0**. A fresh consumer of Intake/2.0.0 restores only the new Tags ID
and executes the same rule. Use the consumer program above to test the
transitively exposed library. Rebuild/repack every dependency that must stop
requesting the old identity; do not rewrite an already published package version.

## Identity boundaries

A restricted ID starts with an ASCII letter, digit or underscore, uses only
ASCII letters/digits, dots, dashes and underscores, and is at most 100 characters.
Adjacent dot/dash pairs, including mixed pairs, are forbidden. Underscores are
allowed: `_Berwyn_Wildlife.Tags` packs successfully under strict SDK-11 analysis.

There are **two validation layers**. `Berwyn..Wildlife`, `Berwyn--Wildlife`,
`Berwyn.-Wildlife`, `Berwyn-.Wildlife` and `Berwyn Wildlife` already fail legacy
ID validation on both tested SDKs with “contains invalid characters”; they are
not examples of NU5052 warning-only packing. The Unicode charity ID passes
legacy validation but raises the new restricted-character warning. Do not infer
pack diagnostics from the restricted validator in isolation or from feed policy.

## Migration

1. Inventory the old ID, package versions, assembly/API contract, direct
   references and dependent-package nuspecs. Check your intended feed's current
   acceptance rules separately from `dotnet pack`.
2. Choose a compliant, available identity. Keep or deliberately change the
   assembly name/API; identity validation does not establish binary compatibility.
3. Build/pack locally under explicit analysis policy. Inspect archive contents,
   restore the replacement into an empty cache, compile and run a real consumer.
4. Update direct references and repack dependent packages under new versions.
   Verify both dependency graphs; never assume ID renaming creates a redirect.
5. Prepare a release/deprecation notice naming the replacement ID, versions and
   migration steps. Coordinate any future approved feed publication first, then
   deprecate the legacy package with replacement metadata where supported.
   This recipe performs **no upload, public deprecation or account write**.

## Constraints

The language-independent pack change was introduced in .NET 11 Preview 6.
The original libraries/consumers use net10/net11 assets and runtime
10.0.13 / 11.0.0-rc.2.26504.105 for execution. No external package dependencies,
provider, workload, native compiler, browser or rendering mode is involved.
PowerShell 7 and the two installed SDK/runtime sets reproduce the local check.
The new warning is not experimental; actual RC2 proof is not GA proof.

## Evidence

`eng/Test-NuGetMigration.ps1` checks both real SDKs, explicit analysis levels,
warnings-as-errors, underscores, repeated/mixed punctuation, package contents,
direct consumers and old/new transitive dependency identities. Compilation and
execution records retain exact SDK NuGet assembly hashes, selected assets,
commands and input hashes. These target-11 additions preserve the exact eight
.NET 10 foundations. Feed publication, other OS execution, actual GA and the
catalogue-wide PIN-959/PIN-963 gates remain separate.

## Official references

- [NU5052 introduction and analysis policy](https://learn.microsoft.com/en-us/dotnet/core/compatibility/sdk/11/nuget-pack-nu5052-packageid)
- [Legacy and restricted ID validation](https://github.com/NuGet/NuGet.Client/blob/623454983addd9cad23a4c16a28c8faf458c7546/src/NuGet.Core/NuGet.Packaging/PackageCreation/Utility/PackageIdValidator.cs)
- [Deprecating packages](https://learn.microsoft.com/en-us/nuget/nuget-org/deprecate-packages)
