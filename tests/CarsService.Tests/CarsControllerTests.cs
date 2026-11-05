using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CarsService.Controllers;
using CarsService.Contracts;
using CarsService.Data;
using CarsService.Models;

namespace CarsService.Tests;

public class CarsControllerTests
{
    private static readonly Guid CarUid = Guid.Parse("109b42f3-198d-4c89-9276-a7520a7120ab");

    private static CarsDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<CarsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new CarsDbContext(options);
    }

    private static void Seed(CarsDbContext db)
    {
        db.Cars.Add(new Car
        {
            CarUid = CarUid,
            Brand = "Mercedes Benz",
            Model = "GLA 250",
            RegistrationNumber = "ЛО777Х799",
            Power = 249,
            Type = "SEDAN",
            Price = 3500,
            Availability = true
        });
        db.Cars.Add(new Car
        {
            CarUid = Guid.NewGuid(),
            Brand = "BMW",
            Model = "X5",
            RegistrationNumber = "А001АА799",
            Power = 300,
            Type = "SUV",
            Price = 5000,
            Availability = false
        });
        db.SaveChanges();
    }

    [Fact]
    public async Task GetCars_DefaultFilters_ReturnsOnlyAvailable()
    {
        using var db = CreateContext();
        Seed(db);
        var controller = new CarsController(db);

        var result = await controller.GetCars(page: 0, size: 10, showAll: false);
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var page = (PaginationResponse)ok.Value!;

        Assert.Equal(1, page.TotalElements);
        Assert.Single(page.Items);
        Assert.True(page.Items[0].Available);
        Assert.Equal("Mercedes Benz", page.Items[0].Brand);
    }

    [Fact]
    public async Task GetCars_ShowAllTrue_ReturnsAllCars()
    {
        using var db = CreateContext();
        Seed(db);
        var controller = new CarsController(db);

        var result = await controller.GetCars(page: 0, size: 10, showAll: true);
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var page = (PaginationResponse)ok.Value!;

        Assert.Equal(2, page.TotalElements);
    }

    [Fact]
    public async Task GetCars_Pagination_RespectsPageAndSize()
    {
        using var db = CreateContext();
        Seed(db);
        var controller = new CarsController(db);

        var result = await controller.GetCars(page: 0, size: 1, showAll: true);
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var page = (PaginationResponse)ok.Value!;

        Assert.Equal(2, page.TotalElements);
        Assert.Single(page.Items);
        Assert.Equal(1, page.PageSize);
    }

    [Fact]
    public async Task GetCar_ExistingCar_ReturnsCar()
    {
        using var db = CreateContext();
        Seed(db);
        var controller = new CarsController(db);

        var result = await controller.GetCar(CarUid);
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var car = (CarResponse)ok.Value!;

        Assert.Equal(CarUid, car.CarUid);
        Assert.Equal(3500, car.Price);
        Assert.True(car.Available);
    }

    [Fact]
    public async Task GetCar_UnknownCar_ReturnsNotFound()
    {
        using var db = CreateContext();
        Seed(db);
        var controller = new CarsController(db);

        var result = await controller.GetCar(Guid.NewGuid());

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task Reserve_MarksCarUnavailable()
    {
        using var db = CreateContext();
        Seed(db);
        var controller = new CarsController(db);

        var result = await controller.Reserve(CarUid);
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var car = (CarResponse)ok.Value!;

        Assert.False(car.Available);
    }

    [Fact]
    public async Task Unreserve_MarksCarAvailable()
    {
        using var db = CreateContext();
        Seed(db);
        var controller = new CarsController(db);

        await controller.Reserve(CarUid);
        var result = await controller.Unreserve(CarUid);
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var car = (CarResponse)ok.Value!;

        Assert.True(car.Available);
    }

    [Fact]
    public async Task Reserve_UnknownCar_ReturnsNotFound()
    {
        using var db = CreateContext();
        Seed(db);
        var controller = new CarsController(db);

        var result = await controller.Reserve(Guid.NewGuid());

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }
}