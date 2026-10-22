using System.ComponentModel.DataAnnotations;
using System.Globalization;
#if NET11_0_OR_GREATER
using System.Runtime.CompilerServices;
#endif

namespace Playbook.Samples;

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

internal static class Program
{
    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    public static async Task Main()
    {
        CultureInfo original = CultureInfo.CurrentCulture;
        CultureInfo originalUi = CultureInfo.CurrentUICulture;
        try
        {
            foreach (string culture in new[] { "en-GB", "fr-FR" })
            {
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture);
                CultureInfo.CurrentUICulture = CultureInfo.CurrentCulture;
                var rule = new RangeAttribute(typeof(decimal), "1.25", "3.75")
                { ParseLimitsInInvariantCulture = true, ConvertValueInInvariantCulture = true };
                string template = culture == "fr-FR" ? "{0} : de {1} à {2}." : "{0}: from {1} to {2}.";
                string text = LocalizedRule.Describe(rule, template, "Depth");
                Require(text.Contains(culture == "fr-FR" ? "1,25" : "1.25", StringComparison.Ordinal), text);
                Require(!rule.IsValid(4m) && rule.IsValid(2m), "Range behavior changed while localizing.");
                Console.WriteLine($"Localized {culture}: {text}");
                var ledger = new PermitLedger();
                Task<List<ValidationResult>> pending = DockValidation.CheckAsync(new(), ledger, default);
                await ledger.Entered.Task.WaitAsync(TimeSpan.FromSeconds(10));
                Require(!pending.IsCompleted, "Validation returned before the decision.");
                ledger.Decision.SetResult(true);
                Require((await pending).Count == 0 && ledger.ObservedCulture?.Name == culture, "Completion/culture lost.");
            }
            var rejected = new PermitLedger();
            rejected.Decision.SetResult(false);
            List<ValidationResult> errors = await DockValidation.CheckAsync(new(), rejected, default);
            Require(errors.Count == 1 && errors[0].MemberNames.Single() == "Permit", "Rejection/member lost.");
            Require((await DockValidation.CheckAsync(new() { Permit = "" }, new(), default)).Count == 1, "Required missing.");
            var canceled = new PermitLedger();
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            Task<List<ValidationResult>> check = DockValidation.CheckAsync(new(), canceled, cts.Token);
            await canceled.Entered.Task;
            await cts.CancelAsync();
            try { await check; throw new InvalidOperationException("Cancellation ignored."); }
            catch (OperationCanceledException) { Require(canceled.ObservedCancellation, "Cancellation did not reach the rule."); }
            var broken = new PermitLedger();
            var fault = new IOException("Ledger fault");
            broken.Decision.SetException(fault);
            try { await DockValidation.CheckAsync(new(), broken, default); throw new InvalidOperationException("Failure swallowed."); }
            catch (IOException e) { Require(ReferenceEquals(e, fault), "Failure identity changed."); }
#if NET11_0_OR_GREATER
            await CheckEntryPointsAsync();
#endif
            Console.WriteLine($"PASS {Environment.Version}: awaited permit acceptance/rejection/failure/cancellation, required rules, en-GB/fr-FR formatting and culture flow.");
        }
        finally { CultureInfo.CurrentCulture = original; CultureInfo.CurrentUICulture = originalUi; }
    }

#if NET11_0_OR_GREATER
    private static async Task CheckEntryPointsAsync()
    {
        var ledger = new PermitLedger();
        ledger.Decision.SetResult(true);
        var model = new DockRequest();
        var context = new ValidationContext(model, ledger, null) { MemberName = nameof(DockRequest.Permit) };
        await Validator.ValidateObjectAsync(model, context, true);
        await Validator.ValidatePropertyAsync(model.Permit, context);
        List<ValidationResult> errors = [];
        Require(await Validator.TryValidatePropertyAsync(model.Permit, context, errors), "Property async failed.");
        ValidationAttribute[] attributes = [new AvailablePermitAttribute()];
        await Validator.ValidateValueAsync(model.Permit, context, attributes);
        Require(await Validator.TryValidateValueAsync(model.Permit, context, errors, attributes), "Value async failed.");
        #pragma warning disable CA1849 // Deliberately test the synchronous rejection path.
        try { Validator.ValidateObject(model, context, true); throw new Exception("Sync path accepted async-only rule."); }
        catch (InvalidOperationException e) { Require(e.Message.Contains("asynchronous", StringComparison.Ordinal), "Wrong sync failure."); }
        #pragma warning restore CA1849
        var objectRule = new VesselManifest();
        Require(!await Validator.TryValidateObjectAsync(objectRule, new ValidationContext(objectRule), errors, true), "Object rule missed.");
        Require(objectRule.AsyncCalls == 1 && objectRule.SyncCalls == 0, "Wrong object validation path.");
        Console.WriteLine("PASS six async Validator families; synchronous path rejected; IAsyncValidatableObject awaited, not sync-called.");
    }

    // snippet:object
    private sealed class VesselManifest : IAsyncValidatableObject
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
#endif
}
