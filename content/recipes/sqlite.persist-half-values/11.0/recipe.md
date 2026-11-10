# Store half-precision values in SQLite

## Goal

Persist nullable Half measurements through SQLite parameters and EF11 mapping, preserving explicit precision and REAL storage boundaries.

## When to use

Osprey gauges measure finite half-precision values. Microsoft.Data.Sqlite11 binds boxed Half and GetFieldValue<Half> reads it; EF11 maps nullable Half directly. EF10 uses an explicit double parameter/read cast and an explicit expression converter. The generic HasConversion<double>() call alone does not supply this older Half converter. Actual zero, rounded 1.1, extrema, minimum subnormal and NULL round-trip; a fresh EF context reads the same nullable values. SQLite stores non-null parameters as REAL and nulls as NULL. Half is a 16-bit CLR type, not a promise of two-byte SQLite disk storage. Precision is already lost when converting 1.1 to Half; REAL cannot recover it. Reading 100000.0 into Half overflows to positive infinity, so validate the finite range before accepting wider values. This example teaches finite scalar columns only. JSON mapping, NaN payload preservation, signed-zero bit identity and encryption are not established by these checks and are not taught as supported routes.

## Migration

1. Decide whether Half precision and range fit the measurement; rounding occurs before a database write.
2. On the older provider bind/read explicit double values and configure conversion expressions. Retain the generic HasConversion<double>() model rejection control.
3. On Microsoft.Data.Sqlite11 bind boxed Half and read GetFieldValue<Half>; EF11 maps the nullable scalar directly.
4. Round-trip zero, rounded 1.1, extrema, minimum subnormal and NULL directly, then read EF writes using a fresh context.
5. Inspect typeof(Value): finite values use REAL, not a two-byte storage class. Validate wider inputs before narrowing; the 100000.0 read becomes positive infinity.

## Requirements and verification

Use SDK10.0.403/C#14/EF10.0.13 versus SDK11.0.100-rc.3.26522.148/C#15/EF11.0.0-rc.2.26504.105. These are separate locked console consumers. Standard bundled SQLite initializes native access. SQL uses the existing disposable SQL2025 route. No workloads, browser rendering or experimental opt-ins are required. Compilation and actual database execution are recorded independently of installed-tool retrieval. No new .NET10 foundation is added.

## Older approach

The older direct path widens Half to double on write and narrows GetDouble on read. EF uses explicit conversion expressions; a separate model rejects the generic conversion overload alone.

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

The current binder and reader consume Half without a user converter. A fresh EF context reads nullable Half properties. Finite numeric equality does not establish bit-preserving serialization.

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

Finite nullable scalar columns on SQLite only. Half does not guarantee two-byte disk storage, recover lost precision or automatically reject overflow. JSON mapping, NaN payloads, signed-zero bit identity, encrypted engines and other native platforms remain unverified.

## Evidence

The .NET 11 SDK baseline is now 11.0.100 GA with preview selection disabled. The .NET 10 comparisons retain their declared SDKs. Earlier RC audit details below are historical; use the current sample pins, dependency locks and GA verification records for current coverage. Existing native exclusions and pending Human gates still apply.

Both independently locked consumers compile and execute through eng/Test-OspreyReadings.ps1 -Record. Saved compilation and execution records include exact inputs and resolved dependencies. The ordinary installed tool is inspected separately through real stdio list/get with an isolated feed/cache and unrelated working directory.
