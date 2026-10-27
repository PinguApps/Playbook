# Carry retirement warnings into the API contract

## Goal

Use Obsolete declarations to mark retiring operations, response types and properties in generated OpenAPI, then remove only transformers that duplicate those flags. The fictional Alder Fen service publishes current and retiring water-depth observations. These three related goals share the complete original executable comparison below.

## Older approach

The fictional Alder Fen service keeps an old gauge endpoint available while consumers move to metre-based observations. Its .NET 10 sample reads Obsolete declarations in a custom operation/schema transformer. The raw document deliberately omits that transformer and demonstrates the metadata gap. The v1 document adds it and marks the old operation, LegacyReading type and GaugeTicks property as deprecated. This remains a valid older solution; the retired endpoint still answers requests.

## Newer approach

The .NET 11 sample retains the same original Obsolete declarations and removes the deprecation transformer class and both registrations. The built-in generator supplies deprecated: true for the operation, schema type and property. Current Reading, its Station property and the /readings operation remain unmarked. Other transformers below still add observation metadata and a reusable response: they have a different purpose and stay registered.

{{snippet:focus}}

## Migration

1. Identify which published operations, model types and properties are actually retiring. Put Obsolete on those source declarations with a useful replacement message; keep active members unmarked.
2. Upgrade and rebuild with the matching ASP.NET Core OpenAPI package. Generate the document before removing a custom transformer, and compare the retiring and current controls.
3. Remove only transformers and registrations that duplicate deprecation flags. In the complete comparison, delete LegacyDeprecationTransformer.cs and both of its registrations; keep the three Fen transformers.
4. Generate again and verify operation, schema and property flags. Keep request checks for the old endpoint during the announced migration period. Obsolete metadata is a warning, not endpoint removal or access control.
5. Recreate both independent directories below. Use each directory's pinned SDK and run dotnet run --configuration Release from that directory. Each example starts ephemeral loopback Kestrel, checks raw/v1/slash documents at the origin root and /fen, performs real requests, prints PASS and stops. These verification programs stop automatically; adapt hosting lifetime separately for a production service. Restore setup may need the network; installed guidance does not.

## Constraints

Operations, schema types and schema properties are covered. Obsolete messages are source warnings; the generated boolean does not communicate a retirement date or remove an endpoint. This example retains runtime access and suppresses CS0618 only at intentional uses of retiring declarations. Do not remove unrelated transformer behavior, documentation or an enforcement policy.

This is an 11.0 edition; the .NET 10 source is a previous-release comparison, not a conditional older-target route. Use ASP.NET Core runtime/package assets for net11.0 and a suitable .NET 11 SDK. Canonical C# modes are explicitly 14.0 and 15.0. Verification pins are SDK 10.0.402 / OpenApi 10.0.13 / Microsoft.OpenApi 2.12.2 and SDK 11.0.100-rc.2.26504.105 / OpenApi 11.0.0-rc.2.26504.105 / Microsoft.OpenApi 3.10.2. They record the actual artifacts, not mandatory consumer patch upgrades. Stable APIs need no experimental opt-in. RC2 preparation under ADR 0004 does not prove final GA behavior. No database/provider, account, key, browser rendering mode or additional workload is required. Actual execution is Windows x64; no Native AOT, trimming or other-OS execution claim is made.

## Evidence

Actual declared .NET 10/11 SDK compilation and loopback Kestrel execution: raw/authored Obsolete operation/type/property flags with current controls; document/operation/schema transformers, inline traversal, response reference and override, object/scalar JSON extensions; exact empty/non-empty PathBase server fixtures, optional root-slash transform and composed HTTP requests. No mock, cross-compilation or third-party generated-client claim. Both projects have locked dependency graphs and saved compile-and-run records with exact inputs, SDKs and actual runtime output. The raw .NET 10 control lacks deprecation metadata; its authored v1 matches .NET 11 without a deprecation transformer. Current members remain unmarked. The root server spelling changes while /fen stays intact. These are execution assertions, not source-only conclusions. The package harness retrieves list/get through the real locally packed and installed stdio tool from an unrelated directory and isolated feed/cache; it checks full canonical Markdown, complete applicability and the 11.0 edition, rejects an older-target edition and preserves JSON/text equality. Local proxy blocking does not establish network isolation; the existing network-disabled CI lane supplies that check. This evidence covers this slice, not comprehensive catalogue or GA release readiness.

Official references: [ASP.NET Core 11 Obsolete metadata support](https://learn.microsoft.com/en-us/aspnet/core/release-notes/aspnetcore-11?view=aspnetcore-11.0); [Customize generated OpenAPI documents](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/openapi/customize-openapi?view=aspnetcore-11.0).


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
