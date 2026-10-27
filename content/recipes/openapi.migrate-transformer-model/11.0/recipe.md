# Keep custom OpenAPI transformations working after an upgrade

## Goal

Recompile ASP.NET Core document, operation and schema transformers against the resolved OpenAPI.NET model, preserving interface-based traversal, references and JSON extension values. The fictional Alder Fen service publishes current and retiring water-depth observations. These three related goals share the complete original executable comparison below.

## Older approach

The original .NET 10 Alder Fen sample resolves Microsoft.OpenApi 2.12.2 through Microsoft.AspNetCore.OpenApi 10.0.13. Its transformers already use Microsoft.OpenApi, IOpenApiSchema, OpenApiResponseReference and JsonNodeExtension. These are working 2.x patterns, not APIs introduced by this migration. The operation transformer explicitly types the reference metadata as OpenApiReferenceWithDescription. Both samples produce the same local reusable 503 response, reference description, object extension and metric schema extension.

## Newer approach

ASP.NET Core 11 resolves Microsoft.OpenApi 3.x. In the tested 3.10.2 model the response reference metadata type is OpenApiReferenceWithDescriptionAndSummary; the explicit declaration changes accordingly. The reusable response content dictionary also changes its value type from OpenApiMediaType to IOpenApiMediaType; the concrete payload still uses OpenApiMediaType. Transformer entry points still accept concrete OpenApiDocument, OpenApiOperation and OpenApiSchema. Nested schemas are interface values: the sample mutates only inline OpenApiSchema instances, leaving references intact. Extension dictionaries accept IOpenApiExtension, so JSON values are wrapped in JsonNodeExtension rather than assigned as bare JsonValue objects.

{{snippet:focus}}

## Migration

1. Inspect the application target and actual resolved dependency graph, including transformer libraries. Align the ASP.NET Core OpenAPI release with the target; do not force 2.x beneath a generator that requires 3.x.
2. Rebuild every custom document, operation and schema transformer and dependent library against that graph. Update explicit response-reference metadata declarations from OpenApiReferenceWithDescription to OpenApiReferenceWithDescriptionAndSummary for this tested pair. Change response content dictionary values to IOpenApiMediaType. Retain existing compatible code.
3. Traverse nested values as IOpenApiSchema. Distinguish inline schemas from reference holders before mutating them. Construct local references with OpenApiResponseReference and ensure the named component exists; do not cast every nested schema to a concrete type.
4. Wrap JSON object/scalar extension payloads in JsonNodeExtension. If older code uses still earlier Any/Models namespaces or parsing helpers, consult its actual package baseline before migrating; those are absent from the declared 2.12.2 sample too.
5. Generate the document and inspect the 503 $ref, its existing component target, reference description override, text/plain string schema, x-observation-area object, x-measurement-system scalar and inline property descriptions. Compilation alone cannot establish serialized output or binary compatibility.
6. Recreate both independent directories below. Use each directory's pinned SDK and run dotnet run --configuration Release from that directory. Each example starts ephemeral loopback Kestrel, checks raw/v1/slash documents at the origin root and /fen, performs real requests, prints PASS and stops. These verification programs stop automatically; adapt hosting lifetime separately for a production service. Restore setup may need the network; installed guidance does not.

## Constraints

This is the ASP.NET Core transitive-dependency migration, not a standalone OpenAPI.NET feature catalogue. The official breaking-change article uses illustrative older Any/Models snippets that do not match this pinned .NET 10 dependency. Actual restored 2.12.2 and 3.10.2 APIs govern the comparison. Newer reference metadata can carry a summary, but the sample needs only the existing description. The 503 declaration documents a possible failure; these successful handlers do not implement or emit a 503. Reference traversal must retain the intended local/external scope. No binary-compatibility promise is made for an unrecompiled third-party transformer.

This is an 11.0 edition; the .NET 10 source is a previous-release comparison, not a conditional older-target route. Use ASP.NET Core runtime/package assets for net11.0 and a suitable .NET 11 SDK. Canonical C# modes are explicitly 14.0 and 15.0. Verification pins are SDK 10.0.402 / OpenApi 10.0.13 / Microsoft.OpenApi 2.12.2 and SDK 11.0.100-rc.2.26504.105 / OpenApi 11.0.0-rc.2.26504.105 / Microsoft.OpenApi 3.10.2. They record the actual artifacts, not mandatory consumer patch upgrades. Stable APIs need no experimental opt-in. RC2 preparation under ADR 0004 does not prove final GA behavior. No database/provider, account, key, browser rendering mode or additional workload is required. Actual execution is Windows x64; no Native AOT, trimming or other-OS execution claim is made.

## Evidence

Actual declared .NET 10/11 SDK compilation and loopback Kestrel execution: raw/authored Obsolete operation/type/property flags with current controls; document/operation/schema transformers, inline traversal, response reference and override, object/scalar JSON extensions; exact empty/non-empty PathBase server fixtures, optional root-slash transform and composed HTTP requests. No mock, cross-compilation or third-party generated-client claim. Both projects have locked dependency graphs and saved compile-and-run records with exact inputs, SDKs and actual runtime output. The raw .NET 10 control lacks deprecation metadata; its authored v1 matches .NET 11 without a deprecation transformer. Current members remain unmarked. The root server spelling changes while /fen stays intact. These are execution assertions, not source-only conclusions. The package harness retrieves list/get through the real locally packed and installed stdio tool from an unrelated directory and isolated feed/cache; it checks full canonical Markdown, complete applicability and the 11.0 edition, rejects an older-target edition and preserves JSON/text equality. Local proxy blocking does not establish network isolation; the existing network-disabled CI lane supplies that check. This evidence covers this slice, not comprehensive catalogue or GA release readiness.

Official references: [ASP.NET Core Microsoft.OpenApi 3.x dependency migration](https://learn.microsoft.com/en-us/aspnet/core/breaking-changes/11/microsoft-openapi-3x); [OpenAPI.NET 2.12.2 source](https://github.com/microsoft/OpenAPI.NET/tree/v2.12.2); [OpenAPI.NET 3.10.2 source](https://github.com/microsoft/OpenAPI.NET/tree/v3.10.2).


### Recreate samples/net10.0/FenContracts

**Directory.Build.props**

{{snippet:n10-directory-build-props}}

**Directory.Build.targets**

{{snippet:n10-directory-build-targets}}

**Directory.Packages.props**

{{snippet:n10-directory-packages-props}}

**global.json**

{{snippet:n10-global-json}}

**FenContracts.csproj**

{{snippet:n10-fencontracts-csproj}}

**Program.cs**

{{snippet:n10-program-cs}}

**Readings.cs**

{{snippet:n10-readings-cs}}

**Transformers.cs**

{{snippet:n10-transformers-cs}}

**ContractChecks.cs**

{{snippet:n10-contractchecks-cs}}

**LegacyDeprecationTransformer.cs**

{{snippet:n10-legacydeprecationtransformer-cs}}


### Recreate samples/net11.0/FenContracts

**Directory.Build.props**

{{snippet:n11-directory-build-props}}

**Directory.Build.targets**

{{snippet:n11-directory-build-targets}}

**Directory.Packages.props**

{{snippet:n11-directory-packages-props}}

**global.json**

{{snippet:n11-global-json}}

**FenContracts.csproj**

{{snippet:n11-fencontracts-csproj}}

**Program.cs**

{{snippet:n11-program-cs}}

**Readings.cs**

{{snippet:n11-readings-cs}}

**Transformers.cs**

{{snippet:n11-transformers-cs}}

**ContractChecks.cs**

{{snippet:n11-contractchecks-cs}}
