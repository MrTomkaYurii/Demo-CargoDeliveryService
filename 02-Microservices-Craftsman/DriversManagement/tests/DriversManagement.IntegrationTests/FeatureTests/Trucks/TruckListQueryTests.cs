namespace DriversManagement.IntegrationTests.FeatureTests.Trucks;

using DriversManagement.Domain.Trucks.Dtos;
using DriversManagement.SharedTestHelpers.Fakes.Truck;
using DriversManagement.Domain.Trucks.Features;
using Domain;
using System.Threading.Tasks;

public class TruckListQueryTests : TestBase
{
    
    [Fact]
    public async Task can_get_truck_list()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        var truckOne = new FakeTruckBuilder().Build();
        var truckTwo = new FakeTruckBuilder().Build();
        var queryParameters = new TruckParametersDto();

        await testingServiceScope.InsertAsync(truckOne, truckTwo);

        // Act
        var query = new GetTruckList.Query(queryParameters);
        var trucks = await testingServiceScope.SendAsync(query);

        // Assert
        trucks.Count.Should().BeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task must_be_permitted()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        testingServiceScope.SetUserNotPermitted(Permissions.CanReadTrucks);
        var queryParameters = new TruckParametersDto();

        // Act
        var command = new GetTruckList.Query(queryParameters);
        Func<Task> act = () => testingServiceScope.SendAsync(command);

        // Assert
        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }
}