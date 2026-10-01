namespace DriversManagement.FunctionalTests.FunctionalTests.Drivers;

using DriversManagement.SharedTestHelpers.Fakes.Driver;
using DriversManagement.FunctionalTests.TestUtilities;
using DriversManagement.Domain;
using System.Net;
using System.Threading.Tasks;

public class GetDriverTests : TestBase
{
    [Fact]
    public async Task get_driver_returns_success_when_entity_exists_using_valid_auth_credentials()
    {
        // Arrange
        var driver = new FakeDriverBuilder().Build();

        var callingUser = await AddNewSuperAdmin();
        FactoryClient.AddAuth(callingUser.Identifier);
        await InsertAsync(driver);

        // Act
        var route = ApiRoutes.Drivers.GetRecord(driver.Id);
        var result = await FactoryClient.GetRequestAsync(route);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
    }
            
    [Fact]
    public async Task get_driver_returns_unauthorized_without_valid_token()
    {
        // Arrange
        var driver = new FakeDriverBuilder().Build();

        // Act
        var route = ApiRoutes.Drivers.GetRecord(driver.Id);
        var result = await FactoryClient.GetRequestAsync(route);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
            
    [Fact]
    public async Task get_driver_returns_forbidden_without_proper_scope()
    {
        // Arrange
        var driver = new FakeDriverBuilder().Build();
        FactoryClient.AddAuth();

        // Act
        var route = ApiRoutes.Drivers.GetRecord(driver.Id);
        var result = await FactoryClient.GetRequestAsync(route);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}