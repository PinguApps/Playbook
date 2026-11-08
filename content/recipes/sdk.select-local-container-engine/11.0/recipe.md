# Choose the local engine for container publishing

## Goal

Keep an SDK container publish directed at the intended local image store. This matters when a workstation has several engines: a successful load into one store does not make the image available in another.

This is theoretical guidance prepared from official sources. Neither comparison has been compiled, and no engine selection or image load has been exercised.

## Older approach

The .NET 10 SDK can publish into Docker or Podman. Explicitly selecting an engine was already useful; it remains a valid choice. The older original dispatch project selects Docker and declares Linux x64 and Arm64 together:

{{snippet:older}}

`PinnedContainerBase` is an input, not an existing image supplied by Playbook. Choose a compatible .NET 10 runtime **index** by digest, with entries for both architectures. No base image digest is invented here.

## Newer approach

SDK 11 can prefer platform-native local tools during automatic discovery: `wslc` on Windows and Apple's `container` CLI on macOS, with Docker and Podman as fallbacks. A machine upgrade can therefore change the default destination. `LocalRegistry` bypasses that discovery; the newer comparison deliberately selects Podman:

{{snippet:newer}}

The accepted property values are `Docker`, `Podman`, `Wslc`, and `MacOSContainer`. The latter two differ from their executable names. Selecting a value does not install its tool, create a VM, authenticate, or prove its multi-architecture support. The separate Podman recipe covers the selected multi-architecture route; this recipe does not claim every engine supports every output shape.

The app is deliberately independent of the engine:

{{snippet:program}}

## Migration

1. Inspect the project and publish profile for `LocalRegistry`, `ContainerRegistry`, `ContainerArchiveOutputPath`, command-line overrides and inherited properties. A remote `ContainerRegistry` or archive destination is a different output path from loading a local engine.
2. Decide which existing image store the consumer uses. Set its supported `LocalRegistry` value in the project or selected publish profile. Leave it absent only when automatic discovery is intentional.
3. Supply `PinnedContainerBase` as a compatible .NET 11 runtime index reference of the form `mcr.microsoft.com/dotnet/runtime@sha256:<actual-index-digest>`. Resolve the digest from the chosen official image; no account or registry publication is part of this recipe.
4. When execution is authorized, invoke the selected SDK's absolute executable from the newer sample directory with `publish -c Release /t:PublishContainer -p:ContainerRegistry= -p:PinnedContainerBase=<reference>`. The sample defaults to Podman; use `-p:LocalRegistry=Docker` for an intended Docker override. These commands have not been run.
5. In a future verification pass, inspect the **same** engine's local image/index store. An image missing from Docker Desktop says nothing about whether it was loaded into Podman. Record the selected tool/version and resulting local reference before claiming success.

## Constraints

`LocalRegistry` names a local tool, not a remote registry hostname. `ContainerRegistry` is the separate remote destination property. An explicit command-line property can override a project value. An unavailable selected engine is a prerequisite failure; silently trying another engine would defeat the decision.

Both samples use SDK-provided container tasks, a framework-dependent console app, and no package references, workloads, provider, rendering mode or Dockerfile. The image needs a compatible runtime base. The sample architecture declarations describe outputs, not machines on which the program has run. Existing Dockerfile-based builds remain appropriate when custom image build steps are required.

The actual base image, engine and manifests are absent from this source-only delivery. The earlier access assessment found Docker but no Podman; Pingu explicitly chose theoretical implementation without obtaining Podman. No installation or live check is requested by consulting this content.

## Evidence and limitations

The canonical .NET10 and .NET11 projects now have actual locked compilation records from Windows, using their declared SDKs and dependencies. The original official-source review is retained separately. Compilation of the rules/decoder library does not compile illustrative .cs.txt host integration or establish behavior.

The pre-GA preview is explicitly provisional. Pending proof remains visible in discovery/detail and blocks stable readiness. The approved preview evidence register names this exact edition, canonical samples, limitations and its final-reconciliation owner; it does not claim successful unperformed checks.

SDK11 explicitly selected Docker, loaded an owned Linux x64 image and ran it with networking disabled; the image was removed. Podman and engine failure/fallback controls remain unverified.
