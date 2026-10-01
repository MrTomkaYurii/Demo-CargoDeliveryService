namespace DriversBlogManagement.UnitTests.Domain.Drivers;

using DriversBlogManagement.SharedTestHelpers.Fakes.Driver;
using DriversBlogManagement.Domain.Drivers;
using DriversBlogManagement.Domain.Drivers.DomainEvents;
using Bogus;
using FluentAssertions.Extensions;
using ValidationException = DriversBlogManagement.Exceptions.ValidationException;

public class CreateDriverTests
{
    private readonly Faker _faker;

    public CreateDriverTests()
    {
        _faker = new Faker();
    }
    
    [Fact]
    public void can_create_valid_driver()
    {
        // Arrange
        var driverToCreate = new FakeDriverForCreation().Generate();
        
        // Act
        var driver = Driver.Create(driverToCreate);

        // Assert
        driver.FirstName.Should().Be(driverToCreate.FirstName);
        driver.LastName.Should().Be(driverToCreate.LastName);
    }

    [Fact]
    public void queue_domain_event_on_create()
    {
        // Arrange
        var driverToCreate = new FakeDriverForCreation().Generate();
        
        // Act
        var driver = Driver.Create(driverToCreate);

        // Assert
        driver.DomainEvents.Count.Should().Be(1);
        driver.DomainEvents.FirstOrDefault().Should().BeOfType(typeof(DriverCreated));
    }
}