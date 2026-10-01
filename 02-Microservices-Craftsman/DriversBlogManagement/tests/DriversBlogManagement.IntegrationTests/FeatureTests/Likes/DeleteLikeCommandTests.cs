namespace DriversBlogManagement.IntegrationTests.FeatureTests.Likes;

using DriversBlogManagement.SharedTestHelpers.Fakes.Like;
using DriversBlogManagement.Domain.Likes.Features;
using Microsoft.EntityFrameworkCore;
using Domain;
using System.Threading.Tasks;

public class DeleteLikeCommandTests : TestBase
{
    [Fact]
    public async Task can_delete_like_from_db()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        var like = new FakeLikeBuilder().Build();
        await testingServiceScope.InsertAsync(like);

        // Act
        var command = new DeleteLike.Command(like.Id);
        await testingServiceScope.SendAsync(command);
        var likeResponse = await testingServiceScope
            .ExecuteDbContextAsync(db => db.Likes
                .CountAsync(l => l.Id == like.Id));

        // Assert
        likeResponse.Should().Be(0);
    }

    [Fact]
    public async Task delete_like_throws_notfoundexception_when_record_does_not_exist()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        var badId = Guid.NewGuid();

        // Act
        var command = new DeleteLike.Command(badId);
        Func<Task> act = () => testingServiceScope.SendAsync(command);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task can_softdelete_like_from_db()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        var like = new FakeLikeBuilder().Build();
        await testingServiceScope.InsertAsync(like);

        // Act
        var command = new DeleteLike.Command(like.Id);
        await testingServiceScope.SendAsync(command);
        var deletedLike = await testingServiceScope.ExecuteDbContextAsync(db => db.Likes
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == like.Id));

        // Assert
        deletedLike?.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task must_be_permitted()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        testingServiceScope.SetUserNotPermitted(Permissions.CanDeleteLike);

        // Act
        var command = new DeleteLike.Command(Guid.NewGuid());
        Func<Task> act = () => testingServiceScope.SendAsync(command);

        // Assert
        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }
}