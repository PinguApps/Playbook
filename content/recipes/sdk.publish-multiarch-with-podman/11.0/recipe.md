# Build a multi-architecture image with Podman

## Goal

Offer one image reference to Linux x64 and Arm64 consumers, with a matching image for each architecture. Use Podman when that is the consumer's chosen local engine, rather than assuming Docker's image store.

This is a source-reviewed implementation plan. Podman is not installed for this work, and no local images, manifests or running containers have been created from the samples.

## Older approach

The SDK-10 comparison chooses Docker for its multi-architecture container path. Docker remains a valid engine after upgrading; migration is not required merely because a second engine is available:

{{snippet:older}}

This is not evidence of an older SDK supporting the new Podman multi-architecture path. Separate single-architecture Podman behavior, if used by an existing pipeline, must not be conflated with combined manifest/index support.

## Newer approach

SDK 11 documents multi-architecture publishing through Podman. The newer project declares the complete RID set and selects Podman explicitly:

{{snippet:newer}}

`ContainerRuntimeIdentifiers` is the output subset of `RuntimeIdentifiers`; here both are `linux-x64;linux-arm64`. Do not add `RuntimeIdentifier=linux-x64` when asking for the combined output: a single-RID selection changes the publishing path. These are Linux targets, not Windows container images.

Both architecture images carry the same original managed dispatch behavior:

{{snippet:program}}

## Migration

1. Confirm that an existing SDK-11 installation and a supported Podman installation/connection are available in the environment where publishing will run. Inventory its version and local manifest-store support. This recipe does not install them or create a Podman machine.
2. Choose a compatible .NET 11 runtime base **index** containing Linux `amd64` and `arm64` manifests. Record its actual digest and supply `PinnedContainerBase=mcr.microsoft.com/dotnet/runtime@sha256:<actual-index-digest>`. One platform's manifest digest cannot stand in for the multi-platform base index.
3. Preserve the two RID lists and `LocalRegistry=Podman`. Restore the declared targets with the selected SDK; a native dependency may need different packages/assets per RID. Keep `ContainerRegistry` empty for this local workflow, and do not set `ContainerArchiveOutputPath` when the intended result is a local Podman load.
4. When execution is authorized, run the selected SDK's absolute executable from the newer sample directory with `publish -c Release /t:PublishContainer -p:ContainerRegistry= -p:PinnedContainerBase=<reference>`. This candidate command is unrun. The older comparison uses its own SDK-10 pin and a matching .NET 10 base index.
5. In future verification, use the selected Podman executable's `manifest inspect` against the actual local reference emitted by the SDK. Expect a manifest list/image index with separate `linux/amd64` and `linux/arm64` descriptors and retain their actual child digests. A single image's `inspect` output is insufficient to establish an index.
6. Verify each child image's runtime/assets and execute on appropriate hosts only if a runtime claim is required. An index descriptor and successful cross-publish do not prove Arm64 execution. Emulation, native execution and cross-compilation are different evidence categories.

## Constraints

An OCI image index or Docker manifest list describes several images; it does not combine their executable files into one architecture-independent filesystem. Consumers select a platform entry. `linux-x64` maps to OCI architecture `amd64`, while `linux-arm64` maps to `arm64`.

Engine support, index format, base-image platforms, runtime packs and registry compatibility are independent prerequisites. SDK publishing of this managed app is not a Dockerfile `RUN` build and does not prove arbitrary native toolchains can cross-build. The example needs no workload, extra package, provider or rendering mode.

The explicit request for theoretical implementation superseded this task's requirement to obtain a live Podman route. It did not supply missing artifacts or authorize provisioning, remote pushes or live checks. No older-target route is asserted from documentation alone.

## Evidence

Official SDK documentation and the local-engine property contract/source were reviewed. Both original project versions and their shared program are canonical sample inputs; their declared SDK/language versions are intended unbuilt pins. All saved records are `official-review`.

Compilation, architecture-specific publication, Podman invocation, manifest creation/inspection, executable behavior and installed offline MCP retrieval remain unverified. Pending proof is carried into discovery/detail and blocks release-ready packaging. No mock stands in for Podman, and no Docker result is presented as Podman evidence. The edition targets 11 with an older .NET 10 comparison and an explicit RC-to-GA limitation.
