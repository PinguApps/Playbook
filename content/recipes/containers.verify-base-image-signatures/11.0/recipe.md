# Verify the signature of a selected .NET base image

## Goal

Quay's manifest worker must use the approved Microsoft runtime image, rather than
trust a mutable tag or the mere presence of a signature artifact. Resolve the image
index and intended platform, verify both digests against an explicit signing
identity and strict trust policy, then use that same platform digest in the build.

## Requirements and recorded artifacts

The verification client is Notation **1.3.2**, with ORAS **1.3.6** for registry
discovery and copying test fixtures. Both are independently versioned native tools;
verification itself needs no .NET SDK/runtime. The companion managed worker uses
SDK10.0.402/C#14/ref10.0.13 or SDK11.0.100-rc.2.26504.105/C#15/ref11 RC2, compiled
on Windows x64 and executed on Linux x64. No package, provider account, optional
workload, UI rendering mode, system trust-store change or signing credential is
required. Public MCR/certificate access is required for verification/setup;
installed Playbook guidance is embedded offline. This is not offline revocation
verification or actual-GA qualification.

| Selection | Index digest | Executed linux/amd64 digest |
| --- | --- | --- |
| runtime:10.0.13-azurelinux3.0 | sha256:5e6d01ac0f7860e7cf22ac0b0b5aebb3d72854b2e3dbcb7729a76aab9c1ce32c | sha256:e454ed02b8c903dff0d7fd40a63451e55f16c186463d168a78e584738244f939 |
| runtime:11.0.0-rc.2-azurelinux4.0 | sha256:fef3cd3bd18e61d14dd7e66289e1a537d6a48fdc2d266e009744e299571c6afe | sha256:3b52667dbaff185a1ddd4cefb16d9517e6945f6e61257faa2e57e6040877d95b |

All references above use repository `mcr.microsoft.com/dotnet/runtime`.
Azure Linux4 remains **Beta** under the reviewed official notes; successful
signature verification does not upgrade OS support status. The signature mechanism
has no experimental opt-in; the application/OS selection has its own support gate.

## Older approach

The actual .NET10 image already has signatures. Notation verifies its index and
selected x64 manifest successfully with the same roots and identity. Preserve this
valid older approach. The .NET11 Preview3 source introduced the documented image
signing guidance; the catalogue release label does not mean .NET10 cannot be
verified. No unsigned older-image workaround or ninth .NET10 foundation is added.

## Newer approach

Acquire the Windows amd64 Notation1.3.2 ZIP from
`https://github.com/notaryproject/notation/releases/download/v1.3.2/notation_1.3.2_windows_amd64.zip`.
Check SHA256 **014f25a530eee17520c8e1eb7380e4bd02ff6fc04479a07a890954e3b7ddfdc7**
before extracting to a private directory. Invoke its absolute `notation.exe` path;
do not install shell completion or alter PATH. The official release checksum
manifest was checked against these exact bytes.

Save this as `Initialize-Trust.ps1`. It downloads and pins the actual signing and
timestamping root bytes, installs them only in a fresh Notation directory, and
imports a strict policy restricted to the runtime repository and Microsoft signer.

{{snippet:initialize}}

Save this as `Verify-Base.ps1`. Every verification runs in a child process with
explicit configuration/cache/plugin directories. A nonzero exit stops selection.

{{snippet:verify}}

These scripts require PowerShell7. The SHA256 checks pin public certificate bytes;
they do not replace deliberate review of future certificate rotation. No certificate
is added to Windows, Linux or the user's ordinary Notation configuration.

## Migration

1. Select the application target, OS and architecture first. Install the exact
   verification client above. Optionally acquire ORAS1.3.6 from
   `https://github.com/oras-project/oras/releases/download/v1.3.6/oras_1.3.6_windows_amd64.zip`,
   SHA256 **ec3d19f386d5d2f0892288772d7b242475a05332aafa64b6773378ca771d76d3**.
   Invoke it by absolute path. Neither tool needs a global installation.
2. Resolve the tag once with `oras manifest fetch mcr.microsoft.com/dotnet/runtime:11.0.0-rc.2-azurelinux4.0 --descriptor`.
   Fetch that index by its returned immutable digest. Read the `manifests` entry
   whose platform is linux/amd64; reject an absent or ambiguous selection. Compare
   both values with the reviewed table. Tags can change: a different result needs
   deliberate requalification rather than a blind pin update.
3. Run `./Initialize-Trust.ps1 -Notation <absolute-notation.exe> -Configuration <absolute-new-directory>`.
   Use a different fresh directory for independent policies. The configuration
   contains the pinned roots and `strict` policy; never replace it with `skip`,
   wildcard trust or an unrestricted registry scope to obtain a passing result.
4. Run `./Verify-Base.ps1 -Notation <absolute-notation.exe> -Configuration <same-directory> -Image mcr.microsoft.com/dotnet/runtime@sha256:fef3cd3bd18e61d14dd7e66289e1a537d6a48fdc2d266e009744e299571c6afe`.
   Repeat for the selected platform digest
   `mcr.microsoft.com/dotnet/runtime@sha256:3b52667dbaff185a1ddd4cefb16d9517e6945f6e61257faa2e57e6040877d95b`.
   Require successful verification of **both** before building. Repeat the same
   index/platform procedure for the older selection when maintaining that target.
5. Put the verified platform digest into the actual worker's Dockerfile:

{{snippet:image}}

6. Build and execute the worker using that Dockerfile, as in
   `containers.migrate-azurelinux-base-images`. Record selected digests, verifier
   version, roots, trust policy and result. Reverify when any relevant input or
   trust decision changes. Clean up only the isolated directories created here.

## Constraints

`oras discover` lists referrers; `notation inspect` displays envelopes. Neither
establishes signature validity, trust, signer identity or the selected repository's
policy. The check uses Notation's real `verify`, including its strict checks and
TSA trust. A valid signature establishes provenance under this policy; it does
not prove vulnerability freedom, application compatibility, support or deployment
approval. Do not infer that signing an index automatically verifies a separately
selected platform artifact.

The rejection fixture uses an ephemeral loopback registry and an explicit
`--insecure-registry` exception there only. Public MCR checks use HTTPS. The fixture
first rejects a copy without referrers, then verifies a recursively copied genuine
signature, removes all genuine signature manifests and attaches an altered COSE
envelope. This proves actual missing/tampered signature rejection; it does not
claim malicious-registry isolation, layer scanning or production registry changes.

## Evidence

`eng/Test-QuayWorker.ps1 -Record` compiles both actual consumers and runs them on
the pinned Azure Linux runtime images. `eng/Test-QuaySignatures.ps1 -Record` checks
tool ZIP/certificate identities, resolves index-to-platform membership, discovers
and inspects real signatures, verifies both indexes and both selected manifests,
executes these canonical scripts, and rejects wrong roots and repository scopes.
Its real loopback registry checks missing and tampered envelopes, with a successful
signed-copy control. Sample records retain exact commands, inputs, native identities
and results; the signature record reuses worker evidence only after freshness
validation. Installed stdio list/get verifies full Markdown, canonical snippets,
target11/edition11 and applicability. PIN-959/PIN-963, actual-GA qualification and
unrelated Human gates remain open. No recurring native matrix is added.
