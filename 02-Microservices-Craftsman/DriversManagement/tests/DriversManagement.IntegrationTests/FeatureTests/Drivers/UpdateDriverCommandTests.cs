namespace DriversManagement.IntegrationTests.FeatureTests.Drivers;

using DriversManagement.SharedTestHelpers.Fakes.Driver;
using DriversManagement.Domain.Drivers.Dtos;
using DriversManagement.Domain.Drivers.Features;
using Domain;
using FluentAssertions.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

public class UpdateDriverCommandTests : TestBase
{
    [Fact]
    public async Task can_update_existing_driver_in_db()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        var driver = new FakeDriverBuilder().Build();
        await testingServiceScope.InsertAsync(driver);
        var updatedDriverDto = new FakeDriverForUpdateDto().Generate();

        // Act
        var command = new UpdateDriver.Command(driver.Id, updatedDriverDto);
        await testingServiceScope.SendAsync(command);
        var updatedDriver = await testingServiceScope
            .ExecuteDbContextAsync(db => db.Drivers
                .FirstOrDefaultAsync(d => d.Id == driver.Id));

        // Assert
        updatedDriver.FirstName.Should().Be(updatedDriverDto.FirstName);
        updatedDriver.LastName.Should().Be(updatedDriverDto.LastName);
        updatedDriver.MiddleName.Should().Be(updatedDriverDto.MiddleName);
        updatedDriver.Gender.Should().Be(updatedDriverDto.Gender);
        updatedDriver.Birthdate.Should().Be(updatedDriverDto.Birthdate);
        updatedDriver.PlaceOfBirth.Should().Be(updatedDriverDto.PlaceOfBirth);
        updatedDriver.Nationality.Should().Be(updatedDriverDto.Nationality);
        updatedDriver.MaritalStatus.Should().Be(updatedDriverDto.MaritalStatus);
        updatedDriver.IdentificationType.Should().Be(updatedDriverDto.IdentificationType);
        updatedDriver.IdentificationNumber.Should().Be(updatedDriverDto.IdentificationNumber);
        updatedDriver.IdentificationExpirationDate.Should().Be(updatedDriverDto.IdentificationExpirationDate);
        updatedDriver.Address.Should().Be(updatedDriverDto.Address);
        updatedDriver.Phone.Should().Be(updatedDriverDto.Phone);
        updatedDriver.Email.Should().Be(updatedDriverDto.Email);
        updatedDriver.DriverLicenseNumber.Should().Be(updatedDriverDto.DriverLicenseNumber);
        updatedDriver.DriverLicenseCategory.Should().Be(updatedDriverDto.DriverLicenseCategory);
        updatedDriver.DriverLicenseIssuingDate.Should().Be(updatedDriverDto.DriverLicenseIssuingDate);
        updatedDriver.DriverLicenseExpirationDate.Should().Be(updatedDriverDto.DriverLicenseExpirationDate);
        updatedDriver.DriverLicenseIssuingAuthority.Should().Be(updatedDriverDto.DriverLicenseIssuingAuthority);
        updatedDriver.EmploymentStatus.Should().Be(updatedDriverDto.EmploymentStatus);
        updatedDriver.EmploymentStartDate.Should().Be(updatedDriverDto.EmploymentStartDate);
        updatedDriver.EmploymentEndDate.Should().Be(updatedDriverDto.EmploymentEndDate);
    }

    [Fact]
    public async Task must_be_permitted()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        testingServiceScope.SetUserNotPermitted(Permissions.CanUpdateDriver);
        var driverOne = new FakeDriverForUpdateDto();

        // Act
        var command = new UpdateDriver.Command(Guid.NewGuid(), driverOne);
        var act = () => testingServiceScope.SendAsync(command);

        // Assert
        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }
}