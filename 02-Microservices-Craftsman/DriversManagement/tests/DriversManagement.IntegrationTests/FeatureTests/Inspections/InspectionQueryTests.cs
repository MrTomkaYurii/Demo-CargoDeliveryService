namespace DriversManagement.IntegrationTests.FeatureTests.Inspections;

using DriversManagement.SharedTestHelpers.Fakes.Inspection;
using DriversManagement.Domain.Inspections.Features;
using Domain;
using FluentAssertions.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

public class InspectionQueryTests : TestBase
{
    [Fact]
    public async Task can_get_existing_inspection_with_accurate_props()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        var inspectionOne = new FakeInspectionBuilder().Build();
        await testingServiceScope.InsertAsync(inspectionOne);

        // Act
        var query = new GetInspection.Query(inspectionOne.Id);
        var inspection = await testingServiceScope.SendAsync(query);

        // Assert
        inspection.InspectionDate.Should().Be(inspectionOne.InspectionDate);
        inspection.Description.Should().Be(inspectionOne.Description);
        inspection.Result.Should().Be(inspectionOne.Result);
    }

    [Fact]
    public async Task get_inspection_throws_notfound_exception_when_record_does_not_exist()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        var badId = Guid.NewGuid();

        // Act
        var query = new GetInspection.Query(badId);
        Func<Task> act = () => testingServiceScope.SendAsync(query);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task must_be_permitted()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        testingServiceScope.SetUserNotPermitted(Permissions.CanReadInspection);

        // Act
        var command = new GetInspection.Query(Guid.NewGuid());
        Func<Task> act = () => testingServiceScope.SendAsync(command);

        // Assert
        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }
}