namespace DriversManagement.IntegrationTests.FeatureTests.Trucks;

using DriversManagement.SharedTestHelpers.Fakes.Truck;
using Domain;
using FluentAssertions.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using DriversManagement.Domain.Trucks.Features;

public class AddTruckCommandTests : TestBase
{
    [Fact]
    public async Task can_add_new_truck_to_db()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        var truckOne = new FakeTruckForCreationDto().Generate();

        // Act
        var command = new AddTruck.Command(truckOne);
        var truckReturned = await testingServiceScope.SendAsync(command);
        var truckCreated = await testingServiceScope.ExecuteDbContextAsync(db => db.Trucks
            .FirstOrDefaultAsync(t => t.Id == truckReturned.Id));

        // Assert
        truckReturned.TruckNumber.Should().Be(truckOne.TruckNumber);
        truckReturned.Model.Should().Be(truckOne.Model);
        truckReturned.Year.Should().Be(truckOne.Year);
        truckReturned.Capacity.Should().Be(truckOne.Capacity);
        truckReturned.FuelType.Should().Be(truckOne.FuelType);
        truckReturned.RegistrationNumber.Should().Be(truckOne.RegistrationNumber);
        truckReturned.VIN.Should().Be(truckOne.VIN);
        truckReturned.EngineNumber.Should().Be(truckOne.EngineNumber);
        truckReturned.InsurancePolicyNumber.Should().Be(truckOne.InsurancePolicyNumber);
        truckReturned.InsuranceInspirationDate.Should().Be(truckOne.InsuranceInspirationDate);

        truckCreated.TruckNumber.Should().Be(truckOne.TruckNumber);
        truckCreated.Model.Should().Be(truckOne.Model);
        truckCreated.Year.Should().Be(truckOne.Year);
        truckCreated.Capacity.Should().Be(truckOne.Capacity);
        truckCreated.FuelType.Should().Be(truckOne.FuelType);
        truckCreated.RegistrationNumber.Should().Be(truckOne.RegistrationNumber);
        truckCreated.VIN.Should().Be(truckOne.VIN);
        truckCreated.EngineNumber.Should().Be(truckOne.EngineNumber);
        truckCreated.InsurancePolicyNumber.Should().Be(truckOne.InsurancePolicyNumber);
        truckCreated.InsuranceInspirationDate.Should().Be(truckOne.InsuranceInspirationDate);
    }

    [Fact]
    public async Task must_be_permitted()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        testingServiceScope.SetUserNotPermitted(Permissions.CanAddTruck);
        var truckOne = new FakeTruckForCreationDto();

        // Act
        var command = new AddTruck.Command(truckOne);
        var act = () => testingServiceScope.SendAsync(command);

        // Assert
        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }
}