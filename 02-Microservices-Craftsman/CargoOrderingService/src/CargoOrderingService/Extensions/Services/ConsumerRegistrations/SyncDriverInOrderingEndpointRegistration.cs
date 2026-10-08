namespace CargoOrderingService.Extensions.Services.ConsumerRegistrations;

using MassTransit;
using MassTransit.RabbitMqTransport;
using RabbitMQ.Client;
using CargoOrderingService.Domain.Drivers.Consumers;

/// <summary>
/// ====================================================================================
/// 📬 ТОПОЛОГІЯ ТА НАЛАШТУВАННЯ ЧЕРГИ СПОЖИВАЧА У CARGOORDERINGSERVICE
/// ------------------------------------------------------------------------------------
/// ПРИЗНАЧЕННЯ:
/// Налаштовує кінцеву точку прийому повідомлень (ReceiveEndpoint) 'ordering-sync-driver'
/// для мікросервісу замовлень.
///
/// КЛЮЧОВІ ПАРАМЕТРИ RABBITMQ:
/// 1. ConfigureConsumeTopology = false:
///    Вимикає автоматичне створення черг за замовчуванням у MassTransit. Ми повністю
///    контролюємо назву черги, її параметри та зв'язки (Bindings).
/// 2. SetQuorumQueue():
///    Вмикає Quorum Queues (на базі консенсусу Raft у кластері RabbitMQ 3.8+).
///    Забезпечує реплікацію та стійкість до втрати даних при перезавантаженні брокера.
/// 3. SetQueueArgument("declare", "lazy"):
///    Оптимізує використання пам'яті (RAM), скидаючи повідомлення на диск за потреби.
/// 4. Bind("driver-created", Fanout) &amp; Bind("driver-updated", Fanout):
///    Створює прив'язки (Bindings) між головними обмінниками подій водіїв та цією чергою.
/// ====================================================================================
/// </summary>
public static class SyncDriverInOrderingEndpointRegistration
{
    public static void SyncDriverInOrderingEndpoint(this IRabbitMqBusFactoryConfigurator cfg, IBusRegistrationContext context)
    {
        cfg.ReceiveEndpoint("ordering-sync-driver", re =>
        {
            re.ConfigureConsumeTopology = false;
            re.SetQuorumQueue();
            re.SetQueueArgument("declare", "lazy");

            re.ConfigureConsumer<SyncDriverInOrdering>(context);

            re.Bind("driver-created", e =>
            {
                e.ExchangeType = ExchangeType.Fanout;
            });
            re.Bind("driver-updated", e =>
            {
                e.ExchangeType = ExchangeType.Fanout;
            });
        });
    }
}
