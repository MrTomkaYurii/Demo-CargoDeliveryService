namespace DriversManagement.FunctionalTests.FunctionalTests.Photos;

using DriversManagement.SharedTestHelpers.Fakes.Photo;
using DriversManagement.FunctionalTests.TestUtilities;
using DriversManagement.Domain;
using System.Net;
using System.Threading.Tasks;

public class UpdatePhotoRecordTests : TestBase
{
    [Fact]
    public async Task put_photo_returns_nocontent_when_entity_exists_and_auth_credentials_are_valid()
    {
        // Arrange
        var photo = new FakePhotoBuilder().Build();
        var updatedPhotoDto = new FakePhotoForUpdateDto().Generate();

        var callingUser = await AddNewSuperAdmin();
        FactoryClient.AddAuth(callingUser.Identifier);
        await InsertAsync(photo);

        // Act
        var route = ApiRoutes.Photos.Put(photo.Id);
        var result = await FactoryClient.PutJsonRequestAsync(route, updatedPhotoDto);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
            
    [Fact]
    public async Task put_photo_returns_unauthorized_without_valid_token()
    {
        // Arrange
        var photo = new FakePhotoBuilder().Build();
        var updatedPhotoDto = new FakePhotoForUpdateDto { }.Generate();

        // Act
        var route = ApiRoutes.Photos.Put(photo.Id);
        var result = await FactoryClient.PutJsonRequestAsync(route, updatedPhotoDto);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
            
    [Fact]
    public async Task put_photo_returns_forbidden_without_proper_scope()
    {
        // Arrange
        var photo = new FakePhotoBuilder().Build();
        var updatedPhotoDto = new FakePhotoForUpdateDto { }.Generate();
        FactoryClient.AddAuth();

        // Act
        var route = ApiRoutes.Photos.Put(photo.Id);
        var result = await FactoryClient.PutJsonRequestAsync(route, updatedPhotoDto);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}