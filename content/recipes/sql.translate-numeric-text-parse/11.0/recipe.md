# Parse stored numeric text inside a SQL query

## Goal

Parse numeric text in SQL Server filters with EF11 single-string Parse translations, choosing deliberate SQL conversion and failure semantics.

## When to use

Osprey survey imports store numeric text. EF11 translates the single-string byte, short, int, long, decimal, float and double Parse calls to tinyint, smallint, int, bigint, decimal(18,2), real and float CAST expressions. The executed filter returns only reading 1. Invalid text, int overflow and comma-decimal text fail at SQL Server with conversion errors; no TryParse guarantee is implied. A nullable result projection propagates SQL NULL. CLR Parse(null) instead throws; culture-sensitive CLR parsing is not the SQL grammar. The French CLR control accepts 1,5 as 1.5 while the integer SQL cast rejects it. Decimal CAST has fixed scale and can round differently from CLR decimal.Parse. No culture/provider overload or arbitrary .NET numeric format is promised. EF10 rejects Parse in a predicate; its fixed SQL TRY_CAST alternative treats failed conversions as NULL and excludes them. TRY_CAST deliberately has different error semantics from CAST. A WHERE guard is not a guarantee of SQL expression evaluation order. Clean input before conversion or use a deliberate TRY_CAST policy.

## Migration

1. Identify stored text format, destination type and the intended conversion failure policy.
2. Keep older TRY_CAST SQL when malformed text should become NULL and be excluded. This remains a valid server-side approach.
3. On EF11 use only a supported single-string Parse overload. Inspect the CAST destination: decimal(18,2) does not preserve arbitrary CLR decimal precision.
4. Execute valid and null cases, then isolate invalid text, integer overflow and comma-decimal failures.
5. Compare CLR culture parsing separately. A WHERE guard does not guarantee SQL expression evaluation order or make CAST accept every CLR format.

## Requirements and verification

Use SDK10.0.403/C#14/EF10.0.13 versus SDK11.0.100-rc.3.26522.148/C#15/EF11.0.0-rc.2.26504.105. These are separate locked console consumers. Standard bundled SQLite initializes native access. SQL uses the existing disposable SQL2025 route. No workloads, browser rendering or experimental opt-ins are required. Compilation and actual database execution are recorded independently of installed-tool retrieval. No new .NET10 foundation is added.

## Older approach

EF10 rejects int.Parse in the predicate before sending SQL. Its fixed TRY_CAST alternative returns the valid matching row while choosing NULL rather than an error for failed conversions.

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

EF11 executes CAST in the filter and seven numeric projection overloads. Isolated invalid text, overflow and comma text controls make each SQL failure observable.

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

[Pinned official feature notes](https://github.com/dotnet/core/blob/60871330fce2b3da383461be027ebdf3ce30f8fb/release-notes/11.0/preview/preview7/efcore.md).

[Official EF11 guidance](https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-11.0/whatsnew).


## Constraints

SQL Server CAST follows server grammar, null propagation, range and scale rules. The French CLR control is a comparison, not a SQL culture option. TRY_CAST deliberately changes failure handling. Other Parse overloads and providers are not promised.

## Evidence

The .NET 11 SDK baseline is now 11.0.100 GA with preview selection disabled. The .NET 10 comparisons retain their declared SDKs. Earlier RC audit details below are historical; use the current sample pins, dependency locks and GA verification records for current coverage. Existing native exclusions and pending Human gates still apply.

Both independently locked consumers compile and execute through eng/Test-OspreyReadings.ps1 -Record. Saved compilation and execution records include exact inputs and resolved dependencies. The ordinary installed tool is inspected separately through real stdio list/get with an isolated feed/cache and unrelated working directory.
