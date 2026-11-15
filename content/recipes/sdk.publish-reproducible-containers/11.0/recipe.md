# Publish repeatable container images from fixed inputs

## Goal

Make an image digest a useful identity for a source snapshot. Two independent container publishes can only be compared meaningfully when the source, compiler, dependencies, base image and authored image configuration are held constant.

This provisional walkthrough retains documented GA intent. Consult the evidence section for actual local observations and the remaining unverified behavior.

## Older approach

SDK 10 can publish the original dispatch app using a pinned base image and deterministic managed compilation. Those choices help control inputs; they do not establish the newer SDK's container timestamp/header/order behavior:

{{snippet:older}}

An existing controlled Dockerfile or external image pipeline can remain suitable. There is no fabricated SDK-10 equivalent of the SDK-11 reproducibility changes here, and ordinary earlier container publishing is not described as broken. The older sample solves the packaging goal without claiming repeatable container digests.

## Newer approach

SDK 11 documents `SOURCE_DATE_EPOCH` as the stable Unix-seconds input for its reproducible container publishing path. The original sample uses one fixed source-snapshot time rather than the wall clock:

{{snippet:newer}}

The exact epoch is a policy input. It must stay unchanged when comparing unchanged inputs; it is not an SDK version or a build number. The sample receives an immutable runtime image **index** through `PinnedContainerBase`, so its Linux x64 and Arm64 selections can both resolve beneath one recorded identity.

The managed payload is intentionally simple:

{{snippet:program}}

## Migration

1. Capture an input manifest: source revision and dirty state, exact SDK, project and publish properties, resolved dependencies, RID set, runtime base index and selected child digests, labels, environment, entry point and image format. Include generated files and any values injected by CI.
2. Resolve a real compatible runtime index digest from the selected official base image. Supply it as `PinnedContainerBase`. A `:11.0` or `:latest` tag can move and is insufficient for a repeatability claim. The sample intentionally supplies no made-up digest.
3. Choose a stable `SOURCE_DATE_EPOCH`, such as the approved source-snapshot timestamp, and pass the identical value to both independent SDK-11 publishes. `-p:SOURCE_DATE_EPOCH=1792454400` is the explicit property form matching this sample. Do not derive it from the current time.
4. When execution is authorized, publish twice from clean, equivalent inputs using the selected SDK and engine. From the newer sample directory the candidate command is `publish -c Release /t:PublishContainer -p:ContainerRegistry= -p:PinnedContainerBase=<reference>`. Use distinct retained artifacts/stores if needed to avoid mistaking a reused result for a rebuild. Two local archive-output publishes were run with these fixed inputs; this route does not load Docker or Podman. The default Podman command remains unverified.
5. Compare the actual per-platform manifest/config/layer digests and the image-index digest using the same format and metadata policy. A changed tag name, index annotation or platform set can change the index even when a layer is identical. Record the actual objects compared and the engine versions.
6. Treat NuGet package reproducibility as a separate check. Identical assembly bytes or normalized `.nupkg` ZIP timestamps do not prove identical container tar headers, config timestamps or manifests; identical container digests do not prove identical `.nupkg` files either.

## Constraints

`SOURCE_DATE_EPOCH` does not freeze mutable base tags, package ranges, generated content, signing, clock-dependent application data or external build tools. Preserve the full inputs before attributing a mismatch to the SDK. Runtime execution is a separate question from artifact identity.

The SDK's remote manifest-existence optimization is separate from reproducible image construction. `ContainerPushNoCache=true` bypasses that manifest-level shortcut; existing blob checks still apply. It is not a general cache cleaner, a reproducibility switch or proof of faster builds. This source-only recipe neither provisions a registry nor authorizes a remote push.

The sample's `LocalRegistry=Podman` is an explicit destination choice, not a requirement of the timestamp feature. Docker can be selected deliberately. The host must support the chosen SDK and engine; both requested Linux architectures need compatible base/runtime assets. No extra NuGet package, provider, UI rendering mode or workload is declared.

## Evidence and limitations

The .NET 11 SDK baseline is now 11.0.100 GA with preview selection disabled. The .NET 10 comparisons retain their declared SDKs. Use the current sample pins, dependency locks and GA verification records for current coverage. The sample-specific limitations below remain; this edition has no pending Human proof gate.

The canonical .NET10 and .NET11 projects now have actual locked compilation records from Windows, using their declared SDKs and dependencies. The original official-source review is retained separately. Compilation alone does not compare image indexes, manifests/configs/layers or archive bytes across independent publishes.

The existing-format GA execution receipt resolves the bounded archive-reproducibility gate. It does not certify engine loading or global stable readiness.

Historical checkpoint: two earlier SDK11 publishes produced matching OCI image-index identities and19 regular-entry content hashes, with different tar-file hashes. That observation retains its original inputs and does not certify the current candidate.


PIN-967 qualification: GA SDK11.0.100 produced two local x64/Arm64 OCI archives with fixed SOURCE_DATE_EPOCH and pinned runtime index; all file entries and content-addressed blobs match after clean. Archive output does not exercise Docker/Podman loading, image execution or registry pushes. The measured runtime index is sha256:3fe5f5495141c97986a49d3a4e76061f6225c3f93f94e8a7e522781376193863; the produced image-index blob is sha256:22c084002b3f98c9d7a3e8ee06da52465cc68a1fea03804681a3503f0124c635. The 21 tar file entries and 19 blobs match; tar-header identity is not asserted.
