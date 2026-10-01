namespace DriversManagement.IntegrationTests.FeatureTests.Photos;

using DriversManagement.SharedTestHelpers.Fakes.Photo;
using DriversManagement.Domain.Photos.Features;
using Microsoft.EntityFrameworkCore;
using Domain;
using System.Threading.Tasks;

public class DeletePhotoCommandTests : TestBase
{
    [Fact]
    public async Task can_delete_photo_from_db()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        var photo = new FakePhotoBuilder().Build();
        await testingServiceScope.InsertAsync(photo);

        // Act
        var command = new DeletePhoto.Command(photo.Id);
        await testingServiceScope.SendAsync(command);
        var photoResponse = await testingServiceScope
            .ExecuteDbContextAsync(db => db.Photos
                .CountAsync(p => p.Id == photo.Id));

        // Assert
        photoResponse.Should().Be(0);
    }

    [Fact]
    public async Task delete_photo_throws_notfoundexception_when_record_does_not_exist()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        var badId = Guid.NewGuid();

        // Act
        var command = new DeletePhoto.Command(badId);
        Func<Task> act = () => testingServiceScope.SendAsync(command);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task can_softdelete_photo_from_db()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        var photo = new FakePhotoBuilder().Build();
        await testingServiceScope.InsertAsync(photo);

        // Act
        var command = new DeletePhoto.Command(photo.Id);
        await testingServiceScope.SendAsync(command);
        var deletedPhoto = await testingServiceScope.ExecuteDbContextAsync(db => db.Photos
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == photo.Id));

        // Assert
        deletedPhoto?.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task must_be_permitted()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        testingServiceScope.SetUserNotPermitted(Permissions.CanDeletePhoto);

        // Act
        var command = new DeletePhoto.Command(Guid.NewGuid());
        Func<Task> act = () => testingServiceScope.SendAsync(command);

        // Assert
        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }
}