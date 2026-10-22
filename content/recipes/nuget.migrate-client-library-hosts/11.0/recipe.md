# Retarget a NuGet client-library host

## Goal

Keep a utility that directly uses NuGet client libraries on a compatible host runtime. A depot utility reads the identity of a local manifest package. The utility's target framework determines which NuGet library asset it can load; the frameworks of packages it inspects are a separate concern.

## Older approach

The previous-release comparison is a .NET 10 / C# 14 application using NuGet.Packaging 7.9.0. Its actual selected compile/runtime library is `lib/net8.0/NuGet.Packaging.dll`; a .NET 10 host can use that asset. This valid older approach does not become broken just because a newer library exists.

{{snippet:older-project}}

{{snippet:older-packages}}

## Newer approach

The current .NET 11 / C# 15 comparison uses the actual NuGet.Packaging 8.0.0-rc.605 package. Its selected compile/runtime library is `lib/net10.0/NuGet.Packaging.dll`. Keep these files together; the central package file supplies versions for the versionless project references:

{{snippet:newer-project}}

{{snippet:newer-packages}}

The exact RC package is available from the public dotnet-tools feed, rather than the checked nuget.org index. This sample-local configuration maps NuGet client packages to that feed and ordinary dependencies to nuget.org. Apply equivalent entries within your authorized repository policy; do not overwrite unrelated sources or mappings:

{{snippet:sources}}

Both hosts execute this original utility. It creates and reads a local unsigned fixture, checks the manifest identity, prints the actual host/library versions, and deletes the fixture:

{{snippet:host}}

The client package's modern .NET minimum is **.NET 10**, not .NET 11. A separately compiled and executed .NET 10 host uses the same current package and its net10.0 assets:

{{snippet:compatible-project}}

Use the newer central package file, source configuration and program with this project. This is an explicit conditional current-library route for target 10, not a ninth historical foundation. No C# 15 construct is needed by the utility. The inspected package also contains net472 assets; no .NET Framework route is advertised because that host was not built and run here.

## Migration

1. Identify direct NuGet client-library references in your utility and inspect their exact package versions, target asset groups and transitive dependencies. NuGet package major numbers are independent of .NET release numbers.
2. For these current net10.0 assets, choose a modern host targeting .NET 10 or .NET 11 and install its matching runtime in the environment that executes the utility. Retarget and test the host; do not rely on runtime roll-forward to make an incompatible compile asset valid.
3. Keep the pinned central package file. `System.Security.Cryptography.Pkcs` 8.0.2 is an explicit compatible patch pin: the default 8.0.1 dependency raised NU1902/NU1903. Audit stays enabled; do not suppress warnings to make this sample build. For a new copy, run `dotnet restore <host.csproj>` once to generate `packages.lock.json`, review its exact dependency closure, then retain it with the project. The bundled evidence lists the closure actually tested.
4. Verify the retained graph with `dotnet restore <host.csproj> --locked-mode`, then run `dotnet run --project <host.csproj> -c Release --no-restore`. Initial restore needs the configured feeds or an already populated authorized cache; the demonstration itself performs no network requests.
5. Inspect `obj/project.assets.json`: both `compile` and `runtime` for NuGet.Packaging must select the documented library asset. Exercise the real utility operation and deployment runtime, rather than treating successful package download as compatibility proof.

## Constraints

This affects applications that **directly reference NuGet's own client libraries**. An ordinary application consuming unrelated packages through SDK restore does not need its TFM changed because the SDK's NuGet host changed. Nor does a local package reader need to retarget each package it examines. Package asset compatibility, the host's runtime, SDK selection and the contents being inspected are separate layers.

These examples exercise package identity reading only. They do not establish remote feed authentication, restore APIs, every NuGet client package, signing verification, plugin loading, trimming or Native AOT compatibility. Windows x64 was executed; other supported .NET platforms require their own claim-specific checks when behavior differs. No provider, rendering mode, workload, external account or provisioned resource is required.

The API is not publisher-designated experimental, but the exact current package and SDK evidence is RC2 rather than GA. Reconcile final assets/dependencies at GA. Keep sample pins manual under the dependency policy. No library is copied out of the SDK and represented as a restored package.

## Evidence

Three original hosts compiled and executed: .NET 10 with NuGet.Packaging 7.9.0; .NET 11 with 8.0.0-rc.605; and .NET 10 with 8.0.0-rc.605. SDK pins were 10.0.402 and 11.0.100-rc.2.26504.105. Their current input-hashed records include exact locked transitive packages, compiler/runtime output and the asserted manifest identity. The behavior script also checks actual resolved compile/runtime asset paths: net8.0 for the older library and net10.0 for both current-library hosts.

These are real restored package consumers and local archive reads, not source review, mocks or cross-compilation. No net8 host, .NET Framework, native/mobile, remote feed operation or GA execution is claimed. Packed stdio tests separately check complete offline guidance and target-appropriate applicability, including the conditional .NET 10 route.
