// ====================================================================================
// 🛠️ .NET 10 ASPIRE SERVICE DEFAULTS - АРХІТЕКТУРНИЙ ЕТАЛОН НАЛАШТУВАНЬ МІКРОСЕРВІСУ
// ------------------------------------------------------------------------------------
// Цей проєкт містить уніфікований набір розширень IHostApplicationBuilder, що
// забезпечує дотримання золотого стандарту розподілених хмарних систем:
// 1. OpenTelemetry: наскрізне трасування (Tracing), системні метрики (Metrics) та OTLP-логи.
// 2. Health Checks: стандартизовані зонди готовності (/health) та життєздатності (/alive).
// 3. Service Discovery: динамічна резолюція логічних імен (наприклад, https+http://cargoorderingservice).
// 4. Polly Resilience: стандартний конвеєр із 5 стратегій захисту мережевих викликів HttpClient.
//
// Кожен мікросервіс підключає все це ОДНИМ рядком: builder.AddServiceDefaults();
// Властивість <IsAspireSharedProject>true</IsAspireSharedProject> у .csproj позначає
// проєкт як спільну бібліотеку Aspire (а не як окремий запускуваний ресурс).
// ====================================================================================

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ServiceDiscovery;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

// Простір імен Microsoft.Extensions.Hosting обрано свідомо: методи розширення стають
// доступними в Program.cs будь-якого сервісу без додаткового using.
namespace Microsoft.Extensions.Hosting;

public static class Extensions
{
    private const string HealthEndpointPath = "/health";
    private const string AlivenessEndpointPath = "/alive";

    /// <summary>
    /// ====================================================================================
    /// 📌 1. ГОЛОВНИЙ МЕТОД ПІДКЛЮЧЕННЯ СТАНДАРТІВ ASPIRE (AddServiceDefaults)
    /// ------------------------------------------------------------------------------------
    /// ПРИЗНАЧЕННЯ:
    /// Єдиний рядок у Program.cs кожного мікросервісу (builder.AddServiceDefaults()), який
    /// автоматично комплектує сервіс усіма необхідними діагностичними інструментами.
    ///
    /// ВХІДНІ ПАРАМЕТРИ ТА ДЖЕНЕРИК-ОБМЕЖЕННЯ:
    /// - TBuilder: будь-який білдер, що реалізує IHostApplicationBuilder (WebApplicationBuilder
    ///   для Web API, HostApplicationBuilder для фонових воркерів тощо). Метод повертає той
    ///   самий тип TBuilder, тому зберігається "fluent"-ланцюжок викликів.
    ///
    /// ЩО НАЛАШТОВУЄТЬСЯ ВСЕРЕДИНІ:
    /// 1. ConfigureOpenTelemetry() — збір трейсів, метрик і логів + OTLP-експорт.
    /// 2. AddDefaultHealthChecks() — базовий health check працездатності процесу.
    /// 3. AddServiceDiscovery() — реєстрація резолвера логічних імен сервісів.
    /// 4. ConfigureHttpClientDefaults(...) — глобальні налаштування для ВСІХ HttpClient,
    ///    створених через IHttpClientFactory (типізованих, іменованих, gRPC-клієнтів).
    /// ====================================================================================
    /// </summary>
    public static TBuilder AddServiceDefaults<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        // 1. Активація спостережуваності (Observability)
        builder.ConfigureOpenTelemetry();

        // 2. Реєстрація базових перевірок стану (Health Checks)
        builder.AddDefaultHealthChecks();

        // 3. Підключення механізму Service Discovery для перетворення логічних імен у реальні адреси
        builder.Services.AddServiceDiscovery();

        // 4. Налаштування політик стійкості для всіх екземплярів HttpClient
        // ConfigureHttpClientDefaults гарантує, що будь-який клієнт, створений через IHttpClientFactory,
        // автоматично отримає підтримку Service Discovery та конвеєр стійкості Polly.
        // ⚠️ Тому НЕ додавайте .AddStandardResilienceHandler() повторно для окремих клієнтів:
        // конвеєри вкладуться один в одного і кількість повторів помножиться. Щоб змінити
        // параметри для конкретного клієнта, спершу викличте .RemoveAllResilienceHandlers().
        builder.Services.ConfigureHttpClientDefaults(http =>
        {
            // Стандартний конвеєр Microsoft.Extensions.Http.Resilience (Polly v8).
            // Стратегії від ЗОВНІШНЬОЇ до ВНУТРІШНЬОЇ (запит проходить їх саме в такому порядку):
            // 1. Rate Limiter — не більше 1000 одночасних запитів (захист власного процесу);
            // 2. Total Request Timeout — 30 с на весь виклик разом з усіма повторами;
            // 3. Retry — до 3 повторів, експоненційна затримка від 2 с + jitter
            //    (для 408, 429, 5xx, HttpRequestException та таймаутів спроби);
            // 4. Circuit Breaker — розмикання при ≥10% збоїв за 30 с (мінімум 100 запитів), пауза 5 с;
            // 5. Attempt Timeout — 10 с на ОДНУ спробу.
            http.AddStandardResilienceHandler();

            // Дозвіл клієнту резолвити URL формату "https+http://driversmanagement" через Aspire Service Discovery
            http.AddServiceDiscovery();
        });

        return builder;
    }

    /// <summary>
    /// ====================================================================================
    /// 📊 2. КОНФІГУРАЦІЯ OPENTELEMETRY (ConfigureOpenTelemetry)
    /// ------------------------------------------------------------------------------------
    /// ПРИЗНАЧЕННЯ:
    /// Створює єдиний конвеєр збору діагностичних даних з трьох стовпів Observability:
    /// Logging, Metrics та Tracing. Усі дані експортуються по стандарту OTLP
    /// (OpenTelemetry Protocol) безпосередньо в .NET Aspire Dashboard.
    ///
    /// ДЕТАЛЬНИЙ РОЗБІР ПАРАМЕТРІВ ТА ОПЦІЙ:
    /// 1. builder.Logging.AddOpenTelemetry:
    ///    - IncludeFormattedMessage = true:
    ///      Зберігає відформатоване людське повідомлення логу на додачу до шаблону.
    ///      Альтернатива: false (менше алокацій і трафіку у високонавантажених системах).
    ///    - IncludeScopes = true:
    ///      Додає до запису дані скоупів (ILogger.BeginScope) — наприклад, ідентифікатор
    ///      повідомлення MassTransit або власні бізнес-метадані.
    ///      (TraceId/SpanId потрапляють у кожен запис незалежно від цього параметра —
    ///      їх бере OTel-провайдер з поточної Activity.)
    ///
    /// 2. WithMetrics (Системні та прикладні лічильники):
    ///    - AddAspNetCoreInstrumentation(): тривалість вхідних HTTP-запитів, статус-коди.
    ///    - AddHttpClientInstrumentation(): латентність вихідних викликів до інших мікросервісів.
    ///    - AddRuntimeInstrumentation(): метрики .NET CLR (GC Gen0/1/2, пам'ять, ThreadPool,
    ///      кількість винятків тощо).
    ///    - Альтернативні опції: .AddMeter("CustomMeterName") для користувацьких бізнес-метрик.
    ///
    /// 3. WithTracing (Наскрізні розподілені трейси):
    ///    - AddSource(builder.Environment.ApplicationName): реєструє власний ActivitySource
    ///      мікросервісу для ручного створення спанів.
    ///    - AddSource("MassTransit"): спани публікації/споживання повідомлень RabbitMQ
    ///      з тим самим TraceId, що й вихідний HTTP-запит.
    ///    - AddSqlClientInstrumentation(): спан на кожен SQL-запит (EF Core працює через
    ///      Microsoft.Data.SqlClient, тому всі запити до SQL Server видно в трейсі).
    ///    - options.Filter: відсікає діагностичні запити Health Checks (/health та /alive),
    ///      які оркестратор виконує кожні кілька секунд — інакше вони засмічували б трейси.
    ///    - Альтернативні параметри: SetSampler(new ParentBasedSampler(new TraceIdRatioBasedSampler(0.1)))
    ///      для контролю відсотка збережених трейсів (Sampling Rate) під великим навантаженням.
    /// ====================================================================================
    /// </summary>
    public static TBuilder ConfigureOpenTelemetry<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
        });

        builder.Services.AddOpenTelemetry()
            .WithMetrics(metrics =>
            {
                metrics.AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation();
            })
            .WithTracing(tracing =>
            {
                tracing.AddSource(builder.Environment.ApplicationName)
                    .AddSource("MassTransit")
                    .AddAspNetCoreInstrumentation(options =>
                    {
                        // Фільтрація Health Checks: не записувати рутинні пінги життєздатності в трасування
                        options.Filter = context =>
                            !context.Request.Path.StartsWithSegments(HealthEndpointPath)
                            && !context.Request.Path.StartsWithSegments(AlivenessEndpointPath);
                    })
                    .AddHttpClientInstrumentation()
                    // SQL-запити EF Core / ADO.NET до SQL Server як дочірні спани (CLIENT)
                    .AddSqlClientInstrumentation();
            });

        // Підключення експортера OTLP (OpenTelemetry Protocol)
        builder.AddOpenTelemetryExporters();

        return builder;
    }

    /// <summary>
    /// ====================================================================================
    /// 📡 3. АВТОМАТИЧНИЙ OTLP ЕКСПОРТЕР (AddOpenTelemetryExporters)
    /// ------------------------------------------------------------------------------------
    /// ПРИЗНАЧЕННЯ:
    /// Перевіряє наявність змінної середовища OTEL_EXPORTER_OTLP_ENDPOINT.
    /// Коли мікросервіс запускається через Aspire AppHost, Aspire сам інжектує цей URL
    /// (OTLP-ендпоінт вбудованого Aspire Dashboard, див. launchSettings.json AppHost).
    ///
    /// АЛЬТЕРНАТИВНІ ВАРІАНТИ:
    /// - Якщо змінна відсутня (юніт-/функціональні тести, запуск без Aspire), OTLP
    ///   експортер не реєструється — жодних спроб з'єднання з неіснуючим колектором.
    /// - У продакшні цей параметр може вказувати на OpenTelemetry Collector, Grafana
    ///   Tempo/Loki, Datadog або Azure Monitor — код сервісу при цьому не змінюється.
    /// ====================================================================================
    /// </summary>
    private static TBuilder AddOpenTelemetryExporters<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        var useOtlpExporter = !string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);

        if (useOtlpExporter)
        {
            // UseOtlpExporter() одним викликом вмикає експорт логів, метрик і трейсів
            builder.Services.AddOpenTelemetry().UseOtlpExporter();
        }

        return builder;
    }

    /// <summary>
    /// ====================================================================================
    /// 🩺 4. ДІАГНОСТИЧНІ ПЕРЕВІРКИ СТАНУ (AddDefaultHealthChecks)
    /// ------------------------------------------------------------------------------------
    /// ПРИЗНАЧЕННЯ:
    /// Реєструє вбудовану систему Health Checks з первинною перевіркою працездатності процесу.
    ///
    /// РОЗБІР ПАРАМЕТРІВ:
    /// - "self": унікальне ім'я перевірки.
    /// - () => HealthCheckResult.Healthy(): тривіальна перевірка — якщо процес здатен
    ///   прийняти HTTP-запит і виконати цей делегат, він живий.
    /// - ["live"]: тег (Tag). Дозволяє розділяти перевірки на категорії:
    ///   * "live" — для Liveness probe (процес живий, чи його треба перезапустити).
    ///   * інші/без тегу — для Readiness probe (БД, брокер повідомлень тощо).
    /// ====================================================================================
    /// </summary>
    public static TBuilder AddDefaultHealthChecks<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.Services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);

        return builder;
    }

    /// <summary>
    /// ====================================================================================
    /// 🚪 5. МАПІНГ ДІАГНОСТИЧНИХ ЕНДПОІНТІВ (MapDefaultEndpoints)
    /// ------------------------------------------------------------------------------------
    /// ПРИЗНАЧЕННЯ:
    /// Мапить два стандартизовані маршрути для оркестраторів (Kubernetes, Docker, Aspire):
    ///
    /// 1. /health — READINESS PROBE (Зонд готовності приймати трафік):
    ///    - Викликає ВСІ зареєстровані перевірки (база даних, черга, сторонні API).
    ///    - Якщо хоча б одна повертає Unhealthy — HTTP 503, і балансувальник
    ///      не спрямовує на інстанс клієнтські запити.
    ///    - Саме цей ендпоінт опитує Aspire через .WithHttpHealthCheck("/health") в AppHost,
    ///      і саме на статус Healthy чекають залежні ресурси у .WaitFor(...).
    ///
    /// 2. /alive — LIVENESS PROBE (Зонд життєздатності процесу):
    ///    - Використовує Predicate: r => r.Tags.Contains("live").
    ///    - Перевіряє виключно життєздатність самого застосунку без зовнішніх залежностей.
    ///    - Якщо повертає збій — процес завис (deadlock, starvation), і оркестратор його перезапускає.
    ///
    /// БЕЗПЕКА ТА СЕРЕДОВИЩЕ:
    /// - Умова app.Environment.IsDevelopment() відкриває ендпоінти лише в розробці
    ///   (AppHost запускає сервіси саме в Development). У продакшні health-ендпоінти
    ///   зазвичай відкривають на окремому внутрішньому порту або захищають авторизацією,
    ///   щоб не розкривати внутрішній стан системи назовні.
    /// ====================================================================================
    /// </summary>
    public static WebApplication MapDefaultEndpoints(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            // /health: Readiness Probe — сервіс готовий приймати зовнішній трафік
            app.MapHealthChecks(HealthEndpointPath);

            // /alive: Liveness Probe — сервіс не завис і його процес функціонує
            app.MapHealthChecks(AlivenessEndpointPath, new HealthCheckOptions
            {
                Predicate = r => r.Tags.Contains("live")
            });
        }

        return app;
    }
}
