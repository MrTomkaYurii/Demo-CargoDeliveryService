namespace DriversBlogManagement.IntegrationTests.FeatureTests.Likes;

using DriversBlogManagement.Domain.Likes.Dtos;
using DriversBlogManagement.SharedTestHelpers.Fakes.Like;
using DriversBlogManagement.Domain.Likes.Features;
using Domain;
using System.Threading.Tasks;

public class GetAllLikesQueryTests : TestBase
{
    
    [Fact]
    public async Task can_get_all_likes()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        var likeOne = new FakeLikeBuilder().Build();
        var likeTwo = new FakeLikeBuilder().Build();

        await testingServiceScope.InsertAsync(likeOne, likeTwo);

        // Act
        var query = new GetAllLikes.Query();
        var likes = await testingServiceScope.SendAsync(query);

        // Assert
        likes.Count.Should().BeGreaterThanOrEqualTo(2);
        likes.FirstOrDefault(x => x.Id == likeOne.Id).Should().NotBeNull();
        likes.FirstOrDefault(x => x.Id == likeTwo.Id).Should().NotBeNull();
    }

    [Fact]
    public async Task must_be_permitted()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        testingServiceScope.SetUserNotPermitted(Permissions.CanReadLikes);

        // Act
        var query = new GetAllLikes.Query();
        Func<Task> act = () => testingServiceScope.SendAsync(command);

        // Assert
        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }
}