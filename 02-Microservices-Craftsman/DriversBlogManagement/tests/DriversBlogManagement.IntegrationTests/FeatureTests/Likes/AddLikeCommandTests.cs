namespace DriversBlogManagement.IntegrationTests.FeatureTests.Likes;

using DriversBlogManagement.SharedTestHelpers.Fakes.Like;
using Domain;
using FluentAssertions.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using DriversBlogManagement.Domain.Likes.Features;

public class AddLikeCommandTests : TestBase
{
    [Fact]
    public async Task can_add_new_like_to_db()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        var likeOne = new FakeLikeForCreationDto().Generate();

        // Act
        var command = new AddLike.Command(likeOne);
        var likeReturned = await testingServiceScope.SendAsync(command);
        var likeCreated = await testingServiceScope.ExecuteDbContextAsync(db => db.Likes
            .FirstOrDefaultAsync(l => l.Id == likeReturned.Id));

        // Assert
        likeReturned.Info.Should().Be(likeOne.Info);

        likeCreated.Info.Should().Be(likeOne.Info);
    }

    [Fact]
    public async Task must_be_permitted()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        testingServiceScope.SetUserNotPermitted(Permissions.CanAddLike);
        var likeOne = new FakeLikeForCreationDto();

        // Act
        var command = new AddLike.Command(likeOne);
        var act = () => testingServiceScope.SendAsync(command);

        // Assert
        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }
}