using Gateway.Services;

var builder = WebApplication.CreateBuilder(args);

// Gateway Service слушает порт 8080 (задаётся через PORT в docker-compose).
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://+:{port}");

// Адреса внутренних сервисов. В docker-compose — hostname сервисов, локально — localhost.
var carsUrl = Environment.GetEnvironmentVariable("CARS_SERVICE_URL") ?? "http://localhost:8070";
var rentalUrl = Environment.GetEnvironmentVariable("RENTAL_SERVICE_URL") ?? "http://localhost:8060";
var paymentUrl = Environment.GetEnvironmentVariable("PAYMENT_SERVICE_URL") ?? "http://localhost:8050";

builder.Services.AddHttpClient("cars", c => c.BaseAddress = new Uri(carsUrl));
builder.Services.AddHttpClient("rental", c => c.BaseAddress = new Uri(rentalUrl));
builder.Services.AddHttpClient("payment", c => c.BaseAddress = new Uri(paymentUrl));

builder.Services.AddScoped(sp =>
{
    var factory = sp.GetRequiredService<IHttpClientFactory>();
    return new GatewayOrchestrator(
        factory.CreateClient("cars"),
        factory.CreateClient("rental"),
        factory.CreateClient("payment"));
});

builder.Services.AddControllers();

var app = builder.Build();

app.MapControllers();

// Endpoint доступности сервиса (используется wait-script.sh).
app.MapGet("/manage/health", () => Results.Ok());

app.Run();