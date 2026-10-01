namespace DriversBlogManagement.IntegrationTests.FeatureTests.Comments;

using DriversBlogManagement.SharedTestHelpers.Fakes.Comment;
using DriversBlogManagement.Domain.Comments.Dtos;
using DriversBlogManagement.Domain.Comments.Features;
using Domain;
using FluentAssertions.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

public class UpdateCommentCommandTests : TestBase
{
    [Fact]
    public async Task can_update_existing_comment_in_db()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        var comment = new FakeCommentBuilder().Build();
        await testingServiceScope.InsertAsync(comment);
        var updatedCommentDto = new FakeCommentForUpdateDto().Generate();

        // Act
        var command = new UpdateComment.Command(comment.Id, updatedCommentDto);
        await testingServiceScope.SendAsync(command);
        var updatedComment = await testingServiceScope
            .ExecuteDbContextAsync(db => db.Comments
                .FirstOrDefaultAsync(c => c.Id == comment.Id));

        // Assert
        updatedComment.Text.Should().Be(updatedCommentDto.Text);
    }

    [Fact]
    public async Task must_be_permitted()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        testingServiceScope.SetUserNotPermitted(Permissions.CanUpdateComment);
        var commentOne = new FakeCommentForUpdateDto();

        // Act
        var command = new UpdateComment.Command(Guid.NewGuid(), commentOne);
        var act = () => testingServiceScope.SendAsync(command);

        // Assert
        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }
}