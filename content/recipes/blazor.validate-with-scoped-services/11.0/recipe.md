# Keep service-backed validation when migrating an EditContext

## Goal

A sail-repair workshop accepts canvas and sailcloth but rejects unsupported material. Its interactive editor creates an EditContext manually. Enable data annotations with the actual scoped application provider and dispose the returned subscription whenever the editor replaces its model/context or closes.

## Older approach — .NET 10

Two obsolete providerless overloads are removed in .NET 11: `AddDataAnnotationsValidation(EditContext)` returns the context itself, while `EnableDataAnnotationsValidation(EditContext)` returns an IDisposable. Neither supplies the application's services. The isolated historical fixtures show that even permitted canvas cannot resolve the catalogue:

{{snippet:legacy}}

Do not copy those fixtures into a live editor. The first context is isolated because that API does not return a subscription to dispose. CS0618 suppression is local to these historical calls. A supported provider-aware approach already exists in .NET 10:

{{snippet:older}}

That is the working older comparison, not a claim that the providerless overload already supported service-backed rules.

## Newer approach — .NET 11

Keep the same supported subscription ownership. `RepairEditor.cs` disposes the old subscription before replacing the model/context:

{{snippet:newer}}

Define `RepairJob.cs` with the real scoped catalogue and synchronous attribute. Returning the member name makes submit errors appear beside the same field as field-change errors:

{{snippet:model}}

Create the Web project and `_Imports.razor` as follows:

{{snippet:project}}

In `_Imports.razor`, declare `@namespace Playbook.Samples` and add `@using` directives for `Playbook.Samples`, `Microsoft.AspNetCore.Components`, `Microsoft.AspNetCore.Components.Forms`, `Microsoft.AspNetCore.Components.Routing`, and `Microsoft.AspNetCore.Components.Web`. Add `@using static Microsoft.AspNetCore.Components.Web.RenderMode` for render-mode names.

Start `Program.cs` with this service registration and application creation. Use the injected component provider; do not build a second provider or create a separate scope for validation:

{{snippet:startup}}

Finish `Program.cs` with the component hosting pipeline:

{{snippet:hosting}}

Use these `App.razor` and `Routes.razor` files to host an Interactive Server editor with prerender disabled:

{{snippet:shell}}

{{snippet:router}}

Create `Repair.razor`. The form is keyed by context so its input components are recreated for a replacement model. There is no second DataAnnotationsValidator subscriber:

{{snippet:form}}

`ValidateAsync` is the current .NET 11 orchestration entry point; it does not turn the catalogue rule into an asynchronous rule. The .NET 10 comparison uses `Validate`. The outputs and old-context probe make the lifecycle behavior observable; production UI can omit that diagnostic instrumentation while retaining disposal and the form key.

## Migration

1. Find manually enabled contexts and identify whether the obsolete call returns an EditContext or a subscription. Do not mistake the first return value for IDisposable.
2. Register the application's validation service with its intended scope. Inject the actual scoped IServiceProvider into the owning interactive component; do not use a root provider or `BuildServiceProvider` to obtain it.
3. Replace the providerless call with `context.EnableDataAnnotationsValidation(services)` and retain its IDisposable. This supported path works on both .NET 10 and .NET 11.
4. Dispose the old subscription before replacing its context/model. Create one new subscription and key the form by its new context. Dispose the current subscription when the component closes. Avoid adding DataAnnotationsValidator as another registration.
5. On .NET 11, await `ValidateAsync` at the submit/probe entry points as shown. Follow the separate `blazor.await-form-rules` guidance for asynchronous form orchestration; do not add asynchronous catalogue rules in this migration.
6. In a real interactive browser, enter plastic and leave the field: expect one error. Submit: expect Rejected with one error. Change to canvas and submit: expect Accepted with no error. The scoped catalogue check count increments once per field change and once per submit.
7. Replace the job repeatedly. Expect an empty current field, the same circuit-scoped catalogue and no service calls when notifying/validating the old contexts. Repeating validation must produce one current handler/message. Also verify final disposal without a browser using the sample's `--verify` diagnostic mode.

## Constraints

This is manual top-level data-annotations integration and interactive component ownership, not a new validation rule engine. Reuse `validation.cover-nested-models` (PIN-772) for canonical generated/nested graph validation, `validation.translate-model-feedback` for shared model feedback and `validation.validate-asynchronously` for asynchronous rules. Those recipes remain independently owned and retrievable; their separate rules are not implemented here.

The edition targets .NET 11. The working .NET 10 comparison preserves an already supported approach, without adding a ninth foundation or advertising a .NET 11 package-on-.NET 10 route. Stable APIs require no experimental opt-in. The current sample declares C# 15, Web SDK/reference assets and shared runtime 11; the previous declares C# 14 and target/runtime 10. SDK-aligned browser files are locked, with no explicit external package, provider, native workload or account requirement.

Measured pins: SDK 10.0.402 / framework and browser assets 10.0.13, and SDK 11.0.100-rc.2.26504.105 / framework and browser assets 11.0.0-rc.2.26504.105. These exact artifacts record tests, not universal exact consumer prerequisites. Static SSR request scopes and lifecycle are not equivalent to Interactive Server circuit scope. No static-SSR, WASM/Auto, nested/generated validation, asynchronous attribute, trimming/AOT, external-provider or GA behavior is claimed.

## Evidence

Both original SailRepairs projects compiled against actual matching dependency sets, with local SDK pins, locked assets, exact commands and input hashes saved through Record-Evidence. Separate `behavior.evidence.json` records capture real Chromium 153 field/submit results and three model replacements, one service call per validation event, one current message and no old-context callbacks. The same circuit-scoped catalogue is used by the component and attribute.

The actual-runtime `--verify` path also resolves distinct real DI scopes, rejects plastic, accepts canvas, checks exactly one current handler across three replacements and verifies final disposal. Both .NET 10 providerless fixtures demonstrate service-resolution failure. No mocked service or metadata-only result substitutes for execution. `eng/Record-RemovedBlazorBehavior.ps1` captures these results only with unchanged canonical inputs.

The source/API audit matched the pinned reference assets again. Installed-package stdio checks verify full canonical Markdown, discovery goal, target/edition, applicability, current compilation/execution evidence and structured/text equality outside the checkout. RC2 proof does not close the GA reconciliation or comprehensive coverage gates, and WEB-089's other migration families remain outstanding.
