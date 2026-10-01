namespace DriversBlogManagement.Domain.PostAboutDrivers.DomainEvents;

public sealed class PostAboutDriverUpdated : DomainEvent
{
    public Guid Id { get; set; } 
}
            