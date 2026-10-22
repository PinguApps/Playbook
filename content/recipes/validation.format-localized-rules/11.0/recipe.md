# Format validation messages using the rule's own arguments

## Goal

A survey station displays depth limits in the user's language and numeric convention. Let a validation rule supply its own limit arguments while the host supplies a translated template and display name.

## Older approach

A .NET 10 host can use resource-backed ErrorMessageResourceType/ErrorMessageResourceName, or explicitly map placeholders for a particular attribute. The latter works for a small known rule set but makes the host responsible for each attribute's formatting contract. This Range-only comparison keeps that responsibility visible.

{{snippet:older}}

## Newer approach

ValidationAttribute.FormatMessage accepts an externally supplied template. Built-in overrides such as RangeAttribute fill rule-specific placeholders: display name at {0}, minimum at {1}, maximum at {2}. The host no longer has to reproduce that Range mapping. The existing FormatErrorMessage method remains callable; removal of a declared override does not remove inherited behavior.

{{snippet:newer}}

## Migration

1. Use a net11.0 console project with C# 15.0, nullable and implicit usings enabled. Import System.ComponentModel.DataAnnotations and System.Globalization. No external package or rendering framework is required.
2. Create RangeAttribute with typeof(decimal), limits "1.25" and "3.75", ParseLimitsInInvariantCulture=true and ConvertValueInInvariantCulture=true. This fixes the stored numeric rule independently of the display language. Do not parse translated numeric input as invariant unless that matches the input contract.
3. Choose the template in the host: for en-GB use "{0}: from {1} to {2}."; for fr-FR use "{0} : de {1} à {2}.". Choose a translated display name as appropriate. Resource lookup is a separate host responsibility.
4. Set CultureInfo.CurrentCulture to the intended formatting culture for the operation, call LocalizedRule.Describe with the rule, chosen template and display name, then restore any temporarily changed ambient culture in finally. CurrentUICulture can govern resource lookup; changing it alone does not set numeric formatting.
5. Check that 2m remains valid and 4m remains invalid in both cultures. The en-GB message contains 1.25/3.75; fr-FR contains 1,25/3,75. Keep custom-attribute placeholder contracts documented when overriding FormatMessage.

## Constraints

FormatMessage formats; it does not retrieve translations, choose a culture, validate a value, or activate a localizer. An invalid composite template can throw. Attribute-specific placeholders differ; a host must supply a suitable template rather than assuming every rule has Range's arguments.

This shared example calls DataAnnotations directly. Microsoft.Extensions.Validation's LocalizerProvider and generated metadata, Blazor forms and endpoint responses are separate integrations. Existing resource-backed attributes remain valid. Neither the old explicit Range mapping nor FormatMessage proves translated UI, AOT/trimming support or a particular web framework's selection policy. No newer-package/older-target route is advertised.

## Evidence

Both actual release projects execute formatting under en-GB and fr-FR and assert culture-specific decimal separators and unchanged range acceptance/rejection. The newer FormatMessage initializes the rule's invariantly parsed limits itself; the older host initializes and maps them explicitly. The related awaited permit check also records culture flow. Saved compilation/execution inputs distinguish the .NET 11 RC2 baseline from GA and from unexecuted resource-provider or rendering integration.
