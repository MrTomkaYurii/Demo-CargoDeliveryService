namespace CargoOrderingService.Extensions.Services.ConsumerRegistrations;

using MassTransit;
using MassTransit.RabbitMqTransport;
using RabbitMQ.Client;
using CargoOrderingService.Domain.Drivers.Consumers;

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
