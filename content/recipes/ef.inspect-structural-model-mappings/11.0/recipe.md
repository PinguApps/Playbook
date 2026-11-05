# Inspect columns across entity and complex paths

## Goal

Inspect relational model metadata after upgrading EF and resolve entity, nested complex, table and view columns without assuming every property belongs to an entity.

## When to use

Use DeclaringType and distinguish IEntityType from IComplexType. Resolve columns with StoreObjectIdentifier for the selected table or view. EF11 complex paths may occur in key/index property lists; preserve list order and walk structural declaring paths. The older comparison retains an explicit entity SiteCode for key/index membership while separately inspecting its nested Location.Code. These are different authored models; this example does not migrate existing data. SQLite executes nested filtering and a keyless view. Native JSON indexes, container mappings and arbitrary provider hooks remain outside this route.

## Migration

1. Inventory your model and provider-specific consumers before changing metadata calls.
2. Pin and restore each complete comparison independently; EF11 requires .NET11.
3. Apply the distinction above and preserve model/property ordering.
4. Run eng/Test-CurlewModels.ps1, including the convention ordering controls. Inspect actual schema and fresh-context results.
5. Repeat affected checks on your provider and GA artifacts.

## Older approach

### Directory.Build.props

{{snippet:n10-directory-build-props}}

### Directory.Build.targets

{{snippet:n10-directory-build-targets}}

### Directory.Packages.props

{{snippet:n10-directory-packages-props}}

### global.json

{{snippet:n10-global-json}}

### CurlewMappings.csproj

{{snippet:n10-curlewmappings-csproj}}

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

### CurlewMappings.csproj

{{snippet:n11-curlewmappings-csproj}}

### Program.cs

{{snippet:n11-program-cs}}

## Constraints

Windows x64 console/SQLite execution only; no rendering mode, NativeAOT or native JSON-index route. Public extension contracts remain upgrade-sensitive.

## Evidence

Independent SDK10.0.402 / C#14 / EF10.0.13 and SDK11 RC2 / C#15 / EF11 RC2 graphs. SQLite3.53.3/3.53.4 are the measured bundled engines. Compilation and execution records identify inputs and locked dependencies. This is RC preparation evidence, not GA proof. No rendering mode or experimental opt-in is involved. No older-runtime EF11 route is supported. PIN-959 and PIN-965 remain separate gates.

## References

[Official conventions](https://learn.microsoft.com/en-us/ef/core/modeling/bulk-configuration#conventions)



SQLite rejects indexing the whole non-JSON complex property in the executed negative control. The current route indexes Location.Code plus Sequence and verifies their ordered property list; the alternate key uses Location.Code plus Id. GetTableColumnMappings and GetViewColumnMappings verify real store mappings. No native JSON/container-index route is advertised.
