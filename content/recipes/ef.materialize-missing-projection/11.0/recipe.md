# Preserve missing non-entity join results

## Goal

Preserve unmatched join rows using nullable scalar markers when shipped EF projections cannot materialize missing objects.

## When to use

Use a nullable marker from a non-null inner key to distinguish an absent row from a populated row. EF11.0.0 SQLite does not implement the documented direct missing-object semantics in the tested shapes. The direct route is withdrawn for this graph; this guidance teaches the executed fallback rather than waiting for a future release.

## Migration

1. Project a nullable key marker and nullable payload scalars inside the join. Keep all potentially missing columns nullable through SQL materialization.
2. Compose SQL filters over those scalars before `ToArray`. Then explicitly construct DTO/null, nullable struct/null or non-nullable default struct on the client. Client grouping is separate from SQL grouping.
3. Run both pinned consumers normally. `FinalProjection` checks matched rows, successful SQL translation followed by unmatched materialization failures, and nullable-struct translation rejection. It then asserts exact fallback results and composition.
4. The fixture has nests 1 and 2, with only nest 1 populated as (NestId=1, Birds=12). The unmatched DTO and nullable struct are null; the non-nullable struct has both fields zero. No direct-route success is inferred from official intended semantics.

## Older approach

The independently pinned .NET10 consumer remains a valid comparison.

### .NET10 Directory.Build.props

{{snippet:n10-directory-build-props}}

### .NET10 Directory.Build.targets

{{snippet:n10-directory-build-targets}}

### .NET10 Directory.Packages.props

{{snippet:n10-directory-packages-props}}

### .NET10 global.json

{{snippet:n10-global-json}}

### .NET10 PloverQueries.csproj

{{snippet:n10-ploverqueries-csproj}}

### .NET10 FinalProjection.cs

{{snippet:n10-finalprojection-cs}}

### .NET10 Program.cs

{{snippet:n10-program-cs}}

## Newer approach

The independently pinned .NET11 consumer records the actual GA behavior.

### .NET11 Directory.Build.props

{{snippet:n11-directory-build-props}}

### .NET11 Directory.Build.targets

{{snippet:n11-directory-build-targets}}

### .NET11 Directory.Packages.props

{{snippet:n11-directory-packages-props}}

### .NET11 FinalProjection.cs

{{snippet:n11-finalprojection-cs}}

### .NET11 global.json

{{snippet:n11-global-json}}

### .NET11 PloverQueries.csproj

{{snippet:n11-ploverqueries-csproj}}

### .NET11 Program.cs

{{snippet:n11-program-cs}}

## Evidence

Actual EF11.0.0 SQLite Windows x64: DTO, anonymous, grouped DTO and default-struct unmatched rows fail materialization after successful translation; nullable-struct composition fails translation. Both pinned consumers verify nullable-marker scalar projection, explicit client DTO/null, nullable/default struct construction, populated (1,12), SQL filtering and client grouping. Direct whole-object missing semantics are withdrawn for this graph. Other providers/platforms are not-tested.

SDK10.0.403/EF10.0.13 and SDK11.0.100/EF11.0.0 run independently locked native SQLite graphs. Fresh verification receipts bind source and dependencies. No InMemory or mock result is used.

## Constraints

EF11 requires net11. Direct Microsoft.Data.Sqlite11 retains its separate net10 route. This recipe does not broaden driver coverage. Historical RC evidence remains historical; PIN-967 and unrelated Human/release gates remain open.

## References

- [Official intended semantics](https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-11.0/whatsnew)
