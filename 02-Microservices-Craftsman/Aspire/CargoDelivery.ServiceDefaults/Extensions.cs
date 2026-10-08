// ====================================================================================
// 🛠️ .NET 10 ASPIRE SERVICE DEFAULTS - АРХІТЕКТУРНИЙ ЕТАЛОН НАЛАШТУВАНЬ МІКРОСЕРВІСУ
// ------------------------------------------------------------------------------------
// Цей проєкт містить уніфікований набір розширень IHostApplicationBuilder, що
// забезпечує дотримання золотого стандарту розподілених хмарних систем:
// 1. OpenTelemetry: наскрізне трасування (Tracing), системні метрики (Metrics) та OTLP-логи.
// 2. Health Checks: стандартизовані зонди готовності (/health) та життєздатності (/alive).
// 3. Service Discovery: динамічна резолюція логічних імен (наприклад, http://cargoorderingservice).
// 4. Polly Resilience: конвеєр із 5 рівнів захисту мережевих викликів HttpClient.
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
    /// - TBuilder: будь-який білдер, що реалізує IHostApplicationBuilder (WebApplicationBuilder,
    ///   HostApplicationBuilder для фонових воркерів, Aspire TestBuilder тощо).
    ///
    /// ЩО НАЛАШТОВУЄТЬСЯ ВСЕРЕДИНІ:
    /// 1. ConfigureOpenTelemetry() — збір трейсів, метрик і логів.
    /// 2. AddDefaultHealthChecks() — базовий health check працездатності процесу.
    /// 3. AddServiceDiscovery() — активація провайдера резолюції імен.
    /// 4. ConfigureHttpClientDefaults(...) — глобальні налаштування стійкості для ВСІХ
    ///    HttpClient у додатку через IHttpClientFactory.
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
        builder.Services.ConfigureHttpClientDefaults(http =>
        {
            // Підключення стандарту стійкості Polly:
            // Включає конвеєр з 5 стратегій:
            // 1. Total Request Timeout (загальний ліміт часу)
            // 2. Retry (повтори при мережевих збоях і HTTP 5xx)
            // 3. Circuit Breaker (автоматичне розмикання ланцюга при каскадних аваріях)
            // 4. Attempt Timeout (таймаут окремої спроби)
            // 5. Rate Limiter (обмеження одночасних запитів)
            http.AddStandardResilienceHandler();

            // Дозвіл клієнту резолвити URL формату "http://driversmanagement" через Aspire Service Discovery
            http.AddServiceDiscovery();
        });

        return builder;
    }

    /// <summary>
    /// ====================================================================================
    /// 📊 2. КОНФІГУРАЦІЯ ОПЕНТЕЛЕМЕТРІЇ (ConfigureOpenTelemetry)
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
    ///      Альтернатива: false (зменшує обсяг пам'яті в високонавантажених системах).
    ///    - IncludeScopes = true:
    ///      Включає параметри скоупів (ILogger.BeginScope), що автоматично додає
    ///      TraceId, SpanId та користувацькі метадані до кожного запису логу.
    ///
    /// 2. WithMetrics (Системні та прикладні лічильники):
    ///    - AddAspNetCoreInstrumentation(): збирає тривалість HTTP-запитів, статус-коди.
    ///    - AddHttpClientInstrumentation(): фіксує латентність зовнішніх викликів до інших мікросервісів.
    ///    - AddRuntimeInstrumentation(): збирає критичні метрики .NET CLR (використання пам'яті,
    ///      кількість збірок сміття GC Gen0/1/2, стан пулу потоків ThreadPool, завантаження CPU).
    ///    - Альтернативні опції: .AddMeter("CustomMeterName") для користувацьких бізнес-метрик.
    ///
    /// 3. WithTracing (Наскрізні розподілені трейси):
    ///    - AddSource(builder.Environment.ApplicationName): реєструє власний ActivitySource
    ///      мікросервісу для ручного створення спанів.
    ///    - AddSource("MassTransit"): автоматично зв'язує повідомлення в RabbitMQ єдиним TraceId.
    ///    - AddSource("Microsoft.EntityFrameworkCore.SqlServer"): трейсить SQL-запити до БД з тривалістю.
    ///    - options.Filter: делегат фільтрації. Критично важливий для усунення «шуму»:
    ///      відсікає діагностичні запити Health Checks (/health та /alive), які виконуються кожні
    ///      кілька секунд і без фільтрації захаращували б базу трейсів мільйонами пустих записів.
    ///    - Альтернативні параметри: SetSampler(new AlwaysOnSampler()) або ParentBasedSampler
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
                    .AddSource("Microsoft.EntityFrameworkCore.SqlServer")
                    .AddAspNetCoreInstrumentation(options =>
                    {
                        // Фільтрація Health Checks: не записувати рутинні пінги життєздатності в трасування
                        options.Filter = context =>
                            !context.Request.Path.StartsWithSegments(HealthEndpointPath)
                            && !context.Request.Path.StartsWithSegments(AlivenessEndpointPath);
                    })
                    .AddHttpClientInstrumentation();
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
    /// Автоматично перевіряє наявність змінної середовища OTEL_EXPORTER_OTLP_ENDPOINT.
    /// Коли мікросервіс запускається через Aspire AppHost, Aspire сам інжектує цей URL
    /// (зазвичай gRPC або HTTP endpoint вбудованого Aspire Dashboard).
    ///
    /// АЛЬТЕРНАТИВНІ ВАРІАНТИ:
    /// - Якщо змінна відсутня (наприклад, у локальних юніт-тестах або без Aspire), OTLP
    ///   експортер не ініціалізується, уникаючи спаму помилками "Connection Refused".
    /// - У продакшні цей параметр може вказувати на Grafana Mimir/Tempo, Datadog або Azure Monitor.
    /// ====================================================================================
    /// </summary>
    private static TBuilder AddOpenTelemetryExporters<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        var useOtlpExporter = !string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);

        if (useOtlpExporter)
        {
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
    /// - () => HealthCheckResult.Healthy(): швидка функція перевірки, яка підтверджує,
    ///   що CLR-потік відповідає і пам'ять не переповнена.
    /// - ["live"]: тег (Tag). Дозволяє розділяти перевірки на категорії:
    ///   * "live" — для Liveness probe (процес живий, чи його треба перезапустити контейнером).
    ///   * "ready" — для Readiness probe (чи підключені БД, брокер повідомлень тощо).
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
    /// Мапить два стандартизовані маршрути для оркестраторів (Kubernetes, Docker, Aspire Dashboard):
    ///
    /// 1. /health — READINESS PROBE (Зонд готовності приймати трафік):
    ///    - Викликає ВСІ зареєстровані перевірки (база даних, черга, сторонні API).
    ///    - Якщо хоча б одна залежність повертає Unhealthy, сервіс повертає HTTP 503,
    ///      і балансувальник навантаження не спрямовує на нього клієнтські запити.
    ///
    /// 2. /alive — LIVENESS PROBE (Зонд життєздатності процесу):
    ///    - Використовує Predicate: r => r.Tags.Contains("live").
    ///    - Перевіряє виключно життєздатність самого застосунку без зовнішніх залежностей.
    ///    - Якщо повертає збій — контейнер завис у deadlock, і Docker/Kubernetes перезапускає процес.
    ///
    /// БЕЗПЕКА ТА СЕРЕДОВИЩЕ:
    /// - Умова app.Environment.IsDevelopment() відкриває ендпоінти для локальної розробки
    ///   та діагностики Aspire Dashboard.
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
