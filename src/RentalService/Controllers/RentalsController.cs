using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentalService.Contracts;
using RentalService.Data;
using RentalService.Models;

namespace RentalService.Controllers;

[ApiController]
[Route("api/v1/rental")]
public class RentalsController : ControllerBase
{
    private const string DateFormat = "yyyy-MM-dd";

    private readonly RentalDbContext _db;

    public RentalsController(RentalDbContext db)
    {
        _db = db;
    }

    [HttpPost]
    public async Task<ActionResult<RentalResponse>> Create(CreateRentalRequest request)
    {
        // Даты приходят в формате "yyyy-MM-dd"; трактуем их как UTC, чтобы
        // offset был 0 и Npgsql записал значение в timestamp with time zone.
        var rental = new Rental
        {
            RentalUid = Guid.NewGuid(),
            Username = request.Username,
            PaymentUid = request.PaymentUid,
            CarUid = request.CarUid,
            DateFrom = ParseDate(request.DateFrom),
            DateTo = ParseDate(request.DateTo),
            Status = "IN_PROGRESS"
        };

        _db.Rentals.Add(rental);
        await _db.SaveChangesAsync();

        return Ok(ToResponse(rental));
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<RentalResponse>>> GetByUsername([FromQuery] string username)
    {
        var rentals = await _db.Rentals.AsNoTracking()
            .Where(r => r.Username == username)
            .OrderBy(r => r.Id)
            .Select(r => ToResponse(r))
            .ToListAsync();

        return Ok(rentals);
    }

    [HttpGet("{rentalUid}")]
    public async Task<ActionResult<RentalResponse>> Get(Guid rentalUid)
    {
        var rental = await _db.Rentals.AsNoTracking().FirstOrDefaultAsync(r => r.RentalUid == rentalUid);
        if (rental is null)
        {
            return NotFound(new { message = "Rental not found" });
        }

        return Ok(ToResponse(rental));
    }

    [HttpPost("{rentalUid}/finish")]
    public async Task<ActionResult<RentalResponse>> Finish(Guid rentalUid)
    {
        var rental = await _db.Rentals.FirstOrDefaultAsync(r => r.RentalUid == rentalUid);
        if (rental is null)
        {
            return NotFound(new { message = "Rental not found" });
        }

        rental.Status = "FINISHED";
        await _db.SaveChangesAsync();

        return Ok(ToResponse(rental));
    }

    [HttpPost("{rentalUid}/cancel")]
    public async Task<ActionResult<RentalResponse>> Cancel(Guid rentalUid)
    {
        var rental = await _db.Rentals.FirstOrDefaultAsync(r => r.RentalUid == rentalUid);
        if (rental is null)
        {
            return NotFound(new { message = "Rental not found" });
        }

        rental.Status = "CANCELED";
        await _db.SaveChangesAsync();

        return Ok(ToResponse(rental));
    }

    private static RentalResponse ToResponse(Rental r) => new(
        r.RentalUid,
        r.Username,
        r.PaymentUid,
        r.CarUid,
        r.DateFrom.ToString(DateFormat),
        r.DateTo.ToString(DateFormat),
        r.Status);

    private static DateTimeOffset ParseDate(string value) =>
        DateTimeOffset.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
}