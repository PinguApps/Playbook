# Open the chosen table item

## Goal

A harbour museum lists objects in its catalogue. Open the chosen object's record beside the table when a visitor clicks its row. Keep an explicit Open button for people using a keyboard.

## Older approach

.NET 10 QuickGrid has no OnRowClick parameter. An interactive TemplateColumn button already provides a direct, accessible way to choose the item. This remains a valid design when an explicit action is preferable to making the whole row clickable. There is no need to add JavaScript listeners or reconstruct a row index.

{{snippet:older}}

The button closes over its own context item. The record opens in a local details section; no database or external service is involved. In a static-only page, use a real details link or server form instead of expecting this .NET event handler to run.

## Newer approach

On .NET 11 attach QuickGrid's typed OnRowClick callback. The callback receives MuseumObject itself, rather than mouse coordinates, a DOM row or a numeric position. The explicit lambda type also supplies Razor's generic type inference. The same Open method serves both input paths.

{{snippet:newer}}

The row callback opens the selected record. QuickGrid adds its row-clickable class when the callback is attached. Keep the package stylesheet if using its pointer affordance, or style that class in your application. Styling alone does not activate an event.

The Open button stops propagation so its click opens the record once. Without that directive a nested control's click also reaches the row callback. Apply this deliberately to independent cell controls; do not suppress the row event across the entire table.

## Migration

1. Use Microsoft.NET.Sdk.Web, net11.0, LangVersion 15.0, nullable and implicit usings. Add Microsoft.AspNetCore.Components.QuickGrid 11. The canonical sample pins SDK 11.0.100-rc.2.26504.105 and QuickGrid/Internal.Assets 11.0.0-rc.2.26504.105. The comparison pins SDK 10.0.402, C# 14.0 and QuickGrid/Internal.Assets 10.0.13. SDK pins reproduce evidence; requirements use release families. Sample packages are locked and maintained manually. No optional workload, provider or account is needed.
2. Put MuseumTable.razor in namespace Playbook.Samples. In _Imports.razor set @namespace Playbook.Samples and import Microsoft.AspNetCore.Components, Microsoft.AspNetCore.Components.Routing, Microsoft.AspNetCore.Components.Web, Microsoft.AspNetCore.Components.QuickGrid and static Microsoft.AspNetCore.Components.Web.RenderMode.
3. Register the interactive server path in Program.cs:

{{snippet:host}}

4. Use this App.razor document and static router:

{{snippet:document}}

5. Create Interactive.razor with @page "/", @rendermode InteractiveServer and <MuseumTable />. Keep the callback and the table inside that interactive subtree. In the .NET 10 document use the direct script path _framework/blazor.web.js. The samples require no script beyond the framework boot script.
6. Open the running page, wait for activation and click the Signal lantern text cell. Expect that object's description and data-selected=42 on #details. Click Tide clock: expect data-selected=17. Tab to Open Signal lantern and press Enter: expect 42 and exactly one more data-opens count.
7. For the negative control, create Static.razor with @page "/static" and <MuseumTable /> without a render mode. Clicking a row or button there leaves data-selected=0 and data-opens=0. Enhanced navigation, prerendered markup or loading the framework script does not make a static component interactive.

## Constraints

- QuickGrid OnRowClick; interactive handler required. Static SSR and prerender alone are excluded. This recipe executes Interactive Server; WebAssembly/Auto require their own compatible client setup and are not execution evidence here.
- Row clicks are a pointer shortcut. They do not make table rows focusable, keyboard-operable or announced as controls. Preserve table semantics and provide a real button or link for the same action. Do not turn tr into a button with role/tabindex. The canonical button works by keyboard.
- A plain details link also works without an interactive circuit, and is appropriate when navigation is the whole goal. This sample instead opens local detail state to make the selected item observable. It does not implement URL sorting/paging or virtualization.
- OnRowClick does not maintain a selection model, perform navigation, fetch data or authorize an operation. The application chooses those behaviors. Use a stable item identity when loading a real record and check authorization at the server operation. Handle failures/loading if the handler performs asynchronous work; return Task rather than async void.
- The measured RC2 callback is not marked experimental and requires no opt-in or diagnostic suppression. Prerelease artifacts are not GA proof. No newer-package/older-target route is declared.

## Evidence

Both actual pinned Web SDK consumers compile with zero warnings/errors. Separate real browser checks activate Interactive Server, click first/last data cells, verify the selected record and description, exercise the accessible button and its single-call propagation rule, and confirm the static control remains inert. Saved execution reports bind source inputs and built assemblies to those checks. Compilation, official source review and browser execution are distinct evidence; no mock or cross-compilation stands in for the row event. The existing packed-tool harness retrieves this entire comparison and applicability over real stdio from an unrelated directory with an isolated local feed/cache. Other operating systems, assistive technologies, WebAssembly/Auto and GA execution are unverified.
