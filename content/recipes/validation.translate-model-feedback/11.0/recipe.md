# Share translated validation feedback across forms and endpoints

## Goal

The observatory's group-size limit should produce the same translated message in
a Blazor form and a Minimal API response, including the rule's maximum of six.

## Older approach

.NET 10 supports resource-backed attributes and explicit shared formatting.
This comparison resolves a resource from CurrentUICulture and formats it under
CurrentCulture. Both the manual form check and endpoint filter use the same
attribute. It is a valid approach; it simply puts resource lookup in the attribute.

{{snippet:old-model}}

{{snippet:old-formatter}}

{{snippet:old-checks}}

## Newer approach

Register AddLocalization and AddValidation. The source-generated validation path
finds templates and display names via IStringLocalizerFactory for the declaring
type. The custom ValidationAttribute overrides FormatMessage(string format,
string name), receiving the selected template and display name and supplying its
own maximum argument. The formatter does not retrieve resources or validate values.

{{snippet:model}}

{{snippet:formatter}}

{{snippet:async-rule}}

The model above also contains OpenVisit, an independent asynchronous rule defined
in minimal-api.await-model-rules and blazor.await-form-rules. Keep its VisitRule
registration when using the complete canonical app; omit that attribute if only
demonstrating localized group-size validation.

## Migration

1. Create the release-specific Web SDK project and public model shown below.
2. Register the validation, localization and rule services before building the app; configure culture before validation executes.
3. Wire the shown form or endpoint adapter, then exercise both accepted and rejected input through the actual host.

Create VisitRequest.resx and VisitRequest.fr-FR.resx alongside VisitRequest.cs.
With RootNamespace Playbook.Samples they embed as
Playbook.Samples.VisitRequest and its French satellite resource. Keep the standard
resx root/resheader structure and add these canonical entries. No ResourcesPath
is set in this sample; a configured path changes the manifest naming convention.

Neutral resource contents:

{{snippet:resources}}

French resource contents:

{{snippet:french}}

The explicit Display.Name value "Party size" is also a localizer key. For messages,
lookup tries a translated explicit ErrorMessage first, then
<DeclaringType>_<Member>_<AttributeClass>_Error,
<DeclaringType>_<AttributeClass>_Error, and <AttributeClass>_Error. AttributeClass
includes the Attribute suffix. The tested member key therefore is
VisitRequest_Guests_PartyLimitAttribute_Error. Type/global entries deliberately
have different text so the real consumers prove the member tier wins.
An untranslated explicit key can fall through to convention keys; if none resolve,
the attribute's own message is the fallback. Keep that fallback meaningful.

For type-level rules there is no member tier. Nullable type wrappers/generic arity
are normalized by the generated lookup helper. ErrorMessageResourceType and
Display.ResourceType attributes perform their own lookup and bypass this localizer;
do not expect the same attribute to use both mechanisms. Use
ValidationOptions.LocalizerProvider when intentionally sharing a different
resource type across declaring types; default lookup uses the declaring model type.

Set request formatting/UI cultures before validation:

{{snippet:culture}}

AddLocalization registers services, not the user's language. Query-string culture
selection works for a request/SSR post. Interactive Server's circuit handshake is
another request: use a culture cookie and reload to establish its language.
The sample's bounded language selector sets that cookie before navigation:

{{snippet:cookie}}

The identical model reaches these actual framework adapters:

{{snippet:endpoint}}

{{snippet:form}}


## Project setup

The newer canonical project uses Microsoft.NET.Sdk.Web, TargetFramework net11.0,
LangVersion 15.0, Nullable enable and ImplicitUsings enable. The previous project
uses net10.0 and C# 14.0. Set RootNamespace to Playbook.Samples so resource names
match the model namespace. The measured Web SDK supplies Extensions.Validation
and its generator from Microsoft.AspNetCore.App.Ref; do not add a redundant
package reference. Keep its generator, runtime and SDK-aligned
Microsoft.AspNetCore.App.Internal.Assets browser files together. Restore/build
each project in its own directory using its global.json pin.

Model classes and attributes below live in .cs files in namespace Playbook.Samples.
Import System.ComponentModel.DataAnnotations, System.Globalization and System.Resources; newer models also import
Microsoft.Extensions.Validation. Framework implicit usings supply the web and DI
extensions. Register the model's generated metadata in the assembly declaring it;
the Blazor root has [ValidatableType]. A missing generator root can silently fall
back to top-level Blazor validation; an undiscovered endpoint has no automatic
validation. Do not treat missing errors as successful coverage.

{{snippet:startup}}

The remote rule is a real HTTP boundary. Register it with
builder.Services.AddHttpClient<VisitRule>(client => client.BaseAddress = new
Uri("http://localhost:5285")); before Build. The older app uses port 5284. A local
GET /rules/{code} returns a JSON boolean after an asynchronous check; in the
canonical app its controllable journal can hold, release and observe each check.
Replace that local rule with the authoritative service for a real application.
Do not send private data or privileged rules to a browser.

{{snippet:remote}}

No paid service, account, provider, native workload or provisioning is required.
The sample /control endpoints and volatile journal are local test instrumentation,
not application administration or persistence. Keep them out of production.

## Constraints and alternatives

Use generated Extensions.Validation metadata; Blazor's top-level DataAnnotations
fallback does not implement these resource conventions. Resource-backed attributes
remain an alternative on that path. This pipeline is shared by Blazor and Minimal
APIs, not MVC/Razor Pages. In WebAssembly deploy the resources and localization
services to the client, and configure its culture; server request middleware alone
cannot change browser execution. The actual comparisons execute static SSR and
Interactive Server, not WebAssembly/Auto, AOT or trimming.

RC2 removed IValidationMessageFormatter. Do not implement that earlier interface
or install the removed Microsoft.Extensions.Validation.Localization package and
AddValidationLocalization API. Cumulative docs can retain that preview text; the
recorded pinned source and actual override build establish this recipe's contract.
There is no experimental opt-in for this .NET 11 route. Its evidence is RC2, not GA.
Keep CurrentCulture for numeric formatting separate from CurrentUICulture for
resource lookup; translated input parsing is another policy.

## Evidence

Both actual projects compile. Real endpoint requests and static Blazor posts yield
exactly "Party size: choose between 1 and 6 guests." in en-GB and
"Taille du groupe : choisissez de 1 à 6 visiteurs." in fr-FR. Both actual
Interactive Server apps also reject the same invalid group with those messages
under culture-cookie-selected circuits. The newer path executes the custom
FormatMessage override and member-over-type/global convention. Other fallback
tiers are pinned-source review, not separately executed claims. Reports retain
exact sample inputs and assembly hashes; the installed package includes all prose,
resources and snippets without needing these reference links or network access.
