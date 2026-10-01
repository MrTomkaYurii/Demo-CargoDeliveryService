namespace DriversBlogManagement.Domain.Likes.Dtos;

using Destructurama.Attributed;

public sealed record LikeDto
{
    public Guid Id { get; set; }
    public string Info { get; set; }

}
