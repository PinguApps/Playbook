# Treat nested document content as values rather than entities

## Goal

Store nested objects and collections inside a Cosmos document using value semantics when those parts need no independent identity.

## Older approach

EF10 OwnsOne and OwnsMany embed related content in the same document. Owned objects are still tracked entity types with identity. This is a valid model when ownership and entity tracking are intended.

{{snippet:n10-query}}

## Newer approach

EF11 Cosmos supports ComplexProperty and ComplexCollection. The fictional reef manifest has one location and a list of specimens. Nested values have no entity key or separate entity entry; the containing document remains the unit of persistence. Complex values do not automatically acquire CLR record equality or immutability.

{{snippet:n11-query}}

## Migration

1. Copy the complete ReefManifest project below, including its transport, package boundary and SDK pin, into its own directory.
2. Use the pinned SDK: `dotnet restore ReefManifest.csproj --use-lock-file`, then `dotnet build ReefManifest.csproj -c Release -p:RestoreLockedMode=true`.
3. Use the approved isolated local emulator route in docs/cosmos-verification.md to supply PLAYBOOK_COSMOS_ENDPOINT, PLAYBOOK_COSMOS_KEY, PLAYBOOK_COSMOS_DATABASE and PLAYBOOK_COSMOS_CERT_SHA256 only to the child. Never substitute a production account. Repository authors run `eng/Test-EmbeddedValues.ps1 -Record` to create and remove marked databases and save fresh proof.
4. Expect `PASS real Cosmos nested values`, Location.Bay = Lagoon and three Samples with Kelp.Count = 7 in independent JSON read-back. EF11 tracks only the root; EF10 tracks owned entries. Remove your marked database after use and stop the empty emulator. No cloud parity or automatic existing-document migration is implied.

## Complete previous project — .NET 10

global.json

{{snippet:n10-global-json}}

Directory.Build.props

{{snippet:n10-directory-build-props}}

Directory.Build.targets

{{snippet:n10-directory-build-targets}}

Directory.Packages.props

{{snippet:n10-directory-packages-props}}

ReefManifest.csproj

{{snippet:n10-reefmanifest-csproj}}

Program.cs

{{snippet:n10-program-cs}}

EmulatorTransport.cs

{{snippet:n10-emulatortransport-cs}}

## Complete current project — .NET 11

global.json

{{snippet:n11-global-json}}

Directory.Build.props

{{snippet:n11-directory-build-props}}

Directory.Build.targets

{{snippet:n11-directory-build-targets}}

Directory.Packages.props

{{snippet:n11-directory-packages-props}}

ReefManifest.csproj

{{snippet:n11-reefmanifest-csproj}}

Program.cs

{{snippet:n11-program-cs}}

EmulatorTransport.cs

{{snippet:n11-emulatortransport-cs}}

## Constraints

Requires Cosmos EF Core 11 on net11.0. Both Complex types sections in the release inventory describe this one capability and are reconciled here. This local route is the approved legacy Linux NoSQL emulator, not cloud parity. The example replaces Location and edits and extends Samples, clears tracking and reloads, and reads the physical JSON independently. Migrating an existing owned model requires checking stored property names, discriminator/id conventions and partition keys before changing production mappings. No automatic data migration is claimed.

Stable API contract using RC2 evidence, without experimental opt-in; actual GA remains unverified. No web renderer, browser or workload is required. The previous sample is a comparison, not another .NET10 foundation.

## Evidence

Actual isolated Cosmos emulator insert/read/update; independent SDK JSON object and array read-back; root-only EF11 tracking versus EF10 owned entries. Windows x64 consumer; no cloud, performance, other-platform or GA claim. Exact SDKs, dependencies, input hashes and actual output are retained in the paired verification records. Compilation and real execution are distinguished from source review. Offline installed MCP checks use the existing isolated feed/cache harness; unusable proxies demonstrate proxy independence, not operating-system network isolation.
