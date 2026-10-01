namespace DriversManagement.IntegrationTests.FeatureTests.Inspections;

using DriversManagement.SharedTestHelpers.Fakes.Inspection;
using Domain;
using FluentAssertions.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using DriversManagement.Domain.Inspections.Features;

public class AddInspectionCommandTests : TestBase
{
    [Fact]
    public async Task can_add_new_inspection_to_db()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        var inspectionOne = new FakeInspectionForCreationDto().Generate();

        // Act
        var command = new AddInspection.Command(inspectionOne);
        var inspectionReturned = await testingServiceScope.SendAsync(command);
        var inspectionCreated = await testingServiceScope.ExecuteDbContextAsync(db => db.Inspections
            .FirstOrDefaultAsync(i => i.Id == inspectionReturned.Id));

        // Assert
        inspectionReturned.InspectionDate.Should().Be(inspectionOne.InspectionDate);
        inspectionReturned.Description.Should().Be(inspectionOne.Description);
        inspectionReturned.Result.Should().Be(inspectionOne.Result);

        inspectionCreated.InspectionDate.Should().Be(inspectionOne.InspectionDate);
        inspectionCreated.Description.Should().Be(inspectionOne.Description);
        inspectionCreated.Result.Should().Be(inspectionOne.Result);
    }

    [Fact]
    public async Task must_be_permitted()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        testingServiceScope.SetUserNotPermitted(Permissions.CanAddInspection);
        var inspectionOne = new FakeInspectionForCreationDto();

        // Act
        var command = new AddInspection.Command(inspectionOne);
        var act = () => testingServiceScope.SendAsync(command);

        // Assert
        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }
}