namespace DriversManagement.IntegrationTests.FeatureTests.Repairs;

using DriversManagement.Domain.Repairs.Dtos;
using DriversManagement.SharedTestHelpers.Fakes.Repair;
using DriversManagement.Domain.Repairs.Features;
using Domain;
using System.Threading.Tasks;

public class RepairListQueryTests : TestBase
{
    
    [Fact]
    public async Task can_get_repair_list()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        var repairOne = new FakeRepairBuilder().Build();
        var repairTwo = new FakeRepairBuilder().Build();
        var queryParameters = new RepairParametersDto();

        await testingServiceScope.InsertAsync(repairOne, repairTwo);

        // Act
        var query = new GetRepairList.Query(queryParameters);
        var repairs = await testingServiceScope.SendAsync(query);

        // Assert
        repairs.Count.Should().BeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task must_be_permitted()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        testingServiceScope.SetUserNotPermitted(Permissions.CanReadRepairs);
        var queryParameters = new RepairParametersDto();

        // Act
        var command = new GetRepairList.Query(queryParameters);
        Func<Task> act = () => testingServiceScope.SendAsync(command);

        // Assert
        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }
}