namespace DriversBlogManagement.UnitTests.Domain.Comments;

using DriversBlogManagement.SharedTestHelpers.Fakes.Comment;
using DriversBlogManagement.Domain.Comments;
using DriversBlogManagement.Domain.Comments.DomainEvents;
using Bogus;
using FluentAssertions.Extensions;
using ValidationException = DriversBlogManagement.Exceptions.ValidationException;

public class UpdateCommentTests
{
    private readonly Faker _faker;

    public UpdateCommentTests()
    {
        _faker = new Faker();
    }
    
    [Fact]
    public void can_update_comment()
    {
        // Arrange
        var comment = new FakeCommentBuilder().Build();
        var updatedComment = new FakeCommentForUpdate().Generate();
        
        // Act
        comment.Update(updatedComment);

        // Assert
        comment.Text.Should().Be(updatedComment.Text);
    }
    
    [Fact]
    public void queue_domain_event_on_update()
    {
        // Arrange
        var comment = new FakeCommentBuilder().Build();
        var updatedComment = new FakeCommentForUpdate().Generate();
        comment.DomainEvents.Clear();
        
        // Act
        comment.Update(updatedComment);

        // Assert
        comment.DomainEvents.Count.Should().Be(1);
        comment.DomainEvents.FirstOrDefault().Should().BeOfType(typeof(CommentUpdated));
    }
}