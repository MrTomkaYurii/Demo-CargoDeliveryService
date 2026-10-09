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
/// 3. (Видалено) SetQueueArgument("declare", "lazy") — неіснуючий аргумент RabbitMQ.
///    Кворум-черга й так зберігає повідомлення на диску (див. коментар у коді нижче).
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
            // Власна топологія: MassTransit не створює обмінники/прив'язки автоматично
            re.ConfigureConsumeTopology = false;

            // Кворум-черга: реплікація через Raft, повідомлення зберігаються на диску
            re.SetQuorumQueue();

            // ⚠️ Раніше тут було re.SetQueueArgument("declare", "lazy") — такого аргументу в RabbitMQ
            // не існує (брокер його ігнорував). "Ліниві" черги вмикались аргументом
            // "x-queue-mode": "lazy" лише для КЛАСИЧНИХ черг і в RabbitMQ 3.12+ втратили сенс.

            // Повтори в пам'яті споживача (MassTransit за замовчуванням НЕ повторює!).
            // Якщо обробка впала (наприклад, БД тимчасово недоступна або deadlock), повідомлення
            // буде оброблене ще 3 рази з паузами 0.2 → 1 → 3 с. Лише після цього MassTransit
            // перемістить його в чергу помилок "<ім'я черги>_error" (Poison Message).
            // Безпечно, бо споживач ідемпотентний (перевіряє існування водія перед вставкою).
            re.UseMessageRetry(r => r.Intervals(200, 1000, 3000));

            // Споживач SyncDriverInOrdering обробляє і IDriverCreated, і IDriverUpdated
            re.ConfigureConsumer<SyncDriverInOrdering>(context);

            // Fanout-обмінники продюсера (DriversManagement) копіюють кожну подію в цю чергу
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
