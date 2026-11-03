using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Validation;

namespace ReedReservations;

[ValidatableType]
public sealed class Reservation
{
    [Required, Available]
    public string Code { get; set; } = "";
    [Range(1, 4)]
    public int Guests { get; set; }
}

public sealed class AvailableAttribute : AsyncValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext context) =>
        throw new InvalidOperationException("An asynchronous check is required.");

    protected override async Task<ValidationResult?> IsValidAsync(object? value,
        ValidationContext context, CancellationToken cancellationToken)
    {
        if (value is not string code || string.IsNullOrWhiteSpace(code)) return ValidationResult.Success;
        var service = context.GetRequiredService<Availability>();
        return await service.IsAvailableAsync(code, cancellationToken)
            ? ValidationResult.Success : new ValidationResult("Unavailable.", [context.MemberName!]);
    }
}
