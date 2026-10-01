namespace DriversManagement.FunctionalTests.FunctionalTests.Photos;

using DriversManagement.SharedTestHelpers.Fakes.Photo;
using DriversManagement.FunctionalTests.TestUtilities;
using DriversManagement.Domain;
using System.Net;
using System.Threading.Tasks;

public class DeletePhotoTests : TestBase
{
    [Fact]
    public async Task delete_photo_returns_nocontent_when_entity_exists_and_auth_credentials_are_valid()
    {
        // Arrange
        var photo = new FakePhotoBuilder().Build();

        var callingUser = await AddNewSuperAdmin();
        FactoryClient.AddAuth(callingUser.Identifier);
        await InsertAsync(photo);

        // Act
        var route = ApiRoutes.Photos.Delete(photo.Id);
        var result = await FactoryClient.DeleteRequestAsync(route);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
            
    [Fact]
    public async Task delete_photo_returns_unauthorized_without_valid_token()
    {
        // Arrange
        var photo = new FakePhotoBuilder().Build();

        // Act
        var route = ApiRoutes.Photos.Delete(photo.Id);
        var result = await FactoryClient.DeleteRequestAsync(route);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
            
    [Fact]
    public async Task delete_photo_returns_forbidden_without_proper_scope()
    {
        // Arrange
        var photo = new FakePhotoBuilder().Build();
        FactoryClient.AddAuth();

        // Act
        var route = ApiRoutes.Photos.Delete(photo.Id);
        var result = await FactoryClient.DeleteRequestAsync(route);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}