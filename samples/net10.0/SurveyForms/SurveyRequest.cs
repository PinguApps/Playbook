using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Validation;

namespace Playbook.Samples;

// snippet:model
// .NET 10 marks these generator attributes experimental.
#pragma warning disable ASP0029
[Microsoft.Extensions.Validation.ValidatableType]
public sealed class SurveyRequest
{
    [Required(ErrorMessage = "Enter a survey lead.")]
    [Display(Name = "Survey lead"), DisplayName("Superseded caption")]
    public string Lead { get; set; } = "";

    public SurveyContact Contact { get; set; } = new();
    public List<Diver> Divers { get; set; } = [new(), new()];

    // This is a deliberately unvalidated reference snapshot, never accepted input.
    [SkipValidation]
    public ReferenceSnapshot Reference { get; set; } = new();
}

public sealed class SurveyContact
{
    [Required(ErrorMessage = "Enter a contact address."), EmailAddress]
    [Display(Name = nameof(Captions.Contact), ResourceType = typeof(Captions))]
    public string Email { get; set; } = "";
}

public sealed class Diver
{
    [Required(ErrorMessage = "Enter a diver name."), DisplayName("Diver name")]
    public string Name { get; set; } = "";
}

public sealed class ReferenceSnapshot
{
    [Required] public string ImportedCode { get; set; } = "";
}
#pragma warning restore ASP0029
// endsnippet:model
