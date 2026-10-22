# Check a server-rendered form before posting

## Goal

A reef-survey clerk should see an empty lead field before sending a form. Keep the model as the rule owner and reject invalid posts even when browser checks are bypassed.

## Older approach

.NET 10 static SSR validates after posting. Immediate feedback can use application JavaScript, HTML constraints or an interactive form. Those remain valid choices. This comparison keeps SSR and duplicates Required in a small initializer; it is only a required-field check, not an equivalent implementation of every annotation.

{{snippet:older}}

Save this startup module as wwwroot/SurveyForms.lib.module.js for the SurveyForms assembly. Its delegated handlers also cover forms replaced by enhanced navigation.

{{snippet:client}}

## Newer approach

.NET 11 emits supported client rules from model metadata. Remove the legacy initializer and data-legacy-* declarations while retaining DataAnnotationsValidator and server checks.

{{snippet:newer}}

Register these services in Program.cs:

{{snippet:startup}}

The model lives in SurveyRequest.cs, outside Razor. The skipped reference snapshot is never validated editable input.

{{snippet:model}}

The example registers SubmissionCounts as a singleton before Build and adds @inject SubmissionCounts Counts to Survey.razor so accepted posts are observable. Its component state and counter are:

{{snippet:state}}

{{snippet:counts}}

DiverEditor.razor begins with @inherits Editor<Diver> and uses this body:

{{snippet:editor}}

## Migration

The model's contact caption uses this complete shared resource wrapper and SDK-embedded Captions.resx / Captions.fr-FR.resx files, placed together in namespace Playbook.Samples. The same resources work in both comparison apps.

{{snippet:resource}}

{{snippet:resources}}

{{snippet:french}}

Model files import System.ComponentModel, System.ComponentModel.DataAnnotations and Microsoft.Extensions.Validation and use namespace Playbook.Samples. The older FieldCaption helper additionally imports System.Linq.Expressions and System.Reflection. Captions.cs imports System.Globalization and System.Resources. Counter methods use System.Threading from implicit usings.

1. Use Microsoft.NET.Sdk.Web with net11.0, C# 15.0, nullable and implicit usings enabled. The measured SDK supplies Microsoft.Extensions.Validation assemblies and ValidationsGenerator from Microsoft.AspNetCore.App.Ref, plus aligned Microsoft.AspNetCore.App.Internal.Assets browser files. A redundant PackageReference produced NU1510; none is needed here. Keep matching generator/runtime/browser assets when using another toolset.
2. In _Imports.razor import Microsoft.AspNetCore.Components, Microsoft.AspNetCore.Components.Forms, Microsoft.AspNetCore.Components.Routing, Microsoft.AspNetCore.Components.Web and Playbook.Samples, and set @namespace Playbook.Samples. Retain a standard App root with a Router/RouteView, HTML document/base href and the framework blazor.web.js script. MapStaticAssets(), UseAntiforgery() and MapRazorComponents<App>() belong in the host before Run(). No interactive render mode is applied. Make Survey.razor routable with @page "/{Mode}".
3. Add the model, component state and repeated editor above. Register builder.Services.AddSingleton<SubmissionCounts>(). Replace the acceptance counter with the real save only after successful server validation.
4. Delete the duplicate initializer and its legacy attributes. Change an empty lead field, submit invalid input, correct it and submit in both modes.
5. Disable browser validation per form with DataAnnotationsValidator.DisableClientValidation=true, or globally with RazorComponentsServiceOptions.DisableClientValidation=true in AddRazorComponents. A formnovalidate button skips client checks only.

## Constraints

This JavaScript path belongs to static SSR. Interactive forms use their .NET EditContext. Only supported built-in attributes have automatic client rules; custom attributes and nonnumeric Range constraints need separate browser implementations or remain server-only. Later streaming updates which insert fields into an existing form have an additional documented limitation.

The measured RC2 sample emits only Model.Lead in client rules. Actual server posts reject its nested contact and collection entries; immediate nested client feedback is not established. Inspect emitted rules on the consumer's actual artifacts. Never remove server validation to make a bypass succeed.

The exact baselines are SDK 10.0.402 / ASP.NET Core 10.0.13 and SDK 11.0.100-rc.2.26504.105 / ASP.NET Core 11.0.0-rc.2.26504.105, Windows x64 and Chromium 152. RC proof is not GA verification, screen-reader operation, trimming/AOT proof or a platform/browser matrix. No conditional newer-package/older-runtime route or extra .NET 10 foundation edition is advertised.

## Evidence

Both apps compile. The real browser blocks invalid submissions without increasing the server post count, accepts valid enhanced/plain posts and preserves a window sentinel only for enhanced posting. A formnovalidate bypass still produces server errors for a nested address and second diver. Actual HTTP tests additionally check per-form/global opt-outs. Compilation records and browser/HTTP reports are separate; none is a mocked consumer.
