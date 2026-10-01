namespace DriversManagement.IntegrationTests.FeatureTests.Inspections;

using DriversManagement.SharedTestHelpers.Fakes.Inspection;
using DriversManagement.Domain.Inspections.Features;
using Microsoft.EntityFrameworkCore;
using Domain;
using System.Threading.Tasks;

public class DeleteInspectionCommandTests : TestBase
{
    [Fact]
    public async Task can_delete_inspection_from_db()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        var inspection = new FakeInspectionBuilder().Build();
        await testingServiceScope.InsertAsync(inspection);

        // Act
        var command = new DeleteInspection.Command(inspection.Id);
        await testingServiceScope.SendAsync(command);
        var inspectionResponse = await testingServiceScope
            .ExecuteDbContextAsync(db => db.Inspections
                .CountAsync(i => i.Id == inspection.Id));

        // Assert
        inspectionResponse.Should().Be(0);
    }

    [Fact]
    public async Task delete_inspection_throws_notfoundexception_when_record_does_not_exist()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        var badId = Guid.NewGuid();

        // Act
        var command = new DeleteInspection.Command(badId);
        Func<Task> act = () => testingServiceScope.SendAsync(command);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task can_softdelete_inspection_from_db()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        var inspection = new FakeInspectionBuilder().Build();
        await testingServiceScope.InsertAsync(inspection);

        // Act
        var command = new DeleteInspection.Command(inspection.Id);
        await testingServiceScope.SendAsync(command);
        var deletedInspection = await testingServiceScope.ExecuteDbContextAsync(db => db.Inspections
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == inspection.Id));

        // Assert
        deletedInspection?.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task must_be_permitted()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        testingServiceScope.SetUserNotPermitted(Permissions.CanDeleteInspection);

        // Act
        var command = new DeleteInspection.Command(Guid.NewGuid());
        Func<Task> act = () => testingServiceScope.SendAsync(command);

        // Assert
        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }
}