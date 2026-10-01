namespace DriversBlogManagement.IntegrationTests.FeatureTests.Drivers;

using DriversBlogManagement.SharedTestHelpers.Fakes.Driver;
using DriversBlogManagement.Domain.Drivers.Features;
using Microsoft.EntityFrameworkCore;
using Domain;
using System.Threading.Tasks;

public class DeleteDriverCommandTests : TestBase
{
    [Fact]
    public async Task can_delete_driver_from_db()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        var driver = new FakeDriverBuilder().Build();
        await testingServiceScope.InsertAsync(driver);

        // Act
        var command = new DeleteDriver.Command(driver.Id);
        await testingServiceScope.SendAsync(command);
        var driverResponse = await testingServiceScope
            .ExecuteDbContextAsync(db => db.Drivers
                .CountAsync(d => d.Id == driver.Id));

        // Assert
        driverResponse.Should().Be(0);
    }

    [Fact]
    public async Task delete_driver_throws_notfoundexception_when_record_does_not_exist()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        var badId = Guid.NewGuid();

        // Act
        var command = new DeleteDriver.Command(badId);
        Func<Task> act = () => testingServiceScope.SendAsync(command);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task can_softdelete_driver_from_db()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        var driver = new FakeDriverBuilder().Build();
        await testingServiceScope.InsertAsync(driver);

        // Act
        var command = new DeleteDriver.Command(driver.Id);
        await testingServiceScope.SendAsync(command);
        var deletedDriver = await testingServiceScope.ExecuteDbContextAsync(db => db.Drivers
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == driver.Id));

        // Assert
        deletedDriver?.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task must_be_permitted()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        testingServiceScope.SetUserNotPermitted(Permissions.CanDeleteDriver);

        // Act
        var command = new DeleteDriver.Command(Guid.NewGuid());
        Func<Task> act = () => testingServiceScope.SendAsync(command);

        // Assert
        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }
}