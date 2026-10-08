// ====================================================================================
// 🛠️ .NET 10 ASPIRE SERVICE DEFAULTS - УНІВЕРСАЛЬНИЙ НАБІР СТАНДАРТІВ МІКРОСЕРВІСУ
// ------------------------------------------------------------------------------------
// Цей проєкт містить спільні розширення конфігурації для кожного мікросервісу системи:
// 1. OpenTelemetry: структуроване логування, метрики рантайму та наскрізний трейсинг (OTLP).
// 2. Health Checks: діагностичні ендпоінти готовності (/health) та життєздатності (/alive).
// 3. Service Discovery: автоматичне перетворення логічних імен сервісів на реальні URL.
// 4. Polly Resilience: стандартні політики повторів, автоматичного вимикача та таймаутів.
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
    /// Головна точка підключення стандартів Aspire для будь-якого мікросервісу.
    /// Викликається в Program.cs: builder.AddServiceDefaults();
    /// </summary>
    public static TBuilder AddServiceDefaults<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        // 1. Налаштування збору логів, метрик та трасування OpenTelemetry
        builder.ConfigureOpenTelemetry();

        // 2. Реєстрація базових Health Checks
        builder.AddDefaultHealthChecks();

        // 3. Активація бібліотеки Aspire Service Discovery
        builder.Services.AddServiceDiscovery();

        // 4. Глобальні налаштування за замовчуванням для всіх HttpClient у системі
        builder.Services.ConfigureHttpClientDefaults(http =>
        {
            // Увімкнення стандарту стійкості Polly (Retry + Circuit Breaker + Timeout)
            http.AddStandardResilienceHandler();

            // Дозвіл HttpClient використовувати логічні імена замість IP-адрес
            http.AddServiceDiscovery();
        });

        return builder;
    }

    /// <summary>
    /// Конфігурація наскрізної спостережуваності (Observability) через екосистему OpenTelemetry.
    /// Передає дані безпосередньо в .NET Aspire Dashboard через OTLP без сторонніх агентів.
    /// </summary>
    public static TBuilder ConfigureOpenTelemetry<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        // Логування з підтримкою OpenTelemetry контексту та TraceId/SpanId
        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
        });

        builder.Services.AddOpenTelemetry()
            .WithMetrics(metrics =>
            {
                // Збір метрик ASP.NET Core, HttpClient та .NET Runtime (пам'ять, GC, CPU)
                metrics.AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation();
            })
            .WithTracing(tracing =>
            {
                // Реєстрація джерел трасування:
                // - Власний сервіс
                // - MassTransit: наскрізне трасування повідомлень через RabbitMQ Broker
                // - EF Core: трасування SQL-запитів до бази даних
                tracing.AddSource(builder.Environment.ApplicationName)
                    .AddSource("MassTransit")
                    .AddSource("Microsoft.EntityFrameworkCore.SqlServer")
                    .AddAspNetCoreInstrumentation(options =>
                    {
                        // Фільтрація: не засмічувати трейси регулярними опитуваннями Health Checks
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
    /// Автоматична активація OTLP експортера, якщо в конфігурації присутній OTEL_EXPORTER_OTLP_ENDPOINT
    /// (цей параметр автоматично інжектується .NET Aspire при старті через AppHost).
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
    /// Реєстрація базового Health Check "self" для перевірки працездатності поточного процесу.
    /// </summary>
    public static TBuilder AddDefaultHealthChecks<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.Services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);

        return builder;
    }

    /// <summary>
    /// Мапінг системних діагностичних ендпоінтів для Kubernetes / Docker / Aspire Dashboard.
    /// Викликається в Program.cs після app.Build(): app.MapDefaultEndpoints();
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
