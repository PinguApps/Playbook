# Migrate an authored Azure Linux base-image selection

## Goal

Quay's manifest worker budgets compressed shipping batches and formats a French
fee. Move its authored runtime image from Azure Linux3 to Azure Linux4 deliberately,
checking the OS, runtime, native zlib call and ICU-backed culture on the actual
image. A smaller base layer or changed AOT compiler alone supplies none of that proof.

## Requirements and recorded artifacts

This recipe selects the ordinary **managed runtime** variant on Linux amd64.
Azure Linux4 is **Beta** in the pinned Preview6 notes; this edition retains that
status. It is a preparation route with actual RC2 execution, not an assertion of
GA OS support or production approval. NativeAOT, distroless-extra/composite,
ARM64, GCC/Clang changes and image-size/performance comparisons are outside this
bounded selection and require their own actual artifact/runtime checks.

| Layer | Older | Current |
| --- | --- | --- |
| SDK / C# compiler / language | 10.0.402 / 5.9.0-1.26505.111 / 14.0 | 11.0.100-rc.2.26504.105 / 5.12.0-1.26504.105 / 15.0 |
| Framework / reference pack | net10.0 / 10.0.13 | net11.0 / 11.0.0-rc.2.26504.105 |
| Runtime image tag | 10.0.13-azurelinux3.0 | 11.0.0-rc.2-azurelinux4.0 |
| Runtime / OS | .NET10.0.13 / Azure Linux3.0 | .NET11.0.0-rc.2.26504.105 / Azure Linux4.0 Beta |

Compilation uses the exact Windows x64 SDK/compiler/reference assets pinned in
the sample record. The compiled portable IL is then executed on the actual Linux
x64 runtime; cross-compilation alone is not reported as execution. There are no
independent NuGet packages, provider accounts, optional workloads or rendering
modes. The app requires glibc, zlib (`libz.so.1`, C ABI `compressBound`/`zlibVersion`)
and non-invariant ICU globalization supplied by these selected images. No native
compiler or preview-language switch is needed. PowerShell7 and a working Linux
amd64 Docker engine reproduce the authoring checks without modifying host PATH.

## Older approach

The .NET10 worker already runs on Azure Linux3 with the required native and
globalization dependencies. Keep this valid route for that target. Save the
older `QuayWorker.csproj`:

{{snippet:project-10}}

Use this older `Dockerfile`, whose platform digest is immutable even if the
descriptive tag later moves:

{{snippet:image-10}}

## Newer approach

For the new target, save this `QuayWorker.csproj` and matching `Dockerfile`:

{{snippet:project-11}}

{{snippet:image-11}}

Both projects use the same original `Program.cs`. It checks OS selection, calls
the real native zlib export, formats the fee with fr-FR and reports the actual
loaded library/runtime paths and SHA256 hashes. It rejects a missing native
library with a deliberate diagnostic and exit23.

{{snippet:worker}}

In a standalone copy, add `OutputType=Exe`, `ImplicitUsings=enable` and
`Nullable=enable` to each project; those three settings come from the repository's
sample boundary here. Save `global.json` beside each project with
`{"sdk":{"version":"10.0.402","rollForward":"disable"}}` for the older directory,
or `{"sdk":{"version":"11.0.100-rc.2.26504.105","rollForward":"disable"}}` for the
current directory. Use the matching complete SDK executable. The required source
and configuration are provided above; no generated sample or remote guide is needed.

## Migration

1. Inventory the authored base tag, application target, architecture and required
   native dependencies. Select the managed `runtime` variant explicitly. Do not
   substitute an AOT SDK image for the runtime or infer ICU/native compatibility
   from a successful managed build.
2. Resolve and verify the chosen image index and platform signatures using
   `containers.verify-base-image-signatures`. The older index is
   `sha256:5e6d01ac0f7860e7cf22ac0b0b5aebb3d72854b2e3dbcb7729a76aab9c1ce32c`;
   the current index is `sha256:fef3cd3bd18e61d14dd7e66289e1a537d6a48fdc2d266e009744e299571c6afe`.
   Their selected linux/amd64 children are pinned in the Dockerfiles. Verification
   does not by itself establish OS support or application behaviour.
3. Publish each matching project into a clean build context's `publish` directory:
   `dotnet publish QuayWorker.csproj -c Release -o <context>/publish -p:UseSharedCompilation=false`.
   Copy its Dockerfile into that context. These framework-dependent DLLs need no
   host-specific apphost and no package dependency. Use the selected SDK directory
   and do not reuse an old publish output after changing the target or dependencies.
4. Build the current image with
   `docker build --platform linux/amd64 --network none -t quay-worker:check <context>`.
   Image acquisition is online preparation; `--network none` constrains build
   steps. Run `docker run --rm --network none --read-only quay-worker:check`.
   Expect Azure Linux4.0, the exact .NET11 RC2 runtime, architecture X64,
   fr-FR fee `1234,50`, a zlib version and a compression budget above4096.
   The Dockerfile uses the image's non-root APP_UID. Compare the older image's
   equivalent output, which must report Azure Linux3.0 and .NET10.0.13.
5. Run the missing-library control:
   `docker run --rm --network none --read-only -e QUAY_ZLIB=libquay_missing.so quay-worker:check`.
   Expect `QUAY_MISSING_NATIVE:libquay_missing.so` and exit23. This deliberately
   selects an absent application dependency; it does not uninstall zlib or change
   the base image. A native loader failure must fail the worker rather than become
   a successful empty batch.
6. Replace the authored base selection only after these checks pass and the
   applicable Beta/support decision is made. Preserve the older target's pinned
   Dockerfile as its valid comparison. Remove only the disposable images/contexts
   created for this check; no registry push or deployment is required.

## Constraints

The native call measures zlib's compression-bound calculation; it does not claim
an actual compression round trip, throughput gain or ABI compatibility for arbitrary
libraries. Actual loaded files are hashed separately from SDK/reference assets.
ICU-backed formatting is checked with globalization invariant mode disabled; an
invariant or ICU-free variant cannot be assumed equivalent. This example uses the
images' supplied libraries and adds no package-manager installation workaround.

The .NET11 recipe edition advertises only target11; the real target10 comparison
adds no ninth historical foundation or general newer-SDK-on-older-target route.
Changing a tag string alone cannot prove the authored application works. Do not
relabel these exact Beta/RC artifacts as actual-GA evidence.

## Evidence

`eng/Test-QuayWorker.ps1 -Record` checks exact SDK/native-host/compiler/reference
hashes, builds both real projects, compares Dockerfile pins with the reviewed
artifact manifest, builds actual images and runs them on Linux x64 with network
disabled, a read-only root and the authored non-root user. It asserts OS/runtime,
architecture, native exports/calls, culture and missing-library failure. Loaded
glibc/zlib/ICU/CoreCLR identities and exact commands/inputs are saved through the
existing sample-evidence workflow. No source-only or mock execution substitutes
for these checks. Installed offline stdio list/get checks complete canonical
Markdown, target/edition, applicability and evidence. PIN-959/PIN-963, actual-GA
reconciliation and unrelated Human gates stay open; no recurring native matrix
or public publication is added.
