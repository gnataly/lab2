using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CarsService.Contracts;
using CarsService.Data;
using CarsService.Models;

namespace CarsService.Controllers;

[ApiController]
[Route("api/v1/cars")]
public class CarsController : ControllerBase
{
    private readonly CarsDbContext _db;

    public CarsController(CarsDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<PaginationResponse>> GetCars(
        [FromQuery] int page = 0,
        [FromQuery] int size = 10,
        [FromQuery] bool showAll = false)
    {
        var query = _db.Cars.AsNoTracking();

        // showAll=false → показываем только доступные (availability = true).
        if (!showAll)
        {
            query = query.Where(c => c.Availability);
        }

        var total = await query.LongCountAsync();

        // Страницы нумеруются с 1 (Postman: page=1 → первая страница).
        var offset = Math.Max(0, (page - 1) * size);

        var cars = await query
            .OrderBy(c => c.Id)
            .Skip(offset)
            .Take(size)
            .Select(c => ToResponse(c))
            .ToListAsync();

        return Ok(new PaginationResponse(page, size, total, cars));
    }

    [HttpGet("{carUid}")]
    public async Task<ActionResult<CarResponse>> GetCar(Guid carUid)
    {
        var car = await _db.Cars.AsNoTracking().FirstOrDefaultAsync(c => c.CarUid == carUid);
        if (car is null)
        {
            return NotFound(new { message = "Car not found" });
        }

        return Ok(ToResponse(car));
    }

    [HttpPost("{carUid}/reserve")]
    public async Task<ActionResult<CarResponse>> Reserve(Guid carUid)
    {
        var car = await _db.Cars.FirstOrDefaultAsync(c => c.CarUid == carUid);
        if (car is null)
        {
            return NotFound(new { message = "Car not found" });
        }

        car.Availability = false;
        await _db.SaveChangesAsync();

        return Ok(ToResponse(car));
    }

    [HttpPost("{carUid}/unreserve")]
    public async Task<ActionResult<CarResponse>> Unreserve(Guid carUid)
    {
        var car = await _db.Cars.FirstOrDefaultAsync(c => c.CarUid == carUid);
        if (car is null)
        {
            return NotFound(new { message = "Car not found" });
        }

        car.Availability = true;
        await _db.SaveChangesAsync();

        return Ok(ToResponse(car));
    }

    private static CarResponse ToResponse(Car c) => new(
        c.CarUid,
        c.Brand,
        c.Model,
        c.RegistrationNumber,
        c.Power,
        c.Type,
        c.Price,
        c.Availability);
}