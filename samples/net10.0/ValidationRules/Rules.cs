using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace Playbook.Samples;

// snippet:async
public sealed class DockRequest
{
    [Required] public string Permit { get; set; } = "reef-24";
}

public static class DockValidation
{
    public static async Task<List<ValidationResult>> CheckAsync(
        DockRequest request, PermitLedger ledger, CancellationToken token)
    {
        List<ValidationResult> errors = [];
        if (!Validator.TryValidateObject(request, new ValidationContext(request), errors, true))
            return errors;
        if (!await ledger.AcceptsAsync(request.Permit, token))
            errors.Add(new ValidationResult("Permit is unavailable.", [nameof(DockRequest.Permit)]));
        return errors;
    }
}
// endsnippet:async

// snippet:localized
public static class LocalizedRule
{
    public static string Describe(RangeAttribute rule, string template, string displayName)
    {
        // An older host owns this Range-specific placeholder mapping.
        _ = rule.IsValid(2m); // Initialize the invariantly parsed limits.
        return string.Format(CultureInfo.CurrentCulture, template, displayName, rule.Minimum, rule.Maximum);
    }
}
// endsnippet:localized
