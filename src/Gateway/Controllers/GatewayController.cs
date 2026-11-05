using Microsoft.AspNetCore.Mvc;
using Gateway.Contracts;
using Gateway.Services;

namespace Gateway.Controllers;

[ApiController]
[Route("api/v1")]
public class GatewayController : ControllerBase
{
    private readonly GatewayOrchestrator _orchestrator;

    public GatewayController(GatewayOrchestrator orchestrator)
    {
        _orchestrator = orchestrator;
    }

    [HttpGet("cars")]
    public async Task<ActionResult<PaginationResponse>> GetCars(
        [FromQuery] int page = 0,
        [FromQuery] int size = 10,
        [FromQuery] bool showAll = false)
    {
        return Ok(await _orchestrator.GetCarsAsync(page, size, showAll));
    }

    [HttpGet("rental")]
    public async Task<ActionResult<IReadOnlyList<RentalResponse>>> GetRentals([FromHeader(Name = "X-User-Name")] string username)
    {
        return Ok(await _orchestrator.GetRentalsAsync(username));
    }

    [HttpGet("rental/{rentalUid}")]
    public async Task<ActionResult<RentalResponse>> GetRental(
        [FromHeader(Name = "X-User-Name")] string username,
        Guid rentalUid)
    {
        try
        {
            return Ok(await _orchestrator.GetRentalAsync(username, rentalUid));
        }
        catch (RentalNotFoundException)
        {
            return NotFound(new { message = "Rental not found" });
        }
    }

    [HttpPost("rental")]
    public async Task<ActionResult<CreateRentalResponse>> CreateRental(
        [FromHeader(Name = "X-User-Name")] string username,
        CreateRentalRequest request)
    {
        return Ok(await _orchestrator.CreateRentalAsync(username, request));
    }

    [HttpPost("rental/{rentalUid}/finish")]
    public async Task<IActionResult> FinishRental(
        [FromHeader(Name = "X-User-Name")] string username,
        Guid rentalUid)
    {
        try
        {
            await _orchestrator.FinishRentalAsync(username, rentalUid);
            return NoContent();
        }
        catch (RentalNotFoundException)
        {
            return NotFound(new { message = "Rental not found" });
        }
    }

    [HttpDelete("rental/{rentalUid}")]
    public async Task<IActionResult> CancelRental(
        [FromHeader(Name = "X-User-Name")] string username,
        Guid rentalUid)
    {
        try
        {
            await _orchestrator.CancelRentalAsync(username, rentalUid);
            return NoContent();
        }
        catch (RentalNotFoundException)
        {
            return NotFound(new { message = "Rental not found" });
        }
    }
}