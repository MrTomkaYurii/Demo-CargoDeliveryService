namespace CargoOrderingService.Extensions.Services.ProducerRegistrations;

using MassTransit;
using MassTransit.RabbitMqTransport;
using SharedKernel.Messages;
using RabbitMQ.Client;

public static class OrderCreatedEndpointRegistration
{
    public static void OrderCreatedEndpoint(this IRabbitMqBusFactoryConfigurator cfg)
    {
        cfg.Message<IOrderCreated>(e => e.SetEntityName("order-created"));
        cfg.Publish<IOrderCreated>(e => e.ExchangeType = ExchangeType.Fanout);
    }
}
