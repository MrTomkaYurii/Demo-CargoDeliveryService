namespace CargoOrderingService.IntegrationTests.FeatureTests.Orders;

using CargoOrderingService.Domain.Orders.Dtos;
using CargoOrderingService.SharedTestHelpers.Fakes.Order;
using CargoOrderingService.Domain.Orders.Features;
using Domain;
using System.Threading.Tasks;

public class GetAllOrdersQueryTests : TestBase
{
    
    [Fact]
    public async Task can_get_all_orders()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        var orderOne = new FakeOrderBuilder().Build();
        var orderTwo = new FakeOrderBuilder().Build();

        await testingServiceScope.InsertAsync(orderOne, orderTwo);

        // Act
        var query = new GetAllOrders.Query();
        var orders = await testingServiceScope.SendAsync(query);

        // Assert
        orders.Count.Should().BeGreaterThanOrEqualTo(2);
        orders.FirstOrDefault(x => x.Id == orderOne.Id).Should().NotBeNull();
        orders.FirstOrDefault(x => x.Id == orderTwo.Id).Should().NotBeNull();
    }

    [Fact]
    public async Task must_be_permitted()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        testingServiceScope.SetUserNotPermitted(Permissions.CanReadOrders);

        // Act
        var query = new GetAllOrders.Query();
        Func<Task> act = () => testingServiceScope.SendAsync(command);

        // Assert
        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }
}