namespace DriversBlogManagement.FunctionalTests.FunctionalTests.PostAboutDrivers;

using DriversBlogManagement.SharedTestHelpers.Fakes.PostAboutDriver;
using DriversBlogManagement.FunctionalTests.TestUtilities;
using DriversBlogManagement.Domain;
using System.Net;
using System.Threading.Tasks;

public class CreatePostAboutDriverTests : TestBase
{
    [Fact]
    public async Task create_postaboutdriver_returns_created_using_valid_dto_and_valid_auth_credentials()
    {
        // Arrange
        var postAboutDriver = new FakePostAboutDriverForCreationDto().Generate();

        var callingUser = await AddNewSuperAdmin();
        FactoryClient.AddAuth(callingUser.Identifier);

        // Act
        var route = ApiRoutes.PostAboutDrivers.Create;
        var result = await FactoryClient.PostJsonRequestAsync(route, postAboutDriver);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Created);
    }
            
    [Fact]
    public async Task create_postaboutdriver_returns_unauthorized_without_valid_token()
    {
        // Arrange
        var postAboutDriver = new FakePostAboutDriverForCreationDto { }.Generate();

        // Act
        var route = ApiRoutes.PostAboutDrivers.Create;
        var result = await FactoryClient.PostJsonRequestAsync(route, postAboutDriver);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
            
    [Fact]
    public async Task create_postaboutdriver_returns_forbidden_without_proper_scope()
    {
        // Arrange
        var postAboutDriver = new FakePostAboutDriverForCreationDto { }.Generate();
        FactoryClient.AddAuth();

        // Act
        var route = ApiRoutes.PostAboutDrivers.Create;
        var result = await FactoryClient.PostJsonRequestAsync(route, postAboutDriver);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}