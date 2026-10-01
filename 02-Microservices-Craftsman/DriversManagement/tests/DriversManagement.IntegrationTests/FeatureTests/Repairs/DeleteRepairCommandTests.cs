namespace DriversManagement.IntegrationTests.FeatureTests.Repairs;

using DriversManagement.SharedTestHelpers.Fakes.Repair;
using DriversManagement.Domain.Repairs.Features;
using Microsoft.EntityFrameworkCore;
using Domain;
using System.Threading.Tasks;

public class DeleteRepairCommandTests : TestBase
{
    [Fact]
    public async Task can_delete_repair_from_db()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        var repair = new FakeRepairBuilder().Build();
        await testingServiceScope.InsertAsync(repair);

        // Act
        var command = new DeleteRepair.Command(repair.Id);
        await testingServiceScope.SendAsync(command);
        var repairResponse = await testingServiceScope
            .ExecuteDbContextAsync(db => db.Repairs
                .CountAsync(r => r.Id == repair.Id));

        // Assert
        repairResponse.Should().Be(0);
    }

    [Fact]
    public async Task delete_repair_throws_notfoundexception_when_record_does_not_exist()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        var badId = Guid.NewGuid();

        // Act
        var command = new DeleteRepair.Command(badId);
        Func<Task> act = () => testingServiceScope.SendAsync(command);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task can_softdelete_repair_from_db()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        var repair = new FakeRepairBuilder().Build();
        await testingServiceScope.InsertAsync(repair);

        // Act
        var command = new DeleteRepair.Command(repair.Id);
        await testingServiceScope.SendAsync(command);
        var deletedRepair = await testingServiceScope.ExecuteDbContextAsync(db => db.Repairs
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == repair.Id));

        // Assert
        deletedRepair?.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task must_be_permitted()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        testingServiceScope.SetUserNotPermitted(Permissions.CanDeleteRepair);

        // Act
        var command = new DeleteRepair.Command(Guid.NewGuid());
        Func<Task> act = () => testingServiceScope.SendAsync(command);

        // Assert
        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }
}