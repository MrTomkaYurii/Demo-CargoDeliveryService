namespace DriversBlogManagement.IntegrationTests.FeatureTests.Likes;

using DriversBlogManagement.SharedTestHelpers.Fakes.Like;
using DriversBlogManagement.Domain.Likes.Dtos;
using DriversBlogManagement.Domain.Likes.Features;
using Domain;
using FluentAssertions.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

public class UpdateLikeCommandTests : TestBase
{
    [Fact]
    public async Task can_update_existing_like_in_db()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        var like = new FakeLikeBuilder().Build();
        await testingServiceScope.InsertAsync(like);
        var updatedLikeDto = new FakeLikeForUpdateDto().Generate();

        // Act
        var command = new UpdateLike.Command(like.Id, updatedLikeDto);
        await testingServiceScope.SendAsync(command);
        var updatedLike = await testingServiceScope
            .ExecuteDbContextAsync(db => db.Likes
                .FirstOrDefaultAsync(l => l.Id == like.Id));

        // Assert
        updatedLike.Info.Should().Be(updatedLikeDto.Info);
    }

    [Fact]
    public async Task must_be_permitted()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        testingServiceScope.SetUserNotPermitted(Permissions.CanUpdateLike);
        var likeOne = new FakeLikeForUpdateDto();

        // Act
        var command = new UpdateLike.Command(Guid.NewGuid(), likeOne);
        var act = () => testingServiceScope.SendAsync(command);

        // Assert
        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }
}