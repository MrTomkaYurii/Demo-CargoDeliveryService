namespace Ordering.Extensions.Services;
using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;
using DriversManagement.Configurations;
using DriversManagement.Extensions.Services.ProducerRegistrations;
using DriversManagement.Resources;

public static class MassTransitServiceExtension
{
    public static void AddMassTransitServices(this IServiceCollection services, 
        IWebHostEnvironment env, 
        IConfiguration configuration)
    {
        var rmqOptions = configuration.GetRabbitMqOptions();
        var rmqConnectionString = configuration.GetConnectionString("rabbitmq");

        if (!env.IsEnvironment(Consts.Testing.IntegrationTestingEnvName) 
            && !env.IsEnvironment(Consts.Testing.FunctionalTestingEnvName))
        {
            services.AddMassTransit(mt =>
            {
                mt.AddConsumers(Assembly.GetExecutingAssembly());
                mt.UsingRabbitMq((context, cfg) =>
                {
                    if (!string.IsNullOrEmpty(rmqConnectionString))
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
                    }
                    else if (rmqOptions != null && !string.IsNullOrEmpty(rmqOptions.Host))
                    {
                        cfg.Host(rmqOptions.Host, 
                            ushort.TryParse(rmqOptions.Port, out var p) ? p : (ushort)5672, 
                            string.IsNullOrEmpty(rmqOptions.VirtualHost) ? "/" : rmqOptions.VirtualHost, 
                            h =>
                            {
                                h.Username(string.IsNullOrEmpty(rmqOptions.Username) ? "guest" : rmqOptions.Username);
                                h.Password(string.IsNullOrEmpty(rmqOptions.Password) ? "guest" : rmqOptions.Password);
                            });
                    }

                    // Producers -- Do Not Delete This Comment
                    cfg.DriverCreatedEndpoint();
                    cfg.DriverUpdatedEndpoint();

                    // Consumers -- Do Not Delete This Comment
                });
            });
            services.AddOptions<MassTransitHostOptions>();
        }
    }
}
