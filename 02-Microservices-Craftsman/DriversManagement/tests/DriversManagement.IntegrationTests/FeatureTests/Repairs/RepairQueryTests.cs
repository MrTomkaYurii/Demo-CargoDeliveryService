namespace DriversManagement.IntegrationTests.FeatureTests.Repairs;

using DriversManagement.SharedTestHelpers.Fakes.Repair;
using DriversManagement.Domain.Repairs.Features;
using Domain;
using FluentAssertions.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

public class RepairQueryTests : TestBase
{
    [Fact]
    public async Task can_get_existing_repair_with_accurate_props()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        var repairOne = new FakeRepairBuilder().Build();
        await testingServiceScope.InsertAsync(repairOne);

        // Act
        var query = new GetRepair.Query(repairOne.Id);
        var repair = await testingServiceScope.SendAsync(query);

        // Assert
        repair.RepairDate.Should().Be(repairOne.RepairDate);
        repair.Description.Should().Be(repairOne.Description);
        repair.Cost.Should().Be(repairOne.Cost);
    }

    [Fact]
    public async Task get_repair_throws_notfound_exception_when_record_does_not_exist()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        var badId = Guid.NewGuid();

        // Act
        var query = new GetRepair.Query(badId);
        Func<Task> act = () => testingServiceScope.SendAsync(query);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task must_be_permitted()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        testingServiceScope.SetUserNotPermitted(Permissions.CanReadRepair);

        // Act
        var command = new GetRepair.Query(Guid.NewGuid());
        Func<Task> act = () => testingServiceScope.SendAsync(command);

        // Assert
        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }
}