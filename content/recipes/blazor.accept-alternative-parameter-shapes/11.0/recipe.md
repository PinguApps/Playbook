# Offer one component setting for alternative UI values

## Goal

A seed library prints a short collection instruction or a richer label. Let callers choose either through one setting. Keep display markup separate from the values that must travel through JavaScript or prerendered state.

## Older approach

.NET 10 / C# 14 has no native union declaration or Razor union-literal conversion. Two parameters are a useful existing contract: Label for ordinary text and LabelTemplate for markup. This component gives the template precedence when both are supplied; document that decision for callers.

{{snippet:older}}

The original callers select the appropriate parameter explicitly:

{{snippet:oldercallers}}

For data boundaries, the older app uses an ordinary DTO with text/batch fields:

{{snippet:olderchoices}}

This DTO works with the existing serializer, JavaScript interop and prerendered component state. An application must enforce its own rule that exactly one field is populated. Keeping this DTO preserves an established wire format. A custom wrapper/converter is another possible older design; this comparison does not claim it provides native C# 15 exhaustiveness or literal inference.

The older BatchPanel uses that DTO for the same restore-and-echo behavior:

{{snippet:olderpanel}}

Use the shared host registration, imports and seed.js shown below. The .NET 10 App.razor uses the framework script's direct address:

{{snippet:olderdocument}}

## Newer approach

Use two distinct contracts. BadgeLabel is for local rendering; SeedChoice contains only JSON-compatible data cases. The native union syntax is C# 15, with no preview language switch on the measured RC2 SDK.

{{snippet:choices}}

The badge matches the active value and Razor encodes string content as text. RenderFragment supplies developer-authored markup; it is not a way to trust arbitrary HTML from a user.

{{snippet:newer}}

Callers can use a literal attribute, a bound union containing text, or a bound union containing a fragment. Razor's string-literal support recognizes the string case; numeric-looking literals are still text here. Use an expression when supplying a non-string case. The compiler tooling's PropertyMetadata.AcceptsStringLiteral belongs to this inference; application components do not set that metadata or need an asset-path attribute.

{{snippet:callers}}

The BatchPanel component crosses a real prerender-to-Interactive Server boundary with SeedChoice. It persists and restores that union and sends it through a JavaScript echo. Both a string and a batch object retain their case and values. Register persistence after initializing state and dispose the subscription with the component. This is prerender state transfer, not durable disk storage or a browser-storage service.

{{snippet:panel}}

Use a stable, unique persistence key for each component instance in a production list. This two-item example derives its keys from distinct immutable choices. The component reads Choice during initialization; it does not implement later parameter changes or save edits. Keep confidential data out of values sent to the browser.

Put this script in wwwroot/seed.js:

{{snippet:echo}}

## Migration

1. Use Microsoft.NET.Sdk.Web, net11.0, LangVersion 15.0, nullable and implicit usings. The canonical build pins SDK 11.0.100-rc.2.26504.105 and locked Microsoft.AspNetCore.App.Internal.Assets 11.0.0-rc.2.26504.105. The .NET 10 comparison pins SDK 10.0.402, language 14.0 and Internal.Assets 10.0.13. These SDK pins reproduce evidence; applicability requires the matching release families. No explicit NuGet package, optional workload, provider or external account is needed.
2. Add Choices.cs, SeedBadge.razor, Labels.razor and BatchPanel.razor above to the Playbook.Samples namespace. In _Imports.razor import Microsoft.AspNetCore.Components, Microsoft.AspNetCore.Components.Routing, Microsoft.AspNetCore.Components.Web, Microsoft.JSInterop, static Microsoft.AspNetCore.Components.Web.RenderMode and Playbook.Samples; set @namespace Playbook.Samples.
3. Replace the old parameter pair only if changing that public component contract is acceptable. Update every caller to Label and choose explicit fallback handling for an unset/default union. EditorRequired diagnoses missing markup arguments; it is not runtime validation.
4. Use this complete Program.cs to register the real server rendering path:

{{snippet:host}}

{{snippet:run}}

5. Use this App.razor; the Router remains static and only the two BatchPanel instances opt into Interactive Server:

{{snippet:document}}

6. Preserve prerendering to transfer component state. Open the page, wait for both panels to show Restored: True after activation, then click each echo button: expect JavaScript: Sea kale: 4 packets / JavaScript: Reserve at the desk. Local text and bold markup must remain visible.
7. Treat a DTO-to-union change as a JSON contract change. The older object has Text/Batch members; the newer serializer writes the active case itself (a JSON string or batch object), without that wrapper. Keep the old DTO at persisted/API boundaries or deliberately migrate stored data and all consumers. No automatic compatibility with old records is claimed.

## Constraints

C# 15; Blazor/Razor; distinguish static fragments and JSON-compatible values.

- Union support does not make every case serializable. BadgeLabel's RenderFragment case is a delegate. Real component persistence, JavaScript interop and render-mode parameter serialization reject it with NotSupportedException. Keep it in one render context. The separate .NET 11 captured-static-content feature recognizes direct non-generic RenderFragment parameters; a union-wrapped fragment does not take that capture path. Generic fragments and live callbacks do not become serializable either.
- SeedChoice's string/object cases have distinct JSON token shapes. System.Text.Json 11 performs case selection. Multiple object cases need an appropriate classifier; overlapping numeric/string contracts with web number handling can be ambiguous. A successful string/object example does not establish arbitrary case compatibility. JSON null/default can have no active case; this component renders No selection. Validate required values at the application boundary.
- JSON-supported bodies/interop/state are distinct from route/query/form binding. A union parameter does not add conversion from those non-body sources. No such binding or older-target union package route is claimed here.
- C# 15 stabilization is measured RC2 evidence with RC1 canonical confirmation. These APIs have no experimental attribute/opt-in on that baseline; GA reconciliation remains pending. This is an 11.0 edition with an actual .NET 10 comparison, not another .NET 10 foundation.
- Executed rendering is Windows x64 Static SSR plus Interactive Server in existing Chrome. WebAssembly/Auto require compatible client assets/types and their own execution checks; no trimming, Native AOT or alternate-platform execution is claimed.

## Evidence

Both actual Web SDK projects compile with their declared SDKs, locked assets and language modes. Focused HTTP execution checks literal/bound text and original fragment output, plus actual component rejection of fragment persistence and union-wrapped fragment boundary transfer. Real Chrome activation checks both data cases restored from prerendered state, both JavaScript echo round trips and rejected fragment interop. No mocked JS runtime or renderer substitutes for these checks. Canonical compilation records and exact artifact/input hashes are retained in the repository PIN-779 verification record. Official release notes support literal inference and stabilization; serializer/framework sources support the general boundary restrictions. Broader classification rules are source review, not a claim that every permutation ran.
