namespace DriversManagement.IntegrationTests.FeatureTests.Photos;

using DriversManagement.Domain.Photos.Dtos;
using DriversManagement.SharedTestHelpers.Fakes.Photo;
using DriversManagement.Domain.Photos.Features;
using Domain;
using System.Threading.Tasks;

public class GetAllPhotosQueryTests : TestBase
{
    
    [Fact]
    public async Task can_get_all_photos()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        var photoOne = new FakePhotoBuilder().Build();
        var photoTwo = new FakePhotoBuilder().Build();

        await testingServiceScope.InsertAsync(photoOne, photoTwo);

        // Act
        var query = new GetAllPhotos.Query();
        var photos = await testingServiceScope.SendAsync(query);

        // Assert
        photos.Count.Should().BeGreaterThanOrEqualTo(2);
        photos.FirstOrDefault(x => x.Id == photoOne.Id).Should().NotBeNull();
        photos.FirstOrDefault(x => x.Id == photoTwo.Id).Should().NotBeNull();
    }

    [Fact]
    public async Task must_be_permitted()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        testingServiceScope.SetUserNotPermitted(Permissions.CanReadPhotos);

        // Act
        var query = new GetAllPhotos.Query();
        Func<Task> act = () => testingServiceScope.SendAsync(command);

        // Assert
        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }
}