# Record handshake failures and inspect ClientHello

## Goal

A fictional lantern station accepts encrypted status requests. When a caller cannot negotiate TLS, its request never reaches an endpoint. Observe the connection before and after HTTPS so that the station can associate ClientHello inspection with the actual negotiation failure.

The new surface is Kestrel's `UseTlsClientHelloListener` connection middleware and `ITlsHandshakeFeature.Exception`. This 11.0 edition compares actual .NET 10 and .NET 11 RC2 applications; it does not claim a .NET 10 route to the new exception property.

## Older approach — inspect through the HTTPS callback

.NET 10 exposes `TlsClientHelloBytesCallback` on the HTTPS options. It remains a valid inspection mechanism on that release. Connection middleware can wrap HTTPS, but `ITlsHandshakeFeature` has no `Exception` property there. Use Kestrel's diagnostic logs to investigate server-side negotiation failures; a client's authentication exception is not the server's retained exception.

The complete older fixture below observes two ClientHellos, serves a successful HTTPS request and verifies that an incompatible client fails. It records no invented server exception feature. Save the following files together in an independent `LanternTls` directory.

### Directory.Build.props
{{snippet:n10-directory-build-props}}

### Directory.Build.targets
{{snippet:n10-directory-build-targets}}

### Directory.Packages.props
{{snippet:n10-directory-packages-props}}

### global.json
{{snippet:n10-global-json}}

### LanternTls.csproj
{{snippet:n10-lanterntls-csproj}}

### Program.cs
{{snippet:n10-program-cs}}

## Newer approach — inspect before TLS and observe completion

Register the observer around downstream connection processing, then register `UseTlsClientHelloListener` **before** `UseHttps`. The outer observer reads `ITlsHandshakeFeature.Exception` after downstream completion, including the failure path. This is connection middleware on `ListenOptions`, not `app.Use` request middleware. Kestrel retains handshake feature values after disposing its TLS stream.

The callback borrows its byte sequence. Read it during the callback; copy bytes into application-owned memory if later processing requires them. The example stores only the byte count, keeping sensitive handshake material out of logs. A connection identifier is a useful correlation key in a production log, but a ClientHello alone does not establish successful authentication.

Save the complete newer files in a separate directory. Restore with the declared SDK, build Release, then execute the built `LanternTls.dll` using that SDK's absolute `dotnet` host. The program exits after checking both outcomes.

### Directory.Build.props
{{snippet:n11-directory-build-props}}

### Directory.Build.targets
{{snippet:n11-directory-build-targets}}

### Directory.Packages.props
{{snippet:n11-directory-packages-props}}

### global.json
{{snippet:n11-global-json}}

### LanternTls.csproj
{{snippet:n11-lanterntls-csproj}}

### Program.cs
{{snippet:n11-program-cs}}

## Migration

1. Check that the application uses Kestrel and targets .NET 11. Keep a .NET 10 application on its supported HTTPS callback until its framework migration is deliberate.
2. Move ClientHello inspection out of `HttpsConnectionAdapterOptions.TlsClientHelloBytesCallback` into `ListenOptions.UseTlsClientHelloListener`. Remove the obsolete callback on .NET 11; do not register both mechanisms.
3. Order connection middleware as shown. Put completion observation outside HTTPS, and ClientHello inspection before HTTPS. Read the feature in `finally` so downstream failure still reaches the observer.
4. Bound inspection and TLS negotiation independently. The example gives each stage three seconds; these budgets can accumulate. Choose production values from the service's needs.
5. For the reproduction, restore each separate project to generate its lock file, build Release and execute its DLL. Expect two ClientHellos, one successful HTTP response and one rejected client; the newer result additionally names the retained `AuthenticationException`.
6. In the application, replace the fixture certificate and protocol restrictions with the normal certificate configuration and OS-selected TLS defaults. Preserve bounded, privacy-conscious logging and refresh the focused check when those inputs change.

## Constraints

- Runtime/framework: ASP.NET Core 11 on Kestrel for the new middleware/exception surface. SDK 11 RC2 and C# 15 are the exact sample baseline, not a mandate to use an RC build in production. The older build uses SDK 10.0.402, C# 14 and .NET 10.0.13.
- Packages/assets: the ASP.NET Core shared framework supplies these APIs; no extra package, provider, account or workload is required. These explicit local build/package files prevent accidental inheritance when the sample is copied into another repository.
- Platform: the fixture actually ran on Windows x64 with TLS 1.2 and TLS 1.3 available. Its deliberate version mismatch and scoped analyzer suppression exist to create a reliable negotiation failure. The sample does not prescribe a TLS 1.2 production cap.
- The temporary certificate is loaded into a usable private-key container for Windows TLS. It is never trusted globally. Successful-client validation pins that exact certificate; the deliberately failing client accepts a certificate only to isolate protocol negotiation. These are loopback test policies, not deployment certificate-validation advice.
- Rendering: no browser or Blazor mode applies. No IIS, HTTP.sys, physical device or remote TLS terminator behavior was exercised. If a proxy terminates TLS, instrument that boundary instead.
- Stable API intent is prepared from RC2 artifacts under ADR 0004. Final GA behavior still needs reconciliation. No experimental opt-in applies to the declared APIs.

## Evidence

Both actual pinned projects were restored, compiled and executed through `eng/Record-Evidence.ps1`; each has its own saved input hashes and resolved dependency inventory. This is real TLS and HTTP execution, not mocked feature data or cross-compilation. The older path proves ClientHello inspection and client rejection; the newer path additionally asserts exactly one retained server-side failure and two observed ClientHellos.

The installed-package `LanternTransport` scenario checks full canonical Markdown, discovery title/goal, complete applicability, fresh sample evidence and edition 11.0 through real stdio list/get from an unrelated directory. Target 10.0 does not return this edition. Package retrieval proves delivery, separately from the TLS execution records. The original inventory identity remains `hosting.inspect-tls-handshake` / WEB-077.

Official context: [Kestrel TLS observability](https://learn.microsoft.com/en-us/aspnet/core/release-notes/aspnetcore-11#tls-handshake-observability-in-kestrel). Essential setup and implementation are included above; the link is supporting material.
