// ====================================================================================
// 🚀 .NET 10 ASPIRE HTTP AGGREGATOR SERVICE - ГОЛОВНИЙ ДИСПЕТЧЕРСЬКИЙ ХАБ
// ------------------------------------------------------------------------------------
// Цей мікросервіс реалізує архітектурний патерн Gateway Aggregator (Scatter-Gather):
// 1. Приймає запит диспетчера на отримання зведеного звіту замовлення та водія.
// 2. Паралельно опрацьовує HTTP-запити до трьох незалежних сервісів через IHttpClientFactory:
//    - http://driversmanagement
//    - http://cargoorderingservice
//    - http://driversblogmanagement
// 3. Забезпечує Service Discovery, Polly Resilience та наскрізне трасування OpenTelemetry.
// ====================================================================================

using CargoDelivery.HttpAggregator.Domain.Clients;
using CargoDelivery.HttpAggregator.Domain.Services;

var builder = WebApplication.CreateBuilder(args);

// ====================================================================================
// 🛠️ 1. ПІДКЛЮЧЕННЯ СТАНДАРТІВ ASPIRE (SERVICE DEFAULTS)
// ------------------------------------------------------------------------------------
// Автоматично налаштовує OpenTelemetry (Traces, Metrics, Logs), Health Checks (/health, /alive)
// та глобальні конфігурації для всіх екземплярів HttpClient.
// ====================================================================================
builder.AddServiceDefaults();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// ====================================================================================
// 🌐 2. РЕЄСТРАЦІЯ ТИПІЗОВАНИХ HTTP-КЛІЄНТІВ З SERVICE DISCOVERY ТА POLLY RESILIENCE
// ------------------------------------------------------------------------------------
// Зверніть увагу: замість хардкоду IP-адрес чи портів використовуються логічні імена,
// які Aspire AppHost автоматично транслює у реальні мережеві адреси.
// ====================================================================================
builder.Services.AddHttpClient<IDriverApiClient, DriverApiClient>(client =>
{
    var baseUrl = builder.Configuration["services:driversmanagement:http:0"]
        ?? builder.Configuration["services:driversmanagement:https:0"]
        ?? "http://driversmanagement";
    client.BaseAddress = new Uri(baseUrl.EndsWith('/') ? baseUrl : $"{baseUrl}/");
}).AddStandardResilienceHandler();

builder.Services.AddHttpClient<ICargoOrderApiClient, CargoOrderApiClient>(client =>
{
    var baseUrl = builder.Configuration["services:cargoorderingservice:http:0"]
        ?? builder.Configuration["services:cargoorderingservice:https:0"]
        ?? "http://cargoorderingservice";
    client.BaseAddress = new Uri(baseUrl.EndsWith('/') ? baseUrl : $"{baseUrl}/");
}).AddStandardResilienceHandler();

builder.Services.AddHttpClient<IDriverBlogApiClient, DriverBlogApiClient>(client =>
{
    var baseUrl = builder.Configuration["services:driversblogmanagement:http:0"]
        ?? builder.Configuration["services:driversblogmanagement:https:0"]
        ?? "http://driversblogmanagement";
    client.BaseAddress = new Uri(baseUrl.EndsWith('/') ? baseUrl : $"{baseUrl}/");
}).AddStandardResilienceHandler();

// ====================================================================================
// 🏛️ 3. РЕЄСТРАЦІЯ ПРИКЛАДНОЇ БІЗНЕС-ЛОГІКИ АГРЕГАТОРА
// ====================================================================================
builder.Services.AddScoped<IShipmentAggregatorService, ShipmentAggregatorService>();

var app = builder.Build();

// Діагностичні ендпоінти Aspire (/health, /alive)
app.MapDefaultEndpoints();

app.UseRouting();
app.MapControllers();

app.Run();

// Публічний клас для підтримки WebApplicationFactory в інтеграційних тестах
public partial class Program { }
