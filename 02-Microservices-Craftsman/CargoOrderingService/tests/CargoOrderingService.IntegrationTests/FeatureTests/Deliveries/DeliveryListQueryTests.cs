namespace CargoOrderingService.IntegrationTests.FeatureTests.Deliveries;

using CargoOrderingService.Domain.Deliveries.Dtos;
using CargoOrderingService.SharedTestHelpers.Fakes.Delivery;
using CargoOrderingService.Domain.Deliveries.Features;
using Domain;
using System.Threading.Tasks;

public class DeliveryListQueryTests : TestBase
{
    
    [Fact]
    public async Task can_get_delivery_list()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        var deliveryOne = new FakeDeliveryBuilder().Build();
        var deliveryTwo = new FakeDeliveryBuilder().Build();
        var queryParameters = new DeliveryParametersDto();

        await testingServiceScope.InsertAsync(deliveryOne, deliveryTwo);

        // Act
        var query = new GetDeliveryList.Query(queryParameters);
        var deliveries = await testingServiceScope.SendAsync(query);

        // Assert
        deliveries.Count.Should().BeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task must_be_permitted()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        testingServiceScope.SetUserNotPermitted(Permissions.CanReadDeliveries);
        var queryParameters = new DeliveryParametersDto();

        // Act
        var command = new GetDeliveryList.Query(queryParameters);
        Func<Task> act = () => testingServiceScope.SendAsync(command);

        // Assert
        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }
}