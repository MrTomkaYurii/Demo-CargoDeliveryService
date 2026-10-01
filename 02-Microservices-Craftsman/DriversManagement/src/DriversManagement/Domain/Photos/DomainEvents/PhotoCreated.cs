namespace DriversManagement.Domain.Photos.DomainEvents;

public sealed class PhotoCreated : DomainEvent
{
    public Photo Photo { get; set; } 
}
            