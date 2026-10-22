using System.ComponentModel.DataAnnotations;

namespace Playbook.Samples;

// Deliberately no ValidatableType and no Minimal API signature that discovers this root.
public sealed class UndiscoveredRequest
{
    [Required] public string Lead { get; set; } = "Reef team";
    public SurveyContact Contact { get; set; } = new();
}
