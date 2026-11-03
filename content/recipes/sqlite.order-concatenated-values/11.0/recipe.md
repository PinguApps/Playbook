# Return an ordered textual summary per group

## Goal

Combine each group into predictable text in SQLite, ordering values inside server-side aggregation.

## Older approach

EF10 can fetch the required rows and group and sort them in the application. This valid approach gives explicit ordinal ordering but transfers all contributing rows. For large result sets a carefully parameterized SQL aggregate is another option.

{{snippet:n10-query}}

## Newer approach

EF11 translates ordering inside string.Join to group_concat with ORDER BY. Order by Position and then unique Id so equal positions have a deterministic tie breaker. Ordering the outer query alone cannot order inputs to an aggregate.

{{snippet:n11-query}}

## Migration

1. Copy the complete HarbourLabels project below, including the independent build/package files and SDK pin, into its own directory.
2. Use the pinned SDK from that directory: `dotnet restore HarbourLabels.csproj --use-lock-file`, then `dotnet build HarbourLabels.csproj -c Release -p:RestoreLockedMode=true`.
3. Run `dotnet run --project HarbourLabels.csproj -c Release --no-build`. It creates a private in-memory SQLite database; no Docker, external database or account is needed.
4. Expect `PASS ordered harbour summaries`, West = Anchor / Buoy / Chain and East = Lamp. The current query prints group_concat with its internal ORDER BY; outer ordering alone is insufficient. The database disappears when the consumer exits.

## Complete previous project — .NET 10

global.json

{{snippet:n10-global-json}}

Directory.Build.props

{{snippet:n10-directory-build-props}}

Directory.Build.targets

{{snippet:n10-directory-build-targets}}

Directory.Packages.props

{{snippet:n10-directory-packages-props}}

HarbourLabels.csproj

{{snippet:n10-harbourlabels-csproj}}

Program.cs

{{snippet:n10-program-cs}}

## Complete current project — .NET 11

global.json

{{snippet:n11-global-json}}

Directory.Build.props

{{snippet:n11-directory-build-props}}

Directory.Build.targets

{{snippet:n11-directory-build-targets}}

Directory.Packages.props

{{snippet:n11-directory-packages-props}}

HarbourLabels.csproj

{{snippet:n11-harbourlabels-csproj}}

Program.cs

{{snippet:n11-program-cs}}

## Constraints

SQLite 3.44+ is required for aggregate ORDER BY. The real current sample uses the provider native bundle and SQLite 3.53.4; the older sample uses 3.53.3. EF11 requires net11.0. This replaces client evaluation/workaround for the demonstrated query shape. Nulls, collation and separators require application-specific rules; this fixture has non-null text and integer order keys. No allocation or throughput improvement is measured.

Stable API contract using RC2 evidence, without experimental opt-in; actual GA remains unverified. No web renderer, browser or workload is required. The previous sample is a comparison, not another .NET10 foundation.

## Evidence

Actual in-memory native SQLite, deterministic two-group summaries including tied positions; EF11 generated SQL contains ordered group_concat. EF10 materializes rows before local grouping. Windows x64 only; no GA claim. Exact SDKs, dependencies, input hashes and actual output are retained in the paired verification records. Compilation and real execution are distinguished from source review. Offline installed MCP checks use the existing isolated feed/cache harness; unusable proxies demonstrate proxy independence, not operating-system network isolation.
