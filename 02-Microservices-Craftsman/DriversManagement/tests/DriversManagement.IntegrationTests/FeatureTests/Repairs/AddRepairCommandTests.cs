namespace DriversManagement.IntegrationTests.FeatureTests.Repairs;

using DriversManagement.SharedTestHelpers.Fakes.Repair;
using Domain;
using FluentAssertions.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using DriversManagement.Domain.Repairs.Features;

public class AddRepairCommandTests : TestBase
{
    [Fact]
    public async Task can_add_new_repair_to_db()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        var repairOne = new FakeRepairForCreationDto().Generate();

        // Act
        var command = new AddRepair.Command(repairOne);
        var repairReturned = await testingServiceScope.SendAsync(command);
        var repairCreated = await testingServiceScope.ExecuteDbContextAsync(db => db.Repairs
            .FirstOrDefaultAsync(r => r.Id == repairReturned.Id));

        // Assert
        repairReturned.RepairDate.Should().Be(repairOne.RepairDate);
        repairReturned.Description.Should().Be(repairOne.Description);
        repairReturned.Cost.Should().Be(repairOne.Cost);

        repairCreated.RepairDate.Should().Be(repairOne.RepairDate);
        repairCreated.Description.Should().Be(repairOne.Description);
        repairCreated.Cost.Should().Be(repairOne.Cost);
    }

    [Fact]
    public async Task must_be_permitted()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        testingServiceScope.SetUserNotPermitted(Permissions.CanAddRepair);
        var repairOne = new FakeRepairForCreationDto();

        // Act
        var command = new AddRepair.Command(repairOne);
        var act = () => testingServiceScope.SendAsync(command);

        // Assert
        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }
}