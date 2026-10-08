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
        var rmqConnectionString = configuration.GetConnectionString("rabbitmq");

        if (!string.IsNullOrEmpty(rmqConnectionString))
        {
            services.AddMassTransit(mt =>
            {
                mt.AddConsumers(Assembly.GetExecutingAssembly());
                mt.UsingRabbitMq((context, cfg) =>
                {
                    var uri = new Uri(rmqConnectionString);
                    var userInfo = uri.UserInfo.Split(':');
                    var username = userInfo.Length > 0 && !string.IsNullOrEmpty(userInfo[0]) ? userInfo[0] : "guest";
                    var password = userInfo.Length > 1 ? userInfo[1] : "guest";
                    var vhost = string.IsNullOrEmpty(uri.AbsolutePath.Trim('/')) ? "/" : uri.AbsolutePath.Trim('/');

                    cfg.Host(uri.Host, uri.Port > 0 ? (ushort)uri.Port : (ushort)5672, vhost, h =>
                    {
                        h.Username(username);
                        h.Password(password);
                    });

                    // Producers
                    cfg.OrderCreatedEndpoint();

                    // Consumers
                    cfg.SyncDriverInOrderingEndpoint(context);
                });
            });
            services.AddOptions<MassTransitHostOptions>();
        }
        else if (rmqOptions != null && !string.IsNullOrEmpty(rmqOptions.Host))
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
