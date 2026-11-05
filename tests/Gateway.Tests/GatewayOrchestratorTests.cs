using System.Net;
using System.Text;
using System.Text.Json;
using Gateway.Contracts;
using Gateway.Services;

namespace Gateway.Tests;

public class GatewayOrchestratorTests
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private static readonly Guid CarUid = Guid.Parse("109b42f3-198d-4c89-9276-a7520a7120ab");
    private static readonly Guid PaymentUid = Guid.Parse("238c733c-fb1e-40a9-aadb-73cb8f90675d");

    [Theory]
    [InlineData("2021-10-08", "2021-10-11", 3)]
    [InlineData("2021-10-11", "2021-10-08", 3)]
    [InlineData("2021-10-08", "2021-10-08", 0)]
    public void CalculateDays_ComputesAbsoluteDayDifference(string from, string to, int expected)
    {
        Assert.Equal(expected, GatewayOrchestrator.CalculateDays(from, to));
    }

    [Fact]
    public async Task CreateRental_OrchestratesServicesAndComputesPrice()
    {
        var handler = new FakeHttpMessageHandler(request =>
        {
            var path = request.RequestUri!.AbsolutePath;
            if (request.Method == HttpMethod.Get && path == $"/api/v1/cars/{CarUid}")
            {
                return JsonResponse(Json(new CarResponse(CarUid, "Mercedes Benz", "GLA 250", "ЛО777Х799", 249, "SEDAN", 3500, true)));
            }
            if (request.Method == HttpMethod.Post && path == $"/api/v1/cars/{CarUid}/reserve")
            {
                return JsonResponse(Json(new CarResponse(CarUid, "Mercedes Benz", "GLA 250", "ЛО777Х799", 249, "SEDAN", 3500, false)));
            }
            if (request.Method == HttpMethod.Post && path == "/api/v1/payments")
            {
                return JsonResponse(Json(new PaymentInfo(PaymentUid, "PAID", 10500)));
            }
            if (request.Method == HttpMethod.Post && path == "/api/v1/rental")
            {
                return JsonResponse(Json(new RentalInternal(Guid.NewGuid(), "Test Max", PaymentUid, CarUid, "2021-10-08", "2021-10-11", "IN_PROGRESS")));
            }
            return new HttpResponseMessage(HttpStatusCode.NotFound);
        });

        var orchestrator = new GatewayOrchestrator(
            Client(handler),
            Client(handler),
            Client(handler));

        var result = await orchestrator.CreateRentalAsync("Test Max", new CreateRentalRequest(CarUid, "2021-10-08", "2021-10-11"));

        Assert.NotEqual(Guid.Empty, result.RentalUid);
        Assert.Equal(CarUid, result.CarUid);
        Assert.Equal("IN_PROGRESS", result.Status);
        Assert.Equal(PaymentUid, result.Payment.PaymentUid);
        Assert.Equal("PAID", result.Payment.Status);
        Assert.Equal(10500, result.Payment.Price);
    }

    [Fact]
    public async Task GetCars_ForwardsPaginationQuery()
    {
        var handler = new FakeHttpMessageHandler(request =>
        {
            if (request.Method == HttpMethod.Get && request.RequestUri!.AbsolutePath == "/api/v1/cars")
            {
                var page = new PaginationResponse(1, 10, 1, new List<CarResponse>
                {
                    new(CarUid, "Mercedes Benz", "GLA 250", "ЛО777Х799", 249, "SEDAN", 3500, true)
                });
                return JsonResponse(Json(page));
            }
            return new HttpResponseMessage(HttpStatusCode.NotFound);
        });

        var orchestrator = new GatewayOrchestrator(
            Client(handler),
            Client(handler),
            Client(handler));

        var result = await orchestrator.GetCarsAsync(1, 10, false);

        Assert.Equal(1, result.TotalElements);
        Assert.Equal(10, result.PageSize);
        Assert.Equal(CarUid, result.Items[0].CarUid);
    }

    private static HttpClient Client(FakeHttpMessageHandler handler)
    {
        var client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };
        return client;
    }

    private static string Json(object value) => JsonSerializer.Serialize(value, SerializerOptions);

    private static HttpResponseMessage JsonResponse(string json) =>
        new(HttpStatusCode.OK) { Content = new StringContent(json, Encoding.UTF8, "application/json") };

    private sealed class FakeHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _responder;

        public FakeHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responder)
        {
            _responder = responder;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(_responder(request));
    }
}