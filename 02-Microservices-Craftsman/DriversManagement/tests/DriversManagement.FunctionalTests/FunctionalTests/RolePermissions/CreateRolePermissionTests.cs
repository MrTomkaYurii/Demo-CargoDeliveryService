namespace DriversManagement.FunctionalTests.FunctionalTests.RolePermissions;

using DriversManagement.SharedTestHelpers.Fakes.RolePermission;
using DriversManagement.FunctionalTests.TestUtilities;
using DriversManagement.Domain;
using System.Net;
using System.Threading.Tasks;

public class CreateRolePermissionTests : TestBase
{
    [Fact]
    public async Task create_rolepermission_returns_created_using_valid_dto_and_valid_auth_credentials()
    {
        // Arrange
        var rolePermission = new FakeRolePermissionForCreationDto().Generate();

        var callingUser = await AddNewSuperAdmin();
        FactoryClient.AddAuth(callingUser.Identifier);

        // Act
        var route = ApiRoutes.RolePermissions.Create;
        var result = await FactoryClient.PostJsonRequestAsync(route, rolePermission);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Created);
    }
            
    [Fact]
    public async Task create_rolepermission_returns_unauthorized_without_valid_token()
    {
        // Arrange
        var rolePermission = new FakeRolePermissionForCreationDto { }.Generate();

        // Act
        var route = ApiRoutes.RolePermissions.Create;
        var result = await FactoryClient.PostJsonRequestAsync(route, rolePermission);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
            
    [Fact]
    public async Task create_rolepermission_returns_forbidden_without_proper_scope()
    {
        // Arrange
        var rolePermission = new FakeRolePermissionForCreationDto { }.Generate();
        FactoryClient.AddAuth();

        // Act
        var route = ApiRoutes.RolePermissions.Create;
        var result = await FactoryClient.PostJsonRequestAsync(route, rolePermission);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}