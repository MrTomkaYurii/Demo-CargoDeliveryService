namespace CargoOrderingService.UnitTests.Domain.Orders;

using CargoOrderingService.SharedTestHelpers.Fakes.Order;
using CargoOrderingService.Domain.Orders;
using CargoOrderingService.Domain.Orders.DomainEvents;
using Bogus;
using FluentAssertions.Extensions;
using ValidationException = CargoOrderingService.Exceptions.ValidationException;

public class CreateOrderTests
{
    private readonly Faker _faker;

    public CreateOrderTests()
    {
        _faker = new Faker();
    }
    
    [Fact]
    public void can_create_valid_order()
    {
        // Arrange
        var orderToCreate = new FakeOrderForCreation().Generate();
        
        // Act
        var order = Order.Create(orderToCreate);

        // Assert
        order.OrderNumber.Should().Be(orderToCreate.OrderNumber);
        order.CustomerName.Should().Be(orderToCreate.CustomerName);
        order.DeliveryDate.Should().Be(orderToCreate.DeliveryDate);
        order.TotalAmount.Should().Be(orderToCreate.TotalAmount);
        order.Status.Should().Be(orderToCreate.Status);
    }

    [Fact]
    public void queue_domain_event_on_create()
    {
        // Arrange
        var orderToCreate = new FakeOrderForCreation().Generate();
        
        // Act
        var order = Order.Create(orderToCreate);

        // Assert
        order.DomainEvents.Count.Should().Be(1);
        order.DomainEvents.FirstOrDefault().Should().BeOfType(typeof(OrderCreated));
    }
}