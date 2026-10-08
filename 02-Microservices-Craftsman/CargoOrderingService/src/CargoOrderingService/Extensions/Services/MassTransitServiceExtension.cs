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

/// <summary>
/// ====================================================================================
/// 📬 АРХІТЕКТУРНИЙ ПАТЕРН: РЕЄСТРАЦІЯ ТА КОНФІГУРАЦІЯ ШИНИ MASSTRANSIT (RABBITMQ)
/// ------------------------------------------------------------------------------------
/// ПРИЗНАЧЕННЯ:
/// Налаштовує брокер повідомлень MassTransit для сервісу замовлень з підтримкою:
/// 1. Aspire Connection String (автоматичне зв'язування з контейнером через Aspire AppHost).
/// 2. Класичної конфігурації RabbitMqOptions (для роботи в standalone Docker-compose/K8s).
/// 3. In-Memory Transport Fallback (для тестів та локального запуску без запущеного брокера).
///
/// ПРИНЦИП РОБОТИ:
/// - mt.AddConsumers(): автоматично знаходить та реєструє споживача SyncDriverInOrdering.
/// - cfg.OrderCreatedEndpoint(): налаштовує Fanout Exchange для публікації IOrderCreated.
/// - cfg.SyncDriverInOrderingEndpoint(): створює Quorum Queue для підписки на зміни водіїв.
/// ====================================================================================
/// </summary>
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
