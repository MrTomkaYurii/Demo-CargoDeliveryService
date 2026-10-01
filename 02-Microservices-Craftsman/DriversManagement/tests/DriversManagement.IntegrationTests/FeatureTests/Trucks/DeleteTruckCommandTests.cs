namespace DriversManagement.IntegrationTests.FeatureTests.Trucks;

using DriversManagement.SharedTestHelpers.Fakes.Truck;
using DriversManagement.Domain.Trucks.Features;
using Microsoft.EntityFrameworkCore;
using Domain;
using System.Threading.Tasks;

public class DeleteTruckCommandTests : TestBase
{
    [Fact]
    public async Task can_delete_truck_from_db()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        var truck = new FakeTruckBuilder().Build();
        await testingServiceScope.InsertAsync(truck);

        // Act
        var command = new DeleteTruck.Command(truck.Id);
        await testingServiceScope.SendAsync(command);
        var truckResponse = await testingServiceScope
            .ExecuteDbContextAsync(db => db.Trucks
                .CountAsync(t => t.Id == truck.Id));

        // Assert
        truckResponse.Should().Be(0);
    }

    [Fact]
    public async Task delete_truck_throws_notfoundexception_when_record_does_not_exist()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        var badId = Guid.NewGuid();

        // Act
        var command = new DeleteTruck.Command(badId);
        Func<Task> act = () => testingServiceScope.SendAsync(command);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task can_softdelete_truck_from_db()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        var truck = new FakeTruckBuilder().Build();
        await testingServiceScope.InsertAsync(truck);

        // Act
        var command = new DeleteTruck.Command(truck.Id);
        await testingServiceScope.SendAsync(command);
        var deletedTruck = await testingServiceScope.ExecuteDbContextAsync(db => db.Trucks
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == truck.Id));

        // Assert
        deletedTruck?.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task must_be_permitted()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        testingServiceScope.SetUserNotPermitted(Permissions.CanDeleteTruck);

        // Act
        var command = new DeleteTruck.Command(Guid.NewGuid());
        Func<Task> act = () => testingServiceScope.SendAsync(command);

        // Assert
        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }
}