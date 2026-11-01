#region reef-resolver
#pragma warning disable ASP0029 // Deliberate use of .NET 10 experimental metadata extension contracts.
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Microsoft.Extensions.Validation;

namespace Playbook.Samples;

public sealed class TankReport
{
    public string Code { get; set; } = "T-17";
    public PumpReport Pump { get; set; } = new();
}
public sealed class PumpReport { public string Serial { get; set; } = "P-3"; }

internal sealed class ReefResolver : IValidatableInfoResolver
{
    public bool TryGetValidatableTypeInfo(Type type, [NotNullWhen(true)] out IValidatableInfo? validatableInfo)
    {
        validatableInfo = type == typeof(TankReport) ? new TankInfo() : type == typeof(PumpReport) ? new PumpInfo() : null;
        return validatableInfo is not null;
    }
    public bool TryGetValidatableParameterInfo(ParameterInfo parameterInfo, [NotNullWhen(true)] out IValidatableInfo? validatableInfo)
    {
        validatableInfo = parameterInfo.Member.DeclaringType == typeof(ReefImport) && parameterInfo.Name == "report" ? new ReportParameter() : null;
        return validatableInfo is not null;
    }
    private sealed class TankInfo() : ValidatableTypeInfo(typeof(TankReport),
        [new ReefProperty(typeof(TankReport), typeof(string), "Code"), new ReefProperty(typeof(TankReport), typeof(PumpReport), "Pump")])
    {
        protected override ValidationAttribute[] GetValidationAttributes() => [];
    }
    private sealed class PumpInfo() : ValidatableTypeInfo(typeof(PumpReport), [new ReefProperty(typeof(PumpReport), typeof(string), "Serial")])
    {
        protected override ValidationAttribute[] GetValidationAttributes() => [];
    }
    private sealed class ReefProperty(Type owner, Type type, string name) : ValidatablePropertyInfo(owner, type, name, name)
    {
        private readonly bool required = type == typeof(string);
        protected override ValidationAttribute[] GetValidationAttributes() =>
            required ? [new RequiredAttribute { ErrorMessage = "Value required." }] : [];
    }
    private sealed class ReportParameter() : ValidatableParameterInfo(typeof(TankReport), "report", "report")
    {
        protected override ValidationAttribute[] GetValidationAttributes() => [];
    }
}
internal static class ReefImport
{
    public static void Accept(TankReport report) { }
    public static async Task<ValidateContext> CheckAsync(TankReport report, IServiceProvider services, CancellationToken token)
    {
        var options = new ValidationOptions();
        options.Resolvers.Add(new ReefResolver());
        var context = new ValidateContext
        {
            ValidationOptions = options,
            ValidationContext = new ValidationContext(report, services, null),
            CurrentValidationPath = "report"
        };
        // The older contract has only ValidateAsync. Await external work explicitly.
        var ledger = (MaintenanceLedger?)context.ValidationContext.GetService(typeof(MaintenanceLedger))
            ?? throw new InvalidOperationException("MaintenanceLedger is required.");
        await ledger.CheckAsync(token);
        options.TryGetValidatableTypeInfo(typeof(TankReport), out var validatableInfo);
        await validatableInfo!.ValidateAsync(report, context, token);
        return context;
    }
}
#pragma warning restore ASP0029
#endregion reef-resolver
