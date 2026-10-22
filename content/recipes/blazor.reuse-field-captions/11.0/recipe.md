# Reuse model captions across form views

## Goal

A survey editor and read-only caption should use the same name for a contact field, including its French resource value, without repeating view strings.

## Older approach

.NET 10 can use a small application-owned metadata helper. It accepts simple member expressions, calls Display.GetName for resource resolution, then falls back to DisplayName and the property name. This is a valid reflection-based path, not a general expression parser.

{{snippet:older}}

The view calls it directly:

{{snippet:oldview}}

## Newer approach

.NET 11 DisplayName owns metadata lookup at the rendering boundary:

{{snippet:newer}}

Lead deliberately carries both attributes to test precedence. Diver.Name tests DisplayName alone; Reference.ImportedCode has neither. Contact.Email points at a public resource property.

{{snippet:model}}

{{snippet:resource}}

Place Captions.resx beside Captions.cs:

{{snippet:resources}}

Place its translation in Captions.fr-FR.resx:

{{snippet:french}}

## Migration

Model files import System.ComponentModel, System.ComponentModel.DataAnnotations and Microsoft.Extensions.Validation and use namespace Playbook.Samples. The older FieldCaption helper additionally imports System.Linq.Expressions and System.Reflection. Captions.cs imports System.Globalization and System.Resources. Counter methods use System.Threading from implicit usings.

1. Use Microsoft.NET.Sdk.Web with net11.0, C# 15.0, nullable and implicit usings enabled. The measured SDK supplies Microsoft.Extensions.Validation assemblies and ValidationsGenerator from Microsoft.AspNetCore.App.Ref, plus aligned Microsoft.AspNetCore.App.Internal.Assets browser files. A redundant PackageReference produced NU1510; none is needed here. Keep matching generator/runtime/browser assets when using another toolset.
2. Import Microsoft.AspNetCore.Components.Forms in the view and Playbook.Samples for the shown model. Bind Model to a SurveyRequest instance and retain the standard Blazor router/host setup.
3. Use namespace Playbook.Samples for the model and resource wrapper. The Web SDK embeds the shown .resx files; the wrapper's Playbook.Samples.Captions resource base name must match the resulting manifest name.
4. Replace helper calls with DisplayName For using the same simple member expression. Keep model metadata as the caption owner.
5. Before mapping components, call UseRequestLocalization with default en-GB and supported cultures/UI cultures en-GB and fr-FR. The sample selects French with culture=fr-FR&ui-culture=fr-FR.
6. Expect Survey lead when both attributes exist, Diver name for DisplayName alone, ImportedCode with neither and Adresse de contact from the resource. For editable controls use Label as in blazor.associate-generated-inputs; DisplayName itself supplies no label element or for/id association.

## Constraints

Display.GetName takes precedence; a null result allows fallback. ResourceType needs a suitable public static getter and correctly embedded resources. Missing resources are deployment errors, not grounds for silently replacing metadata with hard-coded translations.

The caption cache is culture-sensitive and process-lived. Request culture selection is proven here; live WebAssembly culture switching and trimming of reflection/resource metadata aren't. Caption resources don't enable translated validation messages; that is a separate generated-validation goal.

The exact baselines are SDK 10.0.402 / ASP.NET Core 10.0.13 and SDK 11.0.100-rc.2.26504.105 / ASP.NET Core 11.0.0-rc.2.26504.105, Windows x64 and Chromium 152. RC proof is not GA verification, screen-reader operation, trimming/AOT proof or a platform/browser matrix. No conditional newer-package/older-runtime route or extra .NET 10 foundation edition is advertised.

## Evidence

The compiled .NET 10 helper and .NET 11 component render matching captions. Actual browser inspection checks precedence, both fallback paths and French resource loading with document language fr. Actual HTTP rendering repeats localization for both artifacts. No localizer mock or screen-reader session is presented as proof.
