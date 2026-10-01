namespace DriversManagement.IntegrationTests.FeatureTests.Trucks;

using DriversManagement.Domain.Trucks.Dtos;
using DriversManagement.SharedTestHelpers.Fakes.Truck;
using DriversManagement.Domain.Trucks.Features;
using Domain;
using System.Threading.Tasks;

public class GetAllTrucksQueryTests : TestBase
{
    
    [Fact]
    public async Task can_get_all_trucks()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        var truckOne = new FakeTruckBuilder().Build();
        var truckTwo = new FakeTruckBuilder().Build();

        await testingServiceScope.InsertAsync(truckOne, truckTwo);

        // Act
        var query = new GetAllTrucks.Query();
        var trucks = await testingServiceScope.SendAsync(query);

        // Assert
        trucks.Count.Should().BeGreaterThanOrEqualTo(2);
        trucks.FirstOrDefault(x => x.Id == truckOne.Id).Should().NotBeNull();
        trucks.FirstOrDefault(x => x.Id == truckTwo.Id).Should().NotBeNull();
    }

    [Fact]
    public async Task must_be_permitted()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        testingServiceScope.SetUserNotPermitted(Permissions.CanReadTrucks);

        // Act
        var query = new GetAllTrucks.Query();
        Func<Task> act = () => testingServiceScope.SendAsync(command);

        // Assert
        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }
}