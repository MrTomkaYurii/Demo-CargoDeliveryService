namespace DriversManagement.Extensions.Services;

using DriversManagement.Middleware;
using DriversManagement.Services;
using System.Text.Json.Serialization;
using Serilog;
using Hellang.Middleware.ProblemDetails;
using Hellang.Middleware.ProblemDetails.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using Ordering.Extensions.Services;
using DriversBlogManagement;

public static class WebAppServiceConfiguration
{
    public static void ConfigureServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddTransient<IDateTimeProvider, DateTimeProvider>();
        builder.Services.AddSingleton(Log.Logger);
        builder.Services.AddProblemDetails(ProblemDetailsConfigurationExtension.ConfigureProblemDetails)
            .AddProblemDetailsConventions();

        builder.Services.AddCorsService("DriversManagementCorsPolicy", builder.Environment);
        builder.OpenTelemetryRegistration(builder.Configuration, "DriversManagement");
        builder.Services.AddInfrastructure(builder.Environment, builder.Configuration);
        builder.Services.AddMassTransitServices(builder.Environment, builder.Configuration);

        builder.Services.AddControllers()
            .AddJsonOptions(o => o.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);
        builder.Services.AddApiVersioningExtension();

        builder.Services.AddHttpContextAccessor();
        builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        builder.Services.AddBoundaryServices(Assembly.GetExecutingAssembly());

        builder.Services.AddMvc();

        builder.Services.AddHealthChecks();
        builder.Services.AddSwaggerExtension(builder.Configuration);

        // ====================================================================================
        // ⚡ gRPC-КЛІЄНТ ДО DriversBlogManagement (контракт Driver.proto, GrpcServices="Client")
        // ------------------------------------------------------------------------------------
        // AddGrpcClient під капотом використовує IHttpClientFactory, тому на нього ТАКОЖ діють
        // глобальні налаштування ServiceDefaults (Service Discovery + стандартний конвеєр Polly).
        // Логічне ім'я "driversblogmanagement" резолвиться через змінну
        // services__driversblogmanagement__https__0, яку інжектує AppHost (.WithReference(blogApi)).
        // gRPC працює поверх HTTP/2, тому беремо HTTPS-ендпоінт (HTTP/2 узгоджується через TLS ALPN).
        // Якщо сервіс запущено БЕЗ Aspire, використовується адреса з секції GrpcServer:Address.
        // ====================================================================================
        builder.Services.AddGrpcClient<DriverRpc.DriverRpcClient>(options =>
        {
            var grpcAddress = builder.Configuration["services:driversblogmanagement:https:0"] is not null
                ? "https://driversblogmanagement"
                : builder.Configuration.GetSection("GrpcServer")["Address"] ?? "https://localhost:5002";

            options.Address = new Uri(grpcAddress);
        })
        .RemoveAllResilienceHandlers()
        // Пости блогу — некритичні дані профілю: короткий бюджет часу замість стандартних 30 с
        .AddStandardResilienceHandler(SoftDependencyBudget);

        // ====================================================================================
        // 🌐 РЕЄСТРАЦІЯ ТИПІЗОВАНИХ HTTP-КЛІЄНТІВ ДЛЯ GATEWAY AGGREGATOR / BFF
        // ------------------------------------------------------------------------------------
        // Використовуємо IHttpClientFactory, .NET Aspire Service Discovery та Polly Resilience:
        // 1. "https+http://cargoorderingservice"  - логічне ім'я, резолвиться Aspire Service Discovery
        //    ("https+http" = спершу HTTPS-ендпоінт, якщо його немає — HTTP).
        // 2. "https+http://driversblogmanagement" - аналогічно.
        // 3. Стандартний конвеєр Polly (Retry, Circuit Breaker, Timeout) ВЖЕ доданий глобально
        //    через builder.AddServiceDefaults() → ConfigureHttpClientDefaults. Повторний виклик
        //    .AddStandardResilienceHandler() тут вклав би один конвеєр в інший і помножив повтори.
        // ====================================================================================
        builder.Services.AddHttpClient<DriversManagement.Domain.Drivers.Services.Http.ICargoOrderingApiClient, DriversManagement.Domain.Drivers.Services.Http.CargoOrderingApiClient>(client =>
            client.BaseAddress = new Uri("https+http://cargoorderingservice"));

        // Блог — НЕКРИТИЧНА залежність дашборду, тож для нього короткий бюджет часу (див. SoftDependencyBudget)
        builder.Services.AddHttpClient<DriversManagement.Domain.Drivers.Services.Http.IDriversBlogApiClient, DriversManagement.Domain.Drivers.Services.Http.DriversBlogApiClient>(client =>
                client.BaseAddress = new Uri("https+http://driversblogmanagement"))
            .RemoveAllResilienceHandlers()
            .AddStandardResilienceHandler(SoftDependencyBudget);
    }

    /// <summary>
    /// Короткий бюджет Polly для НЕКРИТИЧНИХ залежностей (блог водіїв).
    /// <para>
    /// Стандартні значення (спроба 10 с, загалом 30 с) змусили б користувача чекати пів
    /// хвилини, якщо блог недоступний або "завис". Для даних, без яких екран усе одно
    /// корисний, краще швидко здатися і застосувати Graceful Degradation:
    /// спроба 1 с, один повтор через ~200 мс, загальний ліміт 3 с.
    /// </para>
    /// </summary>
    private static void SoftDependencyBudget(Microsoft.Extensions.Http.Resilience.HttpStandardResilienceOptions options)
    {
        options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(1);
        options.Retry.MaxRetryAttempts = 1;
        options.Retry.Delay = TimeSpan.FromMilliseconds(200);
        options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(3);
    }

    private static void AddBoundaryServices(this IServiceCollection services, params Assembly[] assemblies)
    {
        if (!assemblies.Any())
            throw new ArgumentException("No assemblies found to scan. Supply at least one assembly to scan for handlers.");

        foreach (var assembly in assemblies)
        {
            var rules = assembly.GetTypes()
                .Where(x => !x.IsAbstract && x.IsClass && x.GetInterface(nameof(IDriversManagementScopedService)) == typeof(IDriversManagementScopedService));

            foreach (var rule in rules)
            {
                foreach (var @interface in rule.GetInterfaces())
                {
                    services.Add(new ServiceDescriptor(@interface, rule, ServiceLifetime.Scoped));
                }
            }
        }
    }
}
