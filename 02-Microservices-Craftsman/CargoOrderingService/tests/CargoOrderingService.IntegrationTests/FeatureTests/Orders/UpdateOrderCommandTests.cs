namespace CargoOrderingService.IntegrationTests.FeatureTests.Orders;

using CargoOrderingService.SharedTestHelpers.Fakes.Order;
using CargoOrderingService.Domain.Orders.Dtos;
using CargoOrderingService.Domain.Orders.Features;
using Domain;
using FluentAssertions.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

public class UpdateOrderCommandTests : TestBase
{
    [Fact]
    public async Task can_update_existing_order_in_db()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        var order = new FakeOrderBuilder().Build();
        await testingServiceScope.InsertAsync(order);
        var updatedOrderDto = new FakeOrderForUpdateDto().Generate();

        // Act
        var command = new UpdateOrder.Command(order.Id, updatedOrderDto);
        await testingServiceScope.SendAsync(command);
        var updatedOrder = await testingServiceScope
            .ExecuteDbContextAsync(db => db.Orders
                .FirstOrDefaultAsync(o => o.Id == order.Id));

        // Assert
        updatedOrder.OrderNumber.Should().Be(updatedOrderDto.OrderNumber);
        updatedOrder.CustomerName.Should().Be(updatedOrderDto.CustomerName);
        updatedOrder.DeliveryDate.Should().Be(updatedOrderDto.DeliveryDate);
        updatedOrder.TotalAmount.Should().Be(updatedOrderDto.TotalAmount);
        updatedOrder.Status.Should().Be(updatedOrderDto.Status);
    }

    [Fact]
    public async Task must_be_permitted()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        testingServiceScope.SetUserNotPermitted(Permissions.CanUpdateOrder);
        var orderOne = new FakeOrderForUpdateDto();

        // Act
        var command = new UpdateOrder.Command(Guid.NewGuid(), orderOne);
        var act = () => testingServiceScope.SendAsync(command);

        // Assert
        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }
}