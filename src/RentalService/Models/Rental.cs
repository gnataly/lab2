using System.ComponentModel.DataAnnotations;

namespace RentalService.Models;

public class Rental
{
    public int Id { get; set; }

    [Required]
    public Guid RentalUid { get; set; }

    [Required]
    [MaxLength(80)]
    public string Username { get; set; } = null!;

    [Required]
    public Guid PaymentUid { get; set; }

    [Required]
    public Guid CarUid { get; set; }

    [Required]
    public DateTimeOffset DateFrom { get; set; }

    [Required]
    public DateTimeOffset DateTo { get; set; }

    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = null!;
}