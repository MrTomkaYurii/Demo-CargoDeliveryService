namespace Billing.Extensions.Services;

using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;
using DriversBlogManagement.Extensions.Services.ConsumerRegistrations;
using DriversBlogManagement.Resources;
using DriversBlogManagement.Configurations;

/// <summary>
/// ====================================================================================
/// 📬 РЕЄСТРАЦІЯ ШИНИ MASSTRANSIT (RABBITMQ) У DriversBlogManagement — СТОРОНА СПОЖИВАЧА
/// ------------------------------------------------------------------------------------
/// ПРИЗНАЧЕННЯ:
/// Блог СПОЖИВАЄ події IDriverCreated / IDriverUpdated від DriversManagement і підтримує
/// локальну тіньову (shadow) копію водія — ім'я та прізвище автора під постами.
/// Черга "sync-updated-driver" і її прив'язки до обмінників описані в
/// ConsumerRegistrations/SyncPatientEndpointRegistration.cs, а логіка — у Domain/SyncDriver.cs.
///
/// ЗВІДКИ БЕРЕТЬСЯ АДРЕСА БРОКЕРА (у порядку пріоритету):
/// 1. ConnectionStrings:rabbitmq — рядок підключення, який Aspire AppHost інжектує через
///    .WithReference(rabbitmq) у вигляді amqp://guest:{пароль}@localhost:{порт}.
/// 2. Секція RabbitMq в appsettings.json — для запуску сервісу БЕЗ Aspire.
///
/// У середовищах інтеграційних/функціональних тестів шина не реєструється взагалі.
/// ====================================================================================
/// </summary>
public static class MassTransitServiceExtension
{
    public static void AddMassTransitServices(this IServiceCollection services, IWebHostEnvironment env, IConfiguration configuration)
    {
        var rmqOptions = configuration.GetRabbitMqOptions();
        var rmqConnectionString = configuration.GetConnectionString("rabbitmq");

        if (!env.IsEnvironment(Consts.Testing.IntegrationTestingEnvName) 
            && !env.IsEnvironment(Consts.Testing.FunctionalTestingEnvName))
        {
            services.AddMassTransit(mt =>
            {
                // Автоматично знаходить усі класи IConsumer<T> у збірці сервісу
                mt.AddConsumers(Assembly.GetExecutingAssembly());
                mt.UsingRabbitMq((context, cfg) =>
                {
                    if (!string.IsNullOrEmpty(rmqConnectionString))
                    {
                        // Розбираємо amqp-URI від Aspire: amqp://user:password@host:port/vhost
                        var uri = new Uri(rmqConnectionString);
                        var userInfo = uri.UserInfo.Split(':');
                        // Uri.UserInfo повертає percent-encoded значення (згенерований Aspire пароль
                        // може містити спецсимволи), тому декодуємо його перед передачею брокеру.
                        var username = userInfo.Length > 0 && !string.IsNullOrEmpty(userInfo[0]) ? Uri.UnescapeDataString(userInfo[0]) : "guest";
                        var password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : "guest";
                        var vhost = string.IsNullOrEmpty(uri.AbsolutePath.Trim('/')) ? "/" : uri.AbsolutePath.Trim('/');

                        cfg.Host(uri.Host, uri.Port > 0 ? (ushort)uri.Port : (ushort)5672, vhost, h =>
                        {
                            h.Username(username);
                            h.Password(password);
                        });
                    }
                    // Fallback: класична конфігурація з appsettings (запуск без Aspire)
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

                    // Consumers -- Do Not Delete This Comment
                    cfg.SyncPatientEndpoint(context);
                });
            });
            // MassTransitHostOptions керують стартом/зупинкою шини разом із хостом ASP.NET Core
            services.AddOptions<MassTransitHostOptions>();
        }
    }
}
