// snippet:reef-resolver
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
    public bool TryGetValidatableTypeInfo(Type type, [NotNullWhen(true)] out IValidatableTypeInfo? validatableTypeInfo)
    {
        validatableTypeInfo = type == typeof(TankReport) ? new TankInfo() : type == typeof(PumpReport) ? new PumpInfo() : null;
        return validatableTypeInfo is not null;
    }
    public bool TryGetValidatableParameterInfo(ParameterInfo parameterInfo, [NotNullWhen(true)] out IValidatableParameterInfo? validatableParameterInfo)
    {
        validatableParameterInfo = parameterInfo.Member.DeclaringType == typeof(ReefImport) && parameterInfo.Name == "report" ? new ReportParameter() : null;
        return validatableParameterInfo is not null;
    }
    private sealed class TextProperty(string name, Func<object, string> read) : IValidatablePropertyInfo
    {
        public void Validate(object containingObject, ValidateContext context)
        {
            if (string.IsNullOrWhiteSpace(read(containingObject)))
                context.AddValidationError(new ValidationError
                {
                    Name = name, Path = context.CurrentValidationPath + "." + name,
                    Container = containingObject, ErrorMessage = "Value required."
                });
        }
        public Task ValidateAsync(object containingObject, ValidateContext context, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Validate(containingObject, context);
            return Task.CompletedTask;
        }
    }
    private sealed class TankInfo : IValidatableTypeInfo
    {
        private readonly TextProperty code = new("Code", o => ((TankReport)o).Code);
        public bool TryFindProperty(string propertyName, ValidationOptions validationOptions, [NotNullWhen(true)] out IValidatablePropertyInfo? validatablePropertyInfo)
        { validatablePropertyInfo = propertyName == "Code" ? code : null; return validatablePropertyInfo is not null; }
        private static MaintenanceLedger Ledger(ValidateContext context) =>
            (MaintenanceLedger?)context.ServiceProvider?.GetService(typeof(MaintenanceLedger))
                ?? throw new InvalidOperationException("MaintenanceLedger is required.");
        public void Validate(object? value, ValidateContext context)
        {
            if (value is not TankReport report) return;
            Ledger(context).Check();
            ValidateMembers(report, context);
        }
        public async Task ValidateAsync(object? value, ValidateContext context, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (value is not TankReport report) return;
            await Ledger(context).CheckAsync(cancellationToken);
            ValidateMembers(report, context);
        }
        private void ValidateMembers(TankReport report, ValidateContext context)
        {
            code.Validate(report, context);
            string previous = context.CurrentValidationPath;
            try
            {
                context.CurrentValidationPath += ".Pump";
                new PumpInfo().Validate(report.Pump, context);
            }
            finally { context.CurrentValidationPath = previous; }
        }
    }
    private sealed class PumpInfo : IValidatableTypeInfo
    {
        private readonly TextProperty serial = new("Serial", o => ((PumpReport)o).Serial);
        public bool TryFindProperty(string propertyName, ValidationOptions validationOptions, [NotNullWhen(true)] out IValidatablePropertyInfo? validatablePropertyInfo)
        { validatablePropertyInfo = propertyName == "Serial" ? serial : null; return validatablePropertyInfo is not null; }
        public void Validate(object? value, ValidateContext context) { if (value is PumpReport pump) serial.Validate(pump, context); }
        public Task ValidateAsync(object? value, ValidateContext context, CancellationToken cancellationToken)
        { cancellationToken.ThrowIfCancellationRequested(); Validate(value, context); return Task.CompletedTask; }
    }
    private sealed class ReportParameter : IValidatableParameterInfo
    {
        public void Validate(object? value, ValidateContext context) => new TankInfo().Validate(value, context);
        public Task ValidateAsync(object? value, ValidateContext context, CancellationToken cancellationToken) => new TankInfo().ValidateAsync(value, context, cancellationToken);
    }
}
internal static class ReefImport
{
    public static void Accept(TankReport report) { }
    public static ValidateContext Context(IServiceProvider services, ValidationOptions options) => new()
    { ServiceProvider = services, ValidationOptions = options, CurrentValidationPath = "report" };
}
// endsnippet:reef-resolver
