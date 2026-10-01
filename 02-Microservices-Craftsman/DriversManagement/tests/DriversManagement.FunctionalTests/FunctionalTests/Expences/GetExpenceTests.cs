namespace DriversManagement.FunctionalTests.FunctionalTests.Expences;

using DriversManagement.SharedTestHelpers.Fakes.Expence;
using DriversManagement.FunctionalTests.TestUtilities;
using DriversManagement.Domain;
using System.Net;
using System.Threading.Tasks;

public class GetExpenceTests : TestBase
{
    [Fact]
    public async Task get_expence_returns_success_when_entity_exists_using_valid_auth_credentials()
    {
        // Arrange
        var expence = new FakeExpenceBuilder().Build();

        var callingUser = await AddNewSuperAdmin();
        FactoryClient.AddAuth(callingUser.Identifier);
        await InsertAsync(expence);

        // Act
        var route = ApiRoutes.Expences.GetRecord(expence.Id);
        var result = await FactoryClient.GetRequestAsync(route);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
    }
            
    [Fact]
    public async Task get_expence_returns_unauthorized_without_valid_token()
    {
        // Arrange
        var expence = new FakeExpenceBuilder().Build();

        // Act
        var route = ApiRoutes.Expences.GetRecord(expence.Id);
        var result = await FactoryClient.GetRequestAsync(route);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
            
    [Fact]
    public async Task get_expence_returns_forbidden_without_proper_scope()
    {
        // Arrange
        var expence = new FakeExpenceBuilder().Build();
        FactoryClient.AddAuth();

        // Act
        var route = ApiRoutes.Expences.GetRecord(expence.Id);
        var result = await FactoryClient.GetRequestAsync(route);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}