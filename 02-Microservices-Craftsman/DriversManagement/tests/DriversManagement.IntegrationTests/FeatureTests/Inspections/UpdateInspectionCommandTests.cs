namespace DriversManagement.IntegrationTests.FeatureTests.Inspections;

using DriversManagement.SharedTestHelpers.Fakes.Inspection;
using DriversManagement.Domain.Inspections.Dtos;
using DriversManagement.Domain.Inspections.Features;
using Domain;
using FluentAssertions.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

public class UpdateInspectionCommandTests : TestBase
{
    [Fact]
    public async Task can_update_existing_inspection_in_db()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        var inspection = new FakeInspectionBuilder().Build();
        await testingServiceScope.InsertAsync(inspection);
        var updatedInspectionDto = new FakeInspectionForUpdateDto().Generate();

        // Act
        var command = new UpdateInspection.Command(inspection.Id, updatedInspectionDto);
        await testingServiceScope.SendAsync(command);
        var updatedInspection = await testingServiceScope
            .ExecuteDbContextAsync(db => db.Inspections
                .FirstOrDefaultAsync(i => i.Id == inspection.Id));

        // Assert
        updatedInspection.InspectionDate.Should().Be(updatedInspectionDto.InspectionDate);
        updatedInspection.Description.Should().Be(updatedInspectionDto.Description);
        updatedInspection.Result.Should().Be(updatedInspectionDto.Result);
    }

    [Fact]
    public async Task must_be_permitted()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        testingServiceScope.SetUserNotPermitted(Permissions.CanUpdateInspection);
        var inspectionOne = new FakeInspectionForUpdateDto();

        // Act
        var command = new UpdateInspection.Command(Guid.NewGuid(), inspectionOne);
        var act = () => testingServiceScope.SendAsync(command);

        // Assert
        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }
}