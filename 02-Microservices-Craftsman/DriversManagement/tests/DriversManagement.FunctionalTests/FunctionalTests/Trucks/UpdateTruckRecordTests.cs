namespace DriversManagement.FunctionalTests.FunctionalTests.Trucks;

using DriversManagement.SharedTestHelpers.Fakes.Truck;
using DriversManagement.FunctionalTests.TestUtilities;
using DriversManagement.Domain;
using System.Net;
using System.Threading.Tasks;

public class UpdateTruckRecordTests : TestBase
{
    [Fact]
    public async Task put_truck_returns_nocontent_when_entity_exists_and_auth_credentials_are_valid()
    {
        // Arrange
        var truck = new FakeTruckBuilder().Build();
        var updatedTruckDto = new FakeTruckForUpdateDto().Generate();

        var callingUser = await AddNewSuperAdmin();
        FactoryClient.AddAuth(callingUser.Identifier);
        await InsertAsync(truck);

        // Act
        var route = ApiRoutes.Trucks.Put(truck.Id);
        var result = await FactoryClient.PutJsonRequestAsync(route, updatedTruckDto);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
            
    [Fact]
    public async Task put_truck_returns_unauthorized_without_valid_token()
    {
        // Arrange
        var truck = new FakeTruckBuilder().Build();
        var updatedTruckDto = new FakeTruckForUpdateDto { }.Generate();

        // Act
        var route = ApiRoutes.Trucks.Put(truck.Id);
        var result = await FactoryClient.PutJsonRequestAsync(route, updatedTruckDto);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
            
    [Fact]
    public async Task put_truck_returns_forbidden_without_proper_scope()
    {
        // Arrange
        var truck = new FakeTruckBuilder().Build();
        var updatedTruckDto = new FakeTruckForUpdateDto { }.Generate();
        FactoryClient.AddAuth();

        // Act
        var route = ApiRoutes.Trucks.Put(truck.Id);
        var result = await FactoryClient.PutJsonRequestAsync(route, updatedTruckDto);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}