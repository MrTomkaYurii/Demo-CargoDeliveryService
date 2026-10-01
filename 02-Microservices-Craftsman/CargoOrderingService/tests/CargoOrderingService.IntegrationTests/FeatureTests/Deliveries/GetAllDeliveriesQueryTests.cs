namespace CargoOrderingService.IntegrationTests.FeatureTests.Deliveries;

using CargoOrderingService.Domain.Deliveries.Dtos;
using CargoOrderingService.SharedTestHelpers.Fakes.Delivery;
using CargoOrderingService.Domain.Deliveries.Features;
using Domain;
using System.Threading.Tasks;

public class GetAllDeliveriesQueryTests : TestBase
{
    
    [Fact]
    public async Task can_get_all_deliveries()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        var deliveryOne = new FakeDeliveryBuilder().Build();
        var deliveryTwo = new FakeDeliveryBuilder().Build();

        await testingServiceScope.InsertAsync(deliveryOne, deliveryTwo);

        // Act
        var query = new GetAllDeliveries.Query();
        var deliveries = await testingServiceScope.SendAsync(query);

        // Assert
        deliveries.Count.Should().BeGreaterThanOrEqualTo(2);
        deliveries.FirstOrDefault(x => x.Id == deliveryOne.Id).Should().NotBeNull();
        deliveries.FirstOrDefault(x => x.Id == deliveryTwo.Id).Should().NotBeNull();
    }

    [Fact]
    public async Task must_be_permitted()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        testingServiceScope.SetUserNotPermitted(Permissions.CanReadDeliveries);

        // Act
        var query = new GetAllDeliveries.Query();
        Func<Task> act = () => testingServiceScope.SendAsync(command);

        // Assert
        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }
}