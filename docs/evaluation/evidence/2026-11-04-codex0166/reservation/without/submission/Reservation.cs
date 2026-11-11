using System.ComponentModel.DataAnnotations;

namespace ReedReservations;

public sealed class Reservation
{
    [Required]
    public string Code { get; set; } = "";

    [Range(1, 4)]
    public int Guests { get; set; }

    public async Task<Dictionary<string, string[]>> ValidateAsync(
        Availability availability, CancellationToken cancellationToken)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(this, new ValidationContext(this), results, validateAllProperties: true);
        var errors = results
            .SelectMany(result => result.MemberNames.Select(member =>
                (Member: member, Message: result.ErrorMessage!)))
            .GroupBy(error => error.Member)
            .ToDictionary(group => group.Key, group => group.Select(error => error.Message).ToArray());

        if (errors.Count == 0 && !await availability.IsAvailableAsync(Code, cancellationToken))
        {
            errors[nameof(Code)] = ["The code is unavailable."];
        }

        cancellationToken.ThrowIfCancellationRequested();
        return errors;
    }
}
