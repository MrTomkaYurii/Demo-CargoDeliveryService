namespace CargoOrderingService.Extensions.Services.ProducerRegistrations;

using MassTransit;
using MassTransit.RabbitMqTransport;
using SharedKernel.Messages;
using RabbitMQ.Client;

/// <summary>
/// ====================================================================================
/// 📬 ТОПОЛОГІЯ ОБМІННИКА ДЛЯ ПОДІЇ СТВОРЕННЯ ЗАМОВЛЕННЯ (IOrderCreated)
/// ------------------------------------------------------------------------------------
/// ПРИЗНАЧЕННЯ:
/// Налаштовує RabbitMQ Fanout Exchange 'order-created' для публікації подій замовлень.
/// Будь-який новий мікросервіс (наприклад, Billing або Notifications) може підключитися
/// до шини, створити власну чергу, прив'язатися до 'order-created' та отримувати події
/// без внесення змін у сервіс CargoOrderingService!
/// ====================================================================================
/// </summary>
public static class OrderCreatedEndpointRegistration
{
    public static void OrderCreatedEndpoint(this IRabbitMqBusFactoryConfigurator cfg)
    {
        cfg.Message<IOrderCreated>(e => e.SetEntityName("order-created"));
        cfg.Publish<IOrderCreated>(e => e.ExchangeType = ExchangeType.Fanout);
    }
}
