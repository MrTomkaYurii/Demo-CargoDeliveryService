namespace DriversManagement.Domain.Companies.DomainEvents;

public sealed class CompanyUpdated : DomainEvent
{
    public Guid Id { get; set; } 
}
            