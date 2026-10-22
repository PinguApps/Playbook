using System.ComponentModel.DataAnnotations;

namespace Playbook.Samples;

// snippet:async
public sealed class DockRequest
{
    [Required, AvailablePermit] public string Permit { get; set; } = "reef-24";
}

public sealed class AvailablePermitAttribute : AsyncValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext) =>
        throw new InvalidOperationException("This permit rule requires asynchronous validation.");

    protected override async Task<ValidationResult?> IsValidAsync(
        object? value, ValidationContext validationContext, CancellationToken cancellationToken)
    {
        if (value is not string permit || string.IsNullOrWhiteSpace(permit))
            return ValidationResult.Success; // Required owns missing values.
        var ledger = (PermitLedger?)validationContext.GetService(typeof(PermitLedger))
            ?? throw new InvalidOperationException("Register PermitLedger in the validation context.");
        return await ledger.AcceptsAsync(permit, cancellationToken)
            ? ValidationResult.Success
            : new ValidationResult("Permit is unavailable.", [validationContext.MemberName!]);
    }
}

public static class DockValidation
{
    public static async Task<List<ValidationResult>> CheckAsync(
        DockRequest request, PermitLedger ledger, CancellationToken token)
    {
        List<ValidationResult> errors = [];
        var context = new ValidationContext(request, ledger, null);
        await Validator.TryValidateObjectAsync(request, context, errors, true, token);
        return errors;
    }
}
// endsnippet:async

// snippet:localized
public static class LocalizedRule
{
    public static string Describe(RangeAttribute rule, string template, string displayName) =>
        rule.FormatMessage(template, displayName);
}
// endsnippet:localized
