# Finish remote model checks before an endpoint handler runs

## Goal

Reject a closed observatory visit before the handler records a booking, even when
checking availability requires an asynchronous service call.

## Older approach

On .NET 10, place the awaited business check in an endpoint filter before invoking
next. BookingChecks runs synchronous DataAnnotations and then awaits the service;
it returns member-keyed errors for Results.ValidationProblem. This is useful when
the application deliberately owns the validation boundary. AddValidation's
synchronous foundation alone does not wait for a Task started by an attribute.

{{snippet:old-model}}

{{snippet:old-checks}}

{{snippet:old-endpoint}}

## Newer approach

On .NET 11 the generated Extensions.Validation filter uses the asynchronous path.
Register AddValidation in the model/endpoint assembly and declare a public model
in a .cs file. Blazor roots also use [ValidatableType]; endpoint signature discovery
handles this endpoint's model. The validation attribute requires the new shared
DataAnnotations API, not just a newer Extensions package on an older runtime.

{{snippet:model}}

{{snippet:async-rule}}

{{snippet:endpoint}}

## Migration

1. Create the release-specific Web SDK project and public model shown below.
2. Register the validation, localization and rule services before building the app; configure culture before validation executes.
3. Wire the shown form or endpoint adapter, then exercise both accepted and rejected input through the actual host.

The complete previous/current comparison uses these group-size formatters too:

{{snippet:old-formatter}}

{{snippet:formatter}}

Move the remote rule into the attribute, register VisitRule, and remove only the
redundant filter for this rule. Do not remove authorization, tenant checks or
transaction invariants. Return a member name in ValidationResult so forms and
endpoint errors can identify Pass. The closed-visit response is 400 with an errors
object; a successful sample handler returns 204 and increments the local journal.
Include PartyLimit from validation.translate-model-feedback, or omit that
independent rule when focusing on asynchronous pass checks.


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

This is Minimal API model validation, not MVC or Razor Pages integration.
Register metadata for any models/handlers in referenced assemblies through that
assembly's registration method. A missing model root does not fail closed by
itself: verify invalid requests through the real app. The sample's rule is
side-effect free; a completed pre-handler check does not make a later write atomic.
Enforce racing capacity or uniqueness constraints at the write boundary too.

AsyncValidationAttribute implements both paths. Extensions.Validation's endpoint
pipeline calls the asynchronous path only. Throw from the unsupported synchronous
override; never block on Task.Result or return Success while a check is unfinished.
IAsyncValidatableObject is an alternative for a cross-property rule: implement
both Validate and ValidateAsync, return member names, propagate the token and avoid
dependencies on other concurrently running validators. This recipe's canonical
choice is the attribute; the shared validation.validate-asynchronously recipe
contains the separately verified object-level contract.

Request cancellation flows through the attribute and HttpClient; honor it before
publishing an outcome. This does not cancel work a remote server ignores. Keep
service failures distinct from invalid user input; do not turn an outage into
success. No optional older-target package route is claimed.

## Evidence

Both actual SDK-pinned projects compile. The loopback test holds valid and invalid
HTTP rules, observes entered=1/completed=0/accepted=0 while the request remains
unfinished, then releases each. The valid request yields 204/completed=1/accepted=1;
the invalid one yields 400/completed=1/accepted=0. This establishes execution
ordering through real endpoint filters, not a mock repository or direct attribute
invocation. Exact inputs/artifacts and RC2 limitations accompany the evidence.
