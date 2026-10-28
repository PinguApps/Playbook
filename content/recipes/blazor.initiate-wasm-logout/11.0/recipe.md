# Initiate WebAssembly logout through supported navigation

## Goal

A wetland permit desk signs a visitor out and returns to its public permit list. Preserve the provider logout handshake, callback and local return location while replacing legacy session setup.

## Older approach

The .NET 10 migration fixture injects SignOutSessionStateManager, calls SetSignOutState(), then navigates to the logout component. AddOidcAuthentication supplies the framework registration. Remove any explicit manager registrations/factories too during migration. The entire type disappears in .NET 11: its public (IJSRuntime) constructor, virtual ValueTask SetSignOutState() and virtual Task<bool> ValidateSignOutState().

These methods are not universally no-ops. The old manager writes session storage; the old authenticator reads/removes it only when history state is null. A missing marker rejects logout. The second .NET 10 button already uses supported NavigateToLogout with an explicit local return path, avoiding direct manager calls. That cleanup is valid before upgrading. CS0618 is suppressed only at the deliberate injected property and legacy call.

Save these complete files at the shown paths in a fresh .NET 10 directory. Local Directory files isolate parent build/package policy. Restore once to generate the dependency lock; retain it for locked builds. The repository also retains the actual resolved locks.

### .NET 10: global.json

{{snippet:n10-global-json}}

### .NET 10: Directory.Build.props

{{snippet:n10-directory-build-props}}

### .NET 10: Directory.Build.targets

{{snippet:n10-directory-build-targets}}

### .NET 10: Directory.Packages.props

{{snippet:n10-directory-packages-props}}

### .NET 10: WetlandPermits.csproj

{{snippet:n10-wetlandpermits-csproj}}

### .NET 10: Program.cs

{{snippet:n10-program-cs}}

### .NET 10: App.razor

{{snippet:n10-app-razor}}

### .NET 10: Authentication.razor

{{snippet:n10-authentication-razor}}

### .NET 10: Permits.razor

{{snippet:n10-permits-razor}}

### .NET 10: Probe.razor

Local verification only: the invalid/missing buttons supply deliberate failure inputs; the default button exercises the other supported logout overload. Exclude this probe page from a deployed app.

{{snippet:n10-probe-razor}}

### .NET 10: wwwroot/index.html

{{snippet:n10-wwwroot-index-html}}

## Newer approach

The current component calls NavigateToLogout with the logout route and the local permits return path. The extension puts an InteractiveRequestOptions SignOut entry into browser history before navigation. RemoteAuthenticatorView uses the real service and completes the registered provider callback. The overload with only NavigationManager and logoutPath also works: its destination comes from AuthenticationPaths.LogOutSucceededPath, configured to permits here.

Both measured releases reject a well-formed non-null history entry for the wrong interaction before provider logout. With null history, .NET 10 invokes the session-storage validation fallback; RC2 no longer uses that manager and lets the no-state flow proceed. Keep using NavigateToLogout. The observed RC2 null-state behaviour is a compatibility difference, not a recommended workaround or a custom CSRF protection scheme.

Create a separate .NET 11 directory with these files. The stable replacement already existed before .NET 11; removal of the obsolete API motivates this migration.

### .NET 11: global.json

{{snippet:n11-global-json}}

### .NET 11: Directory.Build.props

{{snippet:n11-directory-build-props}}

### .NET 11: Directory.Build.targets

{{snippet:n11-directory-build-targets}}

### .NET 11: Directory.Packages.props

{{snippet:n11-directory-packages-props}}

### .NET 11: WetlandPermits.csproj

{{snippet:n11-wetlandpermits-csproj}}

### .NET 11: Program.cs

{{snippet:n11-program-cs}}

### .NET 11: App.razor

{{snippet:n11-app-razor}}

### .NET 11: Authentication.razor

{{snippet:n11-authentication-razor}}

### .NET 11: Permits.razor

{{snippet:n11-permits-razor}}

### .NET 11: Probe.razor

Use this local verification page for the same invalid/missing/default checks on the current release. It is not a production sign-out flow.

{{snippet:n11-probe-razor}}

### .NET 11: wwwroot/index.html

{{snippet:n11-wwwroot-index-html}}

## Migration

1. Inspect the actual target, packages, provider registration and authentication routes. Match managed WASM/Authentication and AuthenticationService.js releases; shared-framework metadata does not substitute for these packages.
2. Retain AddOidcAuthentication with the existing registered provider. Configure its exact login-callback and logout-callback origins and RP-initiated logout. Replace this fixture's localhost authority/client with your application registration for real use.
3. Remove direct manager injections/registrations/factories and SetSignOutState/ValidateSignOutState calls. Use NavigateToLogout with an application-owned local return path. Do not accept arbitrary user input as the return URL.
4. Keep authentication/{Action} mapped to RemoteAuthenticatorView, matching authentication JS loaded before Blazor, and the public destination route. Configure LogOutSucceededPath intentionally when using the no-explicit-return overload.
5. Test actual sign-in, logout, provider confirmation, callback, final public route and signed-out identity. A new login should require provider interaction. Test invalid non-null and missing-history behaviour separately on each release.
6. Build each sample with its pinned SDK: run dotnet restore, then dotnet publish -c Release -p:RestoreLockedMode=true. Serve publish/wwwroot as a SPA with index.html fallback and JavaScript/application-wasm MIME types. The authoring runner supplies that host; opening index.html as a file cannot run it.

## Constraints

- Current proof: SDK **11.0.100-rc.2.26504.105**, **net11.0**, C# **15**, WASM/Authentication/browser runtime **11.0.0-rc.2.26504.105**. Previous proof: SDK **10.0.402**, **net10.0**, C# **14**, matching packages/browser runtime **10.0.13**. Exact test pins and full locked dependencies are separate from release-family consumer requirements.
- Browser-hosted interactive WASM only. No server cookie logout, server-side Identity, static SSR execution, authorization enforcement, new authentication system or production resources.
- Stable supported replacement APIs; no experimental warning or preview-language opt-in. RC2 proof does not establish GA. No older-target/newer-package route or ninth .NET 10 foundation is added.
- One loopback oidc-provider 9.5.1 runs real discovery, code/PKCE, signed ID tokens, provider sessions and RP-initiated logout, with a fixed fictional account and ephemeral in-memory state/keys. HTTP is bound to loopback only. This is not production identity infrastructure, external-provider verification or a security assessment. A fake remote service could establish only component branching and is not the logout proof used here.
- Provider session logout, cached application identity, token revocation and API authorization are separate. This bounded result does not promise universal single sign-out or revocation of every issued token. Keep your provider's HTTPS, redirect and session policies.

## Evidence

Both actual target/package sets compile with zero warnings/errors. Compilation and execution records use the existing input-hash/freshness workflow. No mock, source review or cross-compilation is substituted for actual execution.

Chromium exercises .NET 10 legacy session logout, supported navigation on both, the no-explicit-return overload, invalid non-null state on both, missing-state rejection on .NET 10 and missing-state provider logout on RC2. It records history, sign-out session operations, request shapes, callback and public return, without retaining token/code/cookie values. Accepted logout requires a fresh provider interaction on the next sign-in.

For authoring reproduction, install the locked Node fixture with npm ci --prefix samples/WetlandIdentity --ignore-scripts --no-audit --no-fund, invoking npm's CLI through an absolute Node executable. Run eng/Record-WasmAuthentication.ps1 with absolute Chromium, Node and both .NET executable paths. Disable .NET first-run PATH integration with DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=0 in child environments. The script publishes the four consumers, starts one local provider/flow, records four browser execution records and closes its owned servers/browser. Ports 5980–5982 must be free; it never stops another listener or provisions an external account. Exact commands and measured Node/browser/provider versions accompany the evidence.

The ordinary package is installed from a new local-only feed/cache. Actual stdio list/get from an unrelated directory uses explicit target 11.0 and verifies ID/title/goal, complete canonical Markdown, applicability, fresh evidence and text/structured equality. Unusable proxies establish proxy-independent offline retrieval; no new OS network-isolation claim is made. Comprehensive PIN-959 and actual-GA PIN-964 gates remain open.
