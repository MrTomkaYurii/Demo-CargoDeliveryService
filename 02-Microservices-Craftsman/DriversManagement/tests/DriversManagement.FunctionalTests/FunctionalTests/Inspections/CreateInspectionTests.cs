namespace DriversManagement.FunctionalTests.FunctionalTests.Inspections;

using DriversManagement.SharedTestHelpers.Fakes.Inspection;
using DriversManagement.FunctionalTests.TestUtilities;
using DriversManagement.Domain;
using System.Net;
using System.Threading.Tasks;

public class CreateInspectionTests : TestBase
{
    [Fact]
    public async Task create_inspection_returns_created_using_valid_dto_and_valid_auth_credentials()
    {
        // Arrange
        var inspection = new FakeInspectionForCreationDto().Generate();

        var callingUser = await AddNewSuperAdmin();
        FactoryClient.AddAuth(callingUser.Identifier);

        // Act
        var route = ApiRoutes.Inspections.Create;
        var result = await FactoryClient.PostJsonRequestAsync(route, inspection);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Created);
    }
            
    [Fact]
    public async Task create_inspection_returns_unauthorized_without_valid_token()
    {
        // Arrange
        var inspection = new FakeInspectionForCreationDto { }.Generate();

        // Act
        var route = ApiRoutes.Inspections.Create;
        var result = await FactoryClient.PostJsonRequestAsync(route, inspection);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
            
    [Fact]
    public async Task create_inspection_returns_forbidden_without_proper_scope()
    {
        // Arrange
        var inspection = new FakeInspectionForCreationDto { }.Generate();
        FactoryClient.AddAuth();

        // Act
        var route = ApiRoutes.Inspections.Create;
        var result = await FactoryClient.PostJsonRequestAsync(route, inspection);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}