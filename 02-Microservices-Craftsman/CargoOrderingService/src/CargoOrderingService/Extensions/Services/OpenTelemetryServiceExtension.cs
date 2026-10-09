namespace CargoOrderingService.Extensions.Services;

using Microsoft.Extensions.Hosting;

/// <summary>
/// Точка підключення спільних стандартів Aspire (CargoDelivery.ServiceDefaults).
/// <para>
/// Історично Craftsman налаштовував тут OpenTelemetry + Jaeger вручну. Тепер увесь конвеєр
/// спостережуваності (OTLP-логи, метрики, трейси), Health Checks, Service Discovery і
/// стандартний конвеєр Polly для HttpClient підключає ОДИН виклик builder.AddServiceDefaults()
/// (див. Aspire/CargoDelivery.ServiceDefaults/Extensions.cs). Параметри configuration і
/// serviceName залишено для сумісності сигнатури з викликом у WebAppServiceConfiguration:
/// ім'я сервісу в телеметрії тепер задає Aspire через змінну OTEL_SERVICE_NAME.
/// </para>
/// </summary>
public static class OpenTelemetryServiceExtension
{
    public static void OpenTelemetryRegistration(this WebApplicationBuilder builder, IConfiguration configuration, string serviceName)
    {
        builder.AddServiceDefaults();
    }
}