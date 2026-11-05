using System.Net.Http.Json;
using System.Text.Json;
using Gateway.Contracts;

namespace Gateway.Services;

public class GatewayOrchestrator
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly HttpClient _carsClient;
    private readonly HttpClient _rentalClient;
    private readonly HttpClient _paymentClient;

    public GatewayOrchestrator(HttpClient carsClient, HttpClient rentalClient, HttpClient paymentClient)
    {
        _carsClient = carsClient;
        _rentalClient = rentalClient;
        _paymentClient = paymentClient;
    }

    public async Task<PaginationResponse> GetCarsAsync(int page, int size, bool showAll)
    {
        var response = await _carsClient.GetAsync($"/api/v1/cars?page={page}&size={size}&showAll={showAll}");
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<PaginationResponse>(JsonOptions))!;
    }

    public async Task<CreateRentalResponse> CreateRentalAsync(string username, CreateRentalRequest request)
    {
        // 1. Проверяем, что автомобиль существует, и получаем его цену.
        var car = await GetCarAsync(request.CarUid);

        // 2. Резервируем автомобиль (availability = false).
        var reserveResponse = await _carsClient.PostAsync($"/api/v1/cars/{request.CarUid}/reserve", null);
        reserveResponse.EnsureSuccessStatusCode();

        // 3. Считаем стоимость аренды и создаём оплату.
        var days = CalculateDays(request.DateFrom, request.DateTo);
        var totalPrice = days * car.Price;

        var paymentResponse = await _paymentClient.PostAsJsonAsync("/api/v1/payments", new { price = totalPrice }, JsonOptions);
        paymentResponse.EnsureSuccessStatusCode();
        var payment = (await paymentResponse.Content.ReadFromJsonAsync<PaymentInfo>(JsonOptions))!;

        // 4. Создаём аренду.
        var rentalResponse = await _rentalClient.PostAsJsonAsync(
            "/api/v1/rental",
            new { username, paymentUid = payment.PaymentUid, carUid = request.CarUid, dateFrom = request.DateFrom, dateTo = request.DateTo },
            JsonOptions);
        rentalResponse.EnsureSuccessStatusCode();
        var rental = (await rentalResponse.Content.ReadFromJsonAsync<RentalInternal>(JsonOptions))!;

        return new CreateRentalResponse(
            rental.RentalUid,
            rental.Status,
            rental.CarUid,
            rental.DateFrom,
            rental.DateTo,
            payment);
    }

    public async Task<RentalResponse> GetRentalAsync(string username, Guid rentalUid)
    {
        var rental = await GetRentalInternalAsync(rentalUid);
        EnsureOwner(rental, username);
        return await BuildRentalResponseAsync(rental);
    }

    public async Task<IReadOnlyList<RentalResponse>> GetRentalsAsync(string username)
    {
        var response = await _rentalClient.GetAsync($"/api/v1/rental?username={Uri.EscapeDataString(username)}");
        response.EnsureSuccessStatusCode();
        var rentals = (await response.Content.ReadFromJsonAsync<List<RentalInternal>>(JsonOptions)) ?? new List<RentalInternal>();

        var result = new List<RentalResponse>();
        foreach (var rental in rentals)
        {
            result.Add(await BuildRentalResponseAsync(rental));
        }

        return result;
    }

    public async Task FinishRentalAsync(string username, Guid rentalUid)
    {
        var rental = await GetRentalInternalAsync(rentalUid);
        EnsureOwner(rental, username);

        // Снимаем резерв с автомобиля и завершаем аренду.
        var unreserveResponse = await _carsClient.PostAsync($"/api/v1/cars/{rental.CarUid}/unreserve", null);
        unreserveResponse.EnsureSuccessStatusCode();

        var finishResponse = await _rentalClient.PostAsync($"/api/v1/rental/{rentalUid}/finish", null);
        finishResponse.EnsureSuccessStatusCode();
    }

    public async Task CancelRentalAsync(string username, Guid rentalUid)
    {
        var rental = await GetRentalInternalAsync(rentalUid);
        EnsureOwner(rental, username);

        // Снимаем резерв с автомобиля, отменяем аренду и оплату.
        var unreserveResponse = await _carsClient.PostAsync($"/api/v1/cars/{rental.CarUid}/unreserve", null);
        unreserveResponse.EnsureSuccessStatusCode();

        var cancelRentalResponse = await _rentalClient.PostAsync($"/api/v1/rental/{rentalUid}/cancel", null);
        cancelRentalResponse.EnsureSuccessStatusCode();

        var cancelPaymentResponse = await _paymentClient.PostAsync($"/api/v1/payments/{rental.PaymentUid}/cancel", null);
        cancelPaymentResponse.EnsureSuccessStatusCode();
    }

    private async Task<CarResponse> GetCarAsync(Guid carUid)
    {
        var response = await _carsClient.GetAsync($"/api/v1/cars/{carUid}");
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<CarResponse>(JsonOptions))!;
    }

    private async Task<RentalInternal> GetRentalInternalAsync(Guid rentalUid)
    {
        var response = await _rentalClient.GetAsync($"/api/v1/rental/{rentalUid}");
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<RentalInternal>(JsonOptions))!;
    }

    private async Task<RentalResponse> BuildRentalResponseAsync(RentalInternal rental)
    {
        var car = await GetCarAsync(rental.CarUid);

        var paymentResponse = await _paymentClient.GetAsync($"/api/v1/payments/{rental.PaymentUid}");
        paymentResponse.EnsureSuccessStatusCode();
        var payment = (await paymentResponse.Content.ReadFromJsonAsync<PaymentInfo>(JsonOptions))!;

        return new RentalResponse(
            rental.RentalUid,
            rental.Status,
            rental.DateFrom,
            rental.DateTo,
            new CarInfo(car.CarUid, car.Brand, car.Model, car.RegistrationNumber),
            payment);
    }

    private static void EnsureOwner(RentalInternal rental, string username)
    {
        if (!string.Equals(rental.Username, username, StringComparison.Ordinal))
        {
            throw new RentalNotFoundException();
        }
    }

    public static int CalculateDays(string dateFrom, string dateTo)
    {
        var from = DateOnly.Parse(dateFrom);
        var to = DateOnly.Parse(dateTo);
        return Math.Abs(to.DayNumber - from.DayNumber);
    }
}

public sealed class RentalNotFoundException : Exception;