# Select a direct TLS transport (Experimental; artifact gap)

## Goal

Prepare the documented Linux direct-TLS endpoint configuration while retaining ordinary socket HTTPS and recognizing when the available package cannot supply the documented endpoint API.

This is usable documentation-based guidance, authorized before full testing. Read the exact availability and evidence limits below; catalogue target 11 is not GA verification.

## Older approach

The .NET 10 harbour archive serves /catalogue over ordinary Kestrel socket HTTPS on loopback port 5443. Supply CertificatePath and CertificatePassword through local secure configuration. The code neither creates nor trusts certificates. This remains the usable fallback; no older direct-native equivalent is invented.

### .NET 10: `Directory.Build.props`

{{snippet:n10-directory-build-props}}

### .NET 10: `Directory.Build.targets`

{{snippet:n10-directory-build-targets}}

### .NET 10: `Directory.Packages.props`

{{snippet:n10-directory-packages-props}}

### .NET 10: `HarbourTls.csproj`

{{snippet:n10-harbourtls-csproj}}

### .NET 10: `Program.cs`

{{snippet:n10-program-cs}}

### .NET 10: `global.json`

{{snippet:n10-global-json}}

### .NET 10: `packages.lock.json`

{{snippet:n10-packages-lock-json}}

## Newer approach

This original current example follows the pinned official endpoint design and keeps the socket control on port 5443 beside the explicitly selected direct-TLS endpoint on port 5444. Both use HTTP/1 and the same supplied certificate so protocol policy is explicit.

**Documentation-based configuration, not runnable against the inspected package:** NuGet currently lists only Microsoft.AspNetCore.Server.Kestrel.Transport.DirectTls version 11.0.0-rc.1.26425.128. Its public assembly has UseDirectTls/transport options but no DirectTlsEndpoint. Building the current sample reproduces CS0246. Do not add a fabricated shim, remove endpoint selection or relabel registration as a working direct-TLS migration. The package reference records the inspected artifact, not a usable pin for this documented code. Replace it only after obtaining and qualifying a matching artifact.

### .NET 11: `Directory.Build.props`

{{snippet:n11-directory-build-props}}

### .NET 11: `Directory.Build.targets`

{{snippet:n11-directory-build-targets}}

### .NET 11: `Directory.Packages.props`

{{snippet:n11-directory-packages-props}}

### .NET 11: `HarbourTls.csproj`

{{snippet:n11-harbourtls-csproj}}

### .NET 11: `Program.cs`

{{snippet:n11-program-cs}}

### .NET 11: `global.json`

{{snippet:n11-global-json}}

### .NET 11: `packages.lock.json`

{{snippet:n11-packages-lock-json}}

## Migration

1. Save the .NET 10 file set in its own HarbourTls directory. Restore/build with SDK10.0.403, supply the certificate path/password using your local configuration and run only when ready for actual HTTPS checks. Do not commit the password or modify shared trust.
2. Review the complete newer configuration and its exact artifact gap before copying it. The pinned current file set intentionally demonstrates the compilation failure; a successful current build is not claimed.
3. When a matching public artifact exists, update its pin/lock and confirm the documented DirectTlsEndpoint members. Linux, OpenSSL and matching low-level native TLS runtime support must be qualified separately; Windows compilation is insufficient.
4. Register UseDirectTls and select the explicit endpoint. Keep ordinary Listen/UseHttps for the default socket control. Scope ASPNETCORE_DIRECTTLS_001 acknowledgement to that registration/selection.
5. On a qualified Linux setup, request /catalogue on both ports with the same certificate validation policy and force a TLS negotiation failure against each. Independently inspect transport selection; HTTP success alone does not establish which transport ran.
6. Record Linux distribution/architecture, OpenSSL/native/runtime versions, certificate/protocol inputs and actual outcomes before claiming support or performance. Keep ordinary socket HTTPS as the fallback until this qualification is complete.

## Constraints

Experimental Linux-only direct TLS; requires OpenSSL and matching runtime native TLS facilities. No exact supported native combination, Windows transport or GA support is established.

Only ordinary socket HTTPS compilation succeeded. The newer documented endpoint failed compilation against RC1 with CS0246; XML references to DirectTlsEndpoint do not make that public type available.

Certificates come from existing local secure configuration; no certificate trust, production resource, hardware, public package or native installation is authorized. No TLS handshake, failure/control or transport-selection check was executed.

The .NET 10 route remains valid. The current source example teaches the intended contract with an explicit availability gate, not a temporary preview substitute or a working RC1 package route.

## Evidence

The .NET 11 SDK baseline is now 11.0.100 GA with preview selection disabled. The .NET 10 comparisons retain their declared SDKs. Earlier RC audit details below are historical; use the current sample pins, dependency locks and GA verification records for current coverage. Existing native exclusions and pending Human gates still apply.

Pingu authorized documentation-based recipe delivery on 3 November 2026. Final GA, browser/provider/AG-UI, renderer/storage and Linux native TLS behaviour checks remain pending and are not claimed. These deferred checks do not block this ticket; stable release readiness remains gated.

Five available consumers compile on SDK10.0.403 or SDK11.0.100-rc.3.26522.148, with their actual locked dependencies and fresh input hashes. Current HarbourTls instead records the real CS0246 failure for missing DirectTlsEndpoint as official-review, never compilation success. The exact baselines relevant to this recipe are the two evidence records. No sample was executed.

The ordinary packed/installed tool is inspected separately over real stdio list/get, including full canonical Markdown and applicability. That verifies delivery, not the sample application. PIN-959 and PIN-964 retain broader coverage/readiness and actual-GA reconciliation.

Expected future checks: actual Linux HTTPS success and negotiation failure on both explicitly selected direct TLS and default socket endpoints; verify selected transport independently of response success, and record exact OpenSSL/runtime/native prerequisites.

## Official references

- [Pinned DirectTls documentation](https://github.com/dotnet/AspNetCore.Docs/blob/ecf7aca5136aea60aa8db7330f66ac799d3edcf7/aspnetcore/release-notes/aspnetcore-11/includes/direct-tls-transport.md)
- [Inspected DirectTls RC1 package](https://www.nuget.org/packages/Microsoft.AspNetCore.Server.Kestrel.Transport.DirectTls/11.0.0-rc.1.26425.128)
- [Kestrel endpoints](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/servers/kestrel/endpoints)
