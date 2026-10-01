namespace DriversManagement.FunctionalTests.FunctionalTests.Expences;

using DriversManagement.SharedTestHelpers.Fakes.Expence;
using DriversManagement.FunctionalTests.TestUtilities;
using DriversManagement.Domain;
using System.Net;
using System.Threading.Tasks;

public class UpdateExpenceRecordTests : TestBase
{
    [Fact]
    public async Task put_expence_returns_nocontent_when_entity_exists_and_auth_credentials_are_valid()
    {
        // Arrange
        var expence = new FakeExpenceBuilder().Build();
        var updatedExpenceDto = new FakeExpenceForUpdateDto().Generate();

        var callingUser = await AddNewSuperAdmin();
        FactoryClient.AddAuth(callingUser.Identifier);
        await InsertAsync(expence);

        // Act
        var route = ApiRoutes.Expences.Put(expence.Id);
        var result = await FactoryClient.PutJsonRequestAsync(route, updatedExpenceDto);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
            
    [Fact]
    public async Task put_expence_returns_unauthorized_without_valid_token()
    {
        // Arrange
        var expence = new FakeExpenceBuilder().Build();
        var updatedExpenceDto = new FakeExpenceForUpdateDto { }.Generate();

        // Act
        var route = ApiRoutes.Expences.Put(expence.Id);
        var result = await FactoryClient.PutJsonRequestAsync(route, updatedExpenceDto);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
            
    [Fact]
    public async Task put_expence_returns_forbidden_without_proper_scope()
    {
        // Arrange
        var expence = new FakeExpenceBuilder().Build();
        var updatedExpenceDto = new FakeExpenceForUpdateDto { }.Generate();
        FactoryClient.AddAuth();

        // Act
        var route = ApiRoutes.Expences.Put(expence.Id);
        var result = await FactoryClient.PutJsonRequestAsync(route, updatedExpenceDto);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}