using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentalService.Controllers;
using RentalService.Contracts;
using RentalService.Data;

namespace RentalService.Tests;

public class RentalsControllerTests
{
    private static readonly Guid PaymentUid = Guid.NewGuid();
    private static readonly Guid CarUid = Guid.Parse("109b42f3-198d-4c89-9276-a7520a7120ab");

    private static RentalDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<RentalDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new RentalDbContext(options);
    }

    private static CreateRentalRequest NewRequest() => new("Test Max", PaymentUid, CarUid, "2021-10-08", "2021-10-11");

    private static async Task<RentalResponse> CreateRental(RentalsController controller)
    {
        var result = await controller.Create(NewRequest());
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        return (RentalResponse)ok.Value!;
    }

    [Fact]
    public async Task Create_SetsInProgressAndFormatsDates()
    {
        using var db = CreateContext();
        var controller = new RentalsController(db);

        var rental = await CreateRental(controller);

        Assert.NotEqual(Guid.Empty, rental.RentalUid);
        Assert.Equal("Test Max", rental.Username);
        Assert.Equal(PaymentUid, rental.PaymentUid);
        Assert.Equal(CarUid, rental.CarUid);
        Assert.Equal("IN_PROGRESS", rental.Status);
        Assert.Equal("2021-10-08", rental.DateFrom);
        Assert.Equal("2021-10-11", rental.DateTo);
    }

    [Fact]
    public async Task GetByUsername_FiltersRentalsByUser()
    {
        using var db = CreateContext();
        var controller = new RentalsController(db);
        await CreateRental(controller);

        var result = await controller.GetByUsername("Test Max");
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var rentals = (IReadOnlyList<RentalResponse>)ok.Value!;

        Assert.Single(rentals);
        Assert.Equal("Test Max", rentals[0].Username);
    }

    [Fact]
    public async Task GetByUsername_ReturnsEmptyForOtherUser()
    {
        using var db = CreateContext();
        var controller = new RentalsController(db);
        await CreateRental(controller);

        var result = await controller.GetByUsername("Someone Else");
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var rentals = (IReadOnlyList<RentalResponse>)ok.Value!;

        Assert.Empty(rentals);
    }

    [Fact]
    public async Task Get_ExistingRental_ReturnsRental()
    {
        using var db = CreateContext();
        var controller = new RentalsController(db);
        var created = await CreateRental(controller);

        var result = await controller.Get(created.RentalUid);
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var rental = (RentalResponse)ok.Value!;

        Assert.Equal(created.RentalUid, rental.RentalUid);
    }

    [Fact]
    public async Task Get_UnknownRental_ReturnsNotFound()
    {
        using var db = CreateContext();
        var controller = new RentalsController(db);

        var result = await controller.Get(Guid.NewGuid());

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task Finish_ChangesStatusToFinished()
    {
        using var db = CreateContext();
        var controller = new RentalsController(db);
        var created = await CreateRental(controller);

        var result = await controller.Finish(created.RentalUid);
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var rental = (RentalResponse)ok.Value!;

        Assert.Equal("FINISHED", rental.Status);
    }

    [Fact]
    public async Task Cancel_ChangesStatusToCanceled()
    {
        using var db = CreateContext();
        var controller = new RentalsController(db);
        var created = await CreateRental(controller);

        var result = await controller.Cancel(created.RentalUid);
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var rental = (RentalResponse)ok.Value!;

        Assert.Equal("CANCELED", rental.Status);
    }

    [Fact]
    public async Task Finish_UnknownRental_ReturnsNotFound()
    {
        using var db = CreateContext();
        var controller = new RentalsController(db);

        var result = await controller.Finish(Guid.NewGuid());

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }
}