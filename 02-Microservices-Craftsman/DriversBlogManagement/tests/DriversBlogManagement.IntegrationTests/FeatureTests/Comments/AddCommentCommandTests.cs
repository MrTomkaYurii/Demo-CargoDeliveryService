namespace DriversBlogManagement.IntegrationTests.FeatureTests.Comments;

using DriversBlogManagement.SharedTestHelpers.Fakes.Comment;
using Domain;
using FluentAssertions.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using DriversBlogManagement.Domain.Comments.Features;

public class AddCommentCommandTests : TestBase
{
    [Fact]
    public async Task can_add_new_comment_to_db()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        var commentOne = new FakeCommentForCreationDto().Generate();

        // Act
        var command = new AddComment.Command(commentOne);
        var commentReturned = await testingServiceScope.SendAsync(command);
        var commentCreated = await testingServiceScope.ExecuteDbContextAsync(db => db.Comments
            .FirstOrDefaultAsync(c => c.Id == commentReturned.Id));

        // Assert
        commentReturned.Text.Should().Be(commentOne.Text);

        commentCreated.Text.Should().Be(commentOne.Text);
    }

    [Fact]
    public async Task must_be_permitted()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        testingServiceScope.SetUserNotPermitted(Permissions.CanAddComment);
        var commentOne = new FakeCommentForCreationDto();

        // Act
        var command = new AddComment.Command(commentOne);
        var act = () => testingServiceScope.SendAsync(command);

        // Assert
        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }
}