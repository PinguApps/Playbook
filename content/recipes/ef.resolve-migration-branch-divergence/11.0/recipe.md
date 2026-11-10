# Reconcile competing migration histories before merging

## Goal

Re-scaffold one unpublished branch migration after adopting the other branch history so the merged snapshot represents one coherent sequence.

## Older approach

EF10 snapshots do not record the latest migration ID. A source merge can therefore look clean while the migrations describe competing histories. The same deliberate reconciliation workflow is required; a clean text merge is insufficient proof.

{{snippet:n10-program-cs}}

## Newer approach

EF11 snapshots record the latest migration ID. Two branches scaffolding from the same baseline produce different IDs on the same snapshot line. Treat that conflict as a warning about diverged histories. Keep one generated history and scaffold the other change again against it. Never settle the conflict by choosing an ID while retaining both original migrations.

{{snippet:n11-program-cs}}

## Migration

1. For migrations not applied to shared databases: return to the coherent pre-merge branch, remove only its generated migration and restore its baseline snapshot while retaining model edits. Merge the teammate history, combine the model edits, then scaffold your migration again. Inspect its Up/Down and designer model, confirm the snapshot latest ID, run has-pending-model-changes, and apply the sequence to a disposable database. Never run migrations remove on an already invalid merged sequence: its predecessor metadata may omit the teammate change. If a migration has reached a shared database, preserve it and plan a compatible rollback or corrective migration with the deployment owner. This recipe authorizes no deployment.

2. In each isolated project directory, scaffold the unchanged baseline with `dotnet ef migrations add Initial`. On branch A add `e.Property<string>("SignalColour")` in OnModelCreating and scaffold SignalColour. On branch B, starting from the baseline, add `e.Property<int>("LensCount")` and scaffold LensCount. Compare the snapshot latest migration IDs.

3. Retain both model edits but replace B's unpublished generated migration files and snapshot with A's coherent history. Run `dotnet ef migrations add LensCountAfterSignal`, then `dotnet ef migrations has-pending-model-changes`. The new migration must add only LensCount; its snapshot retains both properties and, on EF11, the new latest ID. Apply to a fresh disposable SQLite file with `dotnet ef database update`. Use matching pinned dotnet-ef and Design versions; no shared database is involved. The repository check automates these steps outside the canonical source directories.

## Constraints

Team migration workflow; do not silently accept both histories. Requires EF11/net11.0/SDK11/C#15; historical EF10.0.13/net10.0/SDK10.0.403/C#14 comparison. Exact package assets, provider and prerequisites are in applicability. Windows x64 execution; no renderer or additional workload. Stable contract, RC2 evidence, no experimental opt-in. GA reconciliation remains pending.

## Complete projects

### .NET 10

{{snippet:n10-directory-build-props}}

{{snippet:n10-directory-build-targets}}

{{snippet:n10-directory-packages-props}}

{{snippet:n10-global-json}}

{{snippet:n10-beaconmigrations-csproj}}

### .NET 11

{{snippet:n11-directory-build-props}}

{{snippet:n11-directory-build-targets}}

{{snippet:n11-directory-packages-props}}

{{snippet:n11-global-json}}

{{snippet:n11-beaconmigrations-csproj}}

## Evidence

The .NET 11 SDK baseline is now 11.0.100 GA with preview selection disabled. The .NET 10 comparisons retain their declared SDKs. Earlier RC audit details below are historical; use the current sample pins, dependency locks and GA verification records for current coverage. Existing native exclusions and pending Human gates still apply.

Canonical compilation and focused real local execution are distinct from source review, mocks and cross-compilation. Exact SDKs, packages, input hashes and outputs are retained in the evidence records. Installed offline list/get checks full body, applicability and target11/edition11; target10 has no applicable edition. No cloud parity or GA execution is claimed.

## References

[Official team migration workflow](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/teams).
