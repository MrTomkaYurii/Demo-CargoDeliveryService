namespace DriversBlogManagement.IntegrationTests.FeatureTests.Drivers;

using DriversBlogManagement.SharedTestHelpers.Fakes.Driver;
using Domain;
using FluentAssertions.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using DriversBlogManagement.Domain.Drivers.Features;

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

        driverCreated.FirstName.Should().Be(driverOne.FirstName);
        driverCreated.LastName.Should().Be(driverOne.LastName);
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