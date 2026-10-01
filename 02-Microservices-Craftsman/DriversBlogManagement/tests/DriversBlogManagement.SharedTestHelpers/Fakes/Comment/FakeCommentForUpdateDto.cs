namespace DriversBlogManagement.SharedTestHelpers.Fakes.Comment;

using AutoBogus;
using DriversBlogManagement.Domain.Comments;
using DriversBlogManagement.Domain.Comments.Dtos;

public sealed class FakeCommentForUpdateDto : AutoFaker<CommentForUpdateDto>
{
    public FakeCommentForUpdateDto()
    {
    }
}