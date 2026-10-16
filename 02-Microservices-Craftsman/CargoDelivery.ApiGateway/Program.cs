// ====================================================================================
// 🚪 .NET 10 OCELOT API GATEWAY — ЄДИНА ВХІДНА ТОЧКА (EDGE GATEWAY & AGGREGATOR)
// ====================================================================================
// АРХІТЕКТУРНА РОЛЬ ПРОЄКТУ CargoDelivery.ApiGateway (Порт :5000):
//
// У мікросервісній системі CargoDeliveryService клієнтський застосунок (Web / Mobile / WPF)
// НЕ повинен знати фізичні адреси та порти внутрішніх мікросервісів:
//   • DriversManagement       (:5001)
//   • DriversBlogManagement   (:5002)
//   • CargoOrderingService    (:5003)
//   • CargoDelivery.HttpAggregator (:5005)
//
// Замість цього весь зовнішній трафік проходить через Ocelot API Gateway (https://localhost:5000),
// який виконує три ключові функції:
//   1. Reverse Proxy (Маршрутизація): переписує публічні URL /gateway/* у внутрішні /api/*.
//   2. Cross-Cutting Concerns: Rate Limiting (квоти запитів), мутація заголовків (X-Forwarded-By,
//      X-Correlation-Id) та балансування навантаження (RoundRobin).
//   3. Request Aggregation (Агрегація запитів на рівні шлюзу):
//      - Декларативна JSON-агрегація через секцію "Aggregates" у файлі ocelot.json
//        (GET /gateway/ocelot-aggregate/{driverId});
//      - Програмна C#-агрегація через інтерфейс IDefinedAggregator
//        (GET /gateway/custom-aggregate/{driverId} -> DriverSummaryDefinedAggregator).
// ====================================================================================

using CargoDelivery.ApiGateway.Aggregators;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

// ------------------------------------------------------------------------------------
// КРОК 1: ПІДКЛЮЧЕННЯ ФАЙЛУ КОНФІГУРАЦІЇ OCELOT (ocelot.json)
// ------------------------------------------------------------------------------------
// Параметр reloadOnChange: true дозволяє змінювати маршрути, ліміти або правила агрегації
// прямо під час роботи програми (Hot Reload) без перезапуску контейнера шлюзу!
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

// ------------------------------------------------------------------------------------
// КРОК 2: ІНТЕГРАЦІЯ З .NET 10 ASPIRE (ServiceDefaults)
// ------------------------------------------------------------------------------------
// Автоматично підключає:
// - OpenTelemetry (розподілене трасування W3C traceparent, метрики HTTP та логування);
// - Health Checks (/health для Readiness та /alive для Liveness у дашборді Aspire);
// - Service Discovery.
builder.AddServiceDefaults();

// ------------------------------------------------------------------------------------
// КРОК 3: РЕЄСТРАЦІЯ ВЛАСНИХ КОНТРОЛЕРІВ ТА SWAGGER-ДОКУМЕНТАЦІЇ ШЛЮЗУ
// ------------------------------------------------------------------------------------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "CargoDelivery.ApiGateway (Ocelot)",
        Version = "v1",
        Description = "Єдиний вхідний API-шлюз на базі Ocelot: Reverse Proxy маршрутизація (/gateway/*) " +
                      "та вбудована агрегація запитів (/gateway/ocelot-aggregate/{driverId} і /gateway/custom-aggregate/{driverId})."
    });

    var xmlFile = Path.Combine(AppContext.BaseDirectory, $"{typeof(Program).Assembly.GetName().Name}.xml");
    if (File.Exists(xmlFile))
        options.IncludeXmlComments(xmlFile);
});

// ------------------------------------------------------------------------------------
// КРОК 4: РЕЄСТРАЦІЯ OCELOT ТА КАСТОМНОГО АГРЕГАТОРА В DI-КОНТЕЙНЕРІ
// ------------------------------------------------------------------------------------
// • AddOcelot(builder.Configuration) — реєструє всі внутрішні сервіси Ocelot (маршрутизатор,
//   Rate Limiter, балансувальник навантаження, мультиплексор агрегації).
// • AddSingletonDefinedAggregator<DriverSummaryDefinedAggregator>() — реєструє наш власний
//   C#-клас агрегації, на який посилається поле "Aggregator": "DriverSummaryDefinedAggregator"
//   у файлі ocelot.json.
builder.Services
    .AddOcelot(builder.Configuration)
    .AddSingletonDefinedAggregator<DriverSummaryDefinedAggregator>();

var app = builder.Build();

// Публікуємо стандартні ендпоінти здоров'я .NET Aspire (/health та /alive)
app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options => options.DocumentTitle = "CargoDelivery.ApiGateway (Ocelot) — Swagger");
}

app.UseRouting();

// ------------------------------------------------------------------------------------
// КРОК 5: РОЗГАЛУЖЕННЯ КОНВЕЄРА ASP.NET CORE ТА ТЕРМІНАЛЬНОГО КОНВЕЄРА OCELOT
// ------------------------------------------------------------------------------------
// ВАЖЛИВИЙ АРХІТЕКТУРНИЙ НЮАНС:
// Ocelot Middleware (UseOcelot) є ТЕРМІНАЛЬНИМ — він перехоплює всі запити і не передає
// керування наступним Middleware, якщо викликати його напряму в корені.
//
// Тому ми використовуємо розгалуження app.MapWhen(...):
// 1. Системні маршрути (/swagger, /health, /alive, /api/gateway/routes) обробляються
//    стандартним конвеєром ASP.NET Core та нашими власними контролерами (app.MapControllers()).
// 2. Усі клієнтські запити, що починаються з префікса "/gateway/...", направляються
//    у конвеєр Ocelot (ocelotApp.UseOcelot().Wait()), який проксіює або агрегує їх згідно з ocelot.json.
app.MapControllers();

app.MapWhen(
    ctx => ctx.Request.Path.StartsWithSegments("/gateway"),
    ocelotApp =>
    {
        ocelotApp.UseOcelot().Wait();
    });

app.Run();

public partial class Program { }
