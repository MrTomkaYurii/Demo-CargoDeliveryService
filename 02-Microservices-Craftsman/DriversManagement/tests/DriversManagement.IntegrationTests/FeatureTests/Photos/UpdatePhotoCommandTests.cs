namespace DriversManagement.IntegrationTests.FeatureTests.Photos;

using DriversManagement.SharedTestHelpers.Fakes.Photo;
using DriversManagement.Domain.Photos.Dtos;
using DriversManagement.Domain.Photos.Features;
using Domain;
using FluentAssertions.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

public class UpdatePhotoCommandTests : TestBase
{
    [Fact]
    public async Task can_update_existing_photo_in_db()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        var photo = new FakePhotoBuilder().Build();
        await testingServiceScope.InsertAsync(photo);
        var updatedPhotoDto = new FakePhotoForUpdateDto().Generate();

        // Act
        var command = new UpdatePhoto.Command(photo.Id, updatedPhotoDto);
        await testingServiceScope.SendAsync(command);
        var updatedPhoto = await testingServiceScope
            .ExecuteDbContextAsync(db => db.Photos
                .FirstOrDefaultAsync(p => p.Id == photo.Id));

        // Assert
        updatedPhoto.PhotoData.Should().Be(updatedPhotoDto.PhotoData);
        updatedPhoto.ContentType.Should().Be(updatedPhotoDto.ContentType);
        updatedPhoto.FileName.Should().Be(updatedPhotoDto.FileName);
        updatedPhoto.FileSizels.Should().Be(updatedPhotoDto.FileSizels);
    }

    [Fact]
    public async Task must_be_permitted()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        testingServiceScope.SetUserNotPermitted(Permissions.CanUpdatePhoto);
        var photoOne = new FakePhotoForUpdateDto();

        // Act
        var command = new UpdatePhoto.Command(Guid.NewGuid(), photoOne);
        var act = () => testingServiceScope.SendAsync(command);

        // Assert
        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }
}