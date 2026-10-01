namespace DriversManagement.IntegrationTests.FeatureTests.Photos;

using DriversManagement.SharedTestHelpers.Fakes.Photo;
using DriversManagement.Domain.Photos.Features;
using Domain;
using FluentAssertions.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

public class PhotoQueryTests : TestBase
{
    [Fact]
    public async Task can_get_existing_photo_with_accurate_props()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        var photoOne = new FakePhotoBuilder().Build();
        await testingServiceScope.InsertAsync(photoOne);

        // Act
        var query = new GetPhoto.Query(photoOne.Id);
        var photo = await testingServiceScope.SendAsync(query);

        // Assert
        photo.PhotoData.Should().Be(photoOne.PhotoData);
        photo.ContentType.Should().Be(photoOne.ContentType);
        photo.FileName.Should().Be(photoOne.FileName);
        photo.FileSizels.Should().Be(photoOne.FileSizels);
    }

    [Fact]
    public async Task get_photo_throws_notfound_exception_when_record_does_not_exist()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        var badId = Guid.NewGuid();

        // Act
        var query = new GetPhoto.Query(badId);
        Func<Task> act = () => testingServiceScope.SendAsync(query);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task must_be_permitted()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        testingServiceScope.SetUserNotPermitted(Permissions.CanReadPhoto);

        // Act
        var command = new GetPhoto.Query(Guid.NewGuid());
        Func<Task> act = () => testingServiceScope.SendAsync(command);

        // Assert
        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }
}