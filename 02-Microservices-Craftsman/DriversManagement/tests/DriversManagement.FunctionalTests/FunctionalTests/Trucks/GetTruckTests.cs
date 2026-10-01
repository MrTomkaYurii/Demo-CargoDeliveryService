namespace DriversManagement.FunctionalTests.FunctionalTests.Trucks;

using DriversManagement.SharedTestHelpers.Fakes.Truck;
using DriversManagement.FunctionalTests.TestUtilities;
using DriversManagement.Domain;
using System.Net;
using System.Threading.Tasks;

public class GetTruckTests : TestBase
{
    [Fact]
    public async Task get_truck_returns_success_when_entity_exists_using_valid_auth_credentials()
    {
        // Arrange
        var truck = new FakeTruckBuilder().Build();

        var callingUser = await AddNewSuperAdmin();
        FactoryClient.AddAuth(callingUser.Identifier);
        await InsertAsync(truck);

        // Act
        var route = ApiRoutes.Trucks.GetRecord(truck.Id);
        var result = await FactoryClient.GetRequestAsync(route);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
    }
            
    [Fact]
    public async Task get_truck_returns_unauthorized_without_valid_token()
    {
        // Arrange
        var truck = new FakeTruckBuilder().Build();

        // Act
        var route = ApiRoutes.Trucks.GetRecord(truck.Id);
        var result = await FactoryClient.GetRequestAsync(route);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
            
    [Fact]
    public async Task get_truck_returns_forbidden_without_proper_scope()
    {
        // Arrange
        var truck = new FakeTruckBuilder().Build();
        FactoryClient.AddAuth();

        // Act
        var route = ApiRoutes.Trucks.GetRecord(truck.Id);
        var result = await FactoryClient.GetRequestAsync(route);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}