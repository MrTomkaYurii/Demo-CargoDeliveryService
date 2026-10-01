namespace DriversManagement.Domain.Expences.DomainEvents;

public sealed class ExpenceUpdated : DomainEvent
{
    public Guid Id { get; set; } 
}
            