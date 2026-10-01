namespace DriversManagement.Domain.Inspections.DomainEvents;

public sealed class InspectionUpdated : DomainEvent
{
    public Guid Id { get; set; } 
}
            