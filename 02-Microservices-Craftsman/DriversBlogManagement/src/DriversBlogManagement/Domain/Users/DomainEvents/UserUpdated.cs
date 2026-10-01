namespace DriversBlogManagement.Domain.Users.DomainEvents;

public sealed class UserUpdated : DomainEvent
{
    public Guid Id { get; set; } 
}
            