# Provision only the document indexes the workload needs

## Goal

Choose paths and specialized Cosmos indexes from the queries your container must support.

## Older approach

Provision the container policy explicitly through the Cosmos data-plane API. This remains valid when infrastructure owns container creation. EF10 already supports vector and full-text model configuration. The comparison keeps provisioning outside EF; EF11 adds the automatic/explicit-path policy controls shown below. A more specific included path overrides the /* root exclusion, retaining Title while omitting unrelated scalar paths.

{{snippet:n10-program-cs}}

## Newer approach

EF11 builds the policy for a new container from its model. HasAutomaticIndexing(false) selects explicit model paths; the resulting service policy still has automatic=true so new writes maintain those selected indexes. HasIndex creates a path index or a composite for multiple properties. IsDescending chooses composite directions. Specialized vector/full-text definitions need matching property configuration and supported account capabilities. Location.Bay becomes /Location/Bay/?; Shelves[].Bay becomes /Shelves/[]/Bay/?. These are JSON paths, so inspect mapped names before provisioning. Use HasAutomaticIndexing().Except(path) for an include-most policy. Do not add vector or full-text indexes unless the workload needs them.

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

{{snippet:n10-harbourindexes-csproj}}

### .NET 11 comparison project

{{snippet:n11-directory-build-props}}

{{snippet:n11-directory-build-targets}}

{{snippet:n11-directory-packages-props}}

{{snippet:n11-emulatortransport-cs}}

{{snippet:n11-global-json}}

{{snippet:n11-harbourindexes-csproj}}

## Evidence

Actual new-container policy creation and independent policy read-back, including nested object/collection paths; no index query execution or performance claim. Exact SDK/package pins, input hashes and actual execution output are retained in each sample's verification evidence. RC2 is not GA proof. The isolated installed-tool harness must retrieve full Markdown and applicability offline; proxy blocking does not establish OS network isolation.

## References

[Official EF Cosmos documentation](https://learn.microsoft.com/en-us/ef/core/providers/cosmos/modeling).
