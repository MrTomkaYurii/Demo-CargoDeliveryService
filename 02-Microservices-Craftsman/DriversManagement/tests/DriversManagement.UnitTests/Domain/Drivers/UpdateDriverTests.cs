namespace DriversManagement.UnitTests.Domain.Drivers;

using DriversManagement.SharedTestHelpers.Fakes.Driver;
using DriversManagement.Domain.Drivers;
using DriversManagement.Domain.Drivers.DomainEvents;
using Bogus;
using FluentAssertions.Extensions;
using ValidationException = DriversManagement.Exceptions.ValidationException;

public class UpdateDriverTests
{
    private readonly Faker _faker;

    public UpdateDriverTests()
    {
        _faker = new Faker();
    }
    
    [Fact]
    public void can_update_driver()
    {
        // Arrange
        var driver = new FakeDriverBuilder().Build();
        var updatedDriver = new FakeDriverForUpdate().Generate();
        
        // Act
        driver.Update(updatedDriver);

        // Assert
        driver.FirstName.Should().Be(updatedDriver.FirstName);
        driver.LastName.Should().Be(updatedDriver.LastName);
        driver.MiddleName.Should().Be(updatedDriver.MiddleName);
        driver.Gender.Should().Be(updatedDriver.Gender);
        driver.Birthdate.Should().Be(updatedDriver.Birthdate);
        driver.PlaceOfBirth.Should().Be(updatedDriver.PlaceOfBirth);
        driver.Nationality.Should().Be(updatedDriver.Nationality);
        driver.MaritalStatus.Should().Be(updatedDriver.MaritalStatus);
        driver.IdentificationType.Should().Be(updatedDriver.IdentificationType);
        driver.IdentificationNumber.Should().Be(updatedDriver.IdentificationNumber);
        driver.IdentificationExpirationDate.Should().Be(updatedDriver.IdentificationExpirationDate);
        driver.Address.Should().Be(updatedDriver.Address);
        driver.Phone.Should().Be(updatedDriver.Phone);
        driver.Email.Should().Be(updatedDriver.Email);
        driver.DriverLicenseNumber.Should().Be(updatedDriver.DriverLicenseNumber);
        driver.DriverLicenseCategory.Should().Be(updatedDriver.DriverLicenseCategory);
        driver.DriverLicenseIssuingDate.Should().Be(updatedDriver.DriverLicenseIssuingDate);
        driver.DriverLicenseExpirationDate.Should().Be(updatedDriver.DriverLicenseExpirationDate);
        driver.DriverLicenseIssuingAuthority.Should().Be(updatedDriver.DriverLicenseIssuingAuthority);
        driver.EmploymentStatus.Should().Be(updatedDriver.EmploymentStatus);
        driver.EmploymentStartDate.Should().Be(updatedDriver.EmploymentStartDate);
        driver.EmploymentEndDate.Should().Be(updatedDriver.EmploymentEndDate);
    }
    
    [Fact]
    public void queue_domain_event_on_update()
    {
        // Arrange
        var driver = new FakeDriverBuilder().Build();
        var updatedDriver = new FakeDriverForUpdate().Generate();
        driver.DomainEvents.Clear();
        
        // Act
        driver.Update(updatedDriver);

        // Assert
        driver.DomainEvents.Count.Should().Be(1);
        driver.DomainEvents.FirstOrDefault().Should().BeOfType(typeof(DriverUpdated));
    }
}