namespace DriversManagement.IntegrationTests.FeatureTests.Photos;

using DriversManagement.Domain.Photos.Dtos;
using DriversManagement.SharedTestHelpers.Fakes.Photo;
using DriversManagement.Domain.Photos.Features;
using Domain;
using System.Threading.Tasks;

public class PhotoListQueryTests : TestBase
{
    
    [Fact]
    public async Task can_get_photo_list()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        var photoOne = new FakePhotoBuilder().Build();
        var photoTwo = new FakePhotoBuilder().Build();
        var queryParameters = new PhotoParametersDto();

        await testingServiceScope.InsertAsync(photoOne, photoTwo);

        // Act
        var query = new GetPhotoList.Query(queryParameters);
        var photos = await testingServiceScope.SendAsync(query);

        // Assert
        photos.Count.Should().BeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task must_be_permitted()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        testingServiceScope.SetUserNotPermitted(Permissions.CanReadPhotos);
        var queryParameters = new PhotoParametersDto();

        // Act
        var command = new GetPhotoList.Query(queryParameters);
        Func<Task> act = () => testingServiceScope.SendAsync(command);

        // Assert
        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }
}