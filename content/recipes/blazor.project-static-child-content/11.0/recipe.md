# Place server-rendered content inside interactive controls

## Goal

An archive reading card has a server-prepared description and a button that counts reading requests. Keep the description beside the interactive control without adding a component solely to carry the markup across the render-mode boundary.

## Older approach

On .NET 10, pass serializable values to an interactive wrapper. Construct its fragments inside that interactive hierarchy. This is a useful composition pattern, especially when the wrapper also owns behavior; it is not broken code.

The static page selects the interactive wrapper:

{{snippet:older}}

ReadingWrapper.razor constructs the panel's content on the interactive side:

{{snippet:wrapper}}

The wrapper and panel share a render context. Moving that ChildContent directly onto a render-mode boundary fails on the measured .NET 10 baseline because RenderFragment is a delegate. There is no claimed older package route for the .NET 11 capture behavior.

## Newer approach

On .NET 11, put the non-generic fragments directly on the interactive component. Both Heading and ChildContent are rendered during prerendering, captured by the framework, then reconstructed for the interactive instance.

{{snippet:newer}}

ReadingPanel.razor renders both supplied fragments immediately. Its own button handler belongs to the interactive component, so subsequent clicks update requests without needing a callback from the static parent.

{{snippet:panel}}

The framework transfers the captured render-tree description, not the original delegate or a live closure over the parent. A parent's data change is not a subscription that updates this captured content. Use serializable state and behavior owned by an interactive component when the content needs to change interactively.

## Migration

1. Target net11.0 using Microsoft.NET.Sdk.Web and matching ASP.NET Core 11 reference/runtime and browser assets. The canonical project uses C# 15.0, nullable and implicit usings. SDK 11.0.100-rc.2.26504.105 and its locked Internal.Assets 11.0.0-rc.2.26504.105 are the measured baseline. No explicit package, provider, account, optional workload or experimental opt-in is required. The older project pins SDK 10.0.402, C# 14.0 and Internal.Assets 10.0.13.
2. Keep the App/Router static and mark ReadingPanel InteractiveServer as shown. Remove ReadingWrapper only when it has no other responsibility. Keep any necessary serializable parameters and move live event handling into the interactive component.
3. In _Imports.razor import Microsoft.AspNetCore.Components, Microsoft.AspNetCore.Components.Routing, Microsoft.AspNetCore.Components.Web, static Microsoft.AspNetCore.Components.Web.RenderMode and Playbook.Samples; set @namespace Playbook.Samples. Put these original components together in that namespace.
4. Use this complete Program.cs in both projects:

{{snippet:host}}

{{snippet:run}}

5. Use this complete App.razor in both projects. Its framework script activates the interactive boundary:

{{snippet:document}}

6. Keep prerendering enabled and invoke each supplied fragment during prerendering. Do not hide ChildContent behind a condition that becomes true only after activation. The framework cannot serialize content it never captured.

## Constraints

Cross-render-mode static content; not delegate serialization or arbitrary live callbacks.

- Only non-generic RenderFragment parameters use this capture route. RenderFragment<T> templates and ordinary delegates such as Action are rejected at the boundary. Generic fragment contravariance is a different concern and does not change this restriction.
- Event handlers inside captured markup are skipped with framework warnings. A captured button's parent-owned @onclick does not become live after activation. Put actions in ReadingPanel or another component that owns its behavior. Element/component reference captures and named events are also skipped according to the pinned serializer source; they are not transferred references.
- Disabled prerendering and fragments that are not invoked during prerendering fail. A valid fragment parameter type alone does not establish compatibility.
- Other component parameters still need the usual serialization support. Captured component types must be available in the destination runtime. Keep WebAssembly/Auto client assets and types compatible if choosing those modes; this example executes only Static SSR to Interactive Server and claims no independent WebAssembly/Auto, trimming or AOT proof.
- The contracts have no experimental opt-in in the measured RC2 build. RC execution is not GA verification. This is an 11.0 edition with a .NET 10 comparison, not another curated .NET 10 foundation.

## Evidence

Both actual Web SDK projects compile against their declared SDK, language and locked browser assets. Separate real HTTP execution verifies original rendered text, .NET 10 direct-fragment refusal, and .NET 11 generic/delegate, disabled-prerender and unused-fragment failures. Real Chromium 152 Interactive Server execution verifies both comparison counters, preservation of heading/body across interactive rerenders and a skipped parent-owned captured click handler. There is no mock renderer or cross-compilation substitute for those checks. Pinned framework source supports the other skipped-frame restrictions; those are source review, not separate executed cases. Saved reports and exact artifact/input hashes are documented in the repository's PIN-778 verification record; the complete implementation and requirements needed to apply the recipe are above.
