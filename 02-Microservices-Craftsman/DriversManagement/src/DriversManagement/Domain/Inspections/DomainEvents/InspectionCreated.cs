namespace DriversManagement.Domain.Inspections.DomainEvents;

public sealed class InspectionCreated : DomainEvent
{
    public Inspection Inspection { get; set; } 
}
            