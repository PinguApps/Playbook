# Resume a server-rendered task across pages

## Goal

A harbour permit applicant chooses a berth, reviews it, then returns to make
another change. Keep the berth and step between independent page requests.
Reading progress must not delete it. A one-use outcome belongs in TempData;
the draft belongs in session until it expires or the application clears it.

## Older approach

.NET 10 supports ASP.NET Core ISession in static SSR through the cascaded
HttpContext. Read and write values explicitly, including the missing-value
default. This remains a valid approach when the application needs direct control
of keys or serialization. It has no built-in SupplyParameterFromSession attribute.

net10.0/Progress.razor body:

{{snippet:old-progress}}

net10.0/Review.razor body:

{{snippet:old-review}}

Both pages use HttpContext from Microsoft.AspNetCore.Http, supplied by the Web
SDK implicit usings. Progress.razor injects NavigationManager as Navigation.
PermitInput is a public sealed class in namespace Playbook.Samples with the
single public nullable string property Berth { get; set; }. Its form property
is initialized in OnInitialized only when form binding did not supply a model;
do not overwrite a posted edit with the stored draft.

shared/PermitInput.cs:

{{snippet:input}}

net10.0/HarbourPermit.csproj:

{{snippet:old-project}}

net10.0/Program.cs registrations:

{{snippet:old-startup}}

The older sample also registers MVC TempData for its separate confirmation
comparison. Session itself needs only the cache and AddSession registrations.
After var app = builder.Build(), use this pipeline and finish with app.Run():

net10.0/Program.cs pipeline:

{{snippet:old-pipeline}}

## Newer approach

.NET 11 binds stored values directly to component properties with
SupplyParameterFromSession. The form still binds submitted input separately;
the handler commits it into the session-backed properties, advances the step
and redirects. The review component reads the same keys on the next request.

net11.0/Progress.razor body:

{{snippet:progress}}

net11.0/Review.razor body:

{{snippet:review}}

The framework serializes the final supplied-property values for session storage.
They survive subsequent reads. Use one owner per key in a rendered page; repeated
pages can use the same keys on different requests. Nullable values can clear a
bound value; a non-nullable int naturally shows zero when no saved value exists.

## Migration

1. Keep the previous-release project as a working ISession comparison. Target
   net11.0 and the matching Web SDK/shared framework for the new attribute;
   no newer-package/net10.0 route is claimed.
2. Register a cache and AddSession before Build. Run UseSession before component
   endpoints, with antiforgery retained. Preserve the application's consent,
   timeout, secure-cookie and multi-server storage choices.
3. Create PermitInput as described above. Import Microsoft.AspNetCore.Components,
   .Forms, .Routing and .Web plus Playbook.Samples in _Imports.razor, and set its
   namespace to Playbook.Samples. Put the shown root in App.razor and the page
   implementations in Progress.razor and Review.razor. Inject NavigationManager
   as Navigation on Progress. Use the same root/imports in the older project.
   At the top of Progress.razor add @page "/progress" and
   @inject NavigationManager Navigation. Review.razor starts with @page "/review".
4. Submit berth North-12 at /progress. Follow /review twice: both reads must
   show step 1 and North-12. Submit East-4 from /progress and expect step 2.
   A separate browser/cookie jar must start at step 0 with no berth.
5. When streaming, create the subscription before the first asynchronous flush.
   Hold the work, inspect the response cookie, release it, and read the result in
   a new request. Do not assume a child created after the flush can establish
   a fresh session. Keep one-use confirmation handling separate.

net11.0/HarbourPermit.csproj:

{{snippet:project}}

Nullable and ImplicitUsings are enabled in the complete project blocks. The canonical previous/current SDK pins are 10.0.402 and
11.0.100-rc.2.26504.105, C# 14.0/15.0. The Web SDK supplies matching framework
assemblies and its aligned Internal.Assets dependency; there are no explicit
NuGet packages, native workloads or browser scripts required for these forms.

net11.0/Program.cs registrations:

{{snippet:startup}}

Program.cs imports Playbook.Samples for App and, on .NET 11,
Microsoft.AspNetCore.Components.Endpoints for TempDataProviderType. The Web SDK
implicit usings supply HTTP, configuration and dependency-injection extensions.

Build with var app = builder.Build(), apply the pipeline below and finish with
app.Run(). SessionTempData chooses storage only for the separate TempData
confirmation; it does not change the persistence of session-supplied properties.
SkipSession is a negative-check switch and must remain false in the application.

net11.0/Program.cs pipeline:

{{snippet:pipeline}}

shared/App.razor:

{{snippet:app}}

Both standalone projects use the complete imports below; no repository build
files, CSS, JavaScript, layout, client project or package references are needed.
The project blocks enable nullable and implicit usings explicitly.

shared/_Imports.razor:

One directive per line: `@using Microsoft.AspNetCore.Components`; `@using Microsoft.AspNetCore.Components.Forms`; `@using Microsoft.AspNetCore.Components.Routing`; `@using Microsoft.AspNetCore.Components.Web`; `@using Playbook.Samples`; `@namespace Playbook.Samples`.

Use these capture pins as each project's global.json. Restore creates the
SDK-aligned asset lockfile; subsequent builds may use RestoreLockedMode=true.
The pins reproduce this evidence, not a universal exact consumer requirement.

net10.0/global.json:

{"sdk":{"version":"10.0.402","rollForward":"disable"}}

net11.0/global.json:

{"sdk":{"version":"11.0.100-rc.2.26504.105","rollForward":"disable"}}

### Streaming check

The following original check component subscribes before awaiting a local gate.
An initially empty browser receives its session cookie in the first response
headers. After release, the step changes while headers have already started.
A later /review read must show step 7. In the previous sample, explicit Session.Set
before the await establishes that cookie instead.

net11.0/Stream.razor body:

{{snippet:stream}}

net10.0/Stream.razor body:

{{snippet:old-stream}}

For this local check only, put InspectionGate in namespace Playbook.Samples,
and start Stream.razor with @page "/stream", @attribute [StreamRendering] and
@inject InspectionGate Gate. These directives apply to both release comparisons.
Register it with builder.Services.AddSingleton<InspectionGate>() before Build,
and map POST /control/arm to gate.Arm() and POST /control/release to gate.Release().
These test endpoints never belong in production. The held work has no external
provider; the request's cancellation token releases it if the client disconnects.

shared/InspectionGate.cs class:

{{snippet:gate}}

## Constraints

Static SSR only. Interactive Server/WebAssembly/Auto execution does not supply
request-backed session values; an initial prerender is not evidence of support
in later circuit events. A missing session in static SSR fails fast: both
AddSession and UseSession are required. Place the middleware around endpoints;
adding it after the endpoints execute cannot repair the request.

Cookie/header timing matters under streaming. Subscribe or call Session.Set
before the first await that can flush a response. An existing browser cookie may
hide a late-establishment bug; verify a fresh cookie jar too. The early .NET 11
subscription registers session establishment before headers are sent. The
late state write can then be committed by session middleware after rendering.
This does not allow late cookie TempData writes: select its session-storage
provider for that separate use case. Session-storage TempData still consumes
reads; ordinary session does not.

Session is temporary, per browser cookie and not necessarily per signed-in user.
AddDistributedMemoryCache stores the canonical draft in one server process;
restarts, expiration or cookie loss lose it. A real multi-server application
needs its own compatible distributed cache/session affinity design. ISession
is non-locking; overlapping writes can overwrite progress. A durable draft,
authentication or concurrent conflict policy needs application storage beyond
this recipe. No distributed provider is provisioned or executed here.

Keys are case-insensitive in the supplier. Distinct supplied properties in the
same rendered request must not own the same key. Supported scalar types include
string, int, bool, Guid, DateTime and int-backed enums, their nullable forms and
supported one-dimensional arrays, lists, sets, collections and string-keyed
dictionaries. Arbitrary JSON-serializable domain classes are not supported by
the stored-data serializer. Do not write incompatible raw ISession bytes under
an attribute-owned key during migration; clear or migrate the old representation.

Session cookies follow normal consent/security settings. Do not make them
essential just to hide missing state. Use HTTPS and the application's cookie
policy in production. These stable contracts require no experimental opt-in;
current RC2 evidence is not GA verification.

## Evidence

Actual net10.0/C# 14.0 and net11.0/C# 15.0 Web SDK projects compile with their
declared SDKs and locked assets. Saved compilation evidence is separate from
the real Kestrel request report. That report submits two steps, reads each
draft repeatedly, proves cookie-jar isolation, observes initial streaming bytes
and the early cookie before releasing a held write, then reads step 7. It also
proves missing session middleware fails with HTTP 500 and that a new session
cannot be established after headers. The TempData checks prove its contrasting
one-use behavior through a real redirect and second read under both providers.

The existing packed-tool harness installs from an isolated local feed/cache,
calls real stdio list/get outside the checkout, and checks the complete rendered
canonical body, applicability and edition 11.0 offline. Target 10.0 is rejected
for this new edition. docs/verification/redirect-and-session.md and the fingerprinted
[HTTP execution capture](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-776.http.evidence.json) retain exact commands/artifacts. No mock,
cross-compilation, browser UI, interactive, multi-server or GA execution is claimed.

Official source review: [Blazor state](https://learn.microsoft.com/en-us/aspnet/core/blazor/state-management/server?view=aspnetcore-11.0),
[session fundamentals](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/app-state?view=aspnetcore-10.0),
[RC2 session supplier](https://github.com/dotnet/aspnetcore/blob/4a70870818bf5662f21b0c28f9a8c267dccb77f0/src/Components/Endpoints/src/SessionCascadingValueSupplier.cs)
and [early cookie establishment](https://github.com/dotnet/aspnetcore/blob/4a70870818bf5662f21b0c28f9a8c267dccb77f0/src/Components/Endpoints/src/SessionEstablishmentHelper.cs).
