namespace DriversManagement.IntegrationTests.FeatureTests.Drivers;

using DriversManagement.SharedTestHelpers.Fakes.Driver;
using DriversManagement.Domain.Drivers.Features;
using Domain;
using FluentAssertions.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

public class DriverQueryTests : TestBase
{
    [Fact]
    public async Task can_get_existing_driver_with_accurate_props()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        var driverOne = new FakeDriverBuilder().Build();
        await testingServiceScope.InsertAsync(driverOne);

        // Act
        var query = new GetDriver.Query(driverOne.Id);
        var driver = await testingServiceScope.SendAsync(query);

        // Assert
        driver.FirstName.Should().Be(driverOne.FirstName);
        driver.LastName.Should().Be(driverOne.LastName);
        driver.MiddleName.Should().Be(driverOne.MiddleName);
        driver.Gender.Should().Be(driverOne.Gender);
        driver.Birthdate.Should().Be(driverOne.Birthdate);
        driver.PlaceOfBirth.Should().Be(driverOne.PlaceOfBirth);
        driver.Nationality.Should().Be(driverOne.Nationality);
        driver.MaritalStatus.Should().Be(driverOne.MaritalStatus);
        driver.IdentificationType.Should().Be(driverOne.IdentificationType);
        driver.IdentificationNumber.Should().Be(driverOne.IdentificationNumber);
        driver.IdentificationExpirationDate.Should().Be(driverOne.IdentificationExpirationDate);
        driver.Address.Should().Be(driverOne.Address);
        driver.Phone.Should().Be(driverOne.Phone);
        driver.Email.Should().Be(driverOne.Email);
        driver.DriverLicenseNumber.Should().Be(driverOne.DriverLicenseNumber);
        driver.DriverLicenseCategory.Should().Be(driverOne.DriverLicenseCategory);
        driver.DriverLicenseIssuingDate.Should().Be(driverOne.DriverLicenseIssuingDate);
        driver.DriverLicenseExpirationDate.Should().Be(driverOne.DriverLicenseExpirationDate);
        driver.DriverLicenseIssuingAuthority.Should().Be(driverOne.DriverLicenseIssuingAuthority);
        driver.EmploymentStatus.Should().Be(driverOne.EmploymentStatus);
        driver.EmploymentStartDate.Should().Be(driverOne.EmploymentStartDate);
        driver.EmploymentEndDate.Should().Be(driverOne.EmploymentEndDate);
    }

    [Fact]
    public async Task get_driver_throws_notfound_exception_when_record_does_not_exist()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        var badId = Guid.NewGuid();

        // Act
        var query = new GetDriver.Query(badId);
        Func<Task> act = () => testingServiceScope.SendAsync(query);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task must_be_permitted()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        testingServiceScope.SetUserNotPermitted(Permissions.CanReadDriver);

        // Act
        var command = new GetDriver.Query(Guid.NewGuid());
        Func<Task> act = () => testingServiceScope.SendAsync(command);

        // Assert
        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }
}