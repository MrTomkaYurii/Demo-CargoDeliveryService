namespace CargoOrderingService.UnitTests.Domain.Orders;

using CargoOrderingService.SharedTestHelpers.Fakes.Order;
using CargoOrderingService.Domain.Orders;
using CargoOrderingService.Domain.Orders.DomainEvents;
using Bogus;
using FluentAssertions.Extensions;
using ValidationException = CargoOrderingService.Exceptions.ValidationException;

public class UpdateOrderTests
{
    private readonly Faker _faker;

    public UpdateOrderTests()
    {
        _faker = new Faker();
    }
    
    [Fact]
    public void can_update_order()
    {
        // Arrange
        var order = new FakeOrderBuilder().Build();
        var updatedOrder = new FakeOrderForUpdate().Generate();
        
        // Act
        order.Update(updatedOrder);

        // Assert
        order.OrderNumber.Should().Be(updatedOrder.OrderNumber);
        order.CustomerName.Should().Be(updatedOrder.CustomerName);
        order.DeliveryDate.Should().Be(updatedOrder.DeliveryDate);
        order.TotalAmount.Should().Be(updatedOrder.TotalAmount);
        order.Status.Should().Be(updatedOrder.Status);
    }
    
    [Fact]
    public void queue_domain_event_on_update()
    {
        // Arrange
        var order = new FakeOrderBuilder().Build();
        var updatedOrder = new FakeOrderForUpdate().Generate();
        order.DomainEvents.Clear();
        
        // Act
        order.Update(updatedOrder);

        // Assert
        order.DomainEvents.Count.Should().Be(1);
        order.DomainEvents.FirstOrDefault().Should().BeOfType(typeof(OrderUpdated));
    }
}