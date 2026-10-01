namespace DriversManagement.UnitTests.Domain.Drivers;

using DriversManagement.SharedTestHelpers.Fakes.Driver;
using DriversManagement.Domain.Drivers;
using DriversManagement.Domain.Drivers.DomainEvents;
using Bogus;
using FluentAssertions.Extensions;
using ValidationException = DriversManagement.Exceptions.ValidationException;

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
        driver.MiddleName.Should().Be(driverToCreate.MiddleName);
        driver.Gender.Should().Be(driverToCreate.Gender);
        driver.Birthdate.Should().Be(driverToCreate.Birthdate);
        driver.PlaceOfBirth.Should().Be(driverToCreate.PlaceOfBirth);
        driver.Nationality.Should().Be(driverToCreate.Nationality);
        driver.MaritalStatus.Should().Be(driverToCreate.MaritalStatus);
        driver.IdentificationType.Should().Be(driverToCreate.IdentificationType);
        driver.IdentificationNumber.Should().Be(driverToCreate.IdentificationNumber);
        driver.IdentificationExpirationDate.Should().Be(driverToCreate.IdentificationExpirationDate);
        driver.Address.Should().Be(driverToCreate.Address);
        driver.Phone.Should().Be(driverToCreate.Phone);
        driver.Email.Should().Be(driverToCreate.Email);
        driver.DriverLicenseNumber.Should().Be(driverToCreate.DriverLicenseNumber);
        driver.DriverLicenseCategory.Should().Be(driverToCreate.DriverLicenseCategory);
        driver.DriverLicenseIssuingDate.Should().Be(driverToCreate.DriverLicenseIssuingDate);
        driver.DriverLicenseExpirationDate.Should().Be(driverToCreate.DriverLicenseExpirationDate);
        driver.DriverLicenseIssuingAuthority.Should().Be(driverToCreate.DriverLicenseIssuingAuthority);
        driver.EmploymentStatus.Should().Be(driverToCreate.EmploymentStatus);
        driver.EmploymentStartDate.Should().Be(driverToCreate.EmploymentStartDate);
        driver.EmploymentEndDate.Should().Be(driverToCreate.EmploymentEndDate);
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