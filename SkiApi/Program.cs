using Microsoft.EntityFrameworkCore;
using SkiApi.Data;
using SkiApi.Services;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


// CORS: разрешаем фронтенду обращаться к API
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});


// Подключение к PostgreSQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Host=localhost;Port=5432;Database=ski_db;Username=postgres;Password=357";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// Обычный HttpClient для Weather API (Open-Meteo, без прокси)
builder.Services.AddHttpClient("Weather");

// HttpClient для Telegram — ходит через немецкий прокси
builder.Services.AddHttpClient("Telegram", client =>
{
    client.Timeout = TimeSpan.FromSeconds(15);
})
.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    Proxy = new WebProxy("http://132.243.160.111:8888"),
    UseProxy = true
});

// Регистрируем сервисы
builder.Services.AddScoped<WeatherService>();
builder.Services.AddScoped<NotificationService>();

var app = builder.Build();

app.UseCors("AllowAll");
app.UseDefaultFiles();
app.UseStaticFiles();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Ski API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseAuthorization();
app.MapControllers();
// SPA fallback: любой не-API маршрут отдаёт index.html
app.MapFallbackToFile("index.html");
app.Run();

