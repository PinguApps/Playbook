# Simplify browser form protection while retaining necessary tokens

## Goal

A quay desk accepts cargo permit forms. Simplify startup where automatic browser-origin checks protect those form consumers, while retaining the token checks required by other paths. A 400 response to an untrusted form is a successful defense, not a reason to disable it.

## Older approach

The original .NET 10 app registers antiforgery and runs token middleware. Its static Blazor form, form-bound Minimal API, MVC token filter and Razor Page all require the matching form/cookie token pair. The custom additional-data provider also validates a fictional permit format marker. This is a valid approach on both releases, particularly for older browsers, custom token data and independent compliance requirements.

Save every file below in a standalone QuayPermits directory. The empty Directory files isolate parent build/package settings. There are no hidden template files or application data stores. Pages/Permit.cshtml requires its shown @page directive before its snippet body.

#### .NET 10: `global.json`

{{snippet:n10-global-json}}

#### .NET 10: `Directory.Build.props`

{{snippet:n10-directory-build-props}}

#### .NET 10: `Directory.Build.targets`

{{snippet:n10-directory-build-targets}}

#### .NET 10: `Directory.Packages.props`

{{snippet:n10-directory-packages-props}}

#### .NET 10: `QuayPermits.csproj`

{{snippet:n10-quaypermits-csproj}}

#### .NET 10: `Program.cs`

{{snippet:n10-program-cs}}

#### .NET 10: `FormDocument.cs`

{{snippet:n10-formdocument-cs}}

#### .NET 10: `PermitController.cs`

{{snippet:n10-permitcontroller-cs}}

#### .NET 10: `App.razor`

{{snippet:n10-app-razor}}

#### .NET 10: `Pages/Permit.cshtml`

Start this file with `@page "/page"` as its first line, followed immediately by this canonical body. Razor rejects an HTML comment before @page, so that directive precedes the source snippet marker.

{{snippet:n10-pages-permit-cshtml}}

#### .NET 10: `Pages/Permit.cshtml.cs`

{{snippet:n10-pages-permit-cshtml-cs}}

## Newer approach

WebApplication.CreateBuilder automatically wires the .NET 11 CSRF middleware. It records a verdict for endpoints with IAntiforgeryMetadata requiring validation. Protected form consumers enforce the verdict; a JSON endpoint does not become form-protected. Without explicit token middleware, static SSR stops emitting tokens and form-bound Minimal APIs use the automatic check.

**Use the API actually present in this RC2 artifact.** MVC opts in with [RequireAntiforgeryToken], replacing [ValidateAntiForgeryToken]. The latter still runs a token filter; combining both attributes is rejected. The checked Razor Pages provider still inserts AutoValidateAntiforgeryTokenAttribute, so the Page deliberately retains its token and cookie. Removing UseAntiforgery alone does not migrate that filter. Current migration prose describes broader integration than this pinned artifact proves.

The --keep-tokens true mode retains token generation and middleware for SSR, Minimal APIs and the opted-in MVC action, plus the Page's retained filter. --reject-additional true deliberately rejects otherwise valid token pairs. Keep those paths when tokens are required; this is a real additional-data check, not a mock token validator.

#### .NET 11: `global.json`

{{snippet:n11-global-json}}

#### .NET 11: `Directory.Build.props`

{{snippet:n11-directory-build-props}}

#### .NET 11: `Directory.Build.targets`

{{snippet:n11-directory-build-targets}}

#### .NET 11: `Directory.Packages.props`

{{snippet:n11-directory-packages-props}}

#### .NET 11: `QuayPermits.csproj`

{{snippet:n11-quaypermits-csproj}}

#### .NET 11: `Program.cs`

{{snippet:n11-program-cs}}

#### .NET 11: `FormDocument.cs`

{{snippet:n11-formdocument-cs}}

#### .NET 11: `PermitController.cs`

{{snippet:n11-permitcontroller-cs}}

#### .NET 11: `App.razor`

{{snippet:n11-app-razor}}

#### .NET 11: `Pages/Permit.cshtml`

Start this file with `@page "/page"` as its first line, followed immediately by this canonical body. Razor rejects an HTML comment before @page, so that directive precedes the source snippet marker.

{{snippet:n11-pages-permit-cshtml}}

#### .NET 11: `Pages/Permit.cshtml.cs`

{{snippet:n11-pages-permit-cshtml-cs}}

## Migration

1. Inventory actual form consumers and why they use tokens. Keep token services, middleware and hidden fields when supporting older browsers that may omit origin metadata, using IAntiforgeryAdditionalDataProvider, or satisfying an independent compliance rule. Both protections can be present, but this RC2 token middleware can overwrite the automatic verdict; do not claim two independently enforced layers from their presence alone.
2. For the eligible net11.0 SSR/Minimal API paths, omit explicit AddAntiforgery and UseAntiforgery. AddRazorComponents and MVC/Pages still register services needed by their consumers. For MVC, use [RequireAntiforgeryToken] to select the middleware route. Retain Razor Pages' working token path until the actual framework's filter integration is verified.
3. Review CORS as write trust. The sample's /trusted form endpoint deliberately permits only http://localhost:6019 with AllowCredentials. This exact credentialed origin can submit; another origin receives 400. Replace the fictional local origin with an explicitly trusted production origin after reviewing who can act with ambient credentials. Wildcard or anonymous public-read CORS does not grant this trust.
4. Do not remove a failing check by adding DisableAntiforgery or IgnoreAntiforgeryToken to a browser/cookie endpoint. An exemption needs an actual authentication/exposure reason, such as a separately verified bearer-only boundary. This sample exempts no form endpoint.
5. Restore with each pinned SDK. From a sample directory run dotnet run --project QuayPermits.csproj -c Release --no-restore --urls http://127.0.0.1:6013. Add --keep-tokens true for the retained-token .NET 11 mode. The .NET 10 consumer always retains tokens.
6. Submit /, /minimal, /mvc and /page from their own origin. Each returns the corresponding accepted: lanterns text. Without token middleware, only the retained Razor Page emits a token. Send a real form from another origin to those endpoints and expect 400; /trusted accepts only the named credentialed CORS origin. In token mode, remove the request token to confirm 400 even on a same-origin submission.
7. POST JSON with cargo set to lanterns to /json. It returns JSON accepted: lanterns even with a cross-origin header, preserving the existing non-form behavior. Authorization and business validation are separate responsibilities.

## Constraints

Automatic protection accepts safe methods; Sec-Fetch-Site same-origin or none; or a named, explicitly credentialed CORS origin. Other nonempty Fetch Metadata values, including same-site, are rejected unless that CORS trust applies. Without Fetch Metadata it compares Origin with scheme/host/port. With neither header it allows the request as a non-browser client. These headers are browser signals, not authentication; a scriptable HTTP client can supply them. Review trusted proxy/host configuration in a real deployment.

The local hosts intentionally have no accounts, authentication or application writes. Their accept/reject responses demonstrate form enforcement, not a protected production workflow. A token on one host does not grant another origin access. Preserve token issuance and validation together, including the cookie and custom additional-data policy, wherever required.

The ordinary MVC token filter and automatic Razor Page filter remain token-based in the checked RC2 source. Do not generalize current documentation over this measured difference or silently discard those protections. JSON-only endpoints remain outside this form migration. Other browsers, reverse proxies, distributed Data Protection, production cookie policies and GA artifacts are not verified here.

## Evidence

Actual .NET 10/11 consumers compile with their declared SDKs. Seventy real Kestrel HTTP checks cover same-origin/cross-origin forms on all four paths, missing tokens, valid token pairs, rejected additional data, Fetch Metadata precedence, headerless automatic requests, named CORS trust and unchanged JSON. Those HTTP probes deliberately supply Origin headers; separate real Chrome form clicks establish actual browser submissions.

Browser checks cover automatic SSR/Minimal API/MVC, the retained Page, token-enabled old/new consumers, rejected missing/custom-data tokens and the allowed CORS origin. Chrome replaces some empty MVC/Page 400 responses with ERR_BLOCKED_BY_CLIENT; matching Kestrel status lines retain the actual rejection evidence. No security interstitial was bypassed. See docs/verification/streaming-and-form-protection.md for exact artifacts, receipts and limitations.

The existing packed-tool harness installs the exact local package with an isolated feed/cache, runs real offline stdio list/get outside the checkout, compares the complete expanded Markdown and nested applicability, and checks edition 11.0 plus target-10 refusal. The .NET 10 source is a comparison, not a newly published ninth foundation or an unsupported older-target route.
