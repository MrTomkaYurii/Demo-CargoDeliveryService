namespace DriversManagement.Domain.Companies.DomainEvents;

public sealed class CompanyCreated : DomainEvent
{
    public Company Company { get; set; } 
}
            