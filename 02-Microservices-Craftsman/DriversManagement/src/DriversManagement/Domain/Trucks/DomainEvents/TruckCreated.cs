namespace DriversManagement.Domain.Trucks.DomainEvents;

public sealed class TruckCreated : DomainEvent
{
    public Truck Truck { get; set; } 
}
            