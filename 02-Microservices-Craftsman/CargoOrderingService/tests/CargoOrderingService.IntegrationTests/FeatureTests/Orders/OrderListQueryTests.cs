namespace CargoOrderingService.IntegrationTests.FeatureTests.Orders;

using CargoOrderingService.Domain.Orders.Dtos;
using CargoOrderingService.SharedTestHelpers.Fakes.Order;
using CargoOrderingService.Domain.Orders.Features;
using Domain;
using System.Threading.Tasks;

public class OrderListQueryTests : TestBase
{
    
    [Fact]
    public async Task can_get_order_list()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        var orderOne = new FakeOrderBuilder().Build();
        var orderTwo = new FakeOrderBuilder().Build();
        var queryParameters = new OrderParametersDto();

        await testingServiceScope.InsertAsync(orderOne, orderTwo);

        // Act
        var query = new GetOrderList.Query(queryParameters);
        var orders = await testingServiceScope.SendAsync(query);

        // Assert
        orders.Count.Should().BeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task must_be_permitted()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        testingServiceScope.SetUserNotPermitted(Permissions.CanReadOrders);
        var queryParameters = new OrderParametersDto();

        // Act
        var command = new GetOrderList.Query(queryParameters);
        Func<Task> act = () => testingServiceScope.SendAsync(command);

        // Assert
        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }
}