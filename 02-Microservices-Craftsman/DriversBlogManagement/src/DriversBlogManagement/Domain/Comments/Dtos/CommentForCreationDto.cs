namespace DriversBlogManagement.Domain.Comments.Dtos;

using Destructurama.Attributed;

public sealed record CommentForCreationDto
{
    public string Text { get; set; }

}
