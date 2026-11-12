# Generate partial JSON updates with structured paths

## Goal

Update a JSON root, nested property or array element while preserving siblings when migrating an update-generator extension.

## When to use

The pinned assembly calls the type StructuredJsonPath, not the conflicting source draft name. EF10 carries a string path; EF11 carries segments and separate array indices. The public UpdateSqlGenerator hook is exercised directly to generate a bounded SQL Server assignment; it is not a registered replacement for the SQL Server provider generator and does not implement batching, concurrency or arbitrary provider updates. StructuredJsonPath.ToString takes an optional wildcard character, not a quoting character. Render property segments explicitly: a dot in a property name must remain one quoted segment. Reject unspecified or negative indices. The fixture tests the dotted name odd.name, concrete index 1, nested properties and whole-document replacement; it does not promise every possible SQL JSON path escape or SQL native json behavior. All values use @value. Root replacement intentionally replaces the document; sibling preservation applies to partial changes.

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

### TernPatches.csproj

{{snippet:n10-ternpatches-csproj}}

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

### TernPatches.csproj

{{snippet:n11-ternpatches-csproj}}

### Program.cs

{{snippet:n11-program-cs}}

## Constraints

EF10.0.13/net10.0 versus EF11.0.0/net11.0. SDK10.0.403/C#14 versus SDK11.0.100/C#15. SQL graphs pin Pkcs10.0.13 and SqlClient6.1.7/7.1.1. SQLite uses the locked platform bundle; actual native DLL hashes are in execution records. EF11 requires .NET11; no older-runtime route is offered. No workload or rendering mode applies. No experimental opt-in or NativeAOT claim. GA verification is limited to the recorded checks reconciliation.

SQL Server2025 17.0.5035.5 compatibility170; approved isolated Docker route. Executed on Windows x64; SQL engine runs in the existing Linux container. No other native platform verified.

## Evidence

The .NET 11 SDK baseline is now 11.0.100 GA with preview selection disabled. The .NET 10 comparisons retain their declared SDKs. Use the current sample pins, dependency locks and GA verification records for current coverage. Existing native exclusions and pending Human gates still apply.

The saved compilation and execution records identify exact commands, inputs and dependency graphs. Run eng/Test-TernExtensions.ps1 -Record to refresh them. PIN-959 comprehensive coverage and PIN-967 final readiness checks remain open. No new .NET10 foundation is added.

## References

- [Official guidance](https://learn.microsoft.com/en-us/sql/t-sql/functions/json-modify-transact-sql)


