using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Microsoft.Extensions.Validation;

namespace Playbook.Samples;

// snippet:model
[ValidatableType]
public sealed class VisitRequest
{
    [Required, OpenVisit]
    public string Pass { get; set; } = "open";

    [Display(Name = "Party size"), PartyLimit(6)]
    public int Guests { get; set; } = 2;
}
// endsnippet:model

// snippet:async-rule
public sealed class OpenVisitAttribute : AsyncValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext) =>
        throw new InvalidOperationException("The visit rule requires asynchronous validation.");

    protected override async Task<ValidationResult?> IsValidAsync(
        object? value, ValidationContext validationContext, CancellationToken cancellationToken)
    {
        if (value is not string code || string.IsNullOrWhiteSpace(code))
            return ValidationResult.Success; // Required owns the missing-value message.
        var rule = validationContext.GetRequiredService<VisitRule>();
        return await rule.IsOpenAsync(code, cancellationToken)
            ? ValidationResult.Success
            : new ValidationResult("This visit is closed.", [validationContext.MemberName!]);
    }
}
// endsnippet:async-rule

// snippet:formatter
public sealed class PartyLimitAttribute(int maximum) : ValidationAttribute
{
    public override bool IsValid(object? value) => value is int count && count is > 0 && count <= maximum;

    public override string FormatMessage(string format, string name) =>
        string.Format(CultureInfo.CurrentCulture, format, name, maximum);

    public override string FormatErrorMessage(string name) =>
        FormatMessage("{0}: choose between 1 and {1} guests.", name);
}
// endsnippet:formatter
