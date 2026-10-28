# Construct a custom remote authentication service with logging

## Goal

An astronomy club's WASM badge portal maps an account badge to the club:badge claim and requests observing-list access. Forward the real typed logger while preserving account mapping, options and token outcomes.

## Older approach

The .NET 10 subclass forwards four services to the obsolete constructor. The five-argument constructor is already supported there: SupportedBadgeAuthentication supplies its real logger, and the browser check resolves and exercises that alternative too. Account mapping and options remain the same. CS0618 is suppressed only around the obsolete constructor fixture.

Ordinary applications consuming the framework service through DI need no manual constructor rewrite. This guidance applies to custom subclasses and explicit factories.

Save these complete files at the shown paths in a fresh .NET 10 directory. Local Directory files isolate parent build/package policy. Restore once to generate the dependency lock; retain it for locked builds. The repository also retains the actual resolved locks.

### .NET 10: global.json

{{snippet:n10-global-json}}

### .NET 10: Directory.Build.props

{{snippet:n10-directory-build-props}}

### .NET 10: Directory.Build.targets

{{snippet:n10-directory-build-targets}}

### .NET 10: Directory.Packages.props

{{snippet:n10-directory-packages-props}}

### .NET 10: AstronomyBadges.csproj

{{snippet:n10-astronomybadges-csproj}}

### .NET 10: Program.cs

{{snippet:n10-program-cs}}

### .NET 10: App.razor

{{snippet:n10-app-razor}}

### .NET 10: BadgeAuthentication.cs

{{snippet:n10-badgeauthentication-cs}}

### .NET 10: BadgePrincipalFactory.cs

{{snippet:n10-badgeprincipalfactory-cs}}

### .NET 10: SupportedBadgeAuthentication.cs

{{snippet:n10-supportedbadgeauthentication-cs}}

### .NET 10: wwwroot/index.html

{{snippet:n10-wwwroot-index-html}}

### .NET 10: wwwroot/controlled-auth.js

{{snippet:n10-wwwroot-controlled-auth-js}}

## Newer approach

The fifth parameter is exactly ILogger<RemoteAuthenticationService<RemoteAuthenticationState, BadgeAccount, OidcProviderOptions>>. ILogger<BadgeAuthentication> is a different type. The framework permits null, but forwarding the actual DI logger preserves JavaScript debug/trace configuration instead of discarding diagnostics merely to compile.

BadgePrincipalFactory retains the authenticated account mapping. AuthenticationStateProvider, IRemoteAuthenticationService and IAccessTokenProvider resolve to the same scoped custom instance. The controlled JavaScript adapter returns a fictional badge and toggled token result; it never contacts identity.example.invalid. It demonstrates actual WASM service integration, not external login or token issuance. Do not ship that adapter as authentication.

Create a separate .NET 11 directory with these files. The stable replacement already existed before .NET 11; removal of the obsolete API motivates this migration.

### .NET 11: global.json

{{snippet:n11-global-json}}

### .NET 11: Directory.Build.props

{{snippet:n11-directory-build-props}}

### .NET 11: Directory.Build.targets

{{snippet:n11-directory-build-targets}}

### .NET 11: Directory.Packages.props

{{snippet:n11-directory-packages-props}}

### .NET 11: AstronomyBadges.csproj

{{snippet:n11-astronomybadges-csproj}}

### .NET 11: Program.cs

{{snippet:n11-program-cs}}

### .NET 11: App.razor

{{snippet:n11-app-razor}}

### .NET 11: BadgeAuthentication.cs

{{snippet:n11-badgeauthentication-cs}}

### .NET 11: BadgePrincipalFactory.cs

{{snippet:n11-badgeprincipalfactory-cs}}

### .NET 11: wwwroot/index.html

{{snippet:n11-wwwroot-index-html}}

### .NET 11: wwwroot/controlled-auth.js

{{snippet:n11-wwwroot-controlled-auth-js}}

## Migration

1. Find custom subclasses/factories. Retain their exact state/account/provider generic arguments, existing AddOidcAuthentication settings, auth paths and account mapping.
2. Add and forward the fifth parameter: ILogger of the exact generic framework service. Explicit factories resolve that logger from their real provider. Do not substitute a differently typed logger or null just to silence compilation.
3. Keep scoped options, logging and the matching account factory. Alias the three service interfaces to the same custom scoped instance instead of creating separate authentication-state instances.
4. Compile the actual old/current dependency sets. The obsolete constructor is only migration evidence; the five-argument path works in both measured sets. Retain mapped claims and successful versus interaction-required token outcomes.
5. For the local harness keep controlled-auth.js before Blazor. Check service identity, mapped badge, logger flags, success scopes and interaction-required scopes/return URL. The fake token grants no access.
6. In an existing real provider integration retain matching framework AuthenticationService.js instead of this adapter, plus existing auth routes and registered callbacks. Verify external-provider sign-in/token behaviour separately before claiming it.
7. In each fresh sample run dotnet restore, then dotnet publish -c Release -p:RestoreLockedMode=true with its pinned SDK. Serve publish/wwwroot with SPA fallback and correct JavaScript/WASM MIME types. This interpreted proof requires no optional AOT workload.

## Constraints

- Current proof: SDK **11.0.100-rc.2.26504.105**, **net11.0**, C# **15**, WASM/Authentication/browser runtime **11.0.0-rc.2.26504.105**. Previous proof: SDK **10.0.402**, **net10.0**, C# **14**, matching packages/browser runtime **10.0.13**. Exact test pins and full locked dependencies are separate from release-family consumer requirements.
- Browser-hosted interactive WASM only. No server cookie logout, server-side Identity, static SSR execution, authorization enforcement, new authentication system or production resources.
- Stable supported replacement APIs; no experimental warning or preview-language opt-in. RC2 proof does not establish GA. No older-target/newer-package route or ninth .NET 10 foundation is added.
- The controlled adapter returns a non-authorizing token string. Real framework service, scoped options/logging, account factory and browser interop execute; provider authentication and token issuance do not. This recipe is independent of the logout-provider fixture.
- A mapped badge grants no authorization. Keep required server/API checks and the real provider integration intact.

## Evidence

Both actual target/package sets compile with zero warnings/errors. Compilation and execution records use the existing input-hash/freshness workflow. No mock, source review or cross-compilation is substituted for actual execution.

Chromium resolves real DI aliases, maps night-observer to club:badge and exercises successful versus requiresRedirect requests with observations.read and the intended return URL. It observes JS initialization/logger flags: the legacy four-argument service lacks logging; the .NET 10 supported alternative and current service preserve it. These are controlled-integration observations, not external-provider proof.

For authoring reproduction, install the locked Node fixture with npm ci --prefix samples/WetlandIdentity --ignore-scripts --no-audit --no-fund, invoking npm's CLI through an absolute Node executable. Run eng/Record-WasmAuthentication.ps1 with absolute Chromium, Node and both .NET executable paths. Disable .NET first-run PATH integration with DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=0 in child environments. The script publishes the four consumers, starts one local provider/flow, records four browser execution records and closes its owned servers/browser. Ports 5980–5982 must be free; it never stops another listener or provisions an external account. Exact commands and measured Node/browser/provider versions accompany the evidence.

The ordinary package is installed from a new local-only feed/cache. Actual stdio list/get from an unrelated directory uses explicit target 11.0 and verifies ID/title/goal, complete canonical Markdown, applicability, fresh evidence and text/structured equality. Unusable proxies establish proxy-independent offline retrieval; no new OS network-isolation claim is made. Comprehensive PIN-959 and actual-GA PIN-964 gates remain open.
