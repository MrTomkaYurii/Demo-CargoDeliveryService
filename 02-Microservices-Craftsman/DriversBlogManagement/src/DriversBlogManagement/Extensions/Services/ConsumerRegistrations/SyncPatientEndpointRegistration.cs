namespace DriversBlogManagement.Extensions.Services.ConsumerRegistrations;

using MassTransit;
using MassTransit.RabbitMqTransport;
using RabbitMQ.Client;
using Billing.Domain;

/// <summary>
/// ====================================================================================
/// 📬 ТОПОЛОГІЯ ТА НАЛАШТУВАННЯ ЧЕРГИ СПОЖИВАЧА У DRIVERSBLOGMANAGEMENT
/// ------------------------------------------------------------------------------------
/// ПРИЗНАЧЕННЯ:
/// Налаштовує кінцеву точку прийому повідомлень (ReceiveEndpoint) 'sync-updated-driver'
/// для мікросервісу блогів водіїв.
///
/// КЛЮЧОВІ ПАРАМЕТРИ RABBITMQ:
/// 1. ConfigureConsumeTopology = false:
///    Вимикає автоматичне автостворення топології за замовчуванням, дозволяючи явно
///    визначити ім'я черги та прив'язати її до обмінників.
/// 2. SetQuorumQueue():
///    Вмикає надійну репліковану чергу Quorum Queue (Raft-консенсус).
/// 3. SetQueueArgument("declare", "lazy"):
///    Увімкнення режиму Lazy Queue для оптимального використання пам'яті брокера.
/// 4. Bind("driver-updated", Fanout) &amp; Bind("driver-created", Fanout):
///    Черга підписується на обидва обмінники, забезпечуючи отримання як створених,
///    так і оновлених водіїв через єдиного споживача SyncDriver.
/// ====================================================================================
/// </summary>
public static class SyncPatientEndpointRegistration
{
    public static void SyncPatientEndpoint(this IRabbitMqBusFactoryConfigurator cfg, IBusRegistrationContext context)
    {
        cfg.ReceiveEndpoint("sync-updated-driver", re =>
        {
            // Вимикає стандартну топологію MassTransit для повного ручного контролю
            re.ConfigureConsumeTopology = false;

            // Кворум-черга для високої доступності та надійності даних (RabbitMQ 3.8+)
            re.SetQuorumQueue();

            // Lazy Queue для стабільності та передбачуваної продуктивності
            re.SetQueueArgument("declare", "lazy");

            // Реєстрація класу-споживача, який обробляє повідомлення з черги
            re.ConfigureConsumer<SyncDriver>(context);

            // Прив'язка (Binding) черги до первинних Fanout-обмінників
            re.Bind("driver-updated", e =>
            {
                e.ExchangeType = ExchangeType.Fanout;
            });
            re.Bind("driver-created", e =>
            {
                e.ExchangeType = ExchangeType.Fanout;
            });
        });
    }
}