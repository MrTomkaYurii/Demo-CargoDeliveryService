namespace DriversManagement.FunctionalTests.FunctionalTests.Photos;

using DriversManagement.SharedTestHelpers.Fakes.Photo;
using DriversManagement.FunctionalTests.TestUtilities;
using DriversManagement.Domain;
using System.Net;
using System.Threading.Tasks;

public class CreatePhotoTests : TestBase
{
    [Fact]
    public async Task create_photo_returns_created_using_valid_dto_and_valid_auth_credentials()
    {
        // Arrange
        var photo = new FakePhotoForCreationDto().Generate();

        var callingUser = await AddNewSuperAdmin();
        FactoryClient.AddAuth(callingUser.Identifier);

        // Act
        var route = ApiRoutes.Photos.Create;
        var result = await FactoryClient.PostJsonRequestAsync(route, photo);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Created);
    }
            
    [Fact]
    public async Task create_photo_returns_unauthorized_without_valid_token()
    {
        // Arrange
        var photo = new FakePhotoForCreationDto { }.Generate();

        // Act
        var route = ApiRoutes.Photos.Create;
        var result = await FactoryClient.PostJsonRequestAsync(route, photo);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
            
    [Fact]
    public async Task create_photo_returns_forbidden_without_proper_scope()
    {
        // Arrange
        var photo = new FakePhotoForCreationDto { }.Generate();
        FactoryClient.AddAuth();

        // Act
        var route = ApiRoutes.Photos.Create;
        var result = await FactoryClient.PostJsonRequestAsync(route, photo);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}