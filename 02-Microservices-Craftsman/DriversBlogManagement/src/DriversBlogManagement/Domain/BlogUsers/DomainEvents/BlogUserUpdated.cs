namespace DriversBlogManagement.Domain.BlogUsers.DomainEvents;

public sealed class BlogUserUpdated : DomainEvent
{
    public Guid Id { get; set; } 
}
            