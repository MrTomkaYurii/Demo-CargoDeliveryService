
using MassTransit;
using MassTransit.RabbitMqTransport;
using SharedKernel.Messages;
using RabbitMQ.Client;

namespace DriversManagement.Extensions.Services.ProducerRegistrations
{
    public static class DriverUpdatedEndpointRegistration
    {
        public static void DriverUpdatedEndpoint(this IRabbitMqBusFactoryConfigurator cfg)
        {
            cfg.Message<IDriverUpdated>(e => e.SetEntityName("driver-updated"));  // name of the primary exchange
            cfg.Publish<IDriverUpdated>(e => e.ExchangeType = ExchangeType.Fanout); // primary exchange type
        }
    }
}