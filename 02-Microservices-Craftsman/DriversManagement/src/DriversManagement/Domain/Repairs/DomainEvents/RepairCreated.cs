namespace DriversManagement.Domain.Repairs.DomainEvents;

public sealed class RepairCreated : DomainEvent
{
    public Repair Repair { get; set; } 
}
            