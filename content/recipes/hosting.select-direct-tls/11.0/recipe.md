# Select HTTPS while direct TLS is unavailable (Experimental)

## Goal

Use ordinary socket HTTPS when the direct-TLS documentation is withdrawn and the independently published package cannot supply its former endpoint API.

This is usable documentation-based guidance, authorized before full testing. Read the exact availability and evidence limits below; catalogue target 11 is limited to the recorded checks.

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

GA documentation removed the direct-TLS include; NuGet index offers only RC1, not 11.0.0. RC1 lacks the former documented DirectTlsEndpoint. Current canonical code is ordinary socket HTTPS, not a direct-TLS implementation. Linux/OpenSSL transport selection and negotiation remain not-tested under the native policy. Actual supported direct-TLS artifact/scenario availability remains a PIN-967 stable-gate condition.

The former source-only endpoint illustration is withdrawn from the current sample. Historical code and CS0246 evidence remain at fc07b364cbf83d8a4b6285bbc39f23240adcdb0d. Do not fabricate DirectTlsEndpoint or infer support from UseDirectTls registration. Both examples now retain the supported socket fallback.

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

1. Save the complete file set and restore its exact pins.
2. Supply CertificatePath and CertificatePassword from local secure configuration.
3. Run ordinary socket HTTPS on loopback port 5443; no trust change is needed.
4. Revisit direct TLS only after an authoritative supported scenario and matching actual package become available. Do not substitute the withdrawn endpoint code.

## Constraints

Direct TLS remains an experimental, unavailable route for the former documented endpoint. The retained RC1 dependency is an inspection baseline only. Ordinary socket HTTPS does not establish direct transport selection, Linux/OpenSSL support or negotiation behaviour. Do not use the withdrawn illustration as a supported implementation.

## Evidence

GA documentation removed the direct-TLS include; NuGet index offers only RC1, not 11.0.0. RC1 lacks the former documented DirectTlsEndpoint. Current canonical code is ordinary socket HTTPS, not a direct-TLS implementation. Linux/OpenSSL transport selection and negotiation remain not-tested under the native policy. Actual supported direct-TLS artifact/scenario availability remains a PIN-967 stable-gate condition.

Both socket examples compile with their declared SDKs. The retained RC1 package is not called. No Linux/OpenSSL or direct transport handshake is claimed. Documentation removal was checked against current canonical source; the public package index still lists RC1 alone. Historical RC proof remains in Git. PIN-967 retains final stable readiness.

## Official references

- [Pinned DirectTls documentation](https://github.com/dotnet/AspNetCore.Docs/blob/ecf7aca5136aea60aa8db7330f66ac799d3edcf7/aspnetcore/release-notes/aspnetcore-11/includes/direct-tls-transport.md)
- [Inspected DirectTls RC1 package](https://www.nuget.org/packages/Microsoft.AspNetCore.Server.Kestrel.Transport.DirectTls/11.0.0-rc.1.26425.128)
- [Kestrel endpoints](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/servers/kestrel/endpoints)
