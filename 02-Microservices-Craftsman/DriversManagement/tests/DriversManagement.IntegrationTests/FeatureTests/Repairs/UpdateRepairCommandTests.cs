namespace DriversManagement.IntegrationTests.FeatureTests.Repairs;

using DriversManagement.SharedTestHelpers.Fakes.Repair;
using DriversManagement.Domain.Repairs.Dtos;
using DriversManagement.Domain.Repairs.Features;
using Domain;
using FluentAssertions.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

public class UpdateRepairCommandTests : TestBase
{
    [Fact]
    public async Task can_update_existing_repair_in_db()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        var repair = new FakeRepairBuilder().Build();
        await testingServiceScope.InsertAsync(repair);
        var updatedRepairDto = new FakeRepairForUpdateDto().Generate();

        // Act
        var command = new UpdateRepair.Command(repair.Id, updatedRepairDto);
        await testingServiceScope.SendAsync(command);
        var updatedRepair = await testingServiceScope
            .ExecuteDbContextAsync(db => db.Repairs
                .FirstOrDefaultAsync(r => r.Id == repair.Id));

        // Assert
        updatedRepair.RepairDate.Should().Be(updatedRepairDto.RepairDate);
        updatedRepair.Description.Should().Be(updatedRepairDto.Description);
        updatedRepair.Cost.Should().Be(updatedRepairDto.Cost);
    }

    [Fact]
    public async Task must_be_permitted()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        testingServiceScope.SetUserNotPermitted(Permissions.CanUpdateRepair);
        var repairOne = new FakeRepairForUpdateDto();

        // Act
        var command = new UpdateRepair.Command(Guid.NewGuid(), repairOne);
        var act = () => testingServiceScope.SendAsync(command);

        // Assert
        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }
}