namespace DriversManagement.Domain.Repairs.DomainEvents;

public sealed class RepairUpdated : DomainEvent
{
    public Guid Id { get; set; } 
}
            