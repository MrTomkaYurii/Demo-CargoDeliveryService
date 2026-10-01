namespace DriversManagement.Domain.Expences.DomainEvents;

public sealed class ExpenceCreated : DomainEvent
{
    public Expence Expence { get; set; } 
}
            