namespace DriversManagement.IntegrationTests.FeatureTests.Drivers;

using DriversManagement.SharedTestHelpers.Fakes.Driver;
using Domain;
using FluentAssertions.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using DriversManagement.Domain.Drivers.Features;

public class AddDriverCommandTests : TestBase
{
    [Fact]
    public async Task can_add_new_driver_to_db()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        var driverOne = new FakeDriverForCreationDto().Generate();

        // Act
        var command = new AddDriver.Command(driverOne);
        var driverReturned = await testingServiceScope.SendAsync(command);
        var driverCreated = await testingServiceScope.ExecuteDbContextAsync(db => db.Drivers
            .FirstOrDefaultAsync(d => d.Id == driverReturned.Id));

        // Assert
        driverReturned.FirstName.Should().Be(driverOne.FirstName);
        driverReturned.LastName.Should().Be(driverOne.LastName);
        driverReturned.MiddleName.Should().Be(driverOne.MiddleName);
        driverReturned.Gender.Should().Be(driverOne.Gender);
        driverReturned.Birthdate.Should().Be(driverOne.Birthdate);
        driverReturned.PlaceOfBirth.Should().Be(driverOne.PlaceOfBirth);
        driverReturned.Nationality.Should().Be(driverOne.Nationality);
        driverReturned.MaritalStatus.Should().Be(driverOne.MaritalStatus);
        driverReturned.IdentificationType.Should().Be(driverOne.IdentificationType);
        driverReturned.IdentificationNumber.Should().Be(driverOne.IdentificationNumber);
        driverReturned.IdentificationExpirationDate.Should().Be(driverOne.IdentificationExpirationDate);
        driverReturned.Address.Should().Be(driverOne.Address);
        driverReturned.Phone.Should().Be(driverOne.Phone);
        driverReturned.Email.Should().Be(driverOne.Email);
        driverReturned.DriverLicenseNumber.Should().Be(driverOne.DriverLicenseNumber);
        driverReturned.DriverLicenseCategory.Should().Be(driverOne.DriverLicenseCategory);
        driverReturned.DriverLicenseIssuingDate.Should().Be(driverOne.DriverLicenseIssuingDate);
        driverReturned.DriverLicenseExpirationDate.Should().Be(driverOne.DriverLicenseExpirationDate);
        driverReturned.DriverLicenseIssuingAuthority.Should().Be(driverOne.DriverLicenseIssuingAuthority);
        driverReturned.EmploymentStatus.Should().Be(driverOne.EmploymentStatus);
        driverReturned.EmploymentStartDate.Should().Be(driverOne.EmploymentStartDate);
        driverReturned.EmploymentEndDate.Should().Be(driverOne.EmploymentEndDate);

        driverCreated.FirstName.Should().Be(driverOne.FirstName);
        driverCreated.LastName.Should().Be(driverOne.LastName);
        driverCreated.MiddleName.Should().Be(driverOne.MiddleName);
        driverCreated.Gender.Should().Be(driverOne.Gender);
        driverCreated.Birthdate.Should().Be(driverOne.Birthdate);
        driverCreated.PlaceOfBirth.Should().Be(driverOne.PlaceOfBirth);
        driverCreated.Nationality.Should().Be(driverOne.Nationality);
        driverCreated.MaritalStatus.Should().Be(driverOne.MaritalStatus);
        driverCreated.IdentificationType.Should().Be(driverOne.IdentificationType);
        driverCreated.IdentificationNumber.Should().Be(driverOne.IdentificationNumber);
        driverCreated.IdentificationExpirationDate.Should().Be(driverOne.IdentificationExpirationDate);
        driverCreated.Address.Should().Be(driverOne.Address);
        driverCreated.Phone.Should().Be(driverOne.Phone);
        driverCreated.Email.Should().Be(driverOne.Email);
        driverCreated.DriverLicenseNumber.Should().Be(driverOne.DriverLicenseNumber);
        driverCreated.DriverLicenseCategory.Should().Be(driverOne.DriverLicenseCategory);
        driverCreated.DriverLicenseIssuingDate.Should().Be(driverOne.DriverLicenseIssuingDate);
        driverCreated.DriverLicenseExpirationDate.Should().Be(driverOne.DriverLicenseExpirationDate);
        driverCreated.DriverLicenseIssuingAuthority.Should().Be(driverOne.DriverLicenseIssuingAuthority);
        driverCreated.EmploymentStatus.Should().Be(driverOne.EmploymentStatus);
        driverCreated.EmploymentStartDate.Should().Be(driverOne.EmploymentStartDate);
        driverCreated.EmploymentEndDate.Should().Be(driverOne.EmploymentEndDate);
    }

    [Fact]
    public async Task must_be_permitted()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        testingServiceScope.SetUserNotPermitted(Permissions.CanAddDriver);
        var driverOne = new FakeDriverForCreationDto();

        // Act
        var command = new AddDriver.Command(driverOne);
        var act = () => testingServiceScope.SendAsync(command);

        // Assert
        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }
}