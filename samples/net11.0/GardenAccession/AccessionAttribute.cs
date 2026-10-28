using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components.Forms;

namespace Playbook.Samples;

// snippet:rule
[AttributeUsage(AttributeTargets.Property)]
public sealed class AccessionAttribute : ValidationAttribute, IClientValidationRuleProvider
{
    public AccessionAttribute() => ErrorMessage = "Use two uppercase bed letters, a dash and four ASCII digits.";

    public override bool IsValid(object? value)
    {
        if (value is null or "") return true; // Required owns empty input.
        return value is string text && text.Length == 7
            && text[0] is >= 'A' and <= 'Z' && text[1] is >= 'A' and <= 'Z'
            && text[2] == '-' && text.AsSpan(3).IndexOfAnyExceptInRange('0', '9') < 0;
    }

    public IEnumerable<ClientValidationRule> GetClientValidationRules()
    {
        yield return new("garden.accession", new Dictionary<string, string>
        {
            ["letters"] = "2", ["digits"] = "4", ["separator"] = "-"
        });
    }
}
// endsnippet:rule

// snippet:model
public sealed class PlantEntry
{
    [Required(ErrorMessage = "Enter an accession code."), Accession]
    public string Code { get; set; } = "";
}
// endsnippet:model
