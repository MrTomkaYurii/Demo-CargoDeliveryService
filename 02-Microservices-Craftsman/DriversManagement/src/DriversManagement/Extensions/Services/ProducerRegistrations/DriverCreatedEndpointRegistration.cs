using MassTransit;
using MassTransit.RabbitMqTransport;
using SharedKernel.Messages;
using RabbitMQ.Client;

namespace DriversManagement.Extensions.Services.ProducerRegistrations
{
    public static class DriverCreatedEndpointRegistration
    {
        public static void DriverCreatedEndpoint(this IRabbitMqBusFactoryConfigurator cfg)
        {
            cfg.Message<IDriverCreated>(e => e.SetEntityName("driver-created"));  // name of the primary exchange
            cfg.Publish<IDriverCreated>(e => e.ExchangeType = ExchangeType.Fanout); // primary exchange type
        }
    }
}
