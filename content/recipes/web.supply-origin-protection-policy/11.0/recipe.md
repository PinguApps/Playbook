# Supply an application-specific cross-origin verdict

## Goal

Fern Ledger lets a botanical exchange's own portal and one reviewed partner offer
seeds. Another page on the same site must not gain permission just because its
hostname is related. The application owner maintains an exact origin list and
uses `ICsrfProtection` to apply that decision to protected form submissions.
An origin includes scheme, host and port; **same-site is not same-origin**.

Use the ordinary policy from `web.protect-browser-form-posts` when it already
meets the application's requirements. This recipe changes the policy rather than
adding a second default-policy example. That comparison recipe is not a build
dependency, and its stable identity and edition are unchanged.

## Constraints: applicability and artifacts

The custom service requires **ASP.NET Core 11** with the Web SDK's implicit
`Microsoft.AspNetCore.App` framework reference and a form consumer that enforces
`IAntiforgeryMetadata.RequiresValidation`. A form-bound Minimal API provides that
consumer here. No optional workload, native package, cloud provider, Blazor
rendering mode or experimental diagnostic opt-in is needed. The actual public
contracts have no `ExperimentalAttribute`. RC2 testing does not qualify actual GA.

The measured baselines are SDK **10.0.402**, C# **14.0**, frameworks **10.0.13**,
and SDK **11.0.100-rc.2.26504.105**, C# **15.0**, frameworks
**11.0.0-rc.2.26504.105**. These are reproduction pins, separate from minimum
release-family prerequisites. The examples use primary constructors/collection
expressions but require no C#15-only feature. Saved records identify actual
Roslyn versions, reference hashes and loaded framework/CoreLib paths, product
versions and SHA-256 values. Both reference packs reconcile with the bounded web
audit; runtime execution independently establishes the callable behavior.

Both these non-Razor consumers restore **zero NuGet libraries**. The Web SDK and
shared framework supply the contracts. `Microsoft.AspNetCore.App.Internal.Assets`
is not the assembly containing `ICsrfProtection`, nor a required direct package
for this sample. QuayPermits' Razor assets do not establish a dependency here.
The previous consumer confirms the interface is absent on actual .NET10; no
package backport or older-target custom-service route is advertised.

Execution was Windows x64, Chrome156 and disposable HTTP loopback hosts. Production
origins must use HTTPS; the validator permits HTTP only for loopback reproduction.
These observations do not establish other browsers, operating systems, reverse
proxies, deployment topologies, trimming, NativeAOT or MVC/Razor Pages/SSR behavior.
PIN-959 comprehensive coverage, PIN-964 actual-GA reconciliation and existing
Human gates stay open. This comparison adds no ninth .NET10 foundation.

## Migration

1. Identify every browser origin permitted to act on a signed-in user's behalf.
   Put exact, browser-serialized origins in operator-owned configuration. Review
   ownership/security of the whole trusted site, including hosted user content.
   Never construct this list from a request's Host, Origin or Referer. Configure
   deployment origins explicitly, including ports; the localhost values are fixtures.
2. Retain the working .NET10 token-and-origin route below. On .NET11, register
   `ExchangeProtection` as `ICsrfProtection` before `Build`. The singleton holds
   immutable configuration; it keeps no per-request decision. The observation
   counter is demonstration instrumentation, not part of the trust rule.
3. The custom rule permits safe GET/HEAD/OPTIONS and exact listed origins for
   writes. Missing, opaque `null`, duplicate, combined, wildcard and unlisted
   origins do not match. It deliberately ignores Fetch Metadata and CORS when
   deciding write trust. A normal modern browser sends Origin for these POSTs;
   clients that cannot do that need a separately designed token-required path.
   Configuration rejects empty lists, wildcards, non-loopback HTTP and paths.
4. Keep automatic antiforgery metadata on form-bound endpoints. The RC2 middleware
   calls the DI service and records its verdict; it does not itself terminate every
   request. The form-binding consumer returns 400 before the handler for a denial.
   Merely attaching metadata to arbitrary handlers is not proof of enforcement.
   Do not use `DisableAntiforgery` to make a rejected cookie-backed form work.
5. Configure CORS separately for browser response access. The comparison has an
   explicit credentialed origin, the same origin without credentials, and a
   wildcard policy. The custom list may allow a simple form to execute even when
   CORS prevents JavaScript reading its result. Align both policies deliberately;
   CORS read failure does not undo an accepted write. Wildcard read permission
   alone grants no custom trust.
6. Keep token-dependent clients, additional token data and compliance requirements
   on token-required paths. `--tokens true` runs a **separate** .NET11 token mode
   with the standard provider. RC2 token middleware can overwrite its automatic
   verdict, so this mode is not two independently enforced origin/token layers.
   The .NET10 origin middleware explicitly rejects before token validation and
   therefore retains both checks. Do not mix legacy token filters into an assumed
   universal automatic-policy migration; see the standard recipe for those limits.
7. Retain real authentication, authorization, input validation and HTTPS separately.
   This sample has no identity, permissions or database and must not be deployed
   as a complete exchange. Non-browser clients can forge headers; an allowed
   origin is not a credential. This rule neither prevents XSS at a trusted site
   nor supplies authorization. JSON has no form metadata and stays unchanged;
   secure JSON clients/endpoints through their own explicit application design.
8. Compile and execute both consumers, then exercise the browser controls below.
   Remove local diagnostics from production. Their counters prove actual handler
   invocation; source review, mocked requests and compilation alone cannot replace
   observed browser-controlled headers or CORS enforcement.

## Evidence: observed comparison

The target is `http://localhost:6180`; the partner is `http://localhost:6181`.
`localhost:6183` is an untrusted same-site control and `127.0.0.1:6181` is a
cross-site control. Actual Chrome headers were `same-origin`, `same-site` and
`cross-site` respectively. Here **200/1** means HTTP200 and one handler invocation;
**400/0** means HTTP400 and zero handler invocations.

| Submission to protected form | Standard RC2 | Custom RC2 | .NET10 with valid token pair |
| --- | --- | --- | --- |
| Own origin | 200/1 | 200/1 | 200/1 |
| Partner, ordinary `/seed` | 400/0 | 200/1 | 200/1 |
| Partner, explicit credentialed CORS | 200/1 | 200/1 | 200/1 |
| Partner, noncredentialed or wildcard CORS | 400/0 | 200/1 | 200/1 |
| Untrusted same-site or cross-site origin, every policy | 400/0 | 400/0 | 400/0 |
| Missing both Origin and Fetch Metadata (real HTTP client) | 200/1 | 400/0 | 400/0 |

Every custom browser form/fetch called the DI provider exactly once, including
denials. The standard comparison's custom counter is zero; this does not claim
that the built-in provider was never invoked. JSON executed once with zero custom
calls. Real non-browser HTTP checks cover missing and opaque Origin; Chrome owns
its security headers, so forged headers are not presented as old-browser proof.

For partner fetches, explicit credentialed CORS exposed the response with both
`credentials: 'omit'` and `'include'`. The latter carried the demonstration cookie;
the former did not. Noncredentialed/wildcard policies exposed responses only with
`omit`. With `include`, Chrome blocked reading even when custom policy had already
accepted the POST and invoked its handler. Hostile origins failed every custom
write, including when wildcard CORS exposed the 400 response. Cross-site Lax
cookies were absent; the rejection counter independently establishes enforcement.
The demonstration cookie is not an authenticated user and proves no production
cookie configuration or authentication integration.

Tokens remain a separate boundary: missing/invalid browser token values and
cookie omission returned 400 with zero handlers. The .NET11 token-mode control
accepted a valid pair from unlisted same-site localhost:6183 despite the standard
automatic verdict, illustrating the overwrite boundary. This does not prove a
custom-origin-plus-token combination. The existing form-protection evidence
document retains compact browser observations; sample records retain fresh
compilation/real HTTP evidence and input fingerprints.

## Complete standalone examples

Create the following files in separate .NET10 and .NET11 directories. The empty
Directory files are deliberate inheritance boundaries. `RequestChecks` is the
actual-request verification entry point used by `--verify true`; it starts a
disposable Kestrel listener and stops it afterwards. Appsettings uses JSON comments
only for canonical snippet markers, which the configuration loader accepts.

## Older approach: keep tokens and enforce explicit origins

`global.json`:
{{snippet:n10-global-json}}

`Directory.Build.props`:
{{snippet:n10-directory-build-props}}

`Directory.Build.targets`:
{{snippet:n10-directory-build-targets}}

`Directory.Packages.props`:
{{snippet:n10-directory-packages-props}}

`SeedExchange.csproj`:
{{snippet:n10-seedexchange-csproj}}

`appsettings.json`:
{{snippet:n10-appsettings-json}}

`ExchangeOrigins.cs`:
{{snippet:n10-exchangeorigins-cs}}

`Observations.cs`:
{{snippet:n10-observations-cs}}

`SeedOffer.cs`:
{{snippet:n10-seedoffer-cs}}

`Program.cs`:
{{snippet:n10-program-cs}}

`RequestChecks.cs`:
{{snippet:n10-requestchecks-cs}}

## Newer approach: supply the custom service

`global.json`:
{{snippet:n11-global-json}}

`Directory.Build.props`:
{{snippet:n11-directory-build-props}}

`Directory.Build.targets`:
{{snippet:n11-directory-build-targets}}

`Directory.Packages.props`:
{{snippet:n11-directory-packages-props}}

`SeedExchange.csproj`:
{{snippet:n11-seedexchange-csproj}}

`appsettings.json`:
{{snippet:n11-appsettings-json}}

`ExchangeOrigins.cs`:
{{snippet:n11-exchangeorigins-cs}}

`ExchangeProtection.cs`:
{{snippet:provider}}

`Observations.cs`:
{{snippet:n11-observations-cs}}

`SeedOffer.cs`:
{{snippet:n11-seedoffer-cs}}

`Program.cs`:
{{snippet:n11-program-cs}}

`RequestChecks.cs`:
{{snippet:n11-requestchecks-cs}}

## Reproduce requests and browser controls

From the sample directory, use its matching absolute SDK executable to build
Release, then run `bin/Release/net11.0/SeedExchange.dll --verify true` (substitute
net10.0 for the older sample). Expect PASS lines with exact response/handler/DI
counts and actual assembly identities. Add `--standard true` or `--tokens true`
only for their .NET11 comparison modes. The repository's
`eng/Record-SeedExchange.ps1` captures all four execution modes with locked restore
and PATH preservation; it does not manufacture browser proof.

For browser checks, run the app with
`--urls 'http://localhost:6180;http://localhost:6181;http://localhost:6183'`.
Kestrel's localhost binding also serves 127.0.0.1; do not add a duplicate binding.
Open `http://localhost:6180/form` and submit. For a partner/hostile ordinary form,
use a form on that source origin targeting the target URL, with input name `seed`.
Retained-token modes also need the issued hidden field and matching cookie.

The sample's optional `wwwroot/controls.html` and `controls.js` automate those
ordinary iframe forms and fetches; use the same two files with either sample:

`wwwroot/controls.html`:
{{snippet:browser-html}}

`wwwroot/controls.js`:
{{snippet:browser-js}}

Open `/controls.html?mode=custom` on each of the four origins above and click
**Run checks**. Expect PASS, ten form/fetch observations, exact Origin/FetchSite,
response and handler/DI counts. Repeat against a fresh .NET11 host with
`--standard true` and `mode=standard`, and `--tokens true` with `mode=tokens`.
Run the .NET10 host with `mode=old`. Token modes add two negative controls on the
own-origin page. The controls remove temporary forms/iframes. Stop the host with
Ctrl+C; no seed records or persistent key ring are created. Preserve the trusted
origin configuration rather than allowing all origins to suppress failures.

## Official references

- [Public ICsrfProtection contract](https://github.com/dotnet/aspnetcore/blob/4a70870818bf5662f21b0c28f9a8c267dccb77f0/src/Http/Http.Abstractions/src/Antiforgery/ICsrfProtection.cs).
- [Actual automatic middleware and token overwrite boundary](https://github.com/dotnet/aspnetcore/blob/4a70870818bf5662f21b0c28f9a8c267dccb77f0/src/DefaultBuilder/src/Internal/CsrfProtectionMiddleware.cs).
- [Default-policy comparison](https://github.com/dotnet/aspnetcore/blob/4a70870818bf5662f21b0c28f9a8c267dccb77f0/src/DefaultBuilder/src/Internal/DefaultCsrfProtection.cs).
- [ASP.NET Core 11 migration guidance](https://learn.microsoft.com/en-us/aspnet/core/migration/100-to-110?view=aspnetcore-11.0#automatic-csrf-protection).

Source intent is pinned to `4a70870818bf5662f21b0c28f9a8c267dccb77f0`; actual
framework build commits are independently recorded. Moving documentation or a
package version alone cannot qualify a different artifact or deployment.
