namespace DriversManagement.FunctionalTests.FunctionalTests.RolePermissions;

using DriversManagement.SharedTestHelpers.Fakes.RolePermission;
using DriversManagement.FunctionalTests.TestUtilities;
using DriversManagement.Domain;
using System.Net;
using System.Threading.Tasks;

public class DeleteRolePermissionTests : TestBase
{
    [Fact]
    public async Task delete_rolepermission_returns_nocontent_when_entity_exists_and_auth_credentials_are_valid()
    {
        // Arrange
        var rolePermission = new FakeRolePermissionBuilder().Build();

        var callingUser = await AddNewSuperAdmin();
        FactoryClient.AddAuth(callingUser.Identifier);
        await InsertAsync(rolePermission);

        // Act
        var route = ApiRoutes.RolePermissions.Delete(rolePermission.Id);
        var result = await FactoryClient.DeleteRequestAsync(route);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
            
    [Fact]
    public async Task delete_rolepermission_returns_unauthorized_without_valid_token()
    {
        // Arrange
        var rolePermission = new FakeRolePermissionBuilder().Build();

        // Act
        var route = ApiRoutes.RolePermissions.Delete(rolePermission.Id);
        var result = await FactoryClient.DeleteRequestAsync(route);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
            
    [Fact]
    public async Task delete_rolepermission_returns_forbidden_without_proper_scope()
    {
        // Arrange
        var rolePermission = new FakeRolePermissionBuilder().Build();
        FactoryClient.AddAuth();

        // Act
        var route = ApiRoutes.RolePermissions.Delete(rolePermission.Id);
        var result = await FactoryClient.DeleteRequestAsync(route);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}