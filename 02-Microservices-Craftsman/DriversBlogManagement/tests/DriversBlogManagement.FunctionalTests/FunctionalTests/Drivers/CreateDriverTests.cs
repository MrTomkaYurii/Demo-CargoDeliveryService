namespace DriversBlogManagement.FunctionalTests.FunctionalTests.Drivers;

using DriversBlogManagement.SharedTestHelpers.Fakes.Driver;
using DriversBlogManagement.FunctionalTests.TestUtilities;
using DriversBlogManagement.Domain;
using System.Net;
using System.Threading.Tasks;

public class CreateDriverTests : TestBase
{
    [Fact]
    public async Task create_driver_returns_created_using_valid_dto_and_valid_auth_credentials()
    {
        // Arrange
        var driver = new FakeDriverForCreationDto().Generate();

        var callingUser = await AddNewSuperAdmin();
        FactoryClient.AddAuth(callingUser.Identifier);

        // Act
        var route = ApiRoutes.Drivers.Create;
        var result = await FactoryClient.PostJsonRequestAsync(route, driver);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Created);
    }
            
    [Fact]
    public async Task create_driver_returns_unauthorized_without_valid_token()
    {
        // Arrange
        var driver = new FakeDriverForCreationDto { }.Generate();

        // Act
        var route = ApiRoutes.Drivers.Create;
        var result = await FactoryClient.PostJsonRequestAsync(route, driver);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
            
    [Fact]
    public async Task create_driver_returns_forbidden_without_proper_scope()
    {
        // Arrange
        var driver = new FakeDriverForCreationDto { }.Generate();
        FactoryClient.AddAuth();

        // Act
        var route = ApiRoutes.Drivers.Create;
        var result = await FactoryClient.PostJsonRequestAsync(route, driver);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}