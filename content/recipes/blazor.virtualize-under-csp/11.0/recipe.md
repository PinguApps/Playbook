# Scroll long lists under restrictive style policies

## Goal

A coastal research room reviews a thousand light measurements from the fictional Westmere beacon. Keep a small, scrollable window of readings while the browser refuses inline style attributes and inline stylesheets. Updating the list must not require an exception to those style restrictions.

## Older approach

.NET 10 Virtualize renders calculated spacer heights in style attributes. The comparison keeps the ordinary framework component and makes its existing requirement explicit: only /legacy allows style-src-attr 'unsafe-inline'. The default / route denies those attributes. Both routes still restrict stylesheets to the same origin and deny inline style elements. This narrower attribute exception is still a deliberate relaxation; an app that cannot accept it can keep an older pagination design instead.

{{snippet:older}}

The exception route is a fair working comparison: it reaches reading 701 and returns to reading 1 with a bounded set of actual rendered rows. Removing the exception on the older / route blocks spacer attributes, collapses the reserved space and makes the distant readings unreachable. Moving the app's own CSS out of markup does not repair the component's dynamic spacer attributes. A nonce on an external stylesheet is not permission for arbitrary style attributes. No claim is made that old pagination or an independently maintained component is invalid, or that application code can opt the sealed older Virtualize into the newer renderer.

## Newer approach

Use the .NET 11 Virtualize renderer together with its matching browser assets. Remove the virtualization-specific attribute exception rather than widening style-src. This canonical newer host has one enforcing policy and no legacy exception branch:

{{snippet:newer}}

The list itself uses the same fixed-height data and familiar Virtualize Items contract. No new anchoring or item-reveal API is involved:

{{snippet:list}}

The framework sends numeric data-blazor-virtualize-reserved-height values and, when needed, a numeric data-blazor-virtualize-loop-breaker-transform. Its browser observer validates finite numbers and applies pixel height/translation using CSSOM, the browser's programmatic style interface. It also sets base styles programmatically. Consequently, inspecting the live DOM can still show style attributes. The relevant proof is that the browser enforces the header without rejecting ordinary list rendering and that actual distant items appear inside the viewport. Counting attributes or reading a spacer's height alone proves neither.

## Migration

1. Use Microsoft.NET.Sdk.Web, net11.0, LangVersion 15.0, nullable and implicit usings. Virtualize is supplied by Microsoft.AspNetCore.App; no separate NuGet library is needed. The newer canonical sample pins SDK 11.0.100-rc.2.26504.105, runtime/reference assets and SDK-resolved Microsoft.AspNetCore.App.Internal.Assets 11.0.0-rc.2.26504.105. The older sample pins SDK 10.0.402, C# 14.0 and Internal.Assets 10.0.13, executed on runtime 10.0.13. Lock sample packages and maintain these pins deliberately. These exact pins reproduce prerelease evidence, not a GA support promise.
2. Put Readings.razor in namespace Playbook.Samples, retaining @page "/" and @rendermode InteractiveServer. In the .NET 10 Readings.razor, add @page "/legacy" too: both / and /legacy must resolve to this same interactive list for the strict/exception comparison. In _Imports.razor use @namespace Playbook.Samples and import Microsoft.AspNetCore.Components, Microsoft.AspNetCore.Components.Routing, Microsoft.AspNetCore.Components.Web, Microsoft.AspNetCore.Components.Web.Virtualization and static Microsoft.AspNetCore.Components.Web.RenderMode. Use the host above with using Playbook.Samples.
3. Create wwwroot/readings.css. Set the external rule `#viewport { height: 320px; width: 36rem; max-width: 100%; overflow-y: auto; border: 1px solid #52646e; }` and ` .reading { height: 32px; box-sizing: border-box; line-height: 32px; padding: 0 12px; white-space: nowrap; }`. Fixed row height matches ItemSize=32. Keep decoration in that external file too. Inline container dimensions would independently violate this policy.
4. Use this App.razor document for .NET 11. In the .NET 10 App.razor, use <script src="_framework/blazor.web.js"></script> for the framework boot script instead of the newer document's Assets lookup, matching the compiled older sample. Keep the app stylesheet and optional diagnostic script entries. The sample's external policy-probe.js observes securitypolicyviolation before component markup is parsed; it does not rewrite spacers or suppress errors. In a consumer without that diagnostic file, omit its script element. The remaining document and list are complete without that observer.

{{snippet:document}}

5. Upgrade both server/reference assets and browser files, then delete only the old virtualization exception or app-owned spacer rewrite. Keep script-src, allowed external stylesheets and circuit connection requirements under their own review. The sample permits its own external boot scripts and a localhost WebSocket for Interactive Server. In deployment use the real secure circuit origin. This recipe does not audit script policy, reconnect UI, WebAssembly startup or the application's other components.
6. In the actual browser inspect the enforcing Content-Security-Policy response header, not merely markup or a report-only header. Expect style-src 'self', style-src-elem 'self' and style-src-attr 'none', without unsafe-inline, hashes or nonces for virtualization. Wait for the live connection button to increment its output. Scroll the viewport to 22,400 pixels and verify the visible text Reading 701: Westmere light at its top, followed by 702–710, while fewer than 100 reading nodes exist. Scroll back and verify reading 1 is visible again. Inspect the full ordinary securitypolicyviolation log; it must contain no list failures.
7. Run meaningful policy controls separately after ordinary scrolling. An added element with style="width: 77px" must retain its external 20px width and raise an enforced style-src-attr violation. An inserted inline stylesheet requesting 99px must also retain 20px and raise style-src-elem. By comparison, a trusted script's element.style.setProperty width of 88px takes effect. That distinction explains the framework route; it is not a mechanism for executing untrusted CSS. Reload afterward to remove the diagnostic controls.

## Constraints

- Virtualize; source-reviewed migration, browser evidence still needed for each consumer's browser, real response headers, app CSS and interactive mode. This recipe adds concrete Chrome execution for its own samples; it does not turn historical source review into universal browser proof.
- CSP separates stylesheet sources/elements, inline attributes and programmatic CSS. A strict style-attribute directive does not disable trusted JavaScript's CSSOM updates. Keep untrusted values out of style/script APIs; this renderer validates its own numeric layout values. No custom application spacer mutation is necessary on the newer route.
- Interactive Server was executed with actual reference/runtime/browser artifacts. Prerender and static SSR alone cannot update a virtual window. Matching WebAssembly/Auto setup, other browsers, mobile platforms, AOT, disconnected circuits and deployment policies were not exercised. No data provider, service account, database or optional workload is required.
- The app's real fixed-height CSS and supported scroll container remain necessary. CSP compatibility does not make arbitrary flex/grid layouts or mismatched row heights valid. Item identity, anchoring, dynamic row measurement and programmatic reveal belong to their separate goals.
- These APIs are not marked experimental and need no experimental opt-in. The measured SDK/runtime are RC2; GA reconciliation remains outstanding. No newer-package/older-target route or invented older CSS-policy bypass is advertised.

## Evidence

Both original pinned consumers compile as their actual net10.0/net11.0 targets with zero warnings/errors. Separate real Chrome 155 execution checks the effective response header, a live server callback, visible readings 701–710, bounded rendered IDs, return scrolling and securitypolicyviolation details. The newer strict route has zero ordinary CSP violations. The older exception route also scrolls successfully; its strict negative control reports style-src-attr violations, computed zero spacer heights and no distant reading 701. Both strict routes reject deliberate inline attribute/element controls while programmatic CSS remains effective. Source review, compilation and browser execution are distinct; no mock, calculated offset or cross-compilation substitutes for visible list evidence.

The existing actual packed-tool harness retrieves the complete comparison, canonical snippets, edition 11.0, full authored applicability and current compilation records offline over real stdio, using an isolated local feed/cache and unrelated working directory. No new OS/client/native test matrix is added. Exact authoring artifacts, inputs, failed preview attempts and execution limits are retained with the repository verification record. Installed guidance does not inspect the consuming app; its browser proof must be repeated after relevant app or framework changes.
