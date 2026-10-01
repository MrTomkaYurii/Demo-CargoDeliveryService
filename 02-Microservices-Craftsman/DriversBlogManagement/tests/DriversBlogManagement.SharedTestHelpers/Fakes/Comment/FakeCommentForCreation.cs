namespace DriversBlogManagement.SharedTestHelpers.Fakes.Comment;

using AutoBogus;
using DriversBlogManagement.Domain.Comments;
using DriversBlogManagement.Domain.Comments.Models;

public sealed class FakeCommentForCreation : AutoFaker<CommentForCreation>
{
    public FakeCommentForCreation()
    {
    }
}