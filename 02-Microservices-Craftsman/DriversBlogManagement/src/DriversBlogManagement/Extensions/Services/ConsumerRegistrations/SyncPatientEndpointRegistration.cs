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
/// 3. (Видалено) SetQueueArgument("declare", "lazy") — неіснуючий аргумент RabbitMQ;
///    кворум-черга й так зберігає повідомлення на диску (див. коментар у коді).
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

            // ⚠️ Раніше тут було re.SetQueueArgument("declare", "lazy") — такого аргументу в RabbitMQ
            // не існує (брокер просто ігнорував його). "Ліниві" черги вмикались аргументом
            // "x-queue-mode": "lazy" лише для КЛАСИЧНИХ черг і в RabbitMQ 3.12+ втратили сенс.
            // Кворум-черга (вище) і так зберігає повідомлення на диску та тримає в RAM лише
            // невеликий "хвіст", тож окреме налаштування не потрібне.

            // Повтори в пам'яті споживача (MassTransit за замовчуванням НЕ повторює!).
            // Якщо обробка впала (наприклад, БД тимчасово недоступна або deadlock), повідомлення
            // буде оброблене ще 3 рази з паузами 0.2 → 1 → 3 с. Лише після цього MassTransit
            // перемістить його в чергу помилок "<ім'я черги>_error" (Poison Message).
            // Безпечно, бо споживач ідемпотентний (перевіряє існування водія перед вставкою).
            re.UseMessageRetry(r => r.Intervals(200, 1000, 3000));

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