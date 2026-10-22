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

// snippet:ledger
public sealed class PermitLedger : IServiceProvider
{
    public TaskCompletionSource Entered { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
    public TaskCompletionSource<bool> Decision { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
    public CultureInfo? ObservedCulture { get; private set; }
    public bool ObservedCancellation { get; private set; }

    public async Task<bool> AcceptsAsync(string permit, CancellationToken token)
    {
        Entered.TrySetResult();
        bool accepted;
        try { accepted = await Decision.Task.WaitAsync(token); }
        catch (OperationCanceledException) { ObservedCancellation = token.IsCancellationRequested; throw; }
        ObservedCulture = CultureInfo.CurrentCulture;
        return accepted && permit == "reef-24";
    }

    public object? GetService(Type serviceType) => serviceType == typeof(PermitLedger) ? this : null;
}
// endsnippet:ledger
