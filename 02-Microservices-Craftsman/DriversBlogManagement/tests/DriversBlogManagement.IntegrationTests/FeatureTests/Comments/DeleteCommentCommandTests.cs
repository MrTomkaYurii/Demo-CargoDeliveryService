namespace DriversBlogManagement.IntegrationTests.FeatureTests.Comments;

using DriversBlogManagement.SharedTestHelpers.Fakes.Comment;
using DriversBlogManagement.Domain.Comments.Features;
using Microsoft.EntityFrameworkCore;
using Domain;
using System.Threading.Tasks;

public class DeleteCommentCommandTests : TestBase
{
    [Fact]
    public async Task can_delete_comment_from_db()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        var comment = new FakeCommentBuilder().Build();
        await testingServiceScope.InsertAsync(comment);

        // Act
        var command = new DeleteComment.Command(comment.Id);
        await testingServiceScope.SendAsync(command);
        var commentResponse = await testingServiceScope
            .ExecuteDbContextAsync(db => db.Comments
                .CountAsync(c => c.Id == comment.Id));

        // Assert
        commentResponse.Should().Be(0);
    }

    [Fact]
    public async Task delete_comment_throws_notfoundexception_when_record_does_not_exist()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        var badId = Guid.NewGuid();

        // Act
        var command = new DeleteComment.Command(badId);
        Func<Task> act = () => testingServiceScope.SendAsync(command);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task can_softdelete_comment_from_db()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        var comment = new FakeCommentBuilder().Build();
        await testingServiceScope.InsertAsync(comment);

        // Act
        var command = new DeleteComment.Command(comment.Id);
        await testingServiceScope.SendAsync(command);
        var deletedComment = await testingServiceScope.ExecuteDbContextAsync(db => db.Comments
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == comment.Id));

        // Assert
        deletedComment?.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task must_be_permitted()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        testingServiceScope.SetUserNotPermitted(Permissions.CanDeleteComment);

        // Act
        var command = new DeleteComment.Command(Guid.NewGuid());
        Func<Task> act = () => testingServiceScope.SendAsync(command);

        // Assert
        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }
}