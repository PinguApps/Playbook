# Recover a split read disrupted by concurrent writes

## Goal

Retry an entire bounded split read when concurrent inserts prevent correlation of its separate result sets.

## Older approach

Bracken's tray inventory reads a tray list and its cards in separate SQL commands. A second connection inserts tray20 and its card between those commands. EF10 completes with the original two roots. A fresh complete read sees all three trays. This is a deliberately coordinated reproduction, not a claim that every concurrent write disrupts every split query.

{{snippet:n10-program-cs}}

## Newer approach

EF11 detects the uncorrelated child rows and raises DbQueryConcurrencyException. Discard the failed read context and retry the complete read once in a fresh context. The sample has exactly one injected writer and one retry; a production caller needs a finite retry budget and cancellation, then an explicit failure path. Do not confuse this read exception with DbUpdateConcurrencyException from optimistic writes.

{{snippet:n11-program-cs}}

## Migration

1. Catch DbQueryConcurrencyException around the complete split read. Discard its context and partial materialization. Retry the entire read with a new context using a finite retry budget; after the budget is exhausted, propagate the failure. Keep write retries and their exception handling separate. The executable example injects one writer and permits one retry.

## Constraints

A single query removes this particular inter-command window but can increase row duplication. Snapshot or serializable transactions may provide stronger consistency at provider-specific concurrency and resource costs. Their guarantees, database settings and interactions with retry strategies need separate verification. This sample verifies whole-read retry on SQL Server, not isolation or single-query behavior. No blanket consistency guarantee follows. EnsureCreated is solely disposable fixture setup, never a production migration recommendation.

## Evidence

The .NET 11 SDK baseline is now 11.0.100 GA with preview selection disabled. The .NET 10 comparisons retain their declared SDKs. Earlier RC audit details below are historical; use the current sample pins, dependency locks and GA verification records for current coverage. Existing native exclusions and pending Human gates still apply.

SDK10.0.403 / EF SQL Server10.0.13 and SDK11 RC3 / EF SQL Server11 RC2 compile. eng/Test-BrackenReads.ps1 -Record executes each in a separately owned disposable database on the approved SQL Server2025 route. It commits the separate writer before the child SELECT, observes EF11's actual exception and verifies complete fresh-context results 10/20/30 with one card each. race.evidence.json retains execution separately from compilation. RC evidence is not GA reconciliation; other providers and isolation levels remain untested.

## Complete project files

Keep each release in its own directory. Restore its pinned SDK and packages; no browser, rendering mode or optional workload is required. These files include the complete project configuration; the lock files preserve the measured transitive graph.

### net10.0 BrackenReads.csproj

{{snippet:n10-brackenreads-csproj}}

### net10.0 Directory.Build.props

{{snippet:n10-directory-build-props}}

### net10.0 Directory.Build.targets

{{snippet:n10-directory-build-targets}}

### net10.0 Directory.Packages.props

{{snippet:n10-directory-packages-props}}

### net10.0 global.json

{{snippet:n10-global-json}}

### net11.0 BrackenReads.csproj

{{snippet:n11-brackenreads-csproj}}

### net11.0 Directory.Build.props

{{snippet:n11-directory-build-props}}

### net11.0 Directory.Build.targets

{{snippet:n11-directory-build-targets}}

### net11.0 Directory.Packages.props

{{snippet:n11-directory-packages-props}}

### net11.0 global.json

{{snippet:n11-global-json}}

## References

- [Official querying guidance](https://learn.microsoft.com/en-us/ef/core/querying/single-split-queries)
