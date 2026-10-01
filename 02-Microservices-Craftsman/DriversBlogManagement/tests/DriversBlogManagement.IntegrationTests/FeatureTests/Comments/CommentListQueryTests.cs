namespace DriversBlogManagement.IntegrationTests.FeatureTests.Comments;

using DriversBlogManagement.Domain.Comments.Dtos;
using DriversBlogManagement.SharedTestHelpers.Fakes.Comment;
using DriversBlogManagement.Domain.Comments.Features;
using Domain;
using System.Threading.Tasks;

public class CommentListQueryTests : TestBase
{
    
    [Fact]
    public async Task can_get_comment_list()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        var commentOne = new FakeCommentBuilder().Build();
        var commentTwo = new FakeCommentBuilder().Build();
        var queryParameters = new CommentParametersDto();

        await testingServiceScope.InsertAsync(commentOne, commentTwo);

        // Act
        var query = new GetCommentList.Query(queryParameters);
        var comments = await testingServiceScope.SendAsync(query);

        // Assert
        comments.Count.Should().BeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task must_be_permitted()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        testingServiceScope.SetUserNotPermitted(Permissions.CanReadComments);
        var queryParameters = new CommentParametersDto();

        // Act
        var command = new GetCommentList.Query(queryParameters);
        Func<Task> act = () => testingServiceScope.SendAsync(command);

        // Assert
        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }
}