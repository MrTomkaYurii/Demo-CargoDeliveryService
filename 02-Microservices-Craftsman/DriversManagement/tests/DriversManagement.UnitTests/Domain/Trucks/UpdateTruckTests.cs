namespace DriversManagement.UnitTests.Domain.Trucks;

using DriversManagement.SharedTestHelpers.Fakes.Truck;
using DriversManagement.Domain.Trucks;
using DriversManagement.Domain.Trucks.DomainEvents;
using Bogus;
using FluentAssertions.Extensions;
using ValidationException = DriversManagement.Exceptions.ValidationException;

public class UpdateTruckTests
{
    private readonly Faker _faker;

    public UpdateTruckTests()
    {
        _faker = new Faker();
    }
    
    [Fact]
    public void can_update_truck()
    {
        // Arrange
        var truck = new FakeTruckBuilder().Build();
        var updatedTruck = new FakeTruckForUpdate().Generate();
        
        // Act
        truck.Update(updatedTruck);

        // Assert
        truck.TruckNumber.Should().Be(updatedTruck.TruckNumber);
        truck.Model.Should().Be(updatedTruck.Model);
        truck.Year.Should().Be(updatedTruck.Year);
        truck.Capacity.Should().Be(updatedTruck.Capacity);
        truck.FuelType.Should().Be(updatedTruck.FuelType);
        truck.RegistrationNumber.Should().Be(updatedTruck.RegistrationNumber);
        truck.VIN.Should().Be(updatedTruck.VIN);
        truck.EngineNumber.Should().Be(updatedTruck.EngineNumber);
        truck.InsurancePolicyNumber.Should().Be(updatedTruck.InsurancePolicyNumber);
        truck.InsuranceInspirationDate.Should().Be(updatedTruck.InsuranceInspirationDate);
    }
    
    [Fact]
    public void queue_domain_event_on_update()
    {
        // Arrange
        var truck = new FakeTruckBuilder().Build();
        var updatedTruck = new FakeTruckForUpdate().Generate();
        truck.DomainEvents.Clear();
        
        // Act
        truck.Update(updatedTruck);

        // Assert
        truck.DomainEvents.Count.Should().Be(1);
        truck.DomainEvents.FirstOrDefault().Should().BeOfType(typeof(TruckUpdated));
    }
}