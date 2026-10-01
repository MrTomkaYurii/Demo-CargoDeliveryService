namespace DriversManagement.UnitTests.Domain.Trucks;

using DriversManagement.SharedTestHelpers.Fakes.Truck;
using DriversManagement.Domain.Trucks;
using DriversManagement.Domain.Trucks.DomainEvents;
using Bogus;
using FluentAssertions.Extensions;
using ValidationException = DriversManagement.Exceptions.ValidationException;

public class CreateTruckTests
{
    private readonly Faker _faker;

    public CreateTruckTests()
    {
        _faker = new Faker();
    }
    
    [Fact]
    public void can_create_valid_truck()
    {
        // Arrange
        var truckToCreate = new FakeTruckForCreation().Generate();
        
        // Act
        var truck = Truck.Create(truckToCreate);

        // Assert
        truck.TruckNumber.Should().Be(truckToCreate.TruckNumber);
        truck.Model.Should().Be(truckToCreate.Model);
        truck.Year.Should().Be(truckToCreate.Year);
        truck.Capacity.Should().Be(truckToCreate.Capacity);
        truck.FuelType.Should().Be(truckToCreate.FuelType);
        truck.RegistrationNumber.Should().Be(truckToCreate.RegistrationNumber);
        truck.VIN.Should().Be(truckToCreate.VIN);
        truck.EngineNumber.Should().Be(truckToCreate.EngineNumber);
        truck.InsurancePolicyNumber.Should().Be(truckToCreate.InsurancePolicyNumber);
        truck.InsuranceInspirationDate.Should().Be(truckToCreate.InsuranceInspirationDate);
    }

    [Fact]
    public void queue_domain_event_on_create()
    {
        // Arrange
        var truckToCreate = new FakeTruckForCreation().Generate();
        
        // Act
        var truck = Truck.Create(truckToCreate);

        // Assert
        truck.DomainEvents.Count.Should().Be(1);
        truck.DomainEvents.FirstOrDefault().Should().BeOfType(typeof(TruckCreated));
    }
}