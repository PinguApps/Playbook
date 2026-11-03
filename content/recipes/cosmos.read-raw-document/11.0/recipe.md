# Read complete Cosmos JSON through an explicit document client

## Goal

Replace removed __jObject access when a Cosmos workflow needs complete raw JSON and explicit write ownership.

## Older approach

EF10 exposes JObject shadow state after loading a tracked entity. The consumer reads actual __jObject data; this is historical comparison, not an EF11 compatibility route.

{{snippet:n10-program-cs}}

## Newer approach

EF11 uses System.Text.Json internally and removes __jObject. Use CosmosClient with the database, container, ID and complete partition key. Parse the raw stream into JsonNode and dispose responses and streams. This does not synchronize tracked entities. The sample owns complete JSON replacement, preserves unknown fields and uses IfMatchEtag. Prefer mapped EF queries when raw ownership is unnecessary. Never reconstruct complete replacement JSON from an incomplete DTO.

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

Actual EF10 shadow JSON read, EF11 absence, independent raw-document read/write, unknown-field preservation and stale ETag rejection. Exact input hashes, SDK/compiler/dependency identities, artifact hashes and actual output are retained in paired verification records. No mocks or cross-compilation are presented as execution. Installed offline retrieval separately checks full canonical Markdown, applicability and target11/edition11; target10 has no applicable edition.

## References

[Official EF Cosmos guidance](https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-11.0/breaking-changes).

