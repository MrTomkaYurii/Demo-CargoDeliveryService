
using MassTransit;
using MassTransit.RabbitMqTransport;
using SharedKernel.Messages;
using RabbitMQ.Client;

namespace DriversManagement.Extensions.Services.ProducerRegistrations
{
    /// <summary>
    /// ====================================================================================
    /// 📬 НАЛАШТУВАННЯ ПЕРВИННОГО ОБМІННИКА ДЛЯ ПОДІЇ IDriverUpdated
    /// ------------------------------------------------------------------------------------
    /// ПРИЗНАЧЕННЯ:
    /// Конфігурує назву та тип обмінника (Exchange) у RabbitMQ для контракту IDriverUpdated.
    ///
    /// 1. SetEntityName("driver-updated"):
    ///    Явно задає коротке ім'я обмінника замість повного імені типу.
    /// 2. ExchangeType = ExchangeType.Fanout:
    ///    Обмінник транслює повідомлення про зміну водія всім підписаним сервісам
    ///    (DriversBlogManagement, CargoOrderingService тощо) паралельно.
    /// ====================================================================================
    /// </summary>
    public static class DriverUpdatedEndpointRegistration
    {
        public static void DriverUpdatedEndpoint(this IRabbitMqBusFactoryConfigurator cfg)
        {
            cfg.Message<IDriverUpdated>(e => e.SetEntityName("driver-updated"));  // назва первинного обмінника
            cfg.Publish<IDriverUpdated>(e => e.ExchangeType = ExchangeType.Fanout); // тип обмінника Fanout (широкомовний)
        }
    }
}