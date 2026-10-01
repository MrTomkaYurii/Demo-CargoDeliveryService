namespace DriversManagement.IntegrationTests.FeatureTests.Repairs;

using DriversManagement.Domain.Repairs.Dtos;
using DriversManagement.SharedTestHelpers.Fakes.Repair;
using DriversManagement.Domain.Repairs.Features;
using Domain;
using System.Threading.Tasks;

public class GetAllRepairsQueryTests : TestBase
{
    
    [Fact]
    public async Task can_get_all_repairs()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        var repairOne = new FakeRepairBuilder().Build();
        var repairTwo = new FakeRepairBuilder().Build();

        await testingServiceScope.InsertAsync(repairOne, repairTwo);

        // Act
        var query = new GetAllRepairs.Query();
        var repairs = await testingServiceScope.SendAsync(query);

        // Assert
        repairs.Count.Should().BeGreaterThanOrEqualTo(2);
        repairs.FirstOrDefault(x => x.Id == repairOne.Id).Should().NotBeNull();
        repairs.FirstOrDefault(x => x.Id == repairTwo.Id).Should().NotBeNull();
    }

    [Fact]
    public async Task must_be_permitted()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        testingServiceScope.SetUserNotPermitted(Permissions.CanReadRepairs);

        // Act
        var query = new GetAllRepairs.Query();
        Func<Task> act = () => testingServiceScope.SendAsync(command);

        // Assert
        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }
}