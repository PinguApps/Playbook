# Streaming and form protection evidence

Historical authoring observations from October 2026. Compilation stays in the
sample records; these observations do not establish a new run, broader platform
support or GA verification. Reuse only while the claim and relevant inputs remain
valid; see [capture and refresh](../contributing.md#capture-and-refresh-sample-evidence).
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
