# Remove ineffective EF write-content configuration

## Goal

Remove obsolete EF Cosmos write-content configuration while preserving saved values and concurrency protection.

## When to use

Remove the ContentResponseOnWriteEnabled call from EF configuration. The comparison deliberately retains enabled, disabled and omitted controls; null here means omit the call, not pass null to the bool parameter. The current option is obsolete; its nullable internal setting is not a new public nullable parameter. Saved values, refreshed ETags and stale-write rejection must still work. The direct Cosmos SDK has its own response options: this recipe makes no SDK-wide network, CPU, RU or performance claim.

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

1. Locate each ContentResponseOnWriteEnabled call and record its configured value, saved document values and ETags.
2. Remove that option call while preserving the remaining provider configuration, document identity and ETag concurrency mapping.
3. Run both pinned consumers on isolated data. Compare fresh reads, updates and the negative controls before accepting the change.
4. Remove obsolete comparison controls from application code. Verify your actual provider and authentication route; reconcile GA packages separately.

## Constraints

SDK10.0.403 / C#14 / EF10.0.13 versus SDK11.0.100 / C#15 / EF11.0.0. Both EF consumers select net10.0 versus net11.0 assets. No EF11 on an older runtime or extra .NET10 foundation is offered. GA verification is limited to the recorded checks proof. No workload, rendering mode or experimental opt-in applies. Cosmos SDK3.63.1 (older) and3.63.2 (current), with Newtonsoft.Json13.0.4, are explicitly pinned. Legacy Linux emulator2.14.28.0 uses pinned HTTPS gateway transport on Windows x64. The public local emulator key is not production authentication. Data lasts only for the running emulator session; no restart/cloud durability claim.

Remove the ContentResponseOnWriteEnabled call from EF configuration. The comparison deliberately retains enabled, disabled and omitted controls; null here means omit the call, not pass null to the bool parameter. The current option is obsolete; its nullable internal setting is not a new public nullable parameter. Saved values, refreshed ETags and stale-write rejection must still work. The direct Cosmos SDK has its own response options: this recipe makes no SDK-wide network, CPU, RU or performance claim.

## Evidence

The .NET 11 SDK baseline is now 11.0.100 GA with preview selection disabled. The .NET 10 comparisons retain their declared SDKs. Use the current sample pins, dependency locks and GA verification records for current coverage. Existing native exclusions and pending Human gates still apply.

The sample-evidence workflow captures each exact locked graph, SDK/runtime, inputs and fresh compilation separately from real provider execution. Run `eng/Test-EstuaryProviders.ps1 -Record`. SQL checks insert and query through fresh contexts and inspect retry defaults. EF11 reopens and updates EF10-seeded documents in the same isolated database. Cosmos checks enabled/disabled/omitted options, partition names, persisted values, ETag refresh, stale-write rejection and wrong-partition exclusion. The installed local package is inspected with real stdio list/get from an unrelated directory. PIN-959 comprehensive readiness and PIN-967 final readiness checks remain open.

## References

- [Official EF provider contract](https://learn.microsoft.com/en-us/ef/core/providers/cosmos/modeling)
- [Official EF provider contract](https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-11.0/breaking-changes)
