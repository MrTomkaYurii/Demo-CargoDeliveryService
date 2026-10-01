namespace DriversBlogManagement.IntegrationTests.FeatureTests.Comments;

using DriversBlogManagement.Domain.Comments.Dtos;
using DriversBlogManagement.SharedTestHelpers.Fakes.Comment;
using DriversBlogManagement.Domain.Comments.Features;
using Domain;
using System.Threading.Tasks;

public class GetAllCommentsQueryTests : TestBase
{
    
    [Fact]
    public async Task can_get_all_comments()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        var commentOne = new FakeCommentBuilder().Build();
        var commentTwo = new FakeCommentBuilder().Build();

        await testingServiceScope.InsertAsync(commentOne, commentTwo);

        // Act
        var query = new GetAllComments.Query();
        var comments = await testingServiceScope.SendAsync(query);

        // Assert
        comments.Count.Should().BeGreaterThanOrEqualTo(2);
        comments.FirstOrDefault(x => x.Id == commentOne.Id).Should().NotBeNull();
        comments.FirstOrDefault(x => x.Id == commentTwo.Id).Should().NotBeNull();
    }

    [Fact]
    public async Task must_be_permitted()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        testingServiceScope.SetUserNotPermitted(Permissions.CanReadComments);

        // Act
        var query = new GetAllComments.Query();
        Func<Task> act = () => testingServiceScope.SendAsync(command);

        // Assert
        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }
}