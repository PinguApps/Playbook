# Preserve fields owned by another Cosmos writer

## Goal

Review document ownership before an EF11 upgrade so replacement saves do not erase fields absent from the model.

## Older approach

EF10 retains a backing JObject including the unmapped partnerNote when saving a mapped value. The sample checks raw shadow state and stored data.

{{snippet:n10-program-cs}}

## Newer approach

EF11 reads only mapped fields. A replacement save loses partnerNote in the deliberately incomplete model. Inventory every writer before upgrading. Map all fields the entity must round trip, or use direct CosmosClient for open-ended documents. The sample chooses direct complete JSON ownership: retain every field, edit Value and replace with IfMatchEtag. It verifies partner data survives and stale replacement returns 412. On conflict, reread and reapply the intended edit under the ownership rules; do not resend stale JSON. Do not mix stale tracked EF entities and direct replacements. This is a potential data-loss migration.

{{snippet:n11-program-cs}}

## Migration

1. Copy the complete projects below into separate release directories. Restore the exact pins and build with the declared SDK and locked restore. Run `eng/Test-ReefDocuments.ps1 -Record`; the approved route supplies PLAYBOOK_COSMOS_ENDPOINT, PLAYBOOK_COSMOS_KEY, PLAYBOOK_COSMOS_DATABASE and PLAYBOOK_COSMOS_CERT_SHA256 only to the child. Remove only the unique marked disposable database.

2. Review document ownership and partition keys before changing the application. Separate bulk imports from manual session-token clients. Recheck the stored document after each migration step.

## Constraints

Requires EF Cosmos11/net11.0/C#15/SDK11; comparison EF10.0.13/net10.0/C#14. Both pin Cosmos SDK3.63.1 and Newtonsoft.Json13.0.4. Exact assets and requirements are in applicability. Execution uses Windows x64 and the approved legacy Linux emulator2.14.28.0. No renderer, additional workload, Azure account or production migration is needed. Stable API guidance uses RC2 execution evidence; actual GA reconciliation remains pending. Local session handoff does not establish cloud consistency enforcement, multi-region behavior or replica lag. No throughput improvement is measured.

## Complete projects

### .NET 10 comparison project

{{snippet:n10-directory-build-props}}

{{snippet:n10-directory-build-targets}}

{{snippet:n10-directory-packages-props}}

{{snippet:n10-emulatortransport-cs}}

{{snippet:n10-global-json}}

{{snippet:n10-reefdocuments-csproj}}

### .NET 11 comparison project

{{snippet:n11-directory-build-props}}

{{snippet:n11-directory-build-targets}}

{{snippet:n11-directory-packages-props}}

{{snippet:n11-emulatortransport-cs}}

{{snippet:n11-global-json}}

{{snippet:n11-reefdocuments-csproj}}

## Evidence

The .NET 11 SDK baseline is now 11.0.100 GA with preview selection disabled. The .NET 10 comparisons retain their declared SDKs. Earlier RC audit details below are historical; use the current sample pins, dependency locks and GA verification records for current coverage. Existing native exclusions and pending Human gates still apply.

Real EF10 save preserves an unmapped field; EF11 save removes it. Complete-document replacement preserves it and rejects a stale ETag. Exact input hashes, SDK/compiler/dependency identities, artifact hashes and actual output are retained in paired verification records. No mocks or cross-compilation are presented as execution. Installed offline retrieval separately checks full canonical Markdown, applicability and target11/edition11; target10 has no applicable edition.

## References

[Official EF Cosmos guidance](https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-11.0/breaking-changes).

