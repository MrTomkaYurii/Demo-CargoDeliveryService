namespace DriversBlogManagement.IntegrationTests.FeatureTests.Drivers;

using DriversBlogManagement.Domain.Drivers.Dtos;
using DriversBlogManagement.SharedTestHelpers.Fakes.Driver;
using DriversBlogManagement.Domain.Drivers.Features;
using Domain;
using System.Threading.Tasks;

public class GetAllDriversQueryTests : TestBase
{
    
    [Fact]
    public async Task can_get_all_drivers()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        var driverOne = new FakeDriverBuilder().Build();
        var driverTwo = new FakeDriverBuilder().Build();

        await testingServiceScope.InsertAsync(driverOne, driverTwo);

        // Act
        var query = new GetAllDrivers.Query();
        var drivers = await testingServiceScope.SendAsync(query);

        // Assert
        drivers.Count.Should().BeGreaterThanOrEqualTo(2);
        drivers.FirstOrDefault(x => x.Id == driverOne.Id).Should().NotBeNull();
        drivers.FirstOrDefault(x => x.Id == driverTwo.Id).Should().NotBeNull();
    }

    [Fact]
    public async Task must_be_permitted()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        testingServiceScope.SetUserNotPermitted(Permissions.CanReadDrivers);

        // Act
        var query = new GetAllDrivers.Query();
        Func<Task> act = () => testingServiceScope.SendAsync(command);

        // Assert
        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }
}