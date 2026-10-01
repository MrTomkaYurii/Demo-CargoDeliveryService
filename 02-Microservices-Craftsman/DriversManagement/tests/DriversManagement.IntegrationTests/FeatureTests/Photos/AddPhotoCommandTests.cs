namespace DriversManagement.IntegrationTests.FeatureTests.Photos;

using DriversManagement.SharedTestHelpers.Fakes.Photo;
using Domain;
using FluentAssertions.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using DriversManagement.Domain.Photos.Features;

public class AddPhotoCommandTests : TestBase
{
    [Fact]
    public async Task can_add_new_photo_to_db()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        var photoOne = new FakePhotoForCreationDto().Generate();

        // Act
        var command = new AddPhoto.Command(photoOne);
        var photoReturned = await testingServiceScope.SendAsync(command);
        var photoCreated = await testingServiceScope.ExecuteDbContextAsync(db => db.Photos
            .FirstOrDefaultAsync(p => p.Id == photoReturned.Id));

        // Assert
        photoReturned.PhotoData.Should().Be(photoOne.PhotoData);
        photoReturned.ContentType.Should().Be(photoOne.ContentType);
        photoReturned.FileName.Should().Be(photoOne.FileName);
        photoReturned.FileSizels.Should().Be(photoOne.FileSizels);

        photoCreated.PhotoData.Should().Be(photoOne.PhotoData);
        photoCreated.ContentType.Should().Be(photoOne.ContentType);
        photoCreated.FileName.Should().Be(photoOne.FileName);
        photoCreated.FileSizels.Should().Be(photoOne.FileSizels);
    }

    [Fact]
    public async Task must_be_permitted()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        testingServiceScope.SetUserNotPermitted(Permissions.CanAddPhoto);
        var photoOne = new FakePhotoForCreationDto();

        // Act
        var command = new AddPhoto.Command(photoOne);
        var act = () => testingServiceScope.SendAsync(command);

        // Assert
        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }
}