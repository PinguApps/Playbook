# Associate labels with reusable form inputs

## Goal

A reusable diver editor must give each caption its own input, including when two editors appear together. Support both separate labels and a label wrapping an input.

## Older approach

Explicit HTML for/id coordination is a sound .NET 10 approach. The caller supplies a unique InputId and the editor uses it consistently. The canonical parent passes survey-diver-0 and survey-diver-1.

{{snippet:older}}

The component starts with @inherits Editor<Diver> and declares [Parameter, EditorRequired] public string InputId { get; set; } = "". Caption resolution uses:

{{snippet:helper}}

## Newer approach

.NET 11 Label uses the same expression as InputText. Start DiverEditor.razor with @inherits Editor<Diver> and the standard imports.

{{snippet:newer}}

The parent binds different collection slots through Editor<Diver>, carrying a distinct field prefix into each instance. Its contact uses the wrapping pattern.

{{snippet:form}}

The model lives in SurveyRequest.cs, outside Razor. The skipped reference snapshot is never validated editable input.

{{snippet:model}}

The example registers SubmissionCounts as a singleton before Build and adds @inject SubmissionCounts Counts to Survey.razor so accepted posts are observable. Its component state and counter are:

{{snippet:state}}

{{snippet:counts}}

## Migration

Model files import System.ComponentModel, System.ComponentModel.DataAnnotations and Microsoft.Extensions.Validation and use namespace Playbook.Samples. The older FieldCaption helper additionally imports System.Linq.Expressions and System.Reflection. Captions.cs imports System.Globalization and System.Resources. Counter methods use System.Threading from implicit usings.

1. Use Microsoft.NET.Sdk.Web with net11.0, C# 15.0, nullable and implicit usings enabled. The measured SDK supplies Microsoft.Extensions.Validation assemblies and ValidationsGenerator from Microsoft.AspNetCore.App.Ref, plus aligned Microsoft.AspNetCore.App.Internal.Assets browser files. A redundant PackageReference produced NU1510; none is needed here. Keep matching generator/runtime/browser assets when using another toolset.
2. In _Imports.razor import Microsoft.AspNetCore.Components, Microsoft.AspNetCore.Components.Forms, Microsoft.AspNetCore.Components.Routing, Microsoft.AspNetCore.Components.Web and Playbook.Samples, and set @namespace Playbook.Samples. Retain a standard App root with a Router/RouteView, HTML document/base href and the framework blazor.web.js script. MapStaticAssets(), UseAntiforgery() and MapRazorComponents<App>() belong in the host before Run(). No interactive render mode is applied. Make Survey.razor routable with @page "/{Mode}".
3. Add the shown model and component state. Register AddValidation for this graph and the example counter singleton.
4. Remove manually coordinated IDs when adopting automatic association. A separate Label needs the same field expression as its input; a wrapping Label implicitly associates its child control.
5. Bind each repeated editor through a distinct collection expression such as Model.Divers[row]. The Editor base propagates that prefix.
6. Inspect label.control and every input ID on the full page, then click each label and confirm focus moves to its intended control. Retest after adding forms or repeated groups.

## Constraints

Generated IDs follow field expressions; they aren't random instance identifiers. Duplicating the same expression, including across separate forms, can collide. Keep scopes/paths distinct or deliberately coordinate an explicit input id and matching label for. An explicit id overrides generation and must agree with a separate label.

A wrapping label should contain one intended labelable control. DisplayName renders text and doesn't associate it with an input. Built-in static SSR inputs are verified here; custom InputBase implementations and interactive transitions need their own checks.

The exact baselines are SDK 10.0.402 / ASP.NET Core 10.0.13 and SDK 11.0.100-rc.2.26504.105 / ASP.NET Core 11.0.0-rc.2.26504.105, Windows x64 and Chromium 152. RC proof is not GA verification, screen-reader operation, trimming/AOT proof or a platform/browser matrix. No conditional newer-package/older-runtime route or extra .NET 10 foundation edition is advertised.

## Evidence

Both apps compile and render four editable controls. Real browser DOM checks prove every label.control association, unique IDs across two repeated editors, wrapping contact and separate lead/diver labels. The older manually supplied IDs and newer generated IDs identify corresponding controls. Enhanced/plain submission tests exercise the actual rendered forms; no screen-reader claim is made.
