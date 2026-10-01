namespace DriversBlogManagement.FunctionalTests.FunctionalTests.Drivers;

using DriversBlogManagement.SharedTestHelpers.Fakes.Driver;
using DriversBlogManagement.FunctionalTests.TestUtilities;
using DriversBlogManagement.Domain;
using System.Net;
using System.Threading.Tasks;

public class DeleteDriverTests : TestBase
{
    [Fact]
    public async Task delete_driver_returns_nocontent_when_entity_exists_and_auth_credentials_are_valid()
    {
        // Arrange
        var driver = new FakeDriverBuilder().Build();

        var callingUser = await AddNewSuperAdmin();
        FactoryClient.AddAuth(callingUser.Identifier);
        await InsertAsync(driver);

        // Act
        var route = ApiRoutes.Drivers.Delete(driver.Id);
        var result = await FactoryClient.DeleteRequestAsync(route);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
            
    [Fact]
    public async Task delete_driver_returns_unauthorized_without_valid_token()
    {
        // Arrange
        var driver = new FakeDriverBuilder().Build();

        // Act
        var route = ApiRoutes.Drivers.Delete(driver.Id);
        var result = await FactoryClient.DeleteRequestAsync(route);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
            
    [Fact]
    public async Task delete_driver_returns_forbidden_without_proper_scope()
    {
        // Arrange
        var driver = new FakeDriverBuilder().Build();
        FactoryClient.AddAuth();

        // Act
        var route = ApiRoutes.Drivers.Delete(driver.Id);
        var result = await FactoryClient.DeleteRequestAsync(route);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}