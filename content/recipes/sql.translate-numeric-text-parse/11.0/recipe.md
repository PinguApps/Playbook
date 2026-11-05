# Parse stored numeric text inside a SQL query

## Goal

Parse numeric text in SQL Server filters with EF11 single-string Parse translations, choosing deliberate SQL conversion and failure semantics.

## When to use

Osprey survey imports store numeric text. EF11 translates the single-string byte, short, int, long, decimal, float and double Parse calls to tinyint, smallint, int, bigint, decimal(18,2), real and float CAST expressions. The executed filter returns only reading 1. Invalid text, int overflow and comma-decimal text fail at SQL Server with conversion errors; no TryParse guarantee is implied. A nullable result projection propagates SQL NULL. CLR Parse(null) instead throws; culture-sensitive CLR parsing is not the SQL grammar. The French CLR control accepts 1,5 as 1.5 while the integer SQL cast rejects it. Decimal CAST has fixed scale and can round differently from CLR decimal.Parse. No culture/provider overload or arbitrary .NET numeric format is promised. EF10 rejects Parse in a predicate; its fixed SQL TRY_CAST alternative treats failed conversions as NULL and excludes them. TRY_CAST deliberately has different error semantics from CAST. A WHERE guard is not a guarantee of SQL expression evaluation order. Clean input before conversion or use a deliberate TRY_CAST policy.

## Migration

1. Identify stored types, null policy and provider before changing queries or converters.
2. Restore the complete previous and current consumers independently. EF11 requires .NET11; no EF11-on-net10 route is offered.
3. Apply only the supported shape described above. Preserve intentional older server-side approaches.
4. Run eng/Test-OspreyReadings.ps1 and inspect executed commands and exact results.
5. Recheck on actual GA artifacts and your deployment provider before relying on a broader claim.

## Requirements and verification

Use SDK10.0.403/C#14/EF10.0.13 versus SDK11.0.100-rc.3.26522.148/C#15/EF11.0.0-rc.2.26504.105. These are separate locked console consumers. Standard bundled SQLite initializes native access. SQL uses the existing disposable SQL2025 route. No workloads, browser rendering or experimental opt-ins are required. Compilation and actual database execution are recorded independently of installed-tool retrieval. No new .NET10 foundation is added.

## Older approach

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

[Official EF11 guidance](https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-11.0/whatsnew).


## Constraints

Only the declared scalar/query shapes and providers are covered. No GA, JSON Half mapping, nonfinite bit preservation, other-platform, index or performance claim is made. SQL CAST follows server grammar and range rules, not CLR culture rules.

## Evidence

Both independently locked consumers compile and execute through eng/Test-OspreyReadings.ps1 -Record. Saved compilation and execution records include exact inputs and resolved dependencies. The ordinary installed tool is inspected separately through real stdio list/get with an isolated feed/cache and unrelated working directory.

