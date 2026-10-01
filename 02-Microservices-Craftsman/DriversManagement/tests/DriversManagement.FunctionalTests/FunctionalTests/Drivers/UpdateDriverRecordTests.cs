namespace DriversManagement.FunctionalTests.FunctionalTests.Drivers;

using DriversManagement.SharedTestHelpers.Fakes.Driver;
using DriversManagement.FunctionalTests.TestUtilities;
using DriversManagement.Domain;
using System.Net;
using System.Threading.Tasks;

public class UpdateDriverRecordTests : TestBase
{
    [Fact]
    public async Task put_driver_returns_nocontent_when_entity_exists_and_auth_credentials_are_valid()
    {
        // Arrange
        var driver = new FakeDriverBuilder().Build();
        var updatedDriverDto = new FakeDriverForUpdateDto().Generate();

        var callingUser = await AddNewSuperAdmin();
        FactoryClient.AddAuth(callingUser.Identifier);
        await InsertAsync(driver);

        // Act
        var route = ApiRoutes.Drivers.Put(driver.Id);
        var result = await FactoryClient.PutJsonRequestAsync(route, updatedDriverDto);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
            
    [Fact]
    public async Task put_driver_returns_unauthorized_without_valid_token()
    {
        // Arrange
        var driver = new FakeDriverBuilder().Build();
        var updatedDriverDto = new FakeDriverForUpdateDto { }.Generate();

        // Act
        var route = ApiRoutes.Drivers.Put(driver.Id);
        var result = await FactoryClient.PutJsonRequestAsync(route, updatedDriverDto);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
            
    [Fact]
    public async Task put_driver_returns_forbidden_without_proper_scope()
    {
        // Arrange
        var driver = new FakeDriverBuilder().Build();
        var updatedDriverDto = new FakeDriverForUpdateDto { }.Generate();
        FactoryClient.AddAuth();

        // Act
        var route = ApiRoutes.Drivers.Put(driver.Id);
        var result = await FactoryClient.PutJsonRequestAsync(route, updatedDriverDto);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}