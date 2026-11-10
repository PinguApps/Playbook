# Migrate custom scalar mappings and comparers

## Goal

Preserve custom scalar query, persistence and change tracking when upgrading an EF extension.

## When to use

Use the public relational mapping plugin boundary. EF11 adds generic mapping bases whose default comparer creation can use the closed CLR type; the non-generic EF10 base remains the comparison. A mutable Reading still needs an explicit snapshot and value equality comparer: generic defaults cannot infer application semantics. The mapping converts Reading to int and preserves that converter and comparer through Clone. CoreTypeMapping.Clone parameters move from in to by-value in the pinned contracts; the relational Clone override shown takes RelationalTypeMappingParameters by value in both. JSON structural type mapping describes object/collection structure; it is not the scalar mapping used here. No JSON graph or NativeAOT compatibility follows from this sample.

## Migration

1. Keep a before-upgrade fixture with the same values and failure controls.
2. Migrate the extension contracts described above; preserve unrelated provider services.
3. Copy each complete comparison project into its own directory and restore its locked dependencies.
4. Run the real provider checks with eng/Test-TernExtensions.ps1; compare stored values and the explicit controls.
5. Repeat on your actual provider and reconcile GA artifacts before claiming broader support.

## Older approach

The independent .NET10 project preserves the working older route. Keep these complete files together.

### global.json

{{snippet:n10-global-json}}

### Directory.Build.props

{{snippet:n10-directory-build-props}}

### Directory.Build.targets

{{snippet:n10-directory-build-targets}}

### Directory.Packages.props

{{snippet:n10-directory-packages-props}}

### TernGauges.csproj

{{snippet:n10-terngauges-csproj}}

### Program.cs

{{snippet:n10-program-cs}}

## Newer approach

The independent .NET11 project uses the current pinned extension contracts. Run its controls before applying the migration to another provider.

### global.json

{{snippet:n11-global-json}}

### Directory.Build.props

{{snippet:n11-directory-build-props}}

### Directory.Build.targets

{{snippet:n11-directory-build-targets}}

### Directory.Packages.props

{{snippet:n11-directory-packages-props}}

### TernGauges.csproj

{{snippet:n11-terngauges-csproj}}

### Program.cs

{{snippet:n11-program-cs}}

## Constraints

EF10.0.13/net10.0 versus EF11.0.0/net11.0. SDK10.0.403/C#14 versus SDK11.0.100/C#15. SQL graphs pin Pkcs10.0.13 and SqlClient6.1.7/7.1.0. SQLite uses the locked platform bundle; actual native DLL hashes are in execution records. EF11 requires .NET11; no older-runtime route is offered. No workload or rendering mode applies. No experimental opt-in or NativeAOT claim. RC2 proof is not actual GA reconciliation.

SQL Server2025 17.0.5035.5 compatibility170; approved isolated Docker route. Executed on Windows x64; SQL engine runs in the existing Linux container. No other native platform verified.

## Evidence

The .NET 11 SDK baseline is now 11.0.100 GA with preview selection disabled. The .NET 10 comparisons retain their declared SDKs. Earlier RC audit details below are historical; use the current sample pins, dependency locks and GA verification records for current coverage. Existing native exclusions and pending Human gates still apply.

The saved compilation and execution records identify exact commands, inputs and dependency graphs. Run eng/Test-TernExtensions.ps1 -Record to refresh them. PIN-959 comprehensive coverage and PIN-965 GA reconciliation remain open. No new .NET10 foundation is added.

## References

- [Official guidance](https://learn.microsoft.com/en-us/ef/core/modeling/value-comparers)


