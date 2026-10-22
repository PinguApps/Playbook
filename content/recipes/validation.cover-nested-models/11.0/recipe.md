# Validate nested records and collection entries

## Goal

Reject a missing contact address or second diver's name before accepting a survey. Deliberately exclude reference data without accidentally exempting editable children.

## Older approach

.NET 10 already supports generated graph validation. This comparison deliberately uses its public experimental attributes and a local ASP0029 suppression. The fully qualified ValidatableType avoids ambiguity with the embedded marker supplied by the measured SDK. Existing working graph validation is not broken because attributes later stabilize.

{{snippet:older}}

## Newer approach

The .NET 11 attributes are stable, so this canonical model needs no ASP0029 suppression. Discovery starts from its annotated root and follows concrete nested types and collection items.

{{snippet:newer}}

Call AddValidation in the model's assembly:

{{snippet:startup}}

Connect the registered services to the actual form:

{{snippet:form}}

The example registers SubmissionCounts as a singleton before Build and adds @inject SubmissionCounts Counts to Survey.razor so accepted posts are observable. Its component state and counter are:

{{snippet:state}}

{{snippet:counts}}

The reusable component starts with @inherits Editor<Diver>:

{{snippet:editor}}

## Migration

The model's contact caption uses this complete shared resource wrapper and SDK-embedded Captions.resx / Captions.fr-FR.resx files, placed together in namespace Playbook.Samples. The same resources work in both comparison apps.

{{snippet:resource}}

{{snippet:resources}}

{{snippet:french}}

Model files import System.ComponentModel, System.ComponentModel.DataAnnotations and Microsoft.Extensions.Validation and use namespace Playbook.Samples. The older FieldCaption helper additionally imports System.Linq.Expressions and System.Reflection. Captions.cs imports System.Globalization and System.Resources. Counter methods use System.Threading from implicit usings.

1. Use Microsoft.NET.Sdk.Web with net11.0, C# 15.0, nullable and implicit usings enabled. The measured SDK supplies Microsoft.Extensions.Validation assemblies and ValidationsGenerator from Microsoft.AspNetCore.App.Ref, plus aligned Microsoft.AspNetCore.App.Internal.Assets browser files. A redundant PackageReference produced NU1510; none is needed here. Keep matching generator/runtime/browser assets when using another toolset.
2. In _Imports.razor import Microsoft.AspNetCore.Components, Microsoft.AspNetCore.Components.Forms, Microsoft.AspNetCore.Components.Routing, Microsoft.AspNetCore.Components.Web and Playbook.Samples, and set @namespace Playbook.Samples. Retain a standard App root with a Router/RouteView, HTML document/base href and the framework blazor.web.js script. MapStaticAssets(), UseAntiforgery() and MapRazorComponents<App>() belong in the host before Run(). No interactive render mode is applied. Make Survey.razor routable with @page "/{Mode}".
3. Put models in .cs files in the assembly calling AddValidation. Annotate roots not discovered from endpoint signatures. Keep DataAnnotationsValidator inside EditForm. Register the example counter singleton before Build.
4. Remove the old local ASP0029 suppression after compiling against stable .NET 11 attributes. Don't suppress generator diagnostics to disguise undiscovered models.
5. Apply SkipValidation only to a complete subtree intentionally outside the acceptance boundary. Reference is skipped; Contact and Divers remain covered.
6. Post missing nested and collection fields separately. Then submit valid editable fields with the reference snapshot still empty. In a diagnostic fixture, omit root discovery and verify that top-level success can miss invalid children.

## Constraints

Without discovered metadata, the Blazor fallback checks top-level properties. Missing graph metadata need not throw. A successful build or valid submission therefore cannot prove coverage. The intentionally incomplete /diagnostics/undiscovered fixture demonstrates this failure mode; don't use its configuration as a valid editor.

Cross-assembly models need their own supported generator registration. This proof covers one assembly, concrete children, collections and a property opt-out, not arbitrary polymorphic graphs, external libraries or MVC/Razor Pages. Shared rule concepts remain in validation.validate-asynchronously and validation.format-localized-rules; this recipe owns graph discovery. Client coverage is separate: RC2 emitted only the root rule here while authoritative server validation rejected children.

The exact baselines are SDK 10.0.402 / ASP.NET Core 10.0.13 and SDK 11.0.100-rc.2.26504.105 / ASP.NET Core 11.0.0-rc.2.26504.105, Windows x64 and Chromium 152. RC proof is not GA verification, screen-reader operation, trimming/AOT proof or a platform/browser matrix. No conditional newer-package/older-runtime route or extra .NET 10 foundation edition is advertised.

## Evidence

Actual HTTP posts reject root, child and collection failures, accept valid graphs with invalid skipped reference data, and expose the undiscovered-root boundary. Further posts verify global client opt-out. Real browser submissions repeat child/collection rejection after bypassing client checks in enhanced/plain forms. Both model versions compile; .NET 11 has no obsolete experimental suppression. Exact command/output and separate execution reports accompany the proof.
