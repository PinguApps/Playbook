# Index selected SQLite JSON values

## Goal

Kestrel's harbour crates carry a JSON label and ordered slots. Index the harbour
and priority together, and the first slot's code, without flattening the document.
These checks establish schema and lookup correctness, not a faster query plan.

## Older approach

EF10 maps an owned JSON document. Its migration explicitly creates SQLite
expression indexes using SQL. This remains a valid provider-specific alternative.

{{snippet:n10-program-cs}}

{{snippet:n10-kestrelcrates-csproj}}

After scaffolding the initial migration, add these statements to its Up method:

{{snippet:n10-migration}}

Dropping the table in Down also drops these indexes. Keep these explicit SQL
statements when recreating the migration; EF10's snapshot does not model them.

## Newer approach

EF11 models the JSON paths directly. IsDescending(false, true) belongs to the
two elements even though both reside in the same Label column. A fixed collection
position is supported; an unspecified all-elements path is rejected by SQLite.

{{snippet:n11-program-cs}}

{{snippet:n11-kestrelcrates-csproj}}

### EF11 scaffolded migration

{{snippet:n11-migration}}

## Migration

1. Restore each project independently using its pinned SDK and package lock.
2. Install matching dotnet-ef 10.0.13 or 11.0.0 into a local tool directory.
3. Scaffold `migrations add CreateCrates --project <project-directory>`.
   For EF10 add the two explicit SQL statements above. EF11 scaffolds the path
   model into its snapshot and resolves it when generating migration SQL.
4. Run the project. It applies its migration to a fresh in-memory database,
   verifies exact IDs for scalar and first-position lookups, and reads sqlite_schema.
5. Inspect the composite expression index's Priority DESC and the Slots[0].Code
   expression. Revisit migration SQL when changing paths or document structure.

## Constraints

RelationalJsonIndex accepts elements, parallel collectionIndices, and optional
isDescending. Null means all ascending; an empty list means all descending;
a nonempty list must have one bool per element. Its IsElementDescending resolves
that shorthand. Collection indices distinguish scalar paths, fixed positions and
unspecified positions. The example checks the actual migration operation:
its deduplicated column-level sort list is null and its JSON metadata retains two
distinct element orders. A mismatched sort count throws ArgumentException.

SQL Server's native JSON-index validation is a separate provider contract.
Its GA validator rejects unsupported options, including descending order; an
Internal diagnostic accessor is not a supported application API. Do not transfer
SQLite expression-index support to SQL Server. Existing sql.index-json-path
guidance retains its own service and feature restrictions.

## Evidence

The consumers execute bundled native SQLite on Windows x64. Other platforms,
NativeAOT, SQL Server execution and performance effects are not tested here.
The independent locks record the actual engine and native dependencies.

## Complete configuration

### EF10

{{snippet:n10-directory-build-props}}
{{snippet:n10-directory-build-targets}}
{{snippet:n10-directory-packages-props}}
{{snippet:n10-global-json}}

### EF11

{{snippet:n11-directory-build-props}}
{{snippet:n11-directory-build-targets}}
{{snippet:n11-directory-packages-props}}
{{snippet:n11-global-json}}

## References

- [SQLite expression indexes](https://www.sqlite.org/expridx.html)
- [GA JSON-index metadata source](https://raw.githubusercontent.com/dotnet/dotnet/297acf930fb0f5c07fd4e51d78e521b4ad2e5d1f/src/efcore/src/EFCore.Relational/Metadata/RelationalJsonIndex.cs)
