# Connect a custom server form rule to browser validation

## Goal

A walled garden records plant accessions such as AB-1234. Give immediate feedback for an exact code shape and still reject an invalid submission on the server. Two uppercase ASCII letters, a dash and four ASCII digits are required. No trimming, case folding or Unicode digit conversion occurs.

## Older approach

.NET 10 can apply an original DataAnnotations attribute on the server and maintain a separate browser rule. This remains a valid approach; the framework's static SSR custom-rule export contracts are unavailable on that baseline.

{{snippet:older-rule}}

The model uses Required for empty/whitespace input and Accession for the nonempty shape:

{{snippet:model}}

The old static form and original delegated JS handler provide the same browser/server outcome:

{{snippet:older-form}}

{{snippet:older-client}}

Delegation survives enhanced DOM replacement. The script duplicates both the shape and message; changing the server rule requires changing it too. The formnovalidate button deliberately bypasses the browser handler.

## Newer approach

On .NET 11 implement IClientValidationRuleProvider on the custom attribute. This adds metadata export to the same server rule; it does not turn C# into JavaScript.

{{snippet:newer-rule}}

AddRazorComponents registers the framework's ClientValidationProvider implementation. It reads the rendered inputs and their server-validatable properties, recognizes IClientValidationRuleProvider and serializes its rule name, string parameters and server-formatted error message. The canonical sample uses that registered provider; it does not replace the abstract provider or hand-author the framework's payload. A replacement ClientValidationProvider.RenderClientValidationRules is a whole-form metadata responsibility and is unnecessary for this one attribute.

{{snippet:newer-form}}

Register the matching name once at app startup, after the validation service exists. This initializer is wwwroot/GardenAccession.lib.module.js for assembly GardenAccession:

{{snippet:newer-client}}

Rule names are case-sensitive, registrations are app-wide, and a repeated name replaces the previous validator. An unregistered rule name is skipped by the browser, so emitting metadata alone provides no protection. The JS maps letters/digits/separator from string parameters and returns success=false to use the server's message. Empty values succeed here because Required owns them. Both languages use ASCII ranges, exact lengths and no normalization.

## Migration

1. Keep the server rule authoritative. Use Microsoft.NET.Sdk.Web, net11.0, C# 15.0, nullable and implicit usings enabled. The comparison uses net10.0/C# 14.0. No optional workload, native toolchain or experimental opt-in is required. The exact measured pins are SDK 10.0.402/framework and Internal.Assets 10.0.13 versus SDK 11.0.100-rc.2.26504.105/framework and Internal.Assets 11.0.0-rc.2.26504.105. SDK-aligned Internal.Assets is an implicit dependency, locked in each packages.lock.json; no additional PackageReference is required.
2. Put AccessionAttribute, PlantEntry, Entry.razor and SubmissionCounts in namespace Playbook.Samples. Use _Imports.razor with @namespace Playbook.Samples and @using Microsoft.AspNetCore.Components, Microsoft.AspNetCore.Components.Forms, Microsoft.AspNetCore.Components.Routing, Microsoft.AspNetCore.Http and Playbook.Samples. Use this static document:

{{snippet:document}}

3. Use the host and counters below. The older host simply calls AddRazorComponents() without the DisableClientValidation option; all other host/counter operations are identical. The counters are demonstration instrumentation, not business persistence:

{{snippet:host}}

{{snippet:counts}}

4. Add the assembly-named initializer under wwwroot and serve MapStaticAssets. It works with automatic startup in the document above. With manual startup, register after Blazor.start completes. Do not put initialization in a page script loaded by enhanced navigation or invoke IJSRuntime from static SSR. Remove the old delegated handler when enabling framework rules to avoid two competing client engines.
5. Browse /enhanced and /plain. Invalid lowercase, wrong separator, short code, full-width digits and leading spaces must produce feedback and block submission. AB-1234 must post successfully. Enhanced posting preserves the current document; plain posting navigates to a new document.
6. Check /optout (DataAnnotationsValidator.DisableClientValidation) and a host started with --DisableClientValidation=true separately. Both suppress rule emission while keeping server validation. The formnovalidate button skips one submit check without disabling rules for the form. A direct invalid POST with a real antiforgery token must also be rejected.

## Constraints

Static SSR only, including enhanced and plain forms. Interactive forms use EditContext validation in .NET instead of this static client pipeline. This example intentionally has one top-level synchronous rule; nested model discovery, async checks and localized business validation are separate guidance: blazor.check-input-before-post, blazor.await-form-rules and validation.validate-asynchronously.

Browser validation improves feedback and can always be bypassed. Never use it for authorization, uniqueness, persistence or accepting input. The sample accepts only in OnValidSubmit; a rejected post leaves the acceptance count unchanged. Antiforgery still applies to plain/direct posts.

Requirements describe the .NET 11 release family; exact pins describe measured RC2 evidence. No optional older-target package route, trimming/AOT, WebAssembly/Auto transition, other-browser, screen-reader or actual-GA execution claim is made. The new APIs have no experimental annotation in the inspected RC2 assets.

## Evidence

Four enhanced/plain browser runs across the two releases exercise six invalid shapes, valid acceptance, correct document preservation/navigation, and a bypass rejected by the server. Four corresponding direct-request sets exercise seven values with real antiforgery tokens. RC2 checks additionally inspect the actual custom rule mapping and execute per-form/global opt-outs. Canonical compile and separate execution records bind the actual inputs; browser/request tests are real, not metadata-only or mocked. Installed-package list/get checks verify the full offline body and snippet identity separately.
