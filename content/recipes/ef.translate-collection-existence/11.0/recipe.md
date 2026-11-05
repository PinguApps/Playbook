# Keep collection-existence predicates on the database

## Goal

Filter stored lists and binary values before materialization using EF11 List.Exists and byte array Any; retain executed server-side alternatives on EF10.

## When to use

Osprey survey readings use nullable JSON integer lists and nullable binary signals. EF11 translates Exists to EXISTS over json_each or OPENJSON, and binary Any to length or DATALENGTH. Explicit null guards exclude nulls; empty collections yield no match. The populated row alone matches. EF10 rejects these direct predicates before sending a command; it does not silently evaluate these filters on the client. List.Any remains valid. Use the fixed SQL length query for binary values: EF10 rewrites the apparent Length > 0 alternative to unsupported Any. Keep user values parameterized. No index-use, query-plan or performance claim follows.

## Migration

1. Identify JSON lists versus binary columns and preserve the distinction between null and empty.
2. On EF10 keep List.Any for lists and fixed length/DATALENGTH SQL for binary signals; filter before materialization.
3. With matching EF11 providers replace only these checks with List.Exists and parameterless binary Any.
4. Run populated, empty and null rows on SQLite and SQL Server. Verify only row 1 matches and inspect executed EXISTS and length/DATALENGTH.
5. Retain the EF10 Length > 0 rejection control: the tested graph rewrites it to unsupported Any.

## Requirements and verification

Use SDK10.0.403/C#14/EF10.0.13 versus SDK11.0.100-rc.3.26522.148/C#15/EF11.0.0-rc.2.26504.105. These are separate locked console consumers. Standard bundled SQLite initializes native access. SQL uses the existing disposable SQL2025 route. No workloads, browser rendering or experimental opt-ins are required. Compilation and actual database execution are recorded independently of installed-tool retrieval. No new .NET10 foundation is added.

## Older approach

List.Any already executes on the older database. Fixed authored SQL supplies the binary length alternative; both direct Any and Length > 0 controls fail before a command is sent.

### Directory.Build.props

{{snippet:n10-directory-build-props}}

### Directory.Build.targets

{{snippet:n10-directory-build-targets}}

### Directory.Packages.props

{{snippet:n10-directory-packages-props}}

### global.json

{{snippet:n10-global-json}}

### OspreyReadings.csproj

{{snippet:n10-ospreyreadings-csproj}}

### Program.cs

{{snippet:n10-program-cs}}

### HalfStorage.cs

{{snippet:n10-halfstorage-cs}}

## Newer approach

Current Exists and binary Any execute inside the provider. The interceptor records executed commands; exact row IDs prove the populated case matches while empty and null do not.

### Directory.Build.props

{{snippet:n11-directory-build-props}}

### Directory.Build.targets

{{snippet:n11-directory-build-targets}}

### Directory.Packages.props

{{snippet:n11-directory-packages-props}}

### global.json

{{snippet:n11-global-json}}

### OspreyReadings.csproj

{{snippet:n11-ospreyreadings-csproj}}

### Program.cs

{{snippet:n11-program-cs}}

### HalfStorage.cs

{{snippet:n11-halfstorage-cs}}

## References

[Pinned official feature notes](https://github.com/dotnet/core/blob/60871330fce2b3da383461be027ebdf3ce30f8fb/release-notes/11.0/preview/preview6/efcore.md).

[Official EF11 guidance](https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-11.0/whatsnew).


## Constraints

Nullable JSON integer lists and binary columns only. Null guards exclude null; empty values do not match. Arbitrary predicate methods and other providers are outside the route. Translation establishes neither index use nor a faster query plan.

## Evidence

Both independently locked consumers compile and execute through eng/Test-OspreyReadings.ps1 -Record. Saved compilation and execution records include exact inputs and resolved dependencies. The ordinary installed tool is inspected separately through real stdio list/get with an isolated feed/cache and unrelated working directory.
