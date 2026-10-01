namespace CargoOrderingService.IntegrationTests.FeatureTests.Orders;

using CargoOrderingService.SharedTestHelpers.Fakes.Order;
using CargoOrderingService.Domain.Orders.Features;
using Domain;
using FluentAssertions.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

public class OrderQueryTests : TestBase
{
    [Fact]
    public async Task can_get_existing_order_with_accurate_props()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        var orderOne = new FakeOrderBuilder().Build();
        await testingServiceScope.InsertAsync(orderOne);

        // Act
        var query = new GetOrder.Query(orderOne.Id);
        var order = await testingServiceScope.SendAsync(query);

        // Assert
        order.OrderNumber.Should().Be(orderOne.OrderNumber);
        order.CustomerName.Should().Be(orderOne.CustomerName);
        order.DeliveryDate.Should().Be(orderOne.DeliveryDate);
        order.TotalAmount.Should().Be(orderOne.TotalAmount);
        order.Status.Should().Be(orderOne.Status);
    }

    [Fact]
    public async Task get_order_throws_notfound_exception_when_record_does_not_exist()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        var badId = Guid.NewGuid();

        // Act
        var query = new GetOrder.Query(badId);
        Func<Task> act = () => testingServiceScope.SendAsync(query);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task must_be_permitted()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        testingServiceScope.SetUserNotPermitted(Permissions.CanReadOrder);

        // Act
        var command = new GetOrder.Query(Guid.NewGuid());
        Func<Task> act = () => testingServiceScope.SendAsync(command);

        // Assert
        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }
}