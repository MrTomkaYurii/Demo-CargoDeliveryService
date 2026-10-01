namespace CargoOrderingService.IntegrationTests.FeatureTests.Orders;

using CargoOrderingService.SharedTestHelpers.Fakes.Order;
using CargoOrderingService.Domain.Orders.Features;
using Microsoft.EntityFrameworkCore;
using Domain;
using System.Threading.Tasks;

public class DeleteOrderCommandTests : TestBase
{
    [Fact]
    public async Task can_delete_order_from_db()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        var order = new FakeOrderBuilder().Build();
        await testingServiceScope.InsertAsync(order);

        // Act
        var command = new DeleteOrder.Command(order.Id);
        await testingServiceScope.SendAsync(command);
        var orderResponse = await testingServiceScope
            .ExecuteDbContextAsync(db => db.Orders
                .CountAsync(o => o.Id == order.Id));

        // Assert
        orderResponse.Should().Be(0);
    }

    [Fact]
    public async Task delete_order_throws_notfoundexception_when_record_does_not_exist()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        var badId = Guid.NewGuid();

        // Act
        var command = new DeleteOrder.Command(badId);
        Func<Task> act = () => testingServiceScope.SendAsync(command);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task can_softdelete_order_from_db()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        var order = new FakeOrderBuilder().Build();
        await testingServiceScope.InsertAsync(order);

        // Act
        var command = new DeleteOrder.Command(order.Id);
        await testingServiceScope.SendAsync(command);
        var deletedOrder = await testingServiceScope.ExecuteDbContextAsync(db => db.Orders
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == order.Id));

        // Assert
        deletedOrder?.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task must_be_permitted()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        testingServiceScope.SetUserNotPermitted(Permissions.CanDeleteOrder);

        // Act
        var command = new DeleteOrder.Command(Guid.NewGuid());
        Func<Task> act = () => testingServiceScope.SendAsync(command);

        // Assert
        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }
}