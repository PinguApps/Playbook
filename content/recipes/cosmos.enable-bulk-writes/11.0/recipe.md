# Import many Cosmos documents with explicit bulk execution

## Goal

Enable bulk writes for Cosmos ingestion when individual failures and partial persistence are acceptable, without assuming a throughput gain.

## Older approach

EF10 has no EF bulk switch. Use SDK AllowBulkExecution and concurrent asynchronous item operations. This remains a valid direct-document approach.

{{snippet:n10-program-cs}}

## Newer approach

The documentation currently spells the switch BulkExecutionEnabled; the pinned RC2 assembly exposes BulkExecutionAllowed. The compiled sample uses the actual pinned API. EF11 BulkExecutionAllowed opts in. Set AutoTransactionBehavior.Never explicitly to use individual writes rather than transactional batches. Await completion, inspect failures and persisted data before retrying. Bulk uses fully automatic SDK session management in this baseline. The actual SDK rejects explicit per-request session options with bulk execution, so the sample uses a separate non-bulk manual-token context. Bound production concurrency. This eight-document check measures correctness, not throughput. Bulk scheduling does not make an import atomic.

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

Eight opted-in writes persist; a duplicate fails and leaves the original value intact. No throughput, RU or atomicity claim. Exact input hashes, SDK/compiler/dependency identities, artifact hashes and actual output are retained in paired verification records. No mocks or cross-compilation are presented as execution. Installed offline retrieval separately checks full canonical Markdown, applicability and target11/edition11; target10 has no applicable edition.

## References

[Official EF Cosmos guidance](https://learn.microsoft.com/en-us/ef/core/providers/cosmos/saving).



