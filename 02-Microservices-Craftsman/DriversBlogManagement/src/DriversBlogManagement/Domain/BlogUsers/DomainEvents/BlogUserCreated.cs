namespace DriversBlogManagement.Domain.BlogUsers.DomainEvents;

public sealed class BlogUserCreated : DomainEvent
{
    public BlogUser BlogUser { get; set; } 
}
            