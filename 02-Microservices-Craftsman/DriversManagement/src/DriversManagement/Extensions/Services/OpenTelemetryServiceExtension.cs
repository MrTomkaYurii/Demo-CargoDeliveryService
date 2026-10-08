namespace DriversManagement.Extensions.Services;

using Microsoft.Extensions.Hosting;

public static class OpenTelemetryServiceExtension
{
    public static void OpenTelemetryRegistration(this WebApplicationBuilder builder, IConfiguration configuration, string serviceName)
    {
        builder.AddServiceDefaults();
    }
}