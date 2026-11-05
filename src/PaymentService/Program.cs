using Microsoft.EntityFrameworkCore;
using Npgsql;
using PaymentService.Data;

var builder = WebApplication.CreateBuilder(args);

// Payment Service слушает порт 8050 (задаётся через PORT в docker-compose).
var port = Environment.GetEnvironmentVariable("PORT") ?? "8050";
builder.WebHost.UseUrls($"http://+:{port}");

var connectionString = BuildConnectionString(builder.Configuration);
builder.Services.AddDbContext<PaymentDbContext>(options => options.UseNpgsql(connectionString));
builder.Services.AddControllers();

var app = builder.Build();

// Создаём таблицы при старте.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();
    db.Database.EnsureCreated();
}

app.MapControllers();

// Endpoint доступности сервиса (используется wait-script.sh).
app.MapGet("/manage/health", () => Results.Ok());

app.Run();

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

    return "Host=localhost;Port=5432;Database=payments;Username=program;Password=test";
}