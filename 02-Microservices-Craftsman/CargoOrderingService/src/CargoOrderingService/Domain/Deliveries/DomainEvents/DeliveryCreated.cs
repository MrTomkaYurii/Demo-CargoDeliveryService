namespace CargoOrderingService.Domain.Deliveries.DomainEvents;

public sealed class DeliveryCreated : DomainEvent
{
    public Delivery Delivery { get; set; } 
}
            