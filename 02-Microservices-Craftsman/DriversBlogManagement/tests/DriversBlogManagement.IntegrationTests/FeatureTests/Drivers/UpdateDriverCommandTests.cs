namespace DriversBlogManagement.IntegrationTests.FeatureTests.Drivers;

using DriversBlogManagement.SharedTestHelpers.Fakes.Driver;
using DriversBlogManagement.Domain.Drivers.Dtos;
using DriversBlogManagement.Domain.Drivers.Features;
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