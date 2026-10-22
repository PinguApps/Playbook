using System.Globalization;
using System.Runtime.CompilerServices;
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

// snippet:object
public sealed class VesselManifest : IAsyncValidatableObject
{
    public int AsyncCalls { get; private set; }
    public int SyncCalls { get; private set; }
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        SyncCalls++;
        throw new InvalidOperationException("Validate this manifest asynchronously.");
    }
    public async IAsyncEnumerable<ValidationResult> ValidateAsync(
        ValidationContext validationContext, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await Task.Yield();
        cancellationToken.ThrowIfCancellationRequested();
        AsyncCalls++;
        yield return new ValidationResult("Manifest needs a departure berth.");
    }
}
// endsnippet:object
