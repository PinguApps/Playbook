#if !NET11_0_OR_GREATER
#pragma warning disable ASP0029 // This entire executable exercises the old experimental metadata API.
#endif
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Validation;
#if NET11_0_OR_GREATER
using Microsoft.Extensions.Localization;
#endif

namespace Playbook.Samples;

internal static class Program
{
    private static void Require([DoesNotReturnIf(false)] bool condition, string message)
    { if (!condition) throw new InvalidOperationException(message); }

    private static ServiceProvider Services(MaintenanceLedger? ledger)
    {
        var services = new ServiceCollection();
        if (ledger is not null) services.AddSingleton(ledger);
#if !MISSING_REGISTRATION
        services.AddValidation(o => o.Resolvers.Add(new ReefResolver()));
#endif
        return services.BuildServiceProvider();
    }

    public static async Task Main()
    {
        RuntimeIdentity.Assert();
        using var services = Services(new());
        var options = services.GetRequiredService<IOptions<ValidationOptions>>().Value;
        Require(options.TryGetValidatableTypeInfo(typeof(TankReport), out var type), "Custom resolver missing");
        Require(!options.TryGetValidatableTypeInfo(typeof(Uri), out _), "Unknown type claimed");
        var parameter = typeof(ReefImport).GetMethod(nameof(ReefImport.Accept))!.GetParameters()[0];
        Require(options.TryGetValidatableParameterInfo(parameter, out var param), "Custom parameter missing");
        var bad = new TankReport { Code = "", Pump = new() { Serial = "" } };
#if NET11_0_OR_GREATER
        foreach (bool async in new[] { false, true })
        {
            var valid = ReefImport.Context(services, options);
            if (async) await type.ValidateAsync(new TankReport(), valid, default); else type.Validate(new TankReport(), valid);
            Require(valid.ValidationErrors is null or { Count: 0 }, "Valid report rejected");
            var invalid = ReefImport.Context(services, options);
            if (async) await type.ValidateAsync(bad, invalid, default); else type.Validate(bad, invalid);
            AssertErrors(invalid, bad);
            var parameterContext = ReefImport.Context(services, options);
            if (async) await param.ValidateAsync(bad, parameterContext, default); else param.Validate(bad, parameterContext);
            AssertErrors(parameterContext, bad);
        }
        Require(type.TryFindProperty("Code", options, out var code) && !type.TryFindProperty("missing", options, out _), "Property lookup wrong");
        var direct = ReefImport.Context(services, options);
        await code.ValidateAsync(bad, direct, default);
        Require(direct.ValidationErrors!["report.Code"].Single().Container == bad, "Property requires containing object");
        // AddValidation's generated resolver and our narrow fallback coexist.
        Require(options.TryGetValidatableTypeInfo(typeof(InspectionLabel), out var generated), "Generated metadata missing");
        foreach (bool async in new[] { false, true })
        {
            var generatedContext = ReefImport.Context(services, options);
            var label = new InspectionLabel();
            if (async) await generated.ValidateAsync(label, generatedContext, default); else generated.Validate(label, generatedContext);
            var error = generatedContext.ValidationErrors!["report.Code"].Single();
            Require(error.Name == "Code" && error.Container == label && error.ErrorMessage == "Code required.", "Generated errors wrong");
        }
        await CheckLocalizationAsync(options);
#else
        var valid = await ReefImport.CheckAsync(new(), services, default);
        Require(valid.ValidationErrors is null or { Count: 0 }, "Valid old report rejected");
        var invalid = await ReefImport.CheckAsync(bad, services, default);
        Require(invalid.ValidationErrors is { Count: 2 } && invalid.ValidationErrors["report.Code"].Single() == "Value required." &&
            invalid.ValidationErrors["report.Pump.Serial"].Single() == "Value required.", "Old nested errors wrong");
        var parameterContext = new ValidateContext { ValidationOptions = options, ValidationContext = new ValidationContext(bad, services, null) };
        await param.ValidateAsync(bad, parameterContext, default);
        Require(parameterContext.ValidationErrors?.Count == 2, "Old parameter metadata not run");
#endif
        using var missing = Services(null);
#if NET11_0_OR_GREATER
        try { type.Validate(new TankReport(), ReefImport.Context(missing, options)); throw new Exception("Sync missing service accepted"); }
        catch (InvalidOperationException e) { Require(e.Message == "MaintenanceLedger is required.", "Wrong sync service failure"); }
#endif
        try { await CheckAsync(new(), missing, default); throw new Exception("Missing service accepted"); }
        catch (InvalidOperationException e) { Require(e.Message == "MaintenanceLedger is required.", "Wrong service failure"); }
        var ledger = new MaintenanceLedger(false);
        using var pendingServices = Services(ledger);
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var pending = CheckAsync(new(), pendingServices, cts.Token);
        await ledger.Entered.Task.WaitAsync(cts.Token);
        Require(!pending.IsCompleted, "Async validation did not await ledger");
        ledger.Decision.SetResult();
        Require((await pending).ValidationErrors is null or { Count: 0 }, "Async completion wrong");
        var cancelLedger = new MaintenanceLedger(false);
        using var cancelServices = Services(cancelLedger);
        var canceled = CheckAsync(new(), cancelServices, cts.Token);
        await cancelLedger.Entered.Task.WaitAsync(cts.Token);
        await cts.CancelAsync();
        try { await canceled; throw new Exception("Cancellation swallowed"); }
        catch (OperationCanceledException e) { Require(e.CancellationToken == cts.Token, "Token identity lost"); }
        var faultLedger = new MaintenanceLedger(false);
        using var faultServices = Services(faultLedger);
        var faulted = CheckAsync(new(), faultServices, default);
        var failure = new IOException("Ledger unavailable");
        faultLedger.Decision.SetException(failure);
        try { await faulted; throw new Exception("Ledger failure swallowed"); }
        catch (IOException e) { Require(ReferenceEquals(e, failure), "Ledger failure identity lost"); }
        Console.WriteLine("PASS ReefMaintenance: valid/invalid/nested/type/property/parameter; required service; awaited completion, failure and cancellation; generated/localized checks where available.");
    }

    private static async Task<ValidateContext> CheckAsync(TankReport report, IServiceProvider services, CancellationToken token)
    {
#if NET11_0_OR_GREATER
        var options = services.GetRequiredService<IOptions<ValidationOptions>>().Value;
        var context = ReefImport.Context(services, options);
        options.TryGetValidatableTypeInfo(typeof(TankReport), out var info);
        await info!.ValidateAsync(report, context, token);
        return context;
#else
        return await ReefImport.CheckAsync(report, services, token);
#endif
    }
#if NET11_0_OR_GREATER
    private static void AssertErrors(ValidateContext context, TankReport report)
    {
        Require(context.CurrentValidationPath == "report" && context.ValidationErrors is { Count: 2 }, "Path or error count wrong");
        foreach (var expected in new[] { ("Code", "report.Code", (object)report), ("Serial", "report.Pump.Serial", (object)report.Pump) })
        {
            var error = context.ValidationErrors[expected.Item2].Single();
            Require(error.Name == expected.Item1 && error.Path == expected.Item2 && ReferenceEquals(error.Container, expected.Item3) &&
                error.ErrorMessage == "Value required.", "Structured error wrong");
            Console.WriteLine($"ERROR {error.Name}|{error.Path}|{error.Container.GetType().Name}|{error.ErrorMessage}");
        }
    }

    private static async Task CheckLocalizationAsync(ValidationOptions options)
    {
        using var services = new ServiceCollection().AddSingleton<IStringLocalizerFactory, ReefLocalizers>().BuildServiceProvider();
        List<Type> observed = [];
        options.LocalizerProvider = (type, factory) => { observed.Add(type); return factory.Create(type); };
        options.TryGetValidatableTypeInfo(typeof(InspectionLabel), out var info);
        var context = ReefImport.Context(services, options);
        await info!.ValidateAsync(new InspectionLabel(), context, default);
        Require(observed.Contains(typeof(InspectionLabel)) && context.ValidationErrors!["report.Code"].Single().ErrorMessage == "Code requis.", "LocalizerProvider not integrated");
        Console.WriteLine("LOCALIZED InspectionLabel report.Code: Code requis.");
    }
#endif
}
#if NET11_0_OR_GREATER
[ValidatableType]
internal sealed class InspectionLabel { [Required(ErrorMessage = "Code required.")] public string Code { get; set; } = ""; }

internal sealed class ReefLocalizers : IStringLocalizerFactory, IStringLocalizer
{
    public IStringLocalizer Create(Type resourceSource) => this;
    public IStringLocalizer Create(string baseName, string location) => this;
    public LocalizedString this[string name] => new(name, name == "Code required." ? "Code requis." : name);
    public LocalizedString this[string name, params object[] arguments] => new(name, string.Format(System.Globalization.CultureInfo.CurrentCulture, this[name].Value, arguments));
    public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) => [];
}
#endif
