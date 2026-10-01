namespace DriversManagement.IntegrationTests.FeatureTests.Expences;

using DriversManagement.Domain.Expences.Dtos;
using DriversManagement.SharedTestHelpers.Fakes.Expence;
using DriversManagement.Domain.Expences.Features;
using Domain;
using System.Threading.Tasks;

public class GetAllExpencesQueryTests : TestBase
{
    
    [Fact]
    public async Task can_get_all_expences()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        var expenceOne = new FakeExpenceBuilder().Build();
        var expenceTwo = new FakeExpenceBuilder().Build();

        await testingServiceScope.InsertAsync(expenceOne, expenceTwo);

        // Act
        var query = new GetAllExpences.Query();
        var expences = await testingServiceScope.SendAsync(query);

        // Assert
        expences.Count.Should().BeGreaterThanOrEqualTo(2);
        expences.FirstOrDefault(x => x.Id == expenceOne.Id).Should().NotBeNull();
        expences.FirstOrDefault(x => x.Id == expenceTwo.Id).Should().NotBeNull();
    }

    [Fact]
    public async Task must_be_permitted()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        testingServiceScope.SetUserNotPermitted(Permissions.CanReadExpences);

        // Act
        var query = new GetAllExpences.Query();
        Func<Task> act = () => testingServiceScope.SendAsync(command);

        // Assert
        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }
}