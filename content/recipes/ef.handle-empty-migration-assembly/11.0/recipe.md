# Distinguish intentional schema ownership from missing migrations

## Goal

Keep Canal's mooring application from silently assuming it owns the database. An empty migration assembly can mean the wrong project was deployed, or that a separate schema owner manages the tables.

## Older approach

The EF10 consumer calls both migration entry points against an empty assembly. They return without applying model changes. This does not create the Moorings table or demonstrate that a deployment is healthy. The explicit external-owner read remains a valid approach on both releases.

{{snippet:n10-program-cs}}

## Newer approach

EF11 throws by default when Migrate or MigrateAsync finds no migrations. The canonical check catches only an InvalidOperationException identifying MigrationsNotFound, then checks an independently created SQLite table. Application startup should omit Migrate when schema updates belong elsewhere; the factory also performs no migration. Do not replace Migrate with EnsureCreated for an existing migration-managed database.

{{snippet:n11-program-cs}}

## Migration

1. Check the target assembly, startup project and deployed migration files. An empty assembly is different from an up-to-date database with no pending migrations.
2. If this application owns migrations, correct assembly/project selection and author the missing migration rather than hiding the diagnostic.
3. If another system owns the schema, remove the startup Migrate/MigrateAsync call and arrange schema deployment through that owner. The sample uses direct SQL solely to simulate that separate owner in an in-memory test.
4. If an intentionally retained empty-assembly call is necessary, the documented ConfigureWarnings route can Log or Ignore RelationalEventId.MigrationsNotFound. This recipe verifies removal of the unwanted call instead; diagnostic suppression is not execution-tested here. Suppression does not apply a schema or fix a wrong assembly.

## Constraints

Relational EF11 requires .NET11; paired EF10 targets .NET10. The canonical consumer uses C#15/14, matching SQLite/Design/Tools packages and no browser, renderer, workload, account or external database. Windows x64 is executed; other platforms and providers are not established by this SQLite check. Exact RC3 evidence is not GA evidence.

## Evidence

The .NET 11 SDK baseline is now 11.0.100 GA with preview selection disabled. The .NET 10 comparisons retain their declared SDKs. Earlier RC audit details below are historical; use the current sample pins, dependency locks and GA verification records for current coverage. Existing native exclusions and pending Human gates still apply.

Real in-memory SQLite execution checks both migration entry points and reads a table created by another owner. No mocks, cross-compilation, Visual Studio execution or GA proof is claimed. The installed local-feed harness verifies full canonical body, applicability and edition11 from outside the checkout with unusable proxies; that establishes proxy independence, not OS network isolation.

## Complete canonical projects

{{snippet:n10-canaldesign-csproj}}

{{snippet:n10-directory-build-props}}

{{snippet:n10-directory-build-targets}}

{{snippet:n10-directory-packages-props}}

{{snippet:n10-global-json}}

{{snippet:n11-canaldesign-csproj}}

{{snippet:n11-directory-build-props}}

{{snippet:n11-directory-build-targets}}

{{snippet:n11-directory-packages-props}}

{{snippet:n11-global-json}}


## References

Official EF11 change notes and CLI/breaking-change contracts are listed in structured sources. [EF11 changes](https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-11.0/whatsnew), [CLI defaults](https://learn.microsoft.com/en-us/ef/core/cli/dotnet), [breaking changes](https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-11.0/breaking-changes).
