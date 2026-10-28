# Resume token acquisition without legacy redirect members

## Goal

A hydrophone archive requests a scope to read a recording. An available token
continues without navigation. A missing grant initiates interaction with the actual
request scopes and a fixed application-owned return location. This example stops at
that handoff: the fixture neither signs in a user nor downloads protected content.

## Older approach

The legacy three-argument AccessTokenResult constructor and RedirectUrl getter
exist in .NET 10 and are removed in RC2. The old producer/consumer buttons isolate
those obsolete calls with narrow CS0618 suppressions. They perform ordinary
navigation and cannot carry interactive options.

The supported four-argument constructor, InteractiveRequestUrl, InteractionOptions
and NavigateToLogin already work on .NET 10; the same app exercises that valid path.
Keep the success token branch first. Existing exception callers still use Redirect
or Redirect(configure). There is a real fallback difference: .NET 10 Redirect()
without options reads RedirectUrl. The four-argument constructor does not fill that
legacy property, so the `modern-fallback` control catches its actual
ArgumentNullException; the legacy no-options producer still navigates.

Create a separate project directory and save every file below. Empty local build
files prevent accidental inheritance. The provider is deliberately controlled;
replace it with the application's real provider when integrating the consumer.

### .NET 10.0: `global.json`

{{snippet:n10-global-json}}

### .NET 10.0: `Directory.Build.props`

{{snippet:n10-directory-build-props}}

### .NET 10.0: `Directory.Build.targets`

{{snippet:n10-directory-build-targets}}

### .NET 10.0: `Directory.Packages.props`

{{snippet:n10-directory-packages-props}}

### .NET 10.0: `HydrophoneArchive.csproj`

{{snippet:n10-hydrophonearchive-csproj}}

### .NET 10.0: `Program.cs`

{{snippet:n10-program-cs}}

### .NET 10.0: `App.razor`

{{snippet:n10-app-razor}}

### .NET 10.0: `ArchiveProvider.cs`

{{snippet:n10-archiveprovider-cs}}

### .NET 10.0: `wwwroot/index.html`

{{snippet:n10-wwwroot-index-html}}

## Newer approach

The current producer passes the actual InteractiveRequestOptions as the fourth
argument, including GetToken, scopes and the validated app-owned return location.
Successful results deliberately have null interaction URL/options. The consumer
uses both current properties with NavigateToLogin, rather than navigating to a URL
alone. AccessTokenNotAvailableException has no new InteractiveRequestUrl property:
its public constructor and both Redirect overloads are unchanged. Its RC2
parameterless fallback now navigates to the result's InteractiveRequestUrl even
without options; that fallback still carries no interaction state or scopes.

The complete current app below uses a real WebAssembly NavigationManager. Button
labels make the controlled provider boundary visible; no sample token is displayed.

### .NET 11.0: `global.json`

{{snippet:n11-global-json}}

### .NET 11.0: `Directory.Build.props`

{{snippet:n11-directory-build-props}}

### .NET 11.0: `Directory.Build.targets`

{{snippet:n11-directory-build-targets}}

### .NET 11.0: `Directory.Packages.props`

{{snippet:n11-directory-packages-props}}

### .NET 11.0: `HydrophoneArchive.csproj`

{{snippet:n11-hydrophonearchive-csproj}}

### .NET 11.0: `Program.cs`

{{snippet:n11-program-cs}}

### .NET 11.0: `App.razor`

{{snippet:n11-app-razor}}

### .NET 11.0: `ArchiveProvider.cs`

{{snippet:n11-archiveprovider-cs}}

### .NET 11.0: `wwwroot/index.html`

{{snippet:n11-wwwroot-index-html}}

## Migration

1. Inspect custom AccessTokenResult producers and direct RedirectUrl callers together; leave ordinary exception call sites intact.
2. Retain TryGetToken and the successful API-request path before deciding interaction is required. Never expose the token in the UI or logs.
3. Construct an app-owned return URI and validate externally supplied destinations before use. Pass the actual requested scopes and GetToken options to the four-argument result; use intentional nulls on success.
4. For RequiresRedirect, use InteractiveRequestUrl **and** InteractionOptions with NavigateToLogin. Preserve provider-specific options rather than rebuilding only a URL.
5. Retain Redirect() for existing exception callers. Use the configuring overload only when real options exist. Test .NET 10's legacy no-options fallback and RC2's current-property fallback separately.
6. Build each project from its own directory using its pinned SDK. Serve the built WASM static assets beneath `/archive/` with an index fallback and the supplied base href; the authoring check reads the SDK's actual endpoint manifest. No DevServer/Gateway migration is required by this recipe.
7. Exercise success, direct interaction, both exception overloads and no-options fallback in a browser; inspect history.state.userState. Only a real configured identity provider can establish issuance and protected API access.

## Constraints

Only the IAccessTokenProvider boundary is controlled; the matching WASM runtime and NavigationManager execute in Chromium. No external provider, issued token, authorized API response or downloaded recording is claimed.

Use TryGetToken before RequiresRedirect; successful results need no navigation. A current interactive result needs both InteractiveRequestUrl and InteractionOptions.

ReturnUrl is constructed from a fixed application-owned route and checked against BaseUri. Do not accept an unvalidated external or query-supplied return location.

.NET 10 parameterless Redirect reads legacy RedirectUrl when options are absent. A four-argument no-options result has no legacy URL and fails; RC2 reads InteractiveRequestUrl instead.

Redirect(Action<InteractiveRequestOptions>) requires actual options; do not use it for a no-options result. Existing exception signatures remain unchanged.

The exact test pins are SDK 10.0.402 / framework and packages 10.0.13 / C# 14,
and SDK 11.0.100-rc.2.26504.105 / framework and packages
11.0.0-rc.2.26504.105 / C# 15. Runtime/framework/package/SDK requirements are
separate from these measured baselines. No optional workload or native compilation
is required. Public metadata is not an experimental designation; no experimental
suppression is used. No net10-plus-new-package compatibility route is advertised.

## Evidence

Both actual targets compile with locked resolved dependencies through
eng/Record-Evidence.ps1. Their selected behavior.evidence.json records retain
inputs, actual commands, browser results and measured Razor compiler hashes.
eng/Record-TokenAssetBehavior.ps1 runs the focused real checks using
eng/Test-TokenAssetConsumers.mjs; neither cross-compilation nor metadata inspection
stands in for the claimed execution. Installed stdio inspection checks both
tools, discovery ID/title/goal, the full canonical body, explicit 11.0 edition,
applicability and current evidence from the exact locally installed ordinary tool.
Offline guidance is embedded; inspection does not contact a provider.

The evidence is preparation against RC2, not final GA qualification. PIN-959's
comprehensive delivery gate and PIN-964's actual-GA reconciliation remain open.
