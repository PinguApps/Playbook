# Include archived records while retaining tenant filtering

## Goal

Read active and archived records for one tenant without disabling the tenant query predicate.

The fictional Moss seed archive has two tenants and both active and archived ledger entries. History must broaden only the archival condition. Choose the tenant from trusted application context; this example does not authenticate a user.

## Older approach

EF9 combines both conditions in one predicate and makes the archival condition depend on a context parameter. This is a valid route: construct a fresh context for history with includeArchived enabled; never call IgnoreQueryFilters to bypass the combined predicate.

{{snippet:n9-program-cs}}

## Newer approach

EF10 names the tenant and archive predicates separately. The history query disables only Archive by name. The Tenant predicate remains active. The normal query includes neither override. A deliberate negative control disables all filters and demonstrates why that call is unsafe for this goal.

{{snippet:n10-program-cs}}

## Migration

1. Keep the tenant predicate and replace the combined predicate with two differently named filters. Update history reads to disable only the archive filter. Verify both tenants, active results and historical results against the actual provider. Avoid sharing a tracking context between tenant identities. Filters can be explicitly disabled and do not protect direct SQL or writes; apply authorization separately.

## Constraints

Named filters require EF10; EF9 uses the context parameter shown above. Query filters do not enforce authorization or protect writes. SQLite is the actual tested provider; Windows x64 execution does not establish other providers or platforms.

## Evidence

SDK9.0.319 / EF9.0.13 and SDK10.0.402 / EF10.0.13 compile and execute real Windows x64 SQLite reads. Both tenants return one active and two history rows; the all-filter override returns four rows. See the canonical verification-evidence.json files. No SQL Server, authorization or write-protection claim follows.

## Complete project files

Keep each release in its own directory. Restore its pinned SDK and packages; no browser, rendering mode or optional workload is required. These files include the complete project configuration; the lock files preserve the measured transitive graph.

### net9.0 MossArchive.csproj

{{snippet:n9-mossarchive-csproj}}

### net9.0 Directory.Build.props

{{snippet:n9-directory-build-props}}

### net9.0 Directory.Build.targets

{{snippet:n9-directory-build-targets}}

### net9.0 Directory.Packages.props

{{snippet:n9-directory-packages-props}}

### net9.0 global.json

{{snippet:n9-global-json}}

### net10.0 MossArchive.csproj

{{snippet:n10-mossarchive-csproj}}

### net10.0 Directory.Build.props

{{snippet:n10-directory-build-props}}

### net10.0 Directory.Build.targets

{{snippet:n10-directory-build-targets}}

### net10.0 Directory.Packages.props

{{snippet:n10-directory-packages-props}}

### net10.0 global.json

{{snippet:n10-global-json}}

## References

- [Official querying guidance](https://learn.microsoft.com/en-us/ef/core/querying/filters)
