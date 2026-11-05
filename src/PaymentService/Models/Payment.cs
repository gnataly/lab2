using System.ComponentModel.DataAnnotations;

namespace PaymentService.Models;

public class Payment
{
    public int Id { get; set; }

    [Required]
    public Guid PaymentUid { get; set; }

    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = null!;

    [Required]
    public int Price { get; set; }
}