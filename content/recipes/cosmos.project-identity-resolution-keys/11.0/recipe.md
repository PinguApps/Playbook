# Keep keys in Cosmos identity-resolution projections

## Goal

Keep keys in Cosmos identity-resolution projections when upgrading database tooling or query code; compare a working older route and verify the actual result.

## Older approach

The independently compiled .NET10 consumer retains the working older route described in the migration steps. Read its limitations before replacing it.

## Newer approach

The .NET11 consumer uses the current contract with explicit prerequisites and controls.

{{snippet:n11-samples-net11-0-avocetidentity-avocetidentity-csproj}}

## Migration

1. Identify every primary-key component of the root document. In Avocet logbooks, Id and Coast form the key; Coast is also the partition key. Equal owned values in different roots are not the same entity.
2. Keep all those components in an EF11 owned-entity projection using AsNoTrackingWithIdentityResolution. Select Id, Coast and the owned Detail together. The omitted-key control is rejected with a diagnostic naming the missing root key properties.
3. Materialize and verify both values and identity. The current sample projects Detail twice and checks ReferenceEquals for each root. Stored ringed/unringed documents remain distinct; identity resolution does not merge documents by matching content.
4. On the measured EF10 graph, the omitted-key projection works, but the keyed duplicate projection produces null owned values. The retained negative control checks that observation. Load the root documents with identity resolution first, then project their owned values in memory. This trades a larger document result for a working older route and is explicitly client-side projection.
5. Reopen data through a fresh context before judging persistence. Query identity resolution is temporary and does not attach the results to the context for later SaveChanges. Preserve root/partition identity when issuing subsequent reads or writes.
6. Use the approved isolated emulator route and unique owned databases. Cloud parity, distributed consistency, restart durability, complex-value mappings and raw/unmapped-field round trips are separate claims.

## Constraints

Original Windows x64 comparisons: SDK10.0.403/C#14/EF10.0.13 versus SDK11.0.100-rc.3.26522.148/C#15. EF11 tooling uses RC2; Cosmos uses RC3. EF11 requires net11.0; this is not an older-runtime EF11 route or GA proof. No language-preview opt-in. Console/database guidance has no rendering mode. Keep exact locks; restore requires network access. The installed guidance is offline. No additional .NET10 foundation or recurring platform matrix is introduced.

Cosmos SDK3.63.1/Newtonsoft.Json13.0.4 and the approved digest-pinned legacy emulator are external prerequisites. Use certificate-pinned loopback HTTPS without changing host trust. Database cleanup is mandatory; do not use production accounts.

## samples/net10.0/AvocetIdentity/AvocetIdentity.csproj

{{snippet:n10-samples-net10-0-avocetidentity-avocetidentity-csproj}}

## samples/net10.0/AvocetIdentity/Directory.Build.props

{{snippet:n10-samples-net10-0-avocetidentity-directory-build-props}}

## samples/net10.0/AvocetIdentity/Directory.Build.targets

{{snippet:n10-samples-net10-0-avocetidentity-directory-build-targets}}

## samples/net10.0/AvocetIdentity/Directory.Packages.props

{{snippet:n10-samples-net10-0-avocetidentity-directory-packages-props}}

## samples/net10.0/AvocetIdentity/Program.cs

{{snippet:n10-samples-net10-0-avocetidentity-program-cs}}

## samples/net11.0/AvocetIdentity/AvocetIdentity.csproj

{{snippet:n11-samples-net11-0-avocetidentity-avocetidentity-csproj}}

## samples/net11.0/AvocetIdentity/Directory.Build.props

{{snippet:n11-samples-net11-0-avocetidentity-directory-build-props}}

## samples/net11.0/AvocetIdentity/Directory.Build.targets

{{snippet:n11-samples-net11-0-avocetidentity-directory-build-targets}}

## samples/net11.0/AvocetIdentity/Directory.Packages.props

{{snippet:n11-samples-net11-0-avocetidentity-directory-packages-props}}

## samples/net11.0/AvocetIdentity/Program.cs

{{snippet:n11-samples-net11-0-avocetidentity-program-cs}}


## Evidence

Independent compilation uses each SDK pin and lock. Focused SQLite tooling and Cosmos emulator checks are recorded separately; compilation alone does not establish database behavior. Actual GA and other platforms remain unverified.
