namespace DriversBlogManagement.IntegrationTests.FeatureTests.Drivers;

using DriversBlogManagement.SharedTestHelpers.Fakes.Driver;
using DriversBlogManagement.Domain.Drivers.Features;
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