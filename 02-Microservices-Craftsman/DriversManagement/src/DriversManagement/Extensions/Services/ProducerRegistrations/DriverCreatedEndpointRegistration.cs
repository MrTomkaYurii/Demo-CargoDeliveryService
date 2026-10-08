using MassTransit;
using MassTransit.RabbitMqTransport;
using SharedKernel.Messages;
using RabbitMQ.Client;

namespace DriversManagement.Extensions.Services.ProducerRegistrations
{
    /// <summary>
    /// ====================================================================================
    /// 📬 НАЛАШТУВАННЯ ПЕРВИННОГО ОБМІННИКА ДЛЯ ПОДІЇ IDriverCreated
    /// ------------------------------------------------------------------------------------
    /// ПРИЗНАЧЕННЯ:
    /// Конфігурує назву та тип обмінника (Exchange) у RabbitMQ для контракту IDriverCreated.
    ///
    /// 1. SetEntityName("driver-created"):
    ///    Явно задає коротке ім'я обмінника замість згенерованого за замовчуванням
    ///    довгого імені 'SharedKernel.Messages:IDriverCreated'.
    /// 2. ExchangeType = ExchangeType.Fanout:
    ///    Обмінник транслює кожне опубліковане повідомлення ВСІМ чергам, які до нього
    ///    прив'язані (Broadcasting), ігноруючи Routing Keys. Це забезпечує чистий патерн
    ///    Publish-Subscribe (Pub/Sub).
    /// ====================================================================================
    /// </summary>
    public static class DriverCreatedEndpointRegistration
    {
        public static void DriverCreatedEndpoint(this IRabbitMqBusFactoryConfigurator cfg)
        {
            cfg.Message<IDriverCreated>(e => e.SetEntityName("driver-created"));  // назва первинного обмінника
            cfg.Publish<IDriverCreated>(e => e.ExchangeType = ExchangeType.Fanout); // тип обмінника Fanout (широкомовний)
        }
    }
}
