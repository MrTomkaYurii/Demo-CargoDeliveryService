namespace DriversManagement.FunctionalTests.FunctionalTests.Repairs;

using DriversManagement.SharedTestHelpers.Fakes.Repair;
using DriversManagement.FunctionalTests.TestUtilities;
using DriversManagement.Domain;
using System.Net;
using System.Threading.Tasks;

public class UpdateRepairRecordTests : TestBase
{
    [Fact]
    public async Task put_repair_returns_nocontent_when_entity_exists_and_auth_credentials_are_valid()
    {
        // Arrange
        var repair = new FakeRepairBuilder().Build();
        var updatedRepairDto = new FakeRepairForUpdateDto().Generate();

        var callingUser = await AddNewSuperAdmin();
        FactoryClient.AddAuth(callingUser.Identifier);
        await InsertAsync(repair);

        // Act
        var route = ApiRoutes.Repairs.Put(repair.Id);
        var result = await FactoryClient.PutJsonRequestAsync(route, updatedRepairDto);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
            
    [Fact]
    public async Task put_repair_returns_unauthorized_without_valid_token()
    {
        // Arrange
        var repair = new FakeRepairBuilder().Build();
        var updatedRepairDto = new FakeRepairForUpdateDto { }.Generate();

        // Act
        var route = ApiRoutes.Repairs.Put(repair.Id);
        var result = await FactoryClient.PutJsonRequestAsync(route, updatedRepairDto);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
            
    [Fact]
    public async Task put_repair_returns_forbidden_without_proper_scope()
    {
        // Arrange
        var repair = new FakeRepairBuilder().Build();
        var updatedRepairDto = new FakeRepairForUpdateDto { }.Generate();
        FactoryClient.AddAuth();

        // Act
        var route = ApiRoutes.Repairs.Put(repair.Id);
        var result = await FactoryClient.PutJsonRequestAsync(route, updatedRepairDto);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}