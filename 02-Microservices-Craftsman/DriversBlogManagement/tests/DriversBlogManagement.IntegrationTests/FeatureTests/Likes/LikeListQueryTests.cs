namespace DriversBlogManagement.IntegrationTests.FeatureTests.Likes;

using DriversBlogManagement.Domain.Likes.Dtos;
using DriversBlogManagement.SharedTestHelpers.Fakes.Like;
using DriversBlogManagement.Domain.Likes.Features;
using Domain;
using System.Threading.Tasks;

public class LikeListQueryTests : TestBase
{
    
    [Fact]
    public async Task can_get_like_list()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        var likeOne = new FakeLikeBuilder().Build();
        var likeTwo = new FakeLikeBuilder().Build();
        var queryParameters = new LikeParametersDto();

        await testingServiceScope.InsertAsync(likeOne, likeTwo);

        // Act
        var query = new GetLikeList.Query(queryParameters);
        var likes = await testingServiceScope.SendAsync(query);

        // Assert
        likes.Count.Should().BeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task must_be_permitted()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        testingServiceScope.SetUserNotPermitted(Permissions.CanReadLikes);
        var queryParameters = new LikeParametersDto();

        // Act
        var command = new GetLikeList.Query(queryParameters);
        Func<Task> act = () => testingServiceScope.SendAsync(command);

        // Assert
        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }
}