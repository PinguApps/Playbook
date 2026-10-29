# Remove reliance on obsolete browser token binding

## Goal

Remove abandoned RFC 8471 token-binding observation when modernizing authentication middleware, while preserving HTTPS enforcement, cookie authentication and the application's authorization policy.

The Night archive admits a curator with a valid protected cookie. Its old optional TLS token-binding observation never influenced that decision; the newer middleware removes the observation and keeps the actual policy. Establish this fact for your own app before deleting a binding check. RFC 8471 was not adopted by browsers; the obsolete API is not a usable browser protection. Do not silently weaken an application whose security design requires proof of possession.

## Older approach — optional observation on .NET 10

This valid historical middleware tolerates the absent feature. Cookie authentication and the curator policy decide access, with HTTPS required before either. The request checks use real Kestrel feature absence; no fake token-binding implementation is supplied.

Save the following files under the indicated names in a separate directory. Restore once to create a lock file from the exact direct pins; commit that lock for repeatable builds.

### Directory.Build.props

{{snippet:n10-directory-build-props}}

### Directory.Build.targets

{{snippet:n10-directory-build-targets}}

### Directory.Packages.props

{{snippet:n10-directory-packages-props}}

### global.json

{{snippet:n10-global-json}}

### NightArchive.csproj

{{snippet:n10-nightarchive-csproj}}

### Program.cs

{{snippet:n10-program-cs}}

## Newer approach — preserve the policy without token binding

Only the obsolete observation disappears. UseAuthentication, UseAuthorization, RequireRole and the HTTP rejection remain. The self-contained executable issues its own protected tickets solely to check the policy; production sign-in remains your application's responsibility.

Save the following files under the indicated names in a separate directory. Restore once to create a lock file from the exact direct pins; commit that lock for repeatable builds.

### Directory.Build.props

{{snippet:n11-directory-build-props}}

### Directory.Build.targets

{{snippet:n11-directory-build-targets}}

### Directory.Packages.props

{{snippet:n11-directory-packages-props}}

### global.json

{{snippet:n11-global-json}}

### NightArchive.csproj

{{snippet:n11-nightarchive-csproj}}

### Program.cs

{{snippet:n11-program-cs}}

## Migration steps

1. Find every ITlsTokenBindingFeature read and establish whether it is optional observation or an actual security requirement. Inventory the real cookie/token handler, authorization policies and HTTPS/proxy handling first.

2. Recompile the historical lookup against the real .NET11 reference pack. It produces ASPDEPR010; removal is a migration away from an abandoned protocol, not a feature rename.

3. Remove the optional feature lookup and dead observation code. Keep the application's authentication handler, permission checks and HTTPS enforcement. The sample deliberately rejects plain HTTP; preserve your app's established redirect/proxy behavior instead of copying a test policy blindly.

4. Exercise allowed, authenticated-but-forbidden, missing and invalid credential requests over real HTTPS. Exercise HTTP separately. Only the allowed curator may reach the handler; Kestrel feature absence must never bypass authorization.

5. If security originally relied on token binding, stop this bounded removal and select and verify a supported end-to-end mechanism appropriate to the threat model. RFC5929 channel binding and HTTP.sys hardening belong to a different protocol; do not mechanically substitute TryGetChannelBindingBytes. This recipe claims no replacement binding protection.

## Constraints

Application route: ASP.NET Core11 with its matching SDK; C#15 is the tested language mode; the middleware needs no C#15-only syntax and no extra NuGet library. Exact reproduction uses SDK10.0.402/framework10.0.13 versus SDK11.0.100-rc.2.26504.105/framework11.0.0-rc.2.26504.105. The source audit's Http.Features/MVC references and F# MVC template artifacts are reconciled separately from these runtime checks. Compiler, runtime, selected package assets, commands and input hashes are retained in the existing sample-evidence records.

Both executables check HTTPS200 for a curator, HTTPS403 for a visitor, HTTPS401 for missing/invalid tickets, HTTP400 even with a curator ticket, and exactly one handler invocation. The older Kestrel feature lookup returns absent on all four HTTPS requests. Recompiling it against RC2 reproduces ASPDEPR010; the newer source has no obsolete lookup. The certificate is generated for the fixture and the client accepts only its exact hash; ephemeral keys, cookies and listeners do not alter machine trust or persistent configuration.

No browser rendering or browser support claimed. Real HttpClient requests over TLS and HTTP. Windows x64 is the executed lane. No package-native assets, workloads, external provider, resource provisioning or public publication are required. Stable migration guidance is based on RC2 evidence; actual-GA reconciliation and comprehensive readiness remain open. The eight approved .NET10 foundations and existing recipe/edition identities remain unchanged.

## Evidence and local verification

Fresh sample records contain actual compilation and the five real request outcomes on both runtime sets. The newer record also includes the isolated ASPDEPR010 diagnostic control; metadata inspection alone is not the authentication proof.

From each complete sample directory, use its matching absolute dotnet executable to restore, build in Release and run the resulting DLL. The default execution performs real request checks and exits. Retain the initial lock and use restore --locked-mode for later checks.

## Official references

- [Obsolete token-binding feature](https://github.com/dotnet/aspnetcore/blob/4a70870818bf5662f21b0c28f9a8c267dccb77f0/src/Http/Http.Features/src/ITlsTokenBindingFeature.cs)
- [Official deprecation reason](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.http.features.itlstokenbindingfeature?view=aspnetcore-11.0)
- [RFC 8471 TLS Token Binding](https://www.rfc-editor.org/rfc/rfc8471)
- [RFC 5929 channel bindings](https://www.rfc-editor.org/rfc/rfc5929)
