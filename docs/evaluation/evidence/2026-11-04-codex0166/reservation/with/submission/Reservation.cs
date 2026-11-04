using System.ComponentModel.DataAnnotations;

namespace ReedReservations;

public sealed class Reservation
{
    [Required, Available]
    public string Code { get; set; } = "";

    [Range(1, 4)]
    public int Guests { get; set; }
}
