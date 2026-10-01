namespace DriversManagement.IntegrationTests.FeatureTests.Expences;

using DriversManagement.Domain.Expences.Dtos;
using DriversManagement.SharedTestHelpers.Fakes.Expence;
using DriversManagement.Domain.Expences.Features;
using Domain;
using System.Threading.Tasks;

public class ExpenceListQueryTests : TestBase
{
    
    [Fact]
    public async Task can_get_expence_list()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        var expenceOne = new FakeExpenceBuilder().Build();
        var expenceTwo = new FakeExpenceBuilder().Build();
        var queryParameters = new ExpenceParametersDto();

        await testingServiceScope.InsertAsync(expenceOne, expenceTwo);

        // Act
        var query = new GetExpenceList.Query(queryParameters);
        var expences = await testingServiceScope.SendAsync(query);

        // Assert
        expences.Count.Should().BeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task must_be_permitted()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        testingServiceScope.SetUserNotPermitted(Permissions.CanReadExpences);
        var queryParameters = new ExpenceParametersDto();

        // Act
        var command = new GetExpenceList.Query(queryParameters);
        Func<Task> act = () => testingServiceScope.SendAsync(command);

        // Assert
        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }
}