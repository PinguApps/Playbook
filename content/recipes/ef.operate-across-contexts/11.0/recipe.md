# Operate deliberately across several contexts

## Goal

Fleet has separate berth and beacon databases. Inspect both models, generate scripts, and deliberately apply or remove their schemas without accidentally repeating a command against one database.

## Older approach

With EF10, enumerate the known context names and pass each explicitly. This remains a useful, controlled approach when only a subset should change. Keep failures visible after each invocation.

{{snippet:n10-commands}}

## Newer approach

EF11 accepts a quoted literal wildcard for migrations list, migrations script, database update and database drop. It is not a general context option for add/remove migrations or context info. RC2 has a confirmed wildcard-list defect: it visits contexts, then tries to resolve a context literally named '*', returning exit code 1. Use explicit names for list until a fixed official artifact is verified.

{{snippet:n11-commands}}

Script with a wildcard concatenates the models' SQL into one file. It does not split the result by database. Fleet therefore retains explicit per-context scripts for actual deployment to separate databases. Wildcard update/drop discovers and uses each context's configured connection. Review that set before running either operation; update is not one transaction across databases, and drop is destructive.

## Migration

1. Confirm the discovered contexts and each configured connection before changing databases.
2. Retain explicit names for RC2 list and for deploying separate script artifacts.
3. Use a quoted wildcard only for the supported commands when every discovered context is intended.
4. Check every target and exit code. A partial failure is not a cross-database rollback.
5. Refresh wildcard-list proof when the official EF build changes; do not treat the RC2 failure as a successful list operation.

## Constraints

net11.0, EF11 RC2 and the matching tool are required here. EF10.0.13/net10.0 is a historical comparison, not another published edition. SDK11 RC3/C#15 and SDK10.0.403/C#14 are pinned. SQLite local disposable files are the only executed provider; no external resources, UI, renderer or workloads are involved. Explicit Design is required by the CLI consumer. RC2 evidence is not GA verification. Pingu authorized publication with the reproduced wildcard-list failure on 4 November 2026; successful wildcard-list proof remains incomplete.

## Evidence

Test-FleetTooling builds both actual locked projects in disposable directories. Explicit scripts execute against fresh SQLite databases. On EF11 it reproduces the list error, checks combined script output for both model tables, updates both configured database files and drops both. A separate stdout script executes without filtering; diagnostics remain on stderr. These are real Windows x64 CLI operations, not mocks or cross-compilation. Other providers/platforms and successful wildcard list are unverified.

## Complete canonical sources

{{snippet:n10-capture}}



{{snippet:n10-directory-build-props}}

{{snippet:n10-directory-build-targets}}

{{snippet:n10-directory-packages-props}}

{{snippet:n10-fleettooling-csproj}}

{{snippet:n10-global-json}}

{{snippet:n10-program-cs}}

{{snippet:n11-capture}}



{{snippet:n11-directory-build-props}}

{{snippet:n11-directory-build-targets}}

{{snippet:n11-directory-packages-props}}

{{snippet:n11-fleettooling-csproj}}

{{snippet:n11-global-json}}

{{snippet:n11-program-cs}}


{{snippet:n10-migration-20261104182917-initialbeacon}}

{{snippet:n10-migration-20261104182917-initialbeacon-designer}}

{{snippet:n10-migration-beaconcontextmodelsnapshot}}

{{snippet:n10-migration-20261104182857-initialberth}}

{{snippet:n10-migration-20261104182857-initialberth-designer}}

{{snippet:n10-migration-berthcontextmodelsnapshot}}

{{snippet:n11-migration-20261104182926-initialbeacon}}

{{snippet:n11-migration-20261104182926-initialbeacon-designer}}

{{snippet:n11-migration-beaconcontextmodelsnapshot}}

{{snippet:n11-migration-20261104182923-initialberth}}

{{snippet:n11-migration-20261104182923-initialberth-designer}}

{{snippet:n11-migration-berthcontextmodelsnapshot}}

## References

Official EF11 changes, CLI and breaking-change references are listed in structured sources. Complete guidance above remains available offline.
