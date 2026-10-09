using Destructurama;
using Serilog;
using Serilog.Extensions.Logging;
using Hangfire;
using Hellang.Middleware.ProblemDetails;
using DriversBlogManagement.Extensions.Application;
using DriversBlogManagement.Extensions.Services;
using DriversBlogManagement.Databases;
using DriversBlogManagement.Domain.Drivers.Services;
using DriversBlogManagement.Resources.HangfireUtilities;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

    // ====================================================================================
    // 📝 SERILOG + OPENTELEMETRY: ЯК ЛОГИ ПОТРАПЛЯЮТЬ В ASPIRE DASHBOARD
    // ------------------------------------------------------------------------------------
    // builder.Host.UseSerilog() ЗАМІНЮЄ всі стандартні провайдери логування Microsoft.
    // Серед них і провайдер OpenTelemetry, який підключає AddServiceDefaults() — тому без
    // додаткових кроків логи сервісу видно лише в "Console logs", а вкладка
    // "Structured logs" в Aspire Dashboard порожня.
    // Рішення: LoggerProviderCollection + WriteTo.Providers(...) — Serilog пише як у консоль
    // (за appsettings), так і в усі зареєстровані ILoggerProvider (у т.ч. OTLP-експортер).
    // ====================================================================================
    var serilogProviders = new LoggerProviderCollection();
    Log.Logger = new LoggerConfiguration()
        .ReadFrom.Configuration(builder.Configuration)
        .Enrich.WithProperty("ApplicationName", builder.Environment.ApplicationName)
        .Destructure.UsingAttributes()
        .WriteTo.Providers(serilogProviders)
        .CreateLogger();

    builder.Host.UseSerilog(providers: serilogProviders);
    builder.ConfigureServices();

var app = builder.Build();

    // Configure the HTTP request pipeline.
    app.MapGrpcService<DriverRpcService>();

    using var scope = app.Services.CreateScope();
    if (builder.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }
    else
    {
        app.UseExceptionHandler("/Error");
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
    }
    app.UseProblemDetails();

    // For elevated security, it is recommended to remove this middleware and set your server to only listen on https.
    // A slightly less secure option would be to redirect http to 400, 505, etc.
    app.UseHttpsRedirection();

    app.UseCors("DriversBlogManagementCorsPolicy");

    app.MapHealthChecks("api/health");
app.MapDefaultEndpoints();
    app.UseSerilogRequestLogging();
    app.UseRouting();

    // [AUTH-DISABLED] Автентифікацію (JWT/Keycloak) вимкнено для навчального стенду — див. InfrastructureServiceExtension.
    // [AUTH-DISABLED] app.UseAuthentication();
    // UseAuthorization залишено: без атрибутів [Authorize] нічого не блокує, але знадобиться, якщо авторизацію повернуть.
    app.UseAuthorization();

    app.MapControllers();

    app.UseHangfireDashboard("/hangfire", new DashboardOptions
    {
        AsyncAuthorization = new[] { new HangfireAuthorizationFilter(scope.ServiceProvider) },
        IgnoreAntiforgeryToken = true
    });

    app.UseSwaggerExtension(builder.Configuration, builder.Environment);

    try
    {
        Log.Information("Starting application");
        await app.RunAsync();
    }
    catch (Exception e)
    {
        Log.Error(e, "The application failed to start correctly");
        throw;
    }
    finally
    {
        Log.Information("Shutting down application");
        Log.CloseAndFlush();
    }

    // Make the implicit Program class public so the functional test project can access it
    public partial class Program { }