namespace CargoOrderingService.IntegrationTests.FeatureTests.Orders;

using CargoOrderingService.SharedTestHelpers.Fakes.Order;
using Domain;
using FluentAssertions.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using CargoOrderingService.Domain.Orders.Features;

public class AddOrderCommandTests : TestBase
{
    [Fact]
    public async Task can_add_new_order_to_db()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        var orderOne = new FakeOrderForCreationDto().Generate();

        // Act
        var command = new AddOrder.Command(orderOne);
        var orderReturned = await testingServiceScope.SendAsync(command);
        var orderCreated = await testingServiceScope.ExecuteDbContextAsync(db => db.Orders
            .FirstOrDefaultAsync(o => o.Id == orderReturned.Id));

        // Assert
        orderReturned.OrderNumber.Should().Be(orderOne.OrderNumber);
        orderReturned.CustomerName.Should().Be(orderOne.CustomerName);
        orderReturned.DeliveryDate.Should().Be(orderOne.DeliveryDate);
        orderReturned.TotalAmount.Should().Be(orderOne.TotalAmount);
        orderReturned.Status.Should().Be(orderOne.Status);

        orderCreated.OrderNumber.Should().Be(orderOne.OrderNumber);
        orderCreated.CustomerName.Should().Be(orderOne.CustomerName);
        orderCreated.DeliveryDate.Should().Be(orderOne.DeliveryDate);
        orderCreated.TotalAmount.Should().Be(orderOne.TotalAmount);
        orderCreated.Status.Should().Be(orderOne.Status);
    }

    [Fact]
    public async Task must_be_permitted()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        testingServiceScope.SetUserNotPermitted(Permissions.CanAddOrder);
        var orderOne = new FakeOrderForCreationDto();

        // Act
        var command = new AddOrder.Command(orderOne);
        var act = () => testingServiceScope.SendAsync(command);

        // Assert
        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }
}