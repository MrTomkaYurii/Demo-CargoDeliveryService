namespace DriversManagement.Domain.Photos.DomainEvents;

public sealed class PhotoUpdated : DomainEvent
{
    public Guid Id { get; set; } 
}
            