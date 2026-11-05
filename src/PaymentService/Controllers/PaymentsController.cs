using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PaymentService.Contracts;
using PaymentService.Data;
using PaymentService.Models;

namespace PaymentService.Controllers;

[ApiController]
[Route("api/v1/payments")]
public class PaymentsController : ControllerBase
{
    private readonly PaymentDbContext _db;

    public PaymentsController(PaymentDbContext db)
    {
        _db = db;
    }

    [HttpPost]
    public async Task<ActionResult<PaymentResponse>> Create(CreatePaymentRequest request)
    {
        var payment = new Payment
        {
            PaymentUid = Guid.NewGuid(),
            Status = "PAID",
            Price = request.Price
        };

        _db.Payments.Add(payment);
        await _db.SaveChangesAsync();

        return Ok(ToResponse(payment));
    }

    [HttpGet("{paymentUid}")]
    public async Task<ActionResult<PaymentResponse>> Get(Guid paymentUid)
    {
        var payment = await _db.Payments.AsNoTracking().FirstOrDefaultAsync(p => p.PaymentUid == paymentUid);
        if (payment is null)
        {
            return NotFound(new { message = "Payment not found" });
        }

        return Ok(ToResponse(payment));
    }

    [HttpPost("{paymentUid}/cancel")]
    public async Task<ActionResult<PaymentResponse>> Cancel(Guid paymentUid)
    {
        var payment = await _db.Payments.FirstOrDefaultAsync(p => p.PaymentUid == paymentUid);
        if (payment is null)
        {
            return NotFound(new { message = "Payment not found" });
        }

        payment.Status = "CANCELED";
        await _db.SaveChangesAsync();

        return Ok(ToResponse(payment));
    }

    private static PaymentResponse ToResponse(Payment p) => new(p.PaymentUid, p.Status, p.Price);
}