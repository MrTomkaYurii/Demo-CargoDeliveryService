namespace DriversManagement.Domain.Trucks.DomainEvents;

public sealed class TruckUpdated : DomainEvent
{
    public Guid Id { get; set; } 
}
            