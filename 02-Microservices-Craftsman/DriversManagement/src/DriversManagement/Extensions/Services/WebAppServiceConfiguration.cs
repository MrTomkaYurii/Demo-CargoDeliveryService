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

        builder.Services.AddGrpcClient<DriverRpc.DriverRpcClient>(options =>
        {
            var grpcAddress = builder.Configuration["services:driversblogmanagement:https:0"]
                ?? builder.Configuration["services:driversblogmanagement:http:0"]
                ?? builder.Configuration.GetSection("GrpcServer")["Address"]
                ?? "https://localhost:5002";

            options.Address = new Uri(grpcAddress);
        });
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
