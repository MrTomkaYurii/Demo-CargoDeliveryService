namespace DriversManagement.IntegrationTests.FeatureTests.Inspections;

using DriversManagement.Domain.Inspections.Dtos;
using DriversManagement.SharedTestHelpers.Fakes.Inspection;
using DriversManagement.Domain.Inspections.Features;
using Domain;
using System.Threading.Tasks;

public class GetAllInspectionsQueryTests : TestBase
{
    
    [Fact]
    public async Task can_get_all_inspections()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        var inspectionOne = new FakeInspectionBuilder().Build();
        var inspectionTwo = new FakeInspectionBuilder().Build();

        await testingServiceScope.InsertAsync(inspectionOne, inspectionTwo);

        // Act
        var query = new GetAllInspections.Query();
        var inspections = await testingServiceScope.SendAsync(query);

        // Assert
        inspections.Count.Should().BeGreaterThanOrEqualTo(2);
        inspections.FirstOrDefault(x => x.Id == inspectionOne.Id).Should().NotBeNull();
        inspections.FirstOrDefault(x => x.Id == inspectionTwo.Id).Should().NotBeNull();
    }

    [Fact]
    public async Task must_be_permitted()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        testingServiceScope.SetUserNotPermitted(Permissions.CanReadInspections);

        // Act
        var query = new GetAllInspections.Query();
        Func<Task> act = () => testingServiceScope.SendAsync(command);

        // Assert
        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }
}