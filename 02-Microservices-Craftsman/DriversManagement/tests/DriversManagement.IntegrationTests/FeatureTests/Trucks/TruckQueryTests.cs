namespace DriversManagement.IntegrationTests.FeatureTests.Trucks;

using DriversManagement.SharedTestHelpers.Fakes.Truck;
using DriversManagement.Domain.Trucks.Features;
using Domain;
using FluentAssertions.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

public class TruckQueryTests : TestBase
{
    [Fact]
    public async Task can_get_existing_truck_with_accurate_props()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        var truckOne = new FakeTruckBuilder().Build();
        await testingServiceScope.InsertAsync(truckOne);

        // Act
        var query = new GetTruck.Query(truckOne.Id);
        var truck = await testingServiceScope.SendAsync(query);

        // Assert
        truck.TruckNumber.Should().Be(truckOne.TruckNumber);
        truck.Model.Should().Be(truckOne.Model);
        truck.Year.Should().Be(truckOne.Year);
        truck.Capacity.Should().Be(truckOne.Capacity);
        truck.FuelType.Should().Be(truckOne.FuelType);
        truck.RegistrationNumber.Should().Be(truckOne.RegistrationNumber);
        truck.VIN.Should().Be(truckOne.VIN);
        truck.EngineNumber.Should().Be(truckOne.EngineNumber);
        truck.InsurancePolicyNumber.Should().Be(truckOne.InsurancePolicyNumber);
        truck.InsuranceInspirationDate.Should().Be(truckOne.InsuranceInspirationDate);
    }

    [Fact]
    public async Task get_truck_throws_notfound_exception_when_record_does_not_exist()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        var badId = Guid.NewGuid();

        // Act
        var query = new GetTruck.Query(badId);
        Func<Task> act = () => testingServiceScope.SendAsync(query);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task must_be_permitted()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        testingServiceScope.SetUserNotPermitted(Permissions.CanReadTruck);

        // Act
        var command = new GetTruck.Query(Guid.NewGuid());
        Func<Task> act = () => testingServiceScope.SendAsync(command);

        // Assert
        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }
}