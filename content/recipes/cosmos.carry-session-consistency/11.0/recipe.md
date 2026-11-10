# Carry a caller reading position between Cosmos instances

## Goal

Propagate owner-scoped Cosmos session tokens when a later request uses an independent client and must observe recent writes.

## Older approach

EF10 can use SDK response session headers and ItemRequestOptions.SessionToken on an independent client. Keep tokens tied to the authenticated owner and container.

{{snippet:n10-program-cs}}

## Newer approach

Keep manual session work on a separate non-bulk client: SDK3.63.1 rejects explicit session request options with AllowBulkExecution. Select Manual session-token mode before building the EF11 context. Copy GetSessionTokens after successful work and call UseSessionTokens before the next instance reads. Use the per-container map. The consumer asserts independent CosmosClient instances. Production code must obtain the owner from authenticated server state, enforce authorization separately and retain tokens within owner/account/database/container scope. Never allow a caller to select another owner or use a global last-token slot. Refresh tokens after subsequent requests; bound their storage and do not log them. Tokens are neither credentials nor query continuation tokens. Cosmos session consistency is a prerequisite; a token cannot strengthen an account with a weaker consistency contract.

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

Actual owner-scoped response tokens cross independent clients and the owner partition reads the recent write. Another owner has no stored token. Local execution does not prove cloud replica lag or consistency enforcement. Exact input hashes, SDK/compiler/dependency identities, artifact hashes and actual output are retained in paired verification records. No mocks or cross-compilation are presented as execution. Installed offline retrieval separately checks full canonical Markdown, applicability and target11/edition11; target10 has no applicable edition.

## References

[Official EF Cosmos guidance](https://learn.microsoft.com/en-us/ef/core/providers/cosmos/saving#session-token-management).



