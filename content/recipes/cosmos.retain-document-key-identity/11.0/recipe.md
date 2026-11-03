# Keep existing Cosmos documents reachable after key escaping changes

## Goal

Audit composite and discriminator IDs before upgrading; use a bounded compatibility switch while planning collision-safe IDs.

## Older approach

The EF10 comparison uses the same original reef labels and actual provider. It preserves historical behavior rather than inventing a workaround.

{{snippet:n10-program-cs}}

## Newer approach

EF11 no longer escapes /, \, ? or # in generated composite/discriminator IDs. The separator | is still escaped within a component. Old slash-containing keys can therefore address a different ID after upgrade. The process-wide switch restores the historical mapping; it also restores its collision defect: a slash and the literal ^2F can address one document. Never enable it as a permanent collision fix.

{{snippet:n11-program-cs}}

## Migration

1. Inventory actual stored IDs, logical key components, discriminator-in-ID configuration and complete partition keys. Back up the affected data before migration. A single mapped id with no concatenation is outside this escaping change.

2. If immediate compatibility is necessary, enable the switch at application startup before creating any Cosmos model/context. The sample uses a separate child process to verify default EF11 behavior without leaking that process-wide choice. Restrict writes during this bounded transition and audit ambiguous legacy escape sequences. A missing logical record cannot be recovered by guessing whether ^2F originally represented a slash.

3. Prefer legal, unambiguous key components for new data. A data migration must create a document under a new legal ID, verify its payload and partition, update references and only then remove the old document. Cosmos IDs cannot be renamed in place. Design restart/duplicate handling and rollback before operating on production data; this sample does not run that production migration.

4. Run the paired runner against disposable data first. Remove the switch only after all readers, writers and stored references use the new identity scheme.

## Constraints

EF Cosmos11 requires net11.0, SDK11 and C#15 in this sample; the EF10 comparison uses net10.0, SDK10 and C#14. Exact pins are below and in applicability. Stable API guidance uses RC3 evidence, not GA verification. Windows x64 and the approved legacy Linux emulator only; no browser, renderer, cloud parity or performance claim.

## Complete projects

### .NET 10 comparison project

{{snippet:n10-directory-build-props}}

{{snippet:n10-directory-build-targets}}

{{snippet:n10-directory-packages-props}}

{{snippet:n10-emulatortransport-cs}}

{{snippet:n10-global-json}}

{{snippet:n10-reefidentity-csproj}}

### .NET 11 comparison project

{{snippet:n11-directory-build-props}}

{{snippet:n11-directory-build-targets}}

{{snippet:n11-directory-packages-props}}

{{snippet:n11-emulatortransport-cs}}

{{snippet:n11-global-json}}

{{snippet:n11-reefidentity-csproj}}

## Evidence

Paired exact-artifact records retain actual SDKs, dependencies, input hashes, compilation and local-emulator execution. They cover historical composite IDs, compatibility/collision checks and explicit undefined filtering/fallback versus JSON null. The EF11 consumer additionally checks default reachability and a legal new composite ID in a separate process.

The shared consumer also records projection observations for `cosmos.handle-missing-projection`. Its documented undefined-exception contract requires a real GA retest; that separate limitation does not weaken the identity checks above. Actual GA reconciliation of this identity guidance also remains pending.

No mocks, cloud service parity, production data migration, performance result or GA execution is claimed. Installed offline retrieval separately checks canonical full Markdown, applicability and target11/edition11; older comparison samples add no target10 edition.

## References

[EF11 breaking changes](https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-11.0/breaking-changes).
