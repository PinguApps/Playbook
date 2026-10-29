# Upload packages to a V3 staging endpoint

## Goal

Upload an original package and its symbols to an advertised staging service when
a publisher needs an intermediate upload step before final publication. Aster
Foundry groups its bronze batch uploads with `groupId`. Staging is separate from
publication: these two uploads neither publish a package nor form a transaction.
If the second fails, the first can already exist. The service owns completion,
retention and cleanup; this client offers no commit, rollback or idempotency API.

## Actual artifacts and prerequisites

This edition qualifies a **public SDK-bundled API**, using the complete exact SDK
installation. It does not recommend a nonexistent NuGet.Protocol 8 package.
The public nuget.org version index observed on 29 October 2026 ended at **7.9.0**;
that independently distributed stable package is not the older SDK's prerelease
DLL. No independently distributed package is restored by either publisher.

| Input | Older comparison | Current route |
| --- | --- | --- |
| SDK / product source | 10.0.402 / `a8dd0577c5c13f43a2f57fe33b4ff11b90f6371e` | 11.0.100-rc.2.26504.105 / `820230c386db5b0106c5430b3e5aee92a7826bf5` |
| Runtime and reference pack | 10.0.13 | 11.0.0-rc.2.26504.105 |
| Application / language | net10.0 / C#14 | net11.0 / C#15 |
| Bundled NuGet product | 7.9.0-rc.611 | 8.0.0-rc.605 |
| Actual NuGet DLL target framework | net8.0 | net10.0 |
| NuGet.Protocol DLL SHA-256 | `edb9d5c446900e286c7a792e41d57a6b71e0aa2ce44a2e06f091c5f3e92a52c0` | `8d85c73d7acc85bf09345ffa46ab5f9b558b1ef246097f312b5f688db95bea65` |
| Transport evidence | Actual ordinary publication to local fixture | Actual staging to local fixture |

The DLL's target framework is an artifact fact, not a tested older-target route
or an SDK/package support policy. Only the exact net11 consumer is advertised for
staging. No package-major-to-.NET-major inference is used. The runtime binding
uses the same installation as the executable `dotnet` host; launch its absolute
path, never an apphost from a different installation.

The compatible SDK closure comprises NuGet.Protocol, Packaging, Common,
Configuration, Versioning and Frameworks, Newtonsoft.Json and the SDK's
System.Security.Cryptography.Pkcs/ProtectedData assets. Their dependency graph
comes from the actual `NuGet.CommandLine.XPlat.deps.json`. The sample pins each
file/product/hash plus that manifest, Roslyn compiler, runtime networking/JSON,
reference and native host files in `samples/net11.0/FoundryPublisher/Shared/artifact-pins.json`.
Its six audited NuGet hashes per SDK match PIN-707's actual asset report. Loaded
assembly identities, TFMs, product builds and hashes are recorded separately from
package dependencies (which are empty). Do not mix one DLL into another SDK or
replace the closure with a similarly numbered package.

Supported prerequisites for this bounded example are the complete pinned SDKs,
their matching runtimes/reference packs, Windows x64 and PowerShell7. No optional
workload, native compiler, device, UI/rendering mode or account is needed. The
provider must actually advertise `PackageStaging/1.0.0`; endpoint ownership,
authorization and hosted interoperability require that provider's own contract.
The executed provider was the original disposable loopback HTTP/1.1 server in
`WireServer.cs`, implementing the pinned NuGet resource's wire contract. **No
hosted service was tested or is promised to support it.**

## Older approach

SDK10's actual NuGet.Protocol assembly has no `PackageStagingResourceV3`. Keep
ordinary `PackageUpdateResource` publication when final publication is the
intended operation and the source advertises `PackagePublish/2.0.0`. That older
route really compiles/runs here, discovers its endpoint and sends the actual
packed bytes. It has no staging/group equivalent. The source normalizes
`/published` to `/published/` and uses the fixed multipart filename
`package.nupkg`; do not infer the new staging filename behavior from it.

The older project's relevant settings are:

{{snippet:legacy-settings}}

## Newer approach

The current project's corresponding settings are:

{{snippet:settings}}

Both projects inherit `OutputType=Exe`, nullable/implicit usings enabled and
warnings as errors from `samples/Directory.Build.props`. Pin `global.json` with
`rollForward: disable`; the newer pin additionally uses `allowPrerelease: true`.
Reference with `Private=false`: the exact complete SDK supplies its own runtime
closure. This is a developer tool bound to that installation, not a redistributable
client library or a claim of SDK internal implementation stability.

The complete canonical publisher below is compiled in both actual consumers.
`NET11_0` selects staging; the older branch preserves ordinary publication.
`Open` obtains the actual core providers. `GetResourceAsync` reads the real
service index and invokes the public provider pipeline; the internal resource
constructor is never called or replaced.

{{snippet:publisher}}

Supply an absolute service-index URI and real nupkg/snupkg paths, an optional key
from the caller's secure input, optional group metadata and the caller's token.
Call `Stage(repository, package, symbols, apiKey, groupId, false, token)` after
`Open(index)` for an authorized service. Never put a key in source, a command-line
argument, URL, exception message or receipt. The sample uses only a generated
noncredential marker and prints header **presence**, never its value. It uses
`NullLogger`; production diagnostics need an explicit redaction policy too.

For SDK binding, register the resolver before a separate non-inlined method first
touches NuGet types. The canonical bootstrap shows that boundary and the exact
SDK selection; its `Run` invokes the repository's compiled local proof fixture:

{{snippet:bootstrap}}

For a standalone publisher, keep that resolver/NoInlining boundary and put the
`Open`/`Stage` call above in `Run` instead of `StagingChecks.Run`. Launch with the
matching absolute dotnet executable. Do not add private reflection, copy NuGet's
internal implementation or manufacture a replacement staging resource.

## Endpoint and multipart contract

The pinned provider selects the supported service-index type
`PackageStaging/1.0.0`. The advertised `@id` becomes the resource's exact
`SourceUri`. It is not an ordinary push endpoint inferred from a host name.

| Operation | Actual request for advertised `/foundry/batches/?lane=bronze` |
| --- | --- |
| `PushPackageAsync` | `PUT /foundry/batches/package?lane=bronze`, file field `package` |
| `PushSymbolsAsync` | `PUT /foundry/batches/symbols?lane=bronze`, file field `symbols` |

The resource trims trailing slashes from the endpoint path, appends the route and
retains the query. Each file part has its **original filename**, exact packed
bytes and `application/octet-stream`; the request uses multipart/form-data and
chunked transfer encoding. A non-null group adds a `groupId` text part; null
omits it, while empty sends an empty part. A nonempty API key adds
`X-NuGet-ApiKey`; null omits that header. A 2xx response only establishes that this
upload request succeeded. The API returns no publication/completion receipt.

## Constraints

HTTP uploads are refused by default. The explicit insecure flag in this example
is restricted to a disposable literal `127.0.0.1` fixture, checked for both the
source and discovered endpoint. Its local service index needs its separate
`PackageSource.AllowInsecureConnections` opt-in. Neither opt-in is a TLS trust
override. Use the service's supported secure transport outside that fixture;
this recipe configures no certificates, global TLS bypass or certificate store.
HTTPS trust/refusal, redirects, credentials and hosted authentication flows are
not qualified by this HTTP-only protocol exercise.

A missing/unsupported staging resource returns null; the publisher fails with an
actionable message before any upload. It does not fall back to ordinary push.
`EnsureSuccessStatusCode` rejects HTTP400 in the real check. Cancellation interrupts
an in-flight symbols request after the server received it. The explicit request
timeout also expires on a received package request while the independent outer
token remains live. NuGet may retry, so neither failure nor cancellation means
that the service stored nothing. Reconcile with provider status/cleanup before
retrying; do not assume the group makes uploads atomic or idempotent. The older
`Push` overload's token controls discovery here, not its upload; its own timeout
is separate. No older upload-cancellation equivalence is claimed.

## Original package inputs

The real paired runs execute SDK `pack` on this original library project:

{{snippet:alloy-project}}

Its source is:

{{snippet:alloy-source}}

The sample passes `FoundryTarget=net10.0` or `net11.0` to restore/build/pack the
actual respective artifact. It checks the nupkg's real library and the snupkg's
portable PDB documents, then compares every multipart file byte with those
artifacts. The package boundary clears remote feeds; no publish command targets
nuget.org, and no real API key is required.

## Migration

1. Identify the intended operation: staging versus final publication. Keep a
   valid ordinary publication route if that remains the requirement. Confirm the
   actual provider's supported staging contract before deploying a publisher.
2. Pin the complete SDK and actual application target/runtime/language separately.
   Reconcile the assembly and dependency hashes; do not invent a public package8
   or a net10 staging backport from a DLL's TFM alone.
3. Obtain a resource through `SourceRepository.GetResourceAsync`. Handle null
   explicitly and retain the exact advertised endpoint. Never guess `/package`
   at nuget.org or substitute an internal constructor.
4. Pack an original library with portable symbols. Stage nupkg and snupkg with the
   intended group/null policy, secure key input, timeout and cancellation. Keep
   the insecure flag false except in the disposable loopback check.
5. Verify the real service's accepted uploads and completion/publication policy.
   Exercise rejected status, missing resource and interrupted/timeout requests;
   document partial-upload cleanup and retries. A local fixture pass establishes
   only this pinned protocol behavior, not any hosted provider's support.

## Evidence and limits

`eng/Record-FoundryStaging.ps1` uses the existing sample-evidence workflow to
build/run both actual dependency sets, pack original nupkg/snupkg inputs and
save fresh input hashes and commands. The old run verifies real publication
multipart bytes/routes/key presence, HTTP400 failure and absence of the staging
type. The current run verifies real network discovery/endpoint selection,
original file names, byte identity, chunk framing, key present/absent, group
present/null/empty, 201 success, HTTP400 rejection, unsupported resource absence,
HTTP refusal before upload, loopback-only opt-in, cancellation and request
timeout. The owned TCP listener/clients stop during async disposal; temporary
packages are deleted. Raw diagnostics remain ignored and keys are not logged.

Ordinary installed-tool stdio list/get verifies ID/title/**goal**, the entire
expanded canonical body, explicit target11/edition11, applicability and current
paired evidence offline from an unrelated directory. The target10 request has
no applicable staging edition; the valid old publication comparison remains in
this body. Retrieval executes no uploads. These are Windows x64 managed HTTP
checks, not mock responses injected into NuGet, TLS/provider interoperability,
performance, other-platform or GA evidence. The API requires no experimental
diagnostic suppression or preview-language flag; the SDK itself is RC2/preview.
The exact eight approved .NET10 foundations, PIN-959 qualifying coverage/evidence,
PIN-963 actual-GA reconciliation and unrelated Human gates remain open.

## Official protocol origin

- [Pinned public staging resource](https://github.com/dotnet/dotnet/blob/820230c386db5b0106c5430b3e5aee92a7826bf5/src/nuget-client/src/NuGet.Core/NuGet.Protocol/Resources/PackageStagingResourceV3.cs), SHA-256 `e4baa232bc2ec54c20611706cbd91b48ca7f4019a3680085aa4cb15713be28eb`.
- [Real resource provider](https://github.com/dotnet/dotnet/blob/820230c386db5b0106c5430b3e5aee92a7826bf5/src/nuget-client/src/NuGet.Core/NuGet.Protocol/Providers/PackageStagingResourceV3Provider.cs).
- [Exact service type/version](https://github.com/dotnet/dotnet/blob/820230c386db5b0106c5430b3e5aee92a7826bf5/src/nuget-client/src/NuGet.Core/NuGet.Protocol/ServiceTypes.cs).
- [Older ordinary publication contract](https://github.com/dotnet/dotnet/blob/a8dd0577c5c13f43a2f57fe33b4ff11b90f6371e/src/nuget-client/src/NuGet.Core/NuGet.Protocol/Resources/PackageUpdateResource.cs).
