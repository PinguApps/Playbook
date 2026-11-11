# Query Cosmos type identity through the model discriminator

## Goal

Update model-property references after upgrading while preserving the stored $type JSON name and existing documents.

## Older approach

EF10 calls the default shadow model property $type, matching its JSON name. The comparison writes a Mooring, queries its type through EF.Property and independently reads the stored JSON.

{{snippet:n10-program-cs}}

## Newer approach

EF11 names the default model property Discriminator while retaining $type in JSON. EF.Property takes a model name, not a JSON path. The sample inspects both names, executes the corrected predicate and reads the document through CosmosClient to prove the JSON name remains unchanged. Custom discriminator mappings must be inspected rather than globally replaced.

{{snippet:n11-program-cs}}

## Migration

1. Search string-based model references, projections and metadata code for $type. Inspect FindDiscriminatorProperty and GetJsonPropertyName on the actual model. Update only references to the default model property. Keep raw Cosmos SQL and JSON readers using $type unless a separately planned data migration changes the storage contract. Do not rename existing JSON data to fix a model-only rename.

## Constraints

Model Discriminator versus stored $type; no automatic data rename. Requires EF11/net11.0/SDK11/C#15; historical EF10.0.13/net10.0/SDK10.0.403/C#14 comparison. Exact package assets, provider and prerequisites are in applicability. Windows x64 execution; no renderer or additional workload. Stable contract, GA evidence, no experimental opt-in. GA checks are limited to the recorded scope.

## Complete projects

### .NET 10

{{snippet:n10-directory-build-props}}

{{snippet:n10-directory-build-targets}}

{{snippet:n10-directory-packages-props}}

{{snippet:n10-global-json}}

{{snippet:n10-harbourqueries-csproj}}

{{snippet:n10-emulatortransport-cs}}

### .NET 11

{{snippet:n11-directory-build-props}}

{{snippet:n11-directory-build-targets}}

{{snippet:n11-directory-packages-props}}

{{snippet:n11-global-json}}

{{snippet:n11-harbourqueries-csproj}}

{{snippet:n11-emulatortransport-cs}}

## Evidence

The .NET 11 SDK baseline is now 11.0.100 GA with preview selection disabled. The .NET 10 comparisons retain their declared SDKs. Use the current sample pins, dependency locks and GA verification records for current coverage. Existing native exclusions and pending Human gates still apply.

Canonical compilation and focused real local execution are distinct from source review, mocks and cross-compilation. Exact SDKs, packages, input hashes and outputs are retained in the evidence records. Installed offline list/get checks full body, applicability and target11/edition11; target10 has no applicable edition. No cloud parity or GA execution is claimed.

## References

[Official EF Cosmos guidance](https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-11.0/breaking-changes).
