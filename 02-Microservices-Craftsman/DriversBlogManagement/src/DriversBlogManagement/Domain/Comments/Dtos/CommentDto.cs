namespace DriversBlogManagement.Domain.Comments.Dtos;

using Destructurama.Attributed;

public sealed record CommentDto
{
    public Guid Id { get; set; }
    public string Text { get; set; }

}
