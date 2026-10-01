namespace DriversBlogManagement.IntegrationTests.FeatureTests.Comments;

using DriversBlogManagement.SharedTestHelpers.Fakes.Comment;
using DriversBlogManagement.Domain.Comments.Features;
using Domain;
using FluentAssertions.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

public class CommentQueryTests : TestBase
{
    [Fact]
    public async Task can_get_existing_comment_with_accurate_props()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        var commentOne = new FakeCommentBuilder().Build();
        await testingServiceScope.InsertAsync(commentOne);

        // Act
        var query = new GetComment.Query(commentOne.Id);
        var comment = await testingServiceScope.SendAsync(query);

        // Assert
        comment.Text.Should().Be(commentOne.Text);
    }

    [Fact]
    public async Task get_comment_throws_notfound_exception_when_record_does_not_exist()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        var badId = Guid.NewGuid();

        // Act
        var query = new GetComment.Query(badId);
        Func<Task> act = () => testingServiceScope.SendAsync(query);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task must_be_permitted()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        testingServiceScope.SetUserNotPermitted(Permissions.CanReadComment);

        // Act
        var command = new GetComment.Query(Guid.NewGuid());
        Func<Task> act = () => testingServiceScope.SendAsync(command);

        // Assert
        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }
}