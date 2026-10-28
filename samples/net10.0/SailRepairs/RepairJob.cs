// snippet:model
using System.ComponentModel.DataAnnotations;

namespace Playbook.Samples;

public sealed class RepairJob
{
    [AllowedMaterial]
    public string Material { get; set; } = "";
}

public sealed class MaterialCatalogue
{
    public Guid ScopeId { get; } = Guid.NewGuid();
    public int Checks { get; private set; }
    public bool Allows(string material)
    {
        Checks++;
        return material is "canvas" or "sailcloth";
    }
}

[AttributeUsage(AttributeTargets.Property)]
public sealed class AllowedMaterialAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (validationContext.GetService(typeof(MaterialCatalogue)) is not MaterialCatalogue catalogue)
            return new ValidationResult("Material catalogue unavailable.", new[] { validationContext.MemberName! });
        return catalogue.Allows(value as string ?? "") ? ValidationResult.Success
            : new ValidationResult("Choose canvas or sailcloth.", new[] { validationContext.MemberName! });
    }
}
// endsnippet:model
