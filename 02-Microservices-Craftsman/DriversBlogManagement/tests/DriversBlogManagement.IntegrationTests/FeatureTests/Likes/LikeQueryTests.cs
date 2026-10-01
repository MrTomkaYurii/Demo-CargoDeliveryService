namespace DriversBlogManagement.IntegrationTests.FeatureTests.Likes;

using DriversBlogManagement.SharedTestHelpers.Fakes.Like;
using DriversBlogManagement.Domain.Likes.Features;
using Domain;
using FluentAssertions.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

public class LikeQueryTests : TestBase
{
    [Fact]
    public async Task can_get_existing_like_with_accurate_props()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        var likeOne = new FakeLikeBuilder().Build();
        await testingServiceScope.InsertAsync(likeOne);

        // Act
        var query = new GetLike.Query(likeOne.Id);
        var like = await testingServiceScope.SendAsync(query);

        // Assert
        like.Info.Should().Be(likeOne.Info);
    }

    [Fact]
    public async Task get_like_throws_notfound_exception_when_record_does_not_exist()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        var badId = Guid.NewGuid();

        // Act
        var query = new GetLike.Query(badId);
        Func<Task> act = () => testingServiceScope.SendAsync(query);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task must_be_permitted()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        testingServiceScope.SetUserNotPermitted(Permissions.CanReadLike);

        // Act
        var command = new GetLike.Query(Guid.NewGuid());
        Func<Task> act = () => testingServiceScope.SendAsync(command);

        // Assert
        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }
}