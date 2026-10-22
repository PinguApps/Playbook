# Wait for visit checks before accepting a form

## Goal

An observatory checks whether a visit pass is still open before recording a group.
Show progress while a remote check runs, cancel a superseded field check, and
accept a submission only after its own validation pass finishes.

## Older approach

.NET 10 can await a business service in EditForm.OnSubmit, own a
ValidationMessageStore and publish field errors explicitly. A CancellationTokenSource
and operation identity prevent an older field result overwriting a newer edit.
This remains a valid design when an application needs that ownership; synchronous
validation events must not become async void handlers pretending to be awaited.

{{snippet:old-model}}

{{snippet:old-checks}}

{{snippet:old-form}}

## Newer approach

.NET 11 connects AsyncValidationAttribute to DataAnnotationsValidator. EditForm
awaits EditContext.ValidateAsync before calling OnValidSubmit or OnInvalidSubmit.
EditContext.Validate is obsolete in .NET 11; migrate owned submit validation to
awaited ValidateAsync instead of suppressing its diagnostic or blocking on a Task.
The attribute passes the framework token into HTTP I/O, associates failure with
the member and leaves an empty value to Required. A synchronous caller receives
an explicit exception rather than a fabricated successful remote check.

{{snippet:model}}

{{snippet:async-rule}}

{{snippet:form}}

## Migration

1. Create the release-specific Web SDK project and public model shown below.
2. Register the validation, localization and rule services before building the app; configure culture before validation executes.
3. Wire the shown form or endpoint adapter, then exercise both accepted and rejected input through the actual host.

The complete previous/current comparison uses these group-size formatters too:

{{snippet:old-formatter}}

{{snippet:formatter}}

Use the project/services below. Include the custom PartyLimit attribute from
validation.translate-model-feedback, or remove that separate localization rule
when demonstrating only pass availability. The loopback VisitRule belongs in
the DI container, never in an attribute constructor.

For Razor imports use Microsoft.AspNetCore.Components, .Forms, .Routing and .Web,
plus Playbook.Samples. MapRazorComponents<App>().AddInteractiveServerRenderMode()
after UseAntiforgery and MapStaticAssets; App contains a Router/RouteView and the
_framework/blazor.web.js script resolved through Assets. A page at /static renders
BookingEditor normally. A page at /interactive renders
<BookingEditor @rendermode="InteractiveServer" />; import the static RenderMode
members. SupplyParameterFromForm binds the static POST to Model. FormName is visit.

The parameterless IsValidationPending/IsValidationFaulted methods describe the
form pass. Their field/expression overloads describe a field operation. Check both
when displaying overall progress; the sample disables submission for either and
disables editing during a form pass. Subscribe to OnValidationStateChanged to
refresh progress, and unsubscribe when disposed.

DataAnnotationsValidator already starts tracked asynchronous field validation.
For a separate custom validator component, keep OnFieldChanged synchronous and
call Context.RegisterAsyncFieldValidator(field, token => CheckFieldAsync(token)).
Register submit work in OnValidationRequested with args.AddAsyncValidator(...).
Never also register the same attribute check manually. Pass cancellation into I/O,
check it before publishing messages, clear stale messages and notify after changes.
The form pass supersedes outstanding field work; it does not simply trust an
earlier field success. Rules may run concurrently; never depend on ordering.


## Project setup

The newer canonical project uses Microsoft.NET.Sdk.Web, TargetFramework net11.0,
LangVersion 15.0, Nullable enable and ImplicitUsings enable. The previous project
uses net10.0 and C# 14.0. Set RootNamespace to Playbook.Samples so resource names
match the model namespace. The measured Web SDK supplies Extensions.Validation
and its generator from Microsoft.AspNetCore.App.Ref; do not add a redundant
package reference. Keep its generator, runtime and SDK-aligned
Microsoft.AspNetCore.App.Internal.Assets browser files together. Restore/build
each project in its own directory using its global.json pin.

Model classes and attributes below live in .cs files in namespace Playbook.Samples.
Import System.ComponentModel.DataAnnotations, System.Globalization and System.Resources; newer models also import
Microsoft.Extensions.Validation. Framework implicit usings supply the web and DI
extensions. Register the model's generated metadata in the assembly declaring it;
the Blazor root has [ValidatableType]. A missing generator root can silently fall
back to top-level Blazor validation; an undiscovered endpoint has no automatic
validation. Do not treat missing errors as successful coverage.

{{snippet:startup}}

The remote rule is a real HTTP boundary. Register it with
builder.Services.AddHttpClient<VisitRule>(client => client.BaseAddress = new
Uri("http://localhost:5285")); before Build. The older app uses port 5284. A local
GET /rules/{code} returns a JSON boolean after an asynchronous check; in the
canonical app its controllable journal can hold, release and observe each check.
Replace that local rule with the authoritative service for a real application.
Do not send private data or privileged rules to a browser.

{{snippet:remote}}

No paid service, account, provider, native workload or provisioning is required.
The sample /control endpoints and volatile journal are local test instrumentation,
not application administration or persistence. Keep them out of production.

## Constraints and alternatives

The contracts apply to static SSR, Interactive Server, Interactive WebAssembly and
Auto. Static SSR executes validation on the server during a POST; it cannot show
live .NET field progress between requests. Interactive Server executes on its
circuit. WebAssembly executes the validator in the browser and needs an HttpClient
and server endpoint usable there; register required services during prerendering
too, or intentionally disable prerendering. Auto can run in either location.
The actual canonical consumers here are static SSR and Interactive Server;
WebAssembly/Auto execution, trimming and AOT are not established by those checks.

A fault is different from a rule rejecting input: display retry feedback and do
not accept faulted validation. Cancellation is cooperative; it does not undo server
side effects. Revalidate business constraints at the authoritative write boundary
and enforce atomic invariants there; this demonstration does not reserve a slot.
Keep endpoint validation even when the browser is valid. No newer-package route
on net10.0 is advertised: the async DataAnnotations prerequisite is .NET 11.

## Evidence

Both real projects compile at their declared SDK/language pins. Real static form
posts accept an open pass and reject a closed one. Chromium drives both actual
Interactive Server apps: a held field check is canceled on the next edit without
publishing its stale error; forced submit reruns and awaits a held HTTP rule before
recording exactly one acceptance. English/French interactive formatter checks also
pass. These are loopback execution checks, not mocks or a GA/native/client matrix.
