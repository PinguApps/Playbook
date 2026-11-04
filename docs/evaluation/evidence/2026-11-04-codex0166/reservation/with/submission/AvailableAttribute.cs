using System.ComponentModel.DataAnnotations;

namespace ReedReservations;

public sealed class AvailableAttribute : AsyncValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext) =>
        throw new InvalidOperationException("Availability requires asynchronous validation.");

    protected override async Task<ValidationResult?> IsValidAsync(
        object? value, ValidationContext validationContext, CancellationToken cancellationToken)
    {
        // Required supplies the error for a missing code.
        if (value is not string code || string.IsNullOrWhiteSpace(code))
            return ValidationResult.Success;

        var availability = validationContext.GetRequiredService<Availability>();
        var available = await availability.IsAvailableAsync(code, cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        return available
            ? ValidationResult.Success
            : new ValidationResult("This hide is unavailable.", [validationContext.MemberName!]);
    }
}
