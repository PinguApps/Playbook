# Show a confirmation once after a server-rendered redirect

## Goal

A harbour permit desk records a request and redirects to an outcome page. Carry
the confirmation through that redirect, display it once, then show “No new
outcome” on the next visit. Use framework TempData rather than inventing a
status cookie or query-string protocol. This is transient feedback, not the
authoritative permit record.

## Older approach

.NET 10 has MVC TempData, but not Blazor's ITempData cascading value or
SupplyParameterFromTempData attribute. A static SSR component can bridge to
ITempDataDictionaryFactory and explicitly save before redirecting or returning
the response. This is still valid when an application owns that integration.
MVC's normal filters do not automatically save TempData for component endpoints.

Use the previous Web SDK project and services:

net10.0/HarbourPermit.csproj:

{{snippet:old-project}}

net10.0/Program.cs registrations:

{{snippet:old-startup}}

Build the app with var app = builder.Build(), then configure its pipeline:
Program.cs imports Playbook.Samples so App resolves; its Web SDK implicit usings
supply the HTTP, configuration and dependency-injection extensions.

net10.0/Program.cs pipeline:

{{snippet:old-pipeline}}

Finish Program.cs with app.Run(). In Apply.razor, inject NavigationManager and
Microsoft.AspNetCore.Mvc.ViewFeatures.ITempDataDictionaryFactory as Factory.
In Outcome.razor inject the same Factory. The explicit save happens while
headers can still change; neither page enables streaming.

net10.0/Apply.razor body:

{{snippet:old-apply}}

net10.0/Outcome.razor body:

{{snippet:old-outcome}}

## Newer approach

.NET 11 registers Blazor TempData with AddRazorComponents. The writer receives
Microsoft.AspNetCore.Components.ITempData as a cascading parameter, writes an
outcome, and navigates. Static form POST navigation produces the real redirect.

net11.0/Apply.razor body:

{{snippet:apply}}

The receiving component binds a key with SupplyParameterFromTempData. **Measured
RC2 behavior:** its final property value is written back to TempData. Leaving a
received message in that property repeats it on the next read. Copy it to a
display field, then clear the bound property to consume the confirmation. The
canonical sample proves this through a redirect and two separate GET requests;
do not infer one-use display merely from the attribute name.

net11.0/Outcome.razor body:

{{snippet:outcome}}

Direct ITempData.Get (and the indexer) schedules a read key for deletion.
Peek reads without consuming; Keep or Keep(key) retains values deliberately.
Do not use those retention methods for a confirmation meant to disappear. The
bound-property write-back behavior is a separate concern; clearing it also
applies with the session-storage TempData provider.

## Migration

1. Keep the existing .NET 10 route until the application targets net11.0 with the
   matching Web SDK/shared framework. No newer-package/older-target route is
   claimed. The older project is a comparison, not another foundation edition.
2. Add the newer project/services and pipeline below. Move writer and reader into
   their named Razor files. Remove the MVC bridge only when it has no other users.
3. Import Microsoft.AspNetCore.Components, .Forms, .Routing and .Web plus
   Playbook.Samples in _Imports.razor; set its namespace to Playbook.Samples.
   Put the App component below in App.razor. NavigationManager is injected as
   Navigation in Apply.razor. Use the same App/imports in the previous sample.
   Add the page directives at the top of the named files: Apply.razor uses
   @page "/apply" and @inject NavigationManager Navigation; Outcome.razor uses
   @page "/outcome". On .NET 10, both pages also use
   @inject Microsoft.AspNetCore.Mvc.ViewFeatures.ITempDataDictionaryFactory Factory.
4. Keep the writer and consumer non-streaming for the default cookie provider.
   Submit /apply with its antiforgery token, follow the redirect to /outcome,
   then request /outcome again. Expect the confirmation, then “No new outcome.”
5. If work must change TempData after streaming starts, choose the optional
   session-storage route before building the app, register the session/cache
   services and middleware, and establish its cookie early. Repeat the same
   redirect/second-read check; session-backed TempData remains consumable.

net11.0/HarbourPermit.csproj:

{{snippet:project}}

Nullable and ImplicitUsings are enabled in the complete project blocks. Both projects lock their
SDK-aligned Microsoft.AspNetCore.App.Internal.Assets dependency. No explicit
NuGet package reference, workload, browser asset or native toolchain is needed
for these plain static forms. global.json pins 10.0.402 for the older capture and
11.0.100-rc.2.26504.105 for the current capture; these are test baselines, not
mandatory exact consumer SDK installations.

net11.0/Program.cs registrations:

{{snippet:startup}}

In the newer Program.cs also import Microsoft.AspNetCore.Components.Endpoints
for TempDataProviderType, plus Playbook.Samples for App.

The configuration key SessionTempData defaults to false (cookie provider). Set
it to true for the session-storage alternative. Build the app with
var app = builder.Build(), use this pipeline, then app.Run(). SkipSession is
test instrumentation and must remain false in a configured application.

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

## Constraints

Static SSR only. Interactive Server/WebAssembly/Auto execution has no current
request-backed TempData supply; properties retain their default value there.
An initial prerender does not make later circuit events an HTTP state workflow.

The default provider encrypts cookies with Data Protection. Keep messages small
and share the appropriate key ring across server instances. TempData is not
authorisation, durable storage or a safe place for an entire form draft. Cookies
and headers must be written before the response starts. On RC2, late cookie
TempData writes emit a warning and are not persisted; the streamed response can
still succeed. A successful response alone is insufficient evidence of storage.

For late writes, set TempDataProviderType.SessionStorage. This means server-side
ASP.NET Core session, not browser sessionStorage. Register AddDistributedMemoryCache
(single-process demo) or the application's compatible distributed cache,
AddSession, and UseSession before endpoints. Establish the session before the
first await that flushes: an early SupplyParameterFromSession subscription or
Session.Set can do this. A component introduced only after that flush is too late
to issue a new session cookie. Retain antiforgery and the application's cookie
consent policy; this sample does not declare its session cookie essential.

ITempData and supplied properties support the framework's scalar types (string,
int, bool, Guid, DateTime and int-backed enums), nullable forms and supported
one-dimensional arrays, lists, sets, collections and string-keyed dictionaries.
Arbitrary custom JSON classes are not supported. Keys are case-insensitive;
two supplied properties in the same render must not own the same key.

These stable APIs have no experimental opt-in. All current execution here is
RC2, not GA proof. Missing session middleware fails the static request; it is not
a harmless empty value. Concurrent requests are not an exactly-once delivery
transaction: this recipe proves sequential read consumption, not atomic receipt
or replay prevention. Use application storage when that stronger guarantee matters.

## Evidence

Original samples compile as net10.0/C# 14.0 using SDK 10.0.402 and
net11.0/C# 15.0 using SDK 11.0.100-rc.2.26504.105. The saved records are **compile**
evidence. The separate real Kestrel HTTP report follows a 302, checks the first
and second reads under both newer providers, and compares the valid older MVC
bridge. A held streaming response proves a fresh session cookie arrives before
release, then the late session write survives. Late cookie TempData is absent;
session-backed TempData appears once. An independent cold request proves a new
session cannot be established after headers, and the missing-middleware app
fails instead of fabricating state.

The offline package test uses the existing real installed-tool feed/cache and
unrelated working directory. It compares complete rendered Markdown with the
canonical inputs, selection requirements and edition 11.0, and rejects target
10.0. See the repository report docs/verification/PIN-776.md and its fingerprinted
PIN-776.http.evidence.json for commands/artifacts. No mocks, cross-compilation,
browser UI, multi-server provider, interactive or GA execution is claimed.

Official source review: [Blazor server state](https://learn.microsoft.com/en-us/aspnet/core/blazor/state-management/server?view=aspnetcore-11.0),
[older session and TempData](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/app-state?view=aspnetcore-10.0),
[RC2 bound-property persistence](https://github.com/dotnet/aspnetcore/blob/4a70870818bf5662f21b0c28f9a8c267dccb77f0/src/Components/Endpoints/src/TempData/TempDataCascadingValueSupplier.cs)
and [RC2 streaming persistence](https://github.com/dotnet/aspnetcore/blob/4a70870818bf5662f21b0c28f9a8c267dccb77f0/src/Components/Endpoints/src/DependencyInjection/TempDataService.cs).
