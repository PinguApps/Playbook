# Retain dependent rows when their principal can be missing

## Goal

Use unconstrained relationships when reads must retain orphaned dependents, and distinguish that query decision from migration-only constraint omission.

## When to use

EF11 IsConstrained(false) omits the database foreign key and uses a LEFT JOIN for the required navigation. ExcludeForeignKeyFromMigrations only omits enforcement: the required navigation still uses INNER JOIN and drops an orphan. Both retain the model relationship and its index. The SQL2025 fixture inserts one matched and one orphan crate, then checks generated DDL, actual sys.foreign_keys/sys.indexes and executed results. The Required control deliberately removes its generated constraint solely to demonstrate the bad assumption. EF10 keeps a valid explicit GroupJoin/DefaultIfEmpty left join and explicitly omits the generated constraint; it cannot call IsConstrained. The schema script edit is bounded to this authored fixture, not a general migration rewriter. For production use reviewed migrations. No database referential-integrity guarantee remains: the application and data owner must handle absent principals and decide validation, repair and delete policy. Required CLR navigation annotations do not prevent null projections from an unconstrained join. SQL Server only; no Cosmos or other-provider parity.

## Migration

1. Identify your exact old callable contract and retain a before-upgrade fixture. Do not revive error-obsolete APIs.
2. Apply the contract and behaviour distinction described above. Keep application-specific selection and integrity decisions explicit.
3. Keep each complete project below in its own directory, including its isolated SDK and package files.
4. Run eng/Test-LarkContracts.ps1; check the observed results and failure controls before adapting the code.
5. Repeat affected checks on your production provider and actual GA artifacts.

## Older approach

### Directory.Build.props

{{snippet:n10-directory-build-props}}

### Directory.Build.targets

{{snippet:n10-directory-build-targets}}

### Directory.Packages.props

{{snippet:n10-directory-packages-props}}

### global.json

{{snippet:n10-global-json}}

### LarkRelationships.csproj

{{snippet:n10-larkrelationships-csproj}}

### Program.cs

{{snippet:n10-program-cs}}

## Newer approach

### Directory.Build.props

{{snippet:n11-directory-build-props}}

### Directory.Build.targets

{{snippet:n11-directory-build-targets}}

### Directory.Packages.props

{{snippet:n11-directory-packages-props}}

### global.json

{{snippet:n11-global-json}}

### LarkRelationships.csproj

{{snippet:n11-larkrelationships-csproj}}

### Program.cs

{{snippet:n11-program-cs}}

## Constraints

Independent locked EF10.0.13/net10.0/SDK10.0.402/C#14 and EF11.0.0-rc.2.26504.105/net11.0/SDK11.0.100-rc.2.26504.105/C#15 graphs. EF11 requires .NET11; no older-runtime EF11 route. Current proof uses RC2, not GA. No experimental attribute or opt-in is required on the exercised public contracts.

SQL Server 2025 via the isolated local route. Executed on Windows x64. SQL2025 runs in the existing Linux container. Other providers/platforms and NativeAOT are unverified. No workload or rendering mode applies. Public extension contracts remain upgrade-sensitive.

## Evidence

eng/Test-LarkContracts.ps1 -Record: pinned real compilation and provider checks. No GA, NativeAOT or other-platform claim. Compilation receipts and behaviour records retain exact inputs, resolved dependencies, commands and actual outputs. The ordinary installed stdio harness checks full offline Markdown, canonical snippets, edition and applicability. No ninth .NET10 foundation is added. PIN-959 comprehensive readiness and PIN-965 actual GA reconciliation remain open.

## References

- [Official EF guidance](https://learn.microsoft.com/en-us/ef/core/modeling/relationships/foreign-and-principal-keys)

