namespace DriversBlogManagement.SharedTestHelpers.Fakes.Comment;

using AutoBogus;
using DriversBlogManagement.Domain.Comments;
using DriversBlogManagement.Domain.Comments.Models;

public sealed class FakeCommentForUpdate : AutoFaker<CommentForUpdate>
{
    public FakeCommentForUpdate()
    {
    }
}