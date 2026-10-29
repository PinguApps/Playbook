# Streaming and form protection evidence

## Application-owned origin policy

PIN-1172 authoring observations, **29 October 2026**. This is a separate
SeedExchange experiment; the historical QuayPermits observations below remain
unchanged. Canonical source lives in `samples/net10.0/SeedExchange` and
`samples/net11.0/SeedExchange`. The recipe is
`web.supply-origin-protection-policy/11.0`. Fresh sample records identify the
actual inputs/commands and compile/HTTP results; they do not label HTTP clients
as browsers. The original story is Fern Ledger's botanical exchange.

Exact sets: SDK10.0.402/C#14/framework10.0.13 and
SDK11.0.100-rc.2.26504.105/C#15/framework11.0.0-rc.2.26504.105. Roslyn versions
are 5.9.0-1.26505.111 and 5.12.0-1.26504.105. Actual loaded framework build
commits are `4099f83977f4d29604a9aeb971fb0a7bc1d5f699` and
`820230c386db5b0106c5430b3e5aee92a7826bf5`. Source intent remains pinned separately
to ASP.NET `4a70870818bf5662f21b0c28f9a8c267dccb77f0`. No NuGet libraries,
optional workloads or separately acquired native assets resolve for these pure
Minimal API projects; Internal.Assets in the Razor comparison is not the public
ICsrfProtection contract. .NET10's real consumer observes the interface absent.

The actual reference bytes were compared with the bounded web asset audit:

| Target | Http.Abstractions reference SHA-256 | Default-builder reference SHA-256 |
| --- | --- | --- |
| net10.0 | `0a9cf8792bd7d8ab7e9b8d31e9f144ad20f8fb4d28a822d9dc12242a04c62930` | `a06ee8f3ceb3a96ea99e0bbc7b920d74fd664a50a7fa52b7cc3e52fc59c8e61c` |
| net11.0 | `164a21d5a05f7ab9f1dad2ddb8451caac71a27556998e11a2f146cbfa2e9beb4` | `3e0894ac29b8d7ff5af11d882e442250eeb22ea8b92a072d9cc8675687f53bef` |

`eng/Record-SeedExchange.ps1` uses absolute matching SDK paths, locked restore,
`eng/Record-Evidence.ps1` and before/after input fingerprints. It executes real
Kestrel HTTP requests in four modes: .NET10 retained tokens+origin, .NET11 custom,
standard and retained tokens. Records include loaded runtime assembly paths,
informational versions and SHA-256 values. Missing/opaque Origin, invalid operator
configuration, token omission, JSON separation and exact provider/handler counts
are actual HTTP results. The isolated listener stops after each run. Data
Protection is ephemeral, never a user/production key ring. No persistent PATH or
user/machine environment change is needed.

The parent separately controlled an owned tab through the user's existing Chrome
extension: **Chrome156.0.0.0, Windows x64**. The integrated browser connection was
unavailable. Each host used its sample working directory, the matching absolute
dotnet executable and `--urls 'http://localhost:6180;http://localhost:6181;http://localhost:6183'`.
The .NET11 comparison switches were `--standard true` and `--tokens true`;
.NET10 always retained tokens and the explicit origin check. The served
`wwwroot/controls.html?mode=custom|standard|tokens|old` button ran ordinary iframe
form submissions and browser fetch with `omit`/`include`. JavaScript never sets
Origin or Sec-Fetch-Site. The target was localhost:6180; sources were that origin,
trusted localhost:6181, untrusted localhost:6183 and hostile 127.0.0.1:6181.

All **four sources × four modes × ten** base controls passed. Two additional
own-origin token-negative controls passed in each retained-token mode. An earlier
.NET10 own-origin run was repeated while adding those controls: **174 actual
browser POSTs observed**, including that repeat; 164 distinct final control
combinations. The browser reported same-origin / same-site / cross-site as expected.
Server observations supplied status and handler counts even when CORS hid the
response. The custom provider counter was exactly one for every custom browser
form/fetch; a denial returned400/handler0, an acceptance200/handler1. No broad
authentication/security claim is inferred from these disposable counters.

| Browser form source | Custom RC2, all four paths | Standard RC2 `/seed`, credentialed, public, wildcard | .NET10 token+origin, all four paths | .NET11 token mode, all four paths |
| --- | --- | --- | --- | --- |
| localhost:6180 / same-origin | 200/1 | 200/1 each | 200/1 | 200/1 |
| localhost:6181 / trusted same-site | 200/1 | 400/0, 200/1, 400/0, 400/0 | 200/1 | 200/1 |
| localhost:6183 / untrusted same-site | 400/0 | 400/0 each | 400/0 | 200/1 with valid pair |
| 127.0.0.1:6181 / cross-site | 400/0 | 400/0 each | 400/0 | 400/0 (cookie/token mismatch) |

For trusted partner fetch, credentialed CORS exposed both omit/include responses;
the demo Lax cookie was absent/present respectively. Noncredentialed and wildcard
CORS exposed omit responses, but include threw a browser TypeError **after** the
custom handler executed200/1. Standard policy instead returned400/0 on those
noncredentialed/wildcard paths, regardless of actual credential mode. Hostile
custom fetches returned400/0 even when wildcard omit exposed the rejection.
Same-site untrusted submissions carried the demo cookie but still failed; cross-site
Lax-cookie requests had no cookie, so those alone prove no production credentialed
cross-site behavior. The cookie is demonstration state, not authentication.

Missing/invalid token values with the own-origin cookie returned400/0 in both
token modes. Fetch omit with a token also returned400/0, proving the cookie pair
remained required. The .NET11 token-mode acceptance from localhost:6183 shows the
standard automatic verdict can be overwritten; it is not a composed custom+token
claim. Headerless non-browser HTTP requests returned200/1 under standard RC2,
400/0 under custom RC2 and .NET10 explicit-origin policy. JSON executed200/1 with
zero custom provider calls; that endpoint has no automatic form protection.

The harness removes temporary forms/iframes. The owned browser tab was closed and
only owned loopback hosts were stopped. No account, certificate trust, external
resource, persistent key store or production data was involved. HTTP loopback
does not qualify production HTTPS/proxies, another browser/OS, arbitrary form
consumers, authenticated-user behavior or GA. PIN-959/PIN-964 and existing Human
gates remain open. The ordinary installed-tool inspection in the Inspector
walkthrough verifies delivery independently of these browser observations.

Historical October 2026 authoring observations. Follow the [evidence reuse policy](../contributing.md#choose-the-check-and-retain-its-result).
[Original capture and detailed provenance](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-804-808.md).

## Actual artifacts and behavior

The four consumers are under `samples/net10.0` and `samples/net11.0`, in
`QuayChat` and `QuayPermits`. Each has its own build/package boundaries and
locked restore. SDKs are **10.0.402** and **11.0.100-rc.2.26504.105**;
language modes are **14.0** and **15.0**; actual framework/runtime versions are
**10.0.13** and **11.0.0-rc.2.26504.105**. No workload or account is needed.
Compilation records retain their actual source commit and dirty marker, input
hashes, resolved assets, exact commands and output. They are compilation records,
separate from browser and HTTP execution.

QuayChat uses AI.Abstractions **10.6.0**, selecting its **net10.0** asset on both
targets. The new renderer is the actual Components.AI
**0.1.0-preview.1.26466.103** package, built from
`dotnet/dotnet@b2f96bd7ac75c07fb8067c05c54767d3d5e12e8d`.
It has only a net11.0 compile asset. The SDK prunes its framework-provided
Components.Web dependency. No net10.0 Components.AI route is claimed.
**Components.AI remains experimental throughout .NET 11**, in discovery and detail.

[Chat observations](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-804-808/chat.browser.json) record actual Chrome 155
extension interactions with both Interactive Server consumers, with prerendering
disabled. Each first fragment remains held until Release reply. The real new
RichContentBlock retains its ID while changing Active to Inactive; the second
stream keeps the first exchange visible and receives three input messages.
The older manual IChatClient loop does the same useful task. The deliberate
script-like text remains ordinary Razor text with no script descendant.
The stream source is explicitly scripted; this is real renderer/stream evidence,
not model, remote provider, AG-UI or model-quality evidence.

![Actual held experimental renderer](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-804-808/streaming.png)

[Seventy real HTTP checks](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-804-808/http.evidence.json), produced by
`eng/Test-QuaySamples.ps1`, cover:

- SSR, form-bound Minimal API, MVC and Razor Pages: same-origin acceptance,
  cross-origin rejection, valid token pairs and missing-token rejection.
- Additional-data acceptance/rejection, token emission, headerless automatic
  requests, Fetch Metadata precedence and unchanged JSON behavior.
- Explicit credentialed CORS trust on /trusted; other origins remain rejected.
- A valid token can override an automatic origin verdict in this RC2 pipeline.
  This control uses a deliberately supplied valid pair; it does not claim an
  attacker can obtain that pair.

[Browser form observations](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-804-808/forms.browser.json) separately record
actual form clicks, same-origin requests, cross-site submissions from localhost
to 127.0.0.1, credentialed CORS trust, and missing/custom-data token failures.
Temporary form action/token changes were made through the supported browser
developer interface, then reset by navigation. No server protection was bypassed.
The browser's origin-scoped network capture drops cross-origin headers.
For empty MVC/Page 400 responses, Chrome displays ERR_BLOCKED_BY_CLIENT;
[matching Kestrel status lines](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-804-808/browser-kestrel.txt) establish the
server's 400 response. The Minimal API response names CsrfValidationException.

The recorded DLL hashes still match the final matching-SDK builds. The final
.NET 10 chat browser check was refreshed after whitespace normalization.
Other rebuilt DLLs stayed byte-identical to their browser-tested versions.

## Reconciled source and applicability

[Pinned official source review](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-804-808/sources.evidence.json) is distinct
from compilation and execution. It establishes two material boundaries:

1. The downloadable experimental package supplies the streaming renderer used
   here. Later tool, approval, typed state and durable restore goals remain
   separate work; no unavailable API is invented for this delivery.
2. Current migration prose overstates the checked RC2 MVC/Page integration.
   MVC must use **RequireAntiforgeryToken** for the middleware route.
   ValidateAntiForgeryToken still runs a token filter; combining the attributes
   is rejected. Razor Pages still installs its automatic token filter, so the
   Page retains its token even in the otherwise automatic sample.

Keep token services, middleware, issuance and validation for older browsers,
additional token data or compliance requirements. RC2 can overwrite the automatic
verdict with the token verdict; coexistence alone does not establish two
independent enforced layers. Explicit credentialed CORS origins are write trust.
Wildcard/public-read CORS does not grant that trust in the pinned implementation.
JSON endpoints are unchanged. Authentication, authorization, production cookies,
proxy configuration and application storage are outside these local demonstrations.
