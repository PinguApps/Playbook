# Preserve Cosmos partition identity

## Goal

Replace singular partition metadata helpers without changing which stored documents are found and updated.

## When to use

Replace singular metadata getters/setters with plural property-name lists. The replacement APIs already exist in EF10. A one-property partition becomes a one-element list, in the same order. Resolve property metadata from those names when needed. For convention builders use the list overload of HasPartitionKey/CanSetPartitionKey and preserve configuration-source precedence. Do not confuse a CLR property name with its JSON name, or change stored partition paths. Existing lambda HasPartitionKey configuration remains valid. This fixture preserves the single Depot partition; hierarchical multi-component service execution and data repartitioning are not demonstrated. A wrong partition returns no matching document; it does not scan the correct partition.

## Older approach

The EF10 comparison preserves the previous configuration and real persistence controls. The supported replacements can also be adopted before upgrading.

### .NET 10 global.json

{{snippet:n10-global-json}}

### .NET 10 Directory.Build.props

{{snippet:n10-directory-build-props}}

### .NET 10 Directory.Build.targets

{{snippet:n10-directory-build-targets}}

### .NET 10 Directory.Packages.props

{{snippet:n10-directory-packages-props}}

### .NET 10 EstuaryParcels.csproj

{{snippet:n10-estuaryparcels-csproj}}

### .NET 10 Program.cs

{{snippet:n10-program-cs}}

### .NET 10 EmulatorTransport.cs

{{snippet:n10-emulatortransport-cs}}

### .NET 10 DiagnosticProbe.cs

{{snippet:n10-diagnosticprobe-cs}}

## Newer approach

The EF11 comparison uses the supported metadata/configuration route. The obsolete write-content call remains solely as a labelled behaviour control; remove it in application code.

### .NET 11 global.json

{{snippet:n11-global-json}}

### .NET 11 Directory.Build.props

{{snippet:n11-directory-build-props}}

### .NET 11 Directory.Build.targets

{{snippet:n11-directory-build-targets}}

### .NET 11 Directory.Packages.props

{{snippet:n11-directory-packages-props}}

### .NET 11 EstuaryParcels.csproj

{{snippet:n11-estuaryparcels-csproj}}

### .NET 11 Program.cs

{{snippet:n11-program-cs}}

### .NET 11 EmulatorTransport.cs

{{snippet:n11-emulatortransport-cs}}

### .NET 11 DiagnosticProbe.cs

{{snippet:n11-diagnosticprobe-cs}}

## Migration

1. Record the existing provider configuration, partition property names/order and stored document identities.
2. Apply the supported replacement described above; preserve explicit settings and the original stored key.
3. Run both pinned consumers on isolated data. Compare fresh reads, updates and the negative controls before accepting the change.
4. Remove obsolete comparison controls from application code. Verify your actual provider and authentication route; reconcile GA packages separately.

## Constraints

SDK10.0.403 / C#14 / EF10.0.13 versus SDK11.0.100-rc.3.26522.148 / C#15 / EF11.0.0-rc.2.26504.105. Both EF consumers select net10.0 versus net11.0 assets. No EF11 on an older runtime or extra .NET10 foundation is offered. RC3 evidence is not actual GA proof. No workload, rendering mode or experimental opt-in applies. Cosmos SDK3.63.1 and Newtonsoft.Json13.0.4 explicitly pinned in both graphs. Legacy Linux emulator2.14.28.0 uses pinned HTTPS gateway transport on Windows x64. The public local emulator key is not production authentication. Data lasts only for the running emulator session; no restart/cloud durability claim.

Replace singular metadata getters/setters with plural property-name lists. The replacement APIs already exist in EF10. A one-property partition becomes a one-element list, in the same order. Resolve property metadata from those names when needed. For convention builders use the list overload of HasPartitionKey/CanSetPartitionKey and preserve configuration-source precedence. Do not confuse a CLR property name with its JSON name, or change stored partition paths. Existing lambda HasPartitionKey configuration remains valid. This fixture preserves the single Depot partition; hierarchical multi-component service execution and data repartitioning are not demonstrated. A wrong partition returns no matching document; it does not scan the correct partition.

## Evidence

The .NET 11 SDK baseline is now 11.0.100 GA with preview selection disabled. The .NET 10 comparisons retain their declared SDKs. Earlier RC audit details below are historical; use the current sample pins, dependency locks and GA verification records for current coverage. Existing native exclusions and pending Human gates still apply.

The sample-evidence workflow captures each exact locked graph, SDK/runtime, inputs and fresh compilation separately from real provider execution. Run `eng/Test-EstuaryProviders.ps1 -Record`. SQL checks insert and query through fresh contexts and inspect retry defaults. EF11 reopens and updates EF10-seeded documents in the same isolated database. Cosmos checks enabled/disabled/omitted options, partition names, persisted values, ETag refresh, stale-write rejection and wrong-partition exclusion. The installed local package is inspected with real stdio list/get from an unrelated directory. PIN-959 comprehensive readiness and PIN-965 GA reconciliation remain open.

## References

- [Official EF provider contract](https://learn.microsoft.com/en-us/ef/core/providers/cosmos/modeling)
- [Official EF provider contract](https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-11.0/breaking-changes)
