namespace DriversManagement.IntegrationTests.FeatureTests.Trucks;

using DriversManagement.SharedTestHelpers.Fakes.Truck;
using DriversManagement.Domain.Trucks.Dtos;
using DriversManagement.Domain.Trucks.Features;
using Domain;
using FluentAssertions.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

public class UpdateTruckCommandTests : TestBase
{
    [Fact]
    public async Task can_update_existing_truck_in_db()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        var truck = new FakeTruckBuilder().Build();
        await testingServiceScope.InsertAsync(truck);
        var updatedTruckDto = new FakeTruckForUpdateDto().Generate();

        // Act
        var command = new UpdateTruck.Command(truck.Id, updatedTruckDto);
        await testingServiceScope.SendAsync(command);
        var updatedTruck = await testingServiceScope
            .ExecuteDbContextAsync(db => db.Trucks
                .FirstOrDefaultAsync(t => t.Id == truck.Id));

        // Assert
        updatedTruck.TruckNumber.Should().Be(updatedTruckDto.TruckNumber);
        updatedTruck.Model.Should().Be(updatedTruckDto.Model);
        updatedTruck.Year.Should().Be(updatedTruckDto.Year);
        updatedTruck.Capacity.Should().Be(updatedTruckDto.Capacity);
        updatedTruck.FuelType.Should().Be(updatedTruckDto.FuelType);
        updatedTruck.RegistrationNumber.Should().Be(updatedTruckDto.RegistrationNumber);
        updatedTruck.VIN.Should().Be(updatedTruckDto.VIN);
        updatedTruck.EngineNumber.Should().Be(updatedTruckDto.EngineNumber);
        updatedTruck.InsurancePolicyNumber.Should().Be(updatedTruckDto.InsurancePolicyNumber);
        updatedTruck.InsuranceInspirationDate.Should().Be(updatedTruckDto.InsuranceInspirationDate);
    }

    [Fact]
    public async Task must_be_permitted()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        testingServiceScope.SetUserNotPermitted(Permissions.CanUpdateTruck);
        var truckOne = new FakeTruckForUpdateDto();

        // Act
        var command = new UpdateTruck.Command(Guid.NewGuid(), truckOne);
        var act = () => testingServiceScope.SendAsync(command);

        // Assert
        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }
}