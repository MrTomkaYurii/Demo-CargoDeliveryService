namespace CargoOrderingService.Extensions.Services;

using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;
using CargoOrderingService.Configurations;
using CargoOrderingService.Extensions.Services.ConsumerRegistrations;
using CargoOrderingService.Extensions.Services.ProducerRegistrations;

public static class MassTransitServiceExtension
{
    public static void AddMassTransitServices(this IServiceCollection services, 
        IWebHostEnvironment env, 
        IConfiguration configuration)
    {
        var rmqOptions = configuration.GetRabbitMqOptions();

        if (rmqOptions != null && !string.IsNullOrEmpty(rmqOptions.Host))
        {
            services.AddMassTransit(mt =>
            {
                mt.AddConsumers(Assembly.GetExecutingAssembly());
                mt.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(rmqOptions.Host, 
                        ushort.TryParse(rmqOptions.Port, out var port) ? port : (ushort)5672, 
                        string.IsNullOrEmpty(rmqOptions.VirtualHost) ? "/" : rmqOptions.VirtualHost, 
                        h =>
                        {
                            h.Username(string.IsNullOrEmpty(rmqOptions.Username) ? "guest" : rmqOptions.Username);
                            h.Password(string.IsNullOrEmpty(rmqOptions.Password) ? "guest" : rmqOptions.Password);
                        });

                    // Producers
                    cfg.OrderCreatedEndpoint();

                    // Consumers
                    cfg.SyncDriverInOrderingEndpoint(context);
                });
            });
            services.AddOptions<MassTransitHostOptions>();
        }
        else
        {
            // Fallback для розробки/тестування в пам'яті (In-Memory Bus)
            services.AddMassTransit(mt =>
            {
                mt.AddConsumers(Assembly.GetExecutingAssembly());
                mt.UsingInMemory((context, cfg) =>
                {
                    cfg.ConfigureEndpoints(context);
                });
            });
        }
    }
}
