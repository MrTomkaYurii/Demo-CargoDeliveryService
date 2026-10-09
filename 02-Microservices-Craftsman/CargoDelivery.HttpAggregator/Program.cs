// ====================================================================================
// 🚀 .NET 10 ASPIRE HTTP AGGREGATOR SERVICE - ГОЛОВНИЙ ДИСПЕТЧЕРСЬКИЙ ХАБ
// ------------------------------------------------------------------------------------
// Цей мікросервіс реалізує архітектурний патерн Gateway Aggregator (Scatter-Gather):
// 1. Приймає запит диспетчера на отримання зведеного звіту замовлення та водія.
// 2. Паралельно опрацьовує HTTP-запити до трьох незалежних сервісів через IHttpClientFactory:
//    - https+http://driversmanagement
//    - https+http://cargoorderingservice
//    - https+http://driversblogmanagement
// 3. Забезпечує Service Discovery, Polly Resilience та наскрізне трасування OpenTelemetry.
// ====================================================================================

using CargoDelivery.HttpAggregator.Domain.Clients;
using CargoDelivery.HttpAggregator.Domain.Services;

var builder = WebApplication.CreateBuilder(args);

// ====================================================================================
// 🛠️ 1. ПІДКЛЮЧЕННЯ СТАНДАРТІВ ASPIRE (SERVICE DEFAULTS)
// ------------------------------------------------------------------------------------
// Автоматично налаштовує OpenTelemetry (Traces, Metrics, Logs), Health Checks (/health, /alive)
// та ГЛОБАЛЬНІ налаштування для всіх HttpClient через ConfigureHttpClientDefaults:
//   - AddServiceDiscovery()        → резолв логічних імен на кшталт "driversmanagement";
//   - AddStandardResilienceHandler() → 5-рівневий конвеєр Polly (Rate limiter, Total timeout,
//                                      Retry, Circuit breaker, Attempt timeout).
// Тому нижче для бізнес-клієнтів НЕ потрібно (і не можна!) викликати
// .AddStandardResilienceHandler() вдруге: два стандартні конвеєри вкладаються один в
// один, і кількість повторів множиться (4 × 4 = до 16 реальних запитів на один виклик).
// ====================================================================================
builder.AddServiceDefaults();

builder.Services.AddControllers();

// ====================================================================================
// 📘 SWAGGER / OpenAPI (Swashbuckle) — інтерактивна документація та "пісочниця" API
// ------------------------------------------------------------------------------------
// AddEndpointsApiExplorer збирає метадані про маршрути контролерів, а AddSwaggerGen
// будує з них OpenAPI-документ (/swagger/v1/swagger.json). XML-коментарі (///) з
// контролерів підтягуються як описи ендпоінтів. UI відкривається на /swagger —
// саме туди веде посилання ресурсу httpaggregator в Aspire Dashboard (launchUrl).
// ====================================================================================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "CargoDelivery.HttpAggregator",
        Version = "v1",
        Description = "Gateway Aggregator (Scatter-Gather) + лабораторія стійкості Polly. " +
                      "Водіїв для запитів беріть із DriversManagement: GET https://localhost:5001/api/drivers"
    });

    var xmlFile = Path.Combine(AppContext.BaseDirectory, $"{typeof(Program).Assembly.GetName().Name}.xml");
    if (File.Exists(xmlFile))
        options.IncludeXmlComments(xmlFile);
});

// ====================================================================================
// 🌐 2. РЕЄСТРАЦІЯ ТИПІЗОВАНИХ HTTP-КЛІЄНТІВ (TYPED CLIENTS) ЗІ SERVICE DISCOVERY
// ------------------------------------------------------------------------------------
// Замість хардкоду IP-адрес чи портів використовуються ЛОГІЧНІ імена ресурсів з AppHost.
// Схема "https+http://" — спеціальний синтаксис Aspire Service Discovery:
// "спробуй HTTPS-ендпоінт, а якщо його немає — візьми HTTP".
// Реальну адресу (наприклад, https://localhost:5001) резолвер бере зі змінної
// середовища services__driversmanagement__https__0, яку AppHost інжектує через
// .WithReference(driversApi).
// ====================================================================================
builder.Services.AddHttpClient<IDriverApiClient, DriverApiClient>(client =>
    client.BaseAddress = new Uri("https+http://driversmanagement"));

builder.Services.AddHttpClient<ICargoOrderApiClient, CargoOrderApiClient>(client =>
    client.BaseAddress = new Uri("https+http://cargoorderingservice"));

// Блог — НЕКРИТИЧНА залежність (Graceful Degradation). Стандартний бюджет Polly (30 с,
// спроба по 10 с) змусив би диспетчера чекати пів хвилини, якщо блог "завис". Тому для
// цього клієнта — короткий бюджет: спроба 1 с, один повтор, загалом не більше 3 с.
// Після цього SafeFetchRoadNotesAsync повертає порожній список і IsDegraded = true.
builder.Services.AddHttpClient<IDriverBlogApiClient, DriverBlogApiClient>(client =>
        client.BaseAddress = new Uri("https+http://driversblogmanagement"))
    .RemoveAllResilienceHandlers()
    .AddStandardResilienceHandler(options =>
    {
        options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(1);
        options.Retry.MaxRetryAttempts = 1;
        options.Retry.Delay = TimeSpan.FromMilliseconds(200);
        options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(3);
    });

// ====================================================================================
// 🧪 3. ОКРЕМІ КЛІЄНТИ ДЛЯ ЛАБОРАТОРІЇ СТІЙКОСТІ (ResilienceDemoController)
// ------------------------------------------------------------------------------------
// Демо-клієнти звертаються до ВЛАСНОГО SimulationController агрегатора (адреса
// підставляється під час запиту — див. SimulationApiClient).
// Стандартні параметри Polly розраховані на продакшн (затримка повтору 2 с, таймаут
// спроби 10 с, Circuit Breaker після 100 запитів) — для живої демонстрації вони
// занадто «повільні». Тому для кожного сценарію:
//   1. RemoveAllResilienceHandlers() прибирає глобальний конвеєр із ServiceDefaults;
//   2. AddStandardResilienceHandler(...) додає ТОЙ САМИЙ стандартний конвеєр, але з
//      «навчальними» параметрами, щоб ефект було видно за секунди.
// Кожен сценарій — окремий ІМЕНОВАНИЙ клієнт, отже окремий стан Circuit Breaker:
// експерименти не впливають один на одного.
// ====================================================================================
builder.Services.AddTransient<ISimulationApiClient, SimulationApiClient>();

// Сценарій /retry: швидкі повтори 200 → 400 → 800 мс (+ jitter), решта — стандартні значення
AddSimulationClient(SimulationClients.Retry, options =>
{
    options.Retry.MaxRetryAttempts = 3;
    options.Retry.Delay = TimeSpan.FromMilliseconds(200);
});

// Сценарій /timeout: кожна спроба обривається через 2 с (а /slow відповідає за 5 с),
// повтори відбуваються, але загальний бюджет 8 с завершує весь виклик.
AddSimulationClient(SimulationClients.Timeout, options =>
{
    options.Retry.MaxRetryAttempts = 3;
    options.Retry.Delay = TimeSpan.FromMilliseconds(200);
    options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(2);
    options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(8);
});

// Сценарій /circuit-breaker: чутливий розмикач — Open, якщо за 10 с було ≥ 5 спроб
// і ≥ 50% з них завершилися збоєм. Ланцюг лишається розімкненим 15 с.
// Обмеження валідації Polly: SamplingDuration має бути ≥ 2 × AttemptTimeout.
AddSimulationClient(SimulationClients.CircuitBreaker, options =>
{
    options.Retry.MaxRetryAttempts = 2;
    options.Retry.Delay = TimeSpan.FromMilliseconds(100);
    options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(2);
    options.CircuitBreaker.MinimumThroughput = 5;
    options.CircuitBreaker.FailureRatio = 0.5;
    options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(10);
    options.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(15);
});

void AddSimulationClient(string name, Action<Microsoft.Extensions.Http.Resilience.HttpStandardResilienceOptions> configure)
{
    // RemoveAllResilienceHandlers() — експериментальне API (EXTEXP0001), дозволене в .csproj
    builder.Services.AddHttpClient(name)
        .RemoveAllResilienceHandlers()
        .AddStandardResilienceHandler(configure);
}

// ====================================================================================
// 🏛️ 4. РЕЄСТРАЦІЯ ПРИКЛАДНОЇ БІЗНЕС-ЛОГІКИ АГРЕГАТОРА
// ====================================================================================
builder.Services.AddScoped<IShipmentAggregatorService, ShipmentAggregatorService>();

var app = builder.Build();

// Діагностичні ендпоінти Aspire (/health, /alive)
app.MapDefaultEndpoints();

// Swagger UI лише в розробці: у продакшні опис внутрішнього API не варто виставляти назовні
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options => options.DocumentTitle = "CargoDelivery.HttpAggregator — Swagger");
}

app.UseRouting();
app.MapControllers();

app.Run();

// Публічний клас для підтримки WebApplicationFactory в інтеграційних тестах
public partial class Program { }
