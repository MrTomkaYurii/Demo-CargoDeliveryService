namespace DriversBlogManagement.IntegrationTests.FeatureTests.Drivers;

using DriversBlogManagement.Domain.Drivers.Dtos;
using DriversBlogManagement.SharedTestHelpers.Fakes.Driver;
using DriversBlogManagement.Domain.Drivers.Features;
using Domain;
using System.Threading.Tasks;

public class DriverListQueryTests : TestBase
{
    
    [Fact]
    public async Task can_get_driver_list()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        var driverOne = new FakeDriverBuilder().Build();
        var driverTwo = new FakeDriverBuilder().Build();
        var queryParameters = new DriverParametersDto();

        await testingServiceScope.InsertAsync(driverOne, driverTwo);

        // Act
        var query = new GetDriverList.Query(queryParameters);
        var drivers = await testingServiceScope.SendAsync(query);

        // Assert
        drivers.Count.Should().BeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task must_be_permitted()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        testingServiceScope.SetUserNotPermitted(Permissions.CanReadDrivers);
        var queryParameters = new DriverParametersDto();

        // Act
        var command = new GetDriverList.Query(queryParameters);
        Func<Task> act = () => testingServiceScope.SendAsync(command);

        // Assert
        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }
}