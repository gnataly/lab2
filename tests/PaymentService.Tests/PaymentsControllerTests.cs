using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PaymentService.Controllers;
using PaymentService.Contracts;
using PaymentService.Data;

namespace PaymentService.Tests;

public class PaymentsControllerTests
{
    private static PaymentDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<PaymentDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new PaymentDbContext(options);
    }

    private static async Task<PaymentResponse> CreatePayment(PaymentsController controller, int price)
    {
        var result = await controller.Create(new CreatePaymentRequest(price));
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        return (PaymentResponse)ok.Value!;
    }

    [Fact]
    public async Task Create_ReturnsPaidPaymentWithPrice()
    {
        using var db = CreateContext();
        var controller = new PaymentsController(db);

        var payment = await CreatePayment(controller, 10500);

        Assert.NotEqual(Guid.Empty, payment.PaymentUid);
        Assert.Equal("PAID", payment.Status);
        Assert.Equal(10500, payment.Price);
    }

    [Fact]
    public async Task Get_ExistingPayment_ReturnsPayment()
    {
        using var db = CreateContext();
        var controller = new PaymentsController(db);
        var created = await CreatePayment(controller, 3500);

        var result = await controller.Get(created.PaymentUid);
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var payment = (PaymentResponse)ok.Value!;

        Assert.Equal(created.PaymentUid, payment.PaymentUid);
        Assert.Equal(3500, payment.Price);
    }

    [Fact]
    public async Task Get_UnknownPayment_ReturnsNotFound()
    {
        using var db = CreateContext();
        var controller = new PaymentsController(db);

        var result = await controller.Get(Guid.NewGuid());

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task Cancel_ChangesStatusToCanceled()
    {
        using var db = CreateContext();
        var controller = new PaymentsController(db);
        var created = await CreatePayment(controller, 10500);

        var result = await controller.Cancel(created.PaymentUid);
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var payment = (PaymentResponse)ok.Value!;

        Assert.Equal("CANCELED", payment.Status);
    }

    [Fact]
    public async Task Cancel_UnknownPayment_ReturnsNotFound()
    {
        using var db = CreateContext();
        var controller = new PaymentsController(db);

        var result = await controller.Cancel(Guid.NewGuid());

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }
}