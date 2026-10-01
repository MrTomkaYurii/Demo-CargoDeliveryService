namespace DriversManagement.FunctionalTests.FunctionalTests.Photos;

using DriversManagement.SharedTestHelpers.Fakes.Photo;
using DriversManagement.FunctionalTests.TestUtilities;
using DriversManagement.Domain;
using System.Net;
using System.Threading.Tasks;

public class GetPhotoTests : TestBase
{
    [Fact]
    public async Task get_photo_returns_success_when_entity_exists_using_valid_auth_credentials()
    {
        // Arrange
        var photo = new FakePhotoBuilder().Build();

        var callingUser = await AddNewSuperAdmin();
        FactoryClient.AddAuth(callingUser.Identifier);
        await InsertAsync(photo);

        // Act
        var route = ApiRoutes.Photos.GetRecord(photo.Id);
        var result = await FactoryClient.GetRequestAsync(route);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
    }
            
    [Fact]
    public async Task get_photo_returns_unauthorized_without_valid_token()
    {
        // Arrange
        var photo = new FakePhotoBuilder().Build();

        // Act
        var route = ApiRoutes.Photos.GetRecord(photo.Id);
        var result = await FactoryClient.GetRequestAsync(route);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
            
    [Fact]
    public async Task get_photo_returns_forbidden_without_proper_scope()
    {
        // Arrange
        var photo = new FakePhotoBuilder().Build();
        FactoryClient.AddAuth();

        // Act
        var route = ApiRoutes.Photos.GetRecord(photo.Id);
        var result = await FactoryClient.GetRequestAsync(route);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}