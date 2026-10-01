namespace DriversBlogManagement.Extensions.Services.ConsumerRegistrations;

using MassTransit;
using MassTransit.RabbitMqTransport;
using RabbitMQ.Client;
using Billing.Domain;

public static class SyncPatientEndpointRegistration
{
    public static void SyncPatientEndpoint(this IRabbitMqBusFactoryConfigurator cfg, IBusRegistrationContext context)
    {
        cfg.ReceiveEndpoint("sync-updated-driver", re =>
        {
            // turns off default fanout settings
            re.ConfigureConsumeTopology = false;

            // a replicated queue to provide high availability and data safety. available in RMQ 3.8+
            re.SetQuorumQueue();

            // enables a lazy queue for more stable cluster with better predictive performance.
            // Please note that you should disable lazy queues if you require really high performance, if the queues are always short, or if you have set a max-length policy.
            re.SetQueueArgument("declare", "lazy");

            // the consumers that are subscribed to the endpoint
            re.ConfigureConsumer<SyncDriver>(context);

            // the binding of the intermediary exchange and the primary exchange
            re.Bind("driver-updated", e =>
            {
                e.ExchangeType = ExchangeType.Fanout;
            });
        });
    }
}