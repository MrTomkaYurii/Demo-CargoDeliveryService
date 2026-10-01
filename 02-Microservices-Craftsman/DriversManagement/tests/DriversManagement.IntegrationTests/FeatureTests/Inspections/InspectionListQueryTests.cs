namespace DriversManagement.IntegrationTests.FeatureTests.Inspections;

using DriversManagement.Domain.Inspections.Dtos;
using DriversManagement.SharedTestHelpers.Fakes.Inspection;
using DriversManagement.Domain.Inspections.Features;
using Domain;
using System.Threading.Tasks;

public class InspectionListQueryTests : TestBase
{
    
    [Fact]
    public async Task can_get_inspection_list()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        var inspectionOne = new FakeInspectionBuilder().Build();
        var inspectionTwo = new FakeInspectionBuilder().Build();
        var queryParameters = new InspectionParametersDto();

        await testingServiceScope.InsertAsync(inspectionOne, inspectionTwo);

        // Act
        var query = new GetInspectionList.Query(queryParameters);
        var inspections = await testingServiceScope.SendAsync(query);

        // Assert
        inspections.Count.Should().BeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task must_be_permitted()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        testingServiceScope.SetUserNotPermitted(Permissions.CanReadInspections);
        var queryParameters = new InspectionParametersDto();

        // Act
        var command = new GetInspectionList.Query(queryParameters);
        Func<Task> act = () => testingServiceScope.SendAsync(command);

        // Assert
        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }
}