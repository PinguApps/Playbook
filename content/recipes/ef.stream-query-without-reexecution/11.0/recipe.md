# Use EF's asynchronous query adapter intentionally

## Goal

FenWatch exports the ready survey stations in database order. Build the filter,
ordering and projection as an EF query, then use `AsAsyncEnumerable` at the point
where the consumer starts asynchronous enumeration. `await foreach` disposes its
enumerator on completion, `break` and exceptions; keep its context alive until then.

## Constraints

This 11.0 edition addresses EF query behavior and EF1004, not a general async-LINQ
package migration. The new item is the analyzer/code fix; `AsAsyncEnumerable`
already worked in EF10. Neither spelling is a cache: enumerating twice normally
executes two database queries. If repeated access to a bounded snapshot is wanted,
materialize once with `ToListAsync`, then reuse that list and accept its memory cost.
Do not start a second operation on the same context before the first completes.

The primary sample uses EF11/SQLite 11.0.0-rc.2.26504.105, net11.0, SDK
11.0.100-rc.2.26504.105 and C# 15 mode. The comparison uses EF/SQLite 10.0.13,
net10.0, SDK 10.0.402 and C# 14. No new language syntax or experimental opt-in is
required. EF11 needs .NET11; the separate direct-SQLite older-target route does
not make EF11 compatible with net10.0. Native SQLite versions and all transitive
dependencies are recorded with each sample. No browser/rendering mode applies.

**Important baseline distinction:** runtime10's `ToAsyncEnumerable` enumerates this
EF query synchronously. The actual pinned runtime11 implementation detects an
`IAsyncEnumerable<T>` source and returns it directly; the current control therefore
uses EF's async command path too. EF1004 still reports the spelling. Prefer
`AsAsyncEnumerable` to make EF intent explicit, without claiming that this specific
runtime11 call forces synchronous I/O. A non-EF `IQueryable` needs its own provider
analysis rather than a blind code fix. This difference is covered by execution
and the package-matched runtime/analyzer source links below.

SQLite's ADO.NET async methods execute synchronously underneath. These real SQLite
checks observe EF's async command path and reader disposal; they do not establish
nonblocking native I/O, another provider's cancellation behavior or a speed gain.

## Older approach

The complete .NET10 project measures the misleading adapter and the valid older
`AsAsyncEnumerable` replacement against the same query. The former is a migration
control, not the recommended way to stream an EF query.

#### global.json

{{snippet:n10-global-json}}

#### Directory.Build.props

{{snippet:n10-directory-build-props}}

#### Directory.Build.targets

{{snippet:n10-directory-build-targets}}

#### Directory.Packages.props

{{snippet:n10-directory-packages-props}}

#### FenWatch.csproj

{{snippet:n10-fenwatch-csproj}}

#### Program.cs

{{snippet:n10-program-cs}}

## Newer approach

The complete .NET11 project selects `AsAsyncEnumerable`. Its second, narrowly
suppressed EF1004 call is only a control for the runtime11 fast path; applications
should use the unsuppressed primary call. The interceptor counts actual SQLite
commands and created/disposed readers; no repository, provider or command is mocked.

#### global.json

{{snippet:n11-global-json}}

#### Directory.Build.props

{{snippet:n11-directory-build-props}}

#### Directory.Build.targets

{{snippet:n11-directory-build-targets}}

#### Directory.Packages.props

{{snippet:n11-directory-packages-props}}

#### FenWatch.csproj

{{snippet:n11-fenwatch-csproj}}

#### Program.cs

{{snippet:n11-program-cs}}

## Migration and running

1. Identify the source as an EF `IQueryable`. Retain server-side `Where`, `OrderBy`
   and `Select` before the adapter; operators after it run on the client.
2. Import `Microsoft.EntityFrameworkCore` and replace `ToAsyncEnumerable()` with
   `AsAsyncEnumerable()`. Remove a suppression used solely for that warning.
3. Consume once with `await foreach`, optionally using `WithCancellation(token)`.
   Do not return a deferred stream from a method that has already disposed its
   context. With manual enumerators, use `await using` or a `finally` disposal.
4. In a standalone directory containing the six files of the selected project,
   run `dotnet run --configuration Release` with the declared SDK installed.
   `nuget.org` must be available for initial restore. The project creates and
   discards only its in-memory database.
5. Expect the final `PASS: query adapter` line. Each adapter checks one command
   and one disposal on early exit, two queries/disposals on repeated enumeration,
   and disposal after a consumer exception. The pre-cancelled EF stream must throw
   without leaking a created reader; in-flight/provider cancellation is not tested.

For the compiler warning itself, the repository's `eng/Test-EfQueryAdapter.ps1`
copies the current project outside the checkout, removes only its control
suppression and expects an EF1004 build failure. It then changes the adapter to
`AsAsyncEnumerable` and requires a successful build. This is a compiler/analyzer
check, separate from the real database executions recorded above.

## Evidence

Both declared SDK/package consumers compiled and ran on Windows x64 against actual
SQLite, with the exact resolved graphs locked in `packages.lock.json`. Saved
`verification-evidence.json` records include input hashes, commands, native SQLite
version, runtime version and PASS output. The installed-package Inspector check
separately establishes full offline Markdown, snippet identity and applicability.
These are RC2 observations; GA, non-SQLite I/O and other native platforms remain
unverified. No external service or recurring platform matrix is required.

## Official references

- [Official EF11 release guidance](https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-11.0/whatsnew#other-improvements).
- [Immutable reviewed documentation baseline](https://github.com/dotnet/EntityFramework.Docs/blob/aa051c4c1cad21b2dc03fa401ac0e925c3644606/entity-framework/core/what-is-new/ef-core-11.0/whatsnew.md).
- [EF asynchronous query and context lifetime](https://learn.microsoft.com/en-us/ef/core/miscellaneous/async).
- [SQLite asynchronous I/O limitation](https://learn.microsoft.com/en-us/dotnet/standard/data/sqlite/async).
- [Package-matched runtime adapter implementation](https://github.com/dotnet/dotnet/blob/820230c386db5b0106c5430b3e5aee92a7826bf5/src/runtime/src/libraries/System.Linq.AsyncEnumerable/src/System/Linq/ToAsyncEnumerable.cs).
- [Package-matched EF1004 analyzer](https://github.com/dotnet/dotnet/blob/820230c386db5b0106c5430b3e5aee92a7826bf5/src/efcore/src/EFCore.Analyzers/ToAsyncEnumerableOnQueryableDiagnosticAnalyzer.cs).
