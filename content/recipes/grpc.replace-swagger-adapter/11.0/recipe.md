# Replace the retired gRPC Swagger integration

## Goal

Keep a gRPC service's transcoded JSON interface discoverable after retiring its documentation adapter by generating and checking a protobuf-derived contract at build time.

Exact reproduction uses SDK10.0.402 / framework10.0.13 / C#14 versus SDK11.0.100-rc.2.26504.105 / framework11.0.0-rc.2.26504.105 / C#15. These are tested pins, not minimum supported versions. RC2 is not actual-GA proof. The exact eight approved .NET10 foundations, stable identities, PIN-959/PIN-963/PIN-964 and unrelated Human gates remain unchanged.

## Older approach — .NET 10

The fictional Willow tide survey exposes one annotated unary operation. The actual old Microsoft.AspNetCore.Grpc.Swagger0.10.11 adapter works with explicit JsonTranscoding10.0.13 and Swashbuckle6.6.2 on net10.0, alongside Grpc2.64.0. AddGrpcSwagger integrates with SwaggerGen. IncludeGrpcXmlComments supplies RPC comments; IncludeXmlComments also retains response/message/property comments from the generated XML documentation. This route remains a measured historical alternative, although the adapter was experimental and is now deprecated.

Create the complete standalone files below, restore locked packages, then run `dotnet run --project TideSurvey.csproj -c Release` with the pinned10 SDK. The program starts real HTTP1/HTTP2 listeners, makes an actual gRPC call and matching JSON request, retrieves the adapter document, checks operations/schema/comments and failure controls, and exits. No static hand-authored contract stands in for generation.

### Directory.Build.props

{{snippet:n10-directory-build-props}}

### Directory.Build.targets

{{snippet:n10-directory-build-targets}}

### Directory.Packages.props

{{snippet:n10-directory-packages-props}}

### global.json

{{snippet:n10-global-json}}

### packages.lock.json

{{snippet:n10-packages-lock-json}}

### Program.cs

{{snippet:n10-program-cs}}

### RuntimeIdentity.cs

{{snippet:n10-runtimeidentity-cs}}

### tides.proto

{{snippet:n10-tides-proto}}

### TideSurvey.csproj

{{snippet:n10-tidesurvey-csproj}}

## Newer approach — .NET 11

The replacement moves documentation generation to the build. ASP.NET still serves gRPC and JSON through AddGrpc().AddJsonTranscoding(); only the deprecated documentation adapter and Swashbuckle dependencies are removed. Before implementing this service, actual protoc26.1 from Grpc.Tools2.64.0 and the pinned protoc-gen-openapiv2v2.31.0 release were executed on this original annotated proto with the exact JsonTranscoding RC2 Google imports. The generated operation/schema/comments were present, establishing this document route independently of generic AddOpenApi documentation.

Acquire the official `protoc-gen-openapiv2-v2.31.0-windows-x86_64.exe` and `grpc-gateway_2.31.0_checksums.txt` assets from the pinned release, using `gh release download v2.31.0 --repo grpc-ecosystem/grpc-gateway --pattern protoc-gen-openapiv2-v2.31.0-windows-x86_64.exe --pattern grpc-gateway_2.31.0_checksums.txt --dir <isolated-tools-directory>`. Check BOTH exact SHA256 values above and the executable's entry in the manifest before executing it. The release uses BSD-3-Clause; retain its release/license attribution when distributing the generator. NuGet packages retain their own notices/licenses. No Go SDK, gateway runtime, installer or PATH change is needed.

Create the files below in a separate standalone directory. With the pinned11 SDK, run `dotnet restore TideSurvey.csproj --locked-mode` first to acquire the exact tools and imports. Run `pwsh -NoProfile -File ./Generate-Contract.ps1 -PackageRoot <absolute-restored-package-root> -Generator <absolute-verified-exe>`; for a default Windows restore the package root is the user's `.nuget/packages`, but use the actual restore location. The script writes `obj/tides.swagger.json` and fails on missing content. Then run `dotnet run --project TideSurvey.csproj -c Release --no-restore`. The project copies that generated document into output/publish, and the endpoint returns those bytes. Generation must precede every build after proto changes; a stale copied document is not an automatic runtime reflection contract.

### Directory.Build.props

{{snippet:n11-directory-build-props}}

### Directory.Build.targets

{{snippet:n11-directory-build-targets}}

### Directory.Packages.props

{{snippet:n11-directory-packages-props}}

### Generate-Contract.ps1

{{snippet:n11-generate-contract-ps1}}

### global.json

{{snippet:n11-global-json}}

### packages.lock.json

{{snippet:n11-packages-lock-json}}

### Program.cs

{{snippet:n11-program-cs}}

### RuntimeIdentity.cs

{{snippet:n11-runtimeidentity-cs}}

### tides.proto

{{snippet:n11-tides-proto}}

### TideSurvey.csproj

{{snippet:n11-tidesurvey-csproj}}

## Migration

1. Record actual old adapter, JsonTranscoding, Swashbuckle, Grpc and protobuf-tool dependencies. Confirm their real target assets and run the old service/document checks before changing it.
2. Select the supported protoc-gen-openapiv2 build route and qualify it first: verify the pinned executable/checksums, use matching restored annotation imports and descriptor.proto, run the generator on the original proto, and reject empty operations/schemas or lost comments.
3. Keep the ASP.NET gRPC/transcoding registration and annotated proto. Remove AddGrpcSwagger, SwaggerGen and their deprecated documentation dependencies from the new target; do not invent an AddOpenApi drop-in replacement.
4. Generate Swagger2.0 before building. Copy/serve the exact generated artifact and make document generation/freshness an explicit build-pipeline step. Keep the generator version independent from the server release family.
5. Call the same operation through an actual HTTP2 gRPC client and HTTP1 transcoded JSON. Resolve the generated response schema, compare every actual field/type/value and protobuf comment, and test unknown station, wrong method and wrong path.
6. Review document dialect, JSON mapping and provider/client compatibility before changing consumers. Qualify other annotation patterns/options separately; publish neither an empty document nor a promise of automatic ASP.NET reflection coverage.

## Constraints

There is no matching Swagger11RC2 package or direct AddGrpcSwagger replacement. Generic AddOpenApi registration alone does not establish transcoding operations. This route serves a static build artifact generated from the original annotated proto; regenerate after proto changes.

The output is Swagger2.0/OpenAPIv2, not OpenAPI3.x. Its operations/messages/comments are generated by the supported third-party tool. No automatic runtime reflection, ASP.NET API explorer integration or drop-in adapter compatibility is claimed.

The actual old Swagger0.10.11 package has net10.0 assets and a JsonTranscoding10.0.11 minimum; the measured consumer explicitly selects10.0.13 plus Swashbuckle6.6.2. This historical route really runs on net10.0. Retention on net11.0 has not been qualified and is not offered. The old adapter was experimental and is now deprecated; no obsolete suppression is added.

Only unary GET with one string path field and string/int32 response fields is qualified. Default lowerCamelCase JSON names and matching protobuf imports are used. int64, bytes, enums, optional/oneof, streaming, body/query bindings, custom JSON options and arbitrary Google HTTP patterns need their own contract/runtime checks.

A document that has no paths, omits a response field, drops protobuf comments or disagrees with actual gRPC/JSON wire types is failure. This sample resolves the actual response reference and checks its schema against the actual returned JSON.

Use separate ephemeral HTTP1 and cleartext HTTP2 loopback ports only in the fixture; no certificate trust or global transport switch is changed. Production HTTPS/listeners, authentication and document exposure policy remain application responsibilities. Keep package/tool licenses and provenance; do not hand-author a replacement generator or expected document.

The complete files, configuration, lock, native-tool acquisition conditions and migration steps above are included offline. Restore/download preparation can need network access; installed recipe retrieval does not execute the samples or acquire missing prerequisites. There is no new recurring platform/browser matrix or public publication.

## Evidence

Both locked actual services compile/run. The old adapter produces a nonempty OpenAPI3 document with RPC/message/field comments; the new pinned generator produces Swagger2.0 with corresponding GET operation, path parameter and response schema/comments. Real gRPC and transcoded JSON return station willow, heightCm142 and observedAt2026-10-01T06:30:00Z. The checks resolve each actual document response reference and compare all returned wire-field types, int32 format and comments; unknown station is gRPC NotFound/JSON404, wrong method405 and wrong path404. Native protoc/gRPC C# plugin and independent generator hashes are retained. This proves the bounded primitive mapping, not all Google annotation patterns or generated-client interoperability.

Fresh compilation/execution records retain exact commands, source/input hashes, resolved dependency graphs, platform and measured SDK/framework/native identities. Source audit metadata is reconciled with the actual selected artifacts separately from loaded runtime bytes. Run the repository's eng/Test-WebMigrationBatch.ps1 with both absolute SDK hosts and -RecordEvidence to refresh managed/tool/publish proof; browser proof must be captured independently after relevant layout/input changes. Installed ordinary-artifact list/get inspection compares the complete canonical body, goal, target11/edition11, applicability, fresh evidence and explicit unavailable target10 response. Proxy blocking establishes proxy independence, not network isolation or additional runtime execution.

## References

- [Microsoft: deprecated adapter has no direct replacement](https://learn.microsoft.com/en-us/aspnet/core/grpc/json-transcoding-openapi?view=aspnetcore-10.0)
- [Generator protobuf comments and OpenAPI output](https://grpc-ecosystem.github.io/grpc-gateway/docs/mapping/customizing_openapi_output/)
- [HTTP annotations and required imports](https://grpc-ecosystem.github.io/grpc-gateway/docs/tutorials/adding_annotations/)
- [Pinned generator release and checksums](https://github.com/grpc-ecosystem/grpc-gateway/releases/tag/v2.31.0)
- [Exact historical Microsoft adapter](https://www.nuget.org/packages/Microsoft.AspNetCore.Grpc.Swagger/0.10.11)
