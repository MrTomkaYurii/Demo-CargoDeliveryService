namespace DriversManagement.FunctionalTests.FunctionalTests.Trucks;

using DriversManagement.SharedTestHelpers.Fakes.Truck;
using DriversManagement.FunctionalTests.TestUtilities;
using DriversManagement.Domain;
using System.Net;
using System.Threading.Tasks;

public class CreateTruckTests : TestBase
{
    [Fact]
    public async Task create_truck_returns_created_using_valid_dto_and_valid_auth_credentials()
    {
        // Arrange
        var truck = new FakeTruckForCreationDto().Generate();

        var callingUser = await AddNewSuperAdmin();
        FactoryClient.AddAuth(callingUser.Identifier);

        // Act
        var route = ApiRoutes.Trucks.Create;
        var result = await FactoryClient.PostJsonRequestAsync(route, truck);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Created);
    }
            
    [Fact]
    public async Task create_truck_returns_unauthorized_without_valid_token()
    {
        // Arrange
        var truck = new FakeTruckForCreationDto { }.Generate();

        // Act
        var route = ApiRoutes.Trucks.Create;
        var result = await FactoryClient.PostJsonRequestAsync(route, truck);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
            
    [Fact]
    public async Task create_truck_returns_forbidden_without_proper_scope()
    {
        // Arrange
        var truck = new FakeTruckForCreationDto { }.Generate();
        FactoryClient.AddAuth();

        // Act
        var route = ApiRoutes.Trucks.Create;
        var result = await FactoryClient.PostJsonRequestAsync(route, truck);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}