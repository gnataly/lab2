using Microsoft.EntityFrameworkCore;
using Npgsql;
using CarsService.Data;
using CarsService.Models;

var builder = WebApplication.CreateBuilder(args);

// Cars Service слушает порт 8070 (задаётся через PORT в docker-compose).
var port = Environment.GetEnvironmentVariable("PORT") ?? "8070";
builder.WebHost.UseUrls($"http://+:{port}");

var connectionString = BuildConnectionString(builder.Configuration);
builder.Services.AddDbContext<CarsDbContext>(options => options.UseNpgsql(connectionString));
builder.Services.AddControllers();

var app = builder.Build();

// Создаём таблицы при старте и засеиваем тестовые данные (первый запуск).
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CarsDbContext>();
    db.Database.EnsureCreated();
    SeedTestData(db);
}

app.MapControllers();

// Endpoint доступности сервиса (используется wait-script.sh).
app.MapGet("/manage/health", () => Results.Ok());

app.Run();

static void SeedTestData(CarsDbContext db)
{
    if (db.Cars.Any())
    {
        return;
    }

    db.Cars.Add(new Car
    {
        CarUid = Guid.Parse("109b42f3-198d-4c89-9276-a7520a7120ab"),
        Brand = "Mercedes Benz",
        Model = "GLA 250",
        RegistrationNumber = "ЛО777Х799",
        Power = 249,
        Type = "SEDAN",
        Price = 3500,
        Availability = true
    });

    db.SaveChanges();
}

static string BuildConnectionString(IConfiguration configuration)
{
    var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
    if (!string.IsNullOrWhiteSpace(databaseUrl))
    {
        var uri = new Uri(databaseUrl);
        var userInfo = uri.UserInfo.Split(':', 2);
        var csb = new NpgsqlConnectionStringBuilder
        {
            Host = uri.Host,
            Port = uri.Port > 0 ? uri.Port : 5432,
            Database = uri.AbsolutePath.TrimStart('/'),
            Username = userInfo[0],
            Password = userInfo.Length > 1 ? userInfo[1] : string.Empty,
            SslMode = SslMode.Require
        };
        return csb.ConnectionString;
    }

    var configured = configuration.GetConnectionString("Default");
    if (!string.IsNullOrWhiteSpace(configured))
    {
        return configured;
    }

    return "Host=localhost;Port=5432;Database=cars;Username=program;Password=test";
}