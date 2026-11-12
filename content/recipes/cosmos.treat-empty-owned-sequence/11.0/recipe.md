# Treat empty Cosmos owned lists as collections

## Goal

A reef survey can contain no stops while its optional inspection note is absent. Keep these two meanings distinct after an EF upgrade.

## Older approach

EF10 can leave an uninitialized owned collection null after reading an empty stored array. The older consumer uses a null-safe count. This is a valid compatibility workaround; a collection initializer can also mask the provider difference.

{{snippet:n10-program-cs}}

## Newer approach

EF11 initializes the empty owned list. Count or enumerate it directly and remove the affected null-as-empty workaround. An absent optional owned reference still represents absence; do not replace that null with a fabricated note. The sample deliberately leaves the list uninitialized before materialization, round-trips empty and populated lists, and checks both optional-reference states.

{{snippet:n11-program-cs}}

## Migration

1. Keep the previous and current projects separate. Copy all files below, restore their exact dependencies with `dotnet restore --use-lock-file`, and build with `dotnet build -c Release -p:RestoreLockedMode=true`. The .NET10 comparison is historical, not a new foundation or target10 edition. Standalone copies generate their own lock files during restore; the repository retains the tested locks.

2. In this repository run `eng/Test-ReefMaterialization.ps1 -Record`. The approved local route supplies PLAYBOOK_COSMOS_ENDPOINT, PLAYBOOK_COSMOS_KEY, PLAYBOOK_COSMOS_DATABASE and PLAYBOOK_COSMOS_CERT_SHA256 only to each child. Gateway transport pins the owned emulator certificate; do not reuse that emulator transport for production. Each run creates and removes its own marked database. No Azure provisioning or production migration is needed.

## Constraints

EF Cosmos11 requires net11.0 and SDK11; these samples declare C#15 mode without requiring new language syntax. The older consumer uses net10.0/C#14/SDK10.0.403 and EF10.0.13. Current verification pins SDK11.0.100 and EF11.0.0. The older/current consumers use Cosmos SDK3.63.1/3.63.2 respectively and Newtonsoft.Json13.0.4. EF package assets are net10.0/net11.0 respectively; SDK assets are netstandard2.0 and Newtonsoft.Json selects net6.0. Lock files capture transitives.

Windows x64 with the approved legacy Linux emulator2.14.28.0 is the execution route. No workload, browser or rendering mode applies. Stable APIs are prepared using Recorded evidence; GA reconciliation, cloud parity and other platforms are unverified.

## Complete projects

### .NET 10 project

{{snippet:n10-directory-build-props}}

{{snippet:n10-directory-build-targets}}

{{snippet:n10-directory-packages-props}}

{{snippet:n10-global-json}}

{{snippet:n10-emulatortransport-cs}}

{{snippet:n10-reefmaterialization-csproj}}


### .NET 11 project

{{snippet:n11-directory-build-props}}

{{snippet:n11-directory-build-targets}}

{{snippet:n11-directory-packages-props}}

{{snippet:n11-global-json}}

{{snippet:n11-emulatortransport-cs}}

{{snippet:n11-reefmaterialization-csproj}}

## Evidence

The .NET 11 SDK baseline is now 11.0.100 GA with preview selection disabled. The .NET 10 comparisons retain their declared SDKs. Use the current sample pins, dependency locks and GA verification records for current coverage. Existing native exclusions and pending Human gates still apply.

Canonical samples execute real queries and persistence against the disposable emulator. Their verification records identify exact inputs, dependencies, SDKs and execution output. Installed stdio list/get checks content delivery separately from provider behaviour. Recorded evidence is limited to the recorded checks; proxy blocking alone is not network isolation.

## References

[Official EF11 Cosmos breaking changes](https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-11.0/breaking-changes).



[Pinned official source](https://raw.githubusercontent.com/dotnet/EntityFramework.Docs/aa051c4c1cad21b2dc03fa401ac0e925c3644606/entity-framework/core/what-is-new/ef-core-11.0/breaking-changes.md).

