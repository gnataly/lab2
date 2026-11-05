using System.ComponentModel.DataAnnotations;

namespace CarsService.Models;

public class Car
{
    public int Id { get; set; }

    [Required]
    public Guid CarUid { get; set; }

    [Required]
    [MaxLength(80)]
    public string Brand { get; set; } = null!;

    [Required]
    [MaxLength(80)]
    public string Model { get; set; } = null!;

    [Required]
    [MaxLength(20)]
    public string RegistrationNumber { get; set; } = null!;

    public int? Power { get; set; }

    [Required]
    public int Price { get; set; }

    [MaxLength(20)]
    public string? Type { get; set; }

    [Required]
    public bool Availability { get; set; }
}