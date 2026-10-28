# Choose response compression and accepted request encodings

## Goal

A harbour publishes repetitive tide bulletins and accepts short receipts from several services. New services understand Zstandard; older ones use gzip. The harbour deliberately offers those two response formats and accepts those two upload formats, while rejecting other compressed uploads instead of handing encoded bytes to an application parser.

The authored change is an explicit provider and decoding policy. Merely upgrading to ASP.NET Core 11's automatic Zstandard defaults is not this goal. Response negotiation (`Accept-Encoding`) and upload decoding (`Content-Encoding`) are independent decisions.

## Older approach — a deliberate gzip contract

.NET 10 has built-in gzip response compression and request decompression, but no built-in Zstandard codec/provider. An external library and custom providers can be valid, but none is selected or verified here. The older program offers gzip and identity, and honestly declines Zstandard uploads. Its zstd-only response request gets an uncompressed response, not a falsely labelled gzip payload.

Save these complete files in their own `HarbourEncoding` directory. The fixture binds only loopback HTTP, exercises negotiation and uploads, then stops.

### Directory.Build.props
{{snippet:n10-directory-build-props}}

### Directory.Build.targets
{{snippet:n10-directory-build-targets}}

### Directory.Packages.props
{{snippet:n10-directory-packages-props}}

### global.json
{{snippet:n10-global-json}}

### HarbourEncoding.csproj
{{snippet:n10-harbourencoding-csproj}}

### Program.cs
{{snippet:n10-program-cs}}

## Newer approach — offer Zstandard and retain a gzip fallback

Adding response providers explicitly replaces the automatic provider set. Register Zstandard and gzip to make the intended menu visible, restrict compression to `text/plain`, and choose the example's quality/window settings deliberately. Requests with higher gzip quality select gzip; `zstd;q=0` excludes Zstandard. A client advertising only an unsupported response format can still receive identity content. This fixture does not implement a separate 406 policy for clients that prohibit every available representation.

The request-decompression options already contain built-in decoder instances. Their concrete classes are internal, so retain the registered `zstd`/`gzip` entries and remove `br`/`deflate`; do not try to instantiate internal classes. Unknown encodings and multiple stacked encodings normally pass through request-decompression unchanged. The next middleware checks the remaining header and returns 415. An unencoded body with no `Content-Encoding` header is accepted; this sample's `identity` test uses that form, not an `identity` header.

Both wire and expanded bodies are bounded through Kestrel's request limit and request-decompression's size-limited stream. The reader consumes that stream, so an expanded body over 1024 bytes produces 413. Malformed compressed data that raises `InvalidDataException` becomes 400. These choices are application policy, not new automatic framework rejection defaults.

Save the newer files separately.

### Directory.Build.props
{{snippet:n11-directory-build-props}}

### Directory.Build.targets
{{snippet:n11-directory-build-targets}}

### Directory.Packages.props
{{snippet:n11-directory-packages-props}}

### global.json
{{snippet:n11-global-json}}

### HarbourEncoding.csproj
{{snippet:n11-harbourencoding-csproj}}

### Program.cs
{{snippet:n11-program-cs}}

## Migration

1. Inventory the actual callers and intermediaries. Confirm which response encodings they advertise and which upload encodings the receiver promises to accept. A successful .NET round trip is not proof that every browser or proxy supports Zstandard.
2. Register the response provider set explicitly. This example omits Brotli intentionally and preserves gzip fallback. If you need Brotli, add its provider and verify that changed menu instead of assuming automatic defaults are still appended.
3. Restrict the request registry independently. Place `UseRequestDecompression` before the endpoint reads the body and return an explicit status for remaining `Content-Encoding` values if pass-through is unacceptable.
4. Set a practical decompressed body limit and keep a wire-size limit. Use stricter endpoint limits when appropriate. Budget decoder memory and concurrent decoding as well as total expanded bytes; a response compression window setting does not limit upload decoder memory or expanded length.
5. Keep HTTPS response compression disabled until the application's secret-reflection and side-channel risks are assessed. Production TLS, authentication and reverse-proxy behavior are separate from this loopback reproduction.
6. Build each directory with its pinned SDK using `dotnet build --configuration Release`, then execute `dotnet bin/Release/net10.0/HarbourEncoding.dll` or the corresponding `net11.0` path. Use absolute executable paths and disable CLI-home PATH integration when isolating homes. Expect successful negotiation/decoding, explicit 415 results and expanded-body 413 results.

## Constraints and alternatives

- Runtime/SDK/language: built-in Zstandard response/request support requires ASP.NET Core 11 and the .NET 11 codec. Exact tested baseline: SDK 11.0.100-rc.2.26504.105 / C# 15. Older comparison: SDK 10.0.402 / C# 14 / runtime 10.0.13. No .NET 10 Zstandard route is advertised.
- Packages/assets: matching Microsoft.AspNetCore.App and Microsoft.NETCore.App shared-framework/runtime assets; no separate Zstandard NuGet package. Do not revive the removed preview codec package/namespace as an older-target route.
- Platform/provider/rendering: actual Windows x64 Kestrel loopback HTTP with built-in codecs. Browser/WASI are unsupported by the built-in codec; no client UI or Blazor mode applies. No external provider, workload, account, custom dictionary or native installation is required.
- `Accept-Encoding` does not declare upload compression. A response fallback does not authorize accepting another upload format. Gzip and Zstandard are distinct wire formats; do not relabel one as the other.
- The test disables HttpClient's automatic decompression and checks both the returned header and decoded raw bytes. It tests .NET-to-.NET interoperability, not third-party peers, TLS, proxies, AOT or trimming.
- Stable API intent uses RC2 evidence under ADR 0004. No experimental opt-in applies; final GA reconciliation remains outstanding.

## Evidence

Both actual pinned projects compiled and ran through `eng/Record-Evidence.ps1`. Real Kestrel requests check zstd/gzip/identity response selection, quality weighting and q=0, unsupported response fallback, supported upload body equality, unsupported and stacked upload 415, and expanded-body 413. .NET 10 explicitly lacks Zstandard; its positive upload route is gzip. No mock middleware or cross-compilation is counted as execution.

The existing isolated packed-tool harness retrieves full canonical Markdown, complete applicability, discovery title/goal and target edition 11.0 over actual stdio list/get outside the checkout. Target 10.0 does not advertise this edition. Stable identity: `web.negotiate-zstandard-content`, covering WEB-075 and WEB-096 without treating automatic defaults as a separate feature.

Official context: [response compression](https://learn.microsoft.com/en-us/aspnet/core/performance/response-compression?view=aspnetcore-11.0), [request decompression](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/middleware/request-decompression?view=aspnetcore-11.0), [introduction](https://github.com/dotnet/core/blob/main/release-notes/11.0/preview/preview3/aspnetcore.md), and [RC2 decoder registry](https://github.com/dotnet/aspnetcore/blob/v11.0.0-rc.2.26504.105/src/Middleware/RequestDecompression/src/RequestDecompressionOptions.cs). All required code and policy are included above.
