// Only compiled by the focused failing/fixed variants in Test-ExtensionMetadata.ps1.
#if INACCESSIBLE_MARKER
using Microsoft.Extensions.Validation;
internal static class HiddenLabels
{
    [ValidatableType]
    private sealed class Label { public string Code { get; set; } = ""; }
}
#elif INACCESSIBLE_PROPERTY
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Validation;
[ValidatableType]
internal sealed class HiddenProperty
{
    [Required] internal string Code { get; set; } = "";
}
#endif
