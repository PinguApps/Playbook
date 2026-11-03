# Make the scope of a multi-document save explicit

## Goal

Choose Cosmos save transaction behavior when several document changes must succeed together within one container and partition.

## Older approach

Use an explicit Cosmos SDK transactional batch when atomic writes are required. EF10 SaveChanges writes documents individually; setting the relational-looking transaction property does not add Cosmos transactional batches.

{{snippet:n10-program-cs}}

## Newer approach

The documentation calls the default Auto; the pinned enum spells it WhenNeeded. It batches compatible operations, stops after failure, and cannot undo earlier successful batches. Never writes individually; a later failure can leave earlier writes committed. Always requires the entire save to fit one batch and rejects incompatible partition/container boundaries. A batch has at most 100 operations and 2 MB; triggers and service execution limits also apply. Design retries around the actual committed scope, not the DbContext lifetime.

{{snippet:n11-program-cs}}

## Migration

1. Copy every project file below into separate previous/current-release directories. Restore with the pinned SDK using `dotnet restore --use-lock-file`, then `dotnet build -c Release -p:RestoreLockedMode=true`.
2. Use the approved local emulator route to supply PLAYBOOK_COSMOS_ENDPOINT, PLAYBOOK_COSMOS_KEY, PLAYBOOK_COSMOS_DATABASE and PLAYBOOK_COSMOS_CERT_SHA256 only to the sample child. Run `eng/Test-CosmosPolicies.ps1 -Record` in this repository.
3. Inspect the actual policy or persisted document counts before adopting the configuration. Preserve existing container identity, partition keys and stored names. EnsureCreated does not alter an existing container policy; plan existing-container changes separately.
4. Remove only your marked disposable database. No production migration, cloud provisioning or package publication is part of this example.

## Constraints

Requires EF Cosmos11 on net11.0, C#15 mode, SDK11; the comparison uses EF10.0.13/net10.0/C#14. Exact RC2 packages, SDKs and transport are shown below. The SDK3.63.1 and Newtonsoft.Json13.0.4 dependencies and selected assets are recorded in metadata and lock files. Windows x64 execution with the approved Linux legacy emulator2.14.28.0 is the checked provider route. No web renderer, browser or additional workload is needed. API configuration is prepared from RC evidence; no GA or cloud parity claim. Vector/full-text service capabilities must be enabled on the destination account; accepting a policy proves neither query behavior nor performance.

## Complete projects

### .NET 10 comparison project

{{snippet:n10-directory-build-props}}

{{snippet:n10-directory-build-targets}}

{{snippet:n10-directory-packages-props}}

{{snippet:n10-emulatortransport-cs}}

{{snippet:n10-global-json}}

{{snippet:n10-harboursaves-csproj}}

### .NET 11 comparison project

{{snippet:n11-directory-build-props}}

{{snippet:n11-directory-build-targets}}

{{snippet:n11-directory-packages-props}}

{{snippet:n11-emulatortransport-cs}}

{{snippet:n11-global-json}}

{{snippet:n11-harboursaves-csproj}}

## Evidence

Actual same-partition save success and conflict rollback, cross-partition and cross-container mode outcomes; previous-release explicit SDK batch success and conflict rollback. No global transaction claim. Exact SDK/package pins, input hashes and actual execution output are retained in each sample's verification evidence. RC2 is not GA proof. The isolated installed-tool harness must retrieve full Markdown and applicability offline; proxy blocking does not establish OS network isolation.

## References

[Official EF Cosmos documentation](https://learn.microsoft.com/en-us/ef/core/providers/cosmos/saving).


