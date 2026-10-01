namespace DriversBlogManagement.Domain.Likes.DomainEvents;

public sealed class LikeCreated : DomainEvent
{
    public Like Like { get; set; } 
}
            