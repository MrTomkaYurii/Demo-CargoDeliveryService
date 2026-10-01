namespace CargoOrderingService.FunctionalTests.FunctionalTests.RolePermissions;

using CargoOrderingService.SharedTestHelpers.Fakes.RolePermission;
using CargoOrderingService.FunctionalTests.TestUtilities;
using CargoOrderingService.Domain;
using System.Net;
using System.Threading.Tasks;

public class UpdateRolePermissionRecordTests : TestBase
{
    [Fact]
    public async Task put_rolepermission_returns_nocontent_when_entity_exists_and_auth_credentials_are_valid()
    {
        // Arrange
        var rolePermission = new FakeRolePermissionBuilder().Build();
        var updatedRolePermissionDto = new FakeRolePermissionForUpdateDto().Generate();

        var callingUser = await AddNewSuperAdmin();
        FactoryClient.AddAuth(callingUser.Identifier);
        await InsertAsync(rolePermission);

        // Act
        var route = ApiRoutes.RolePermissions.Put(rolePermission.Id);
        var result = await FactoryClient.PutJsonRequestAsync(route, updatedRolePermissionDto);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
            
    [Fact]
    public async Task put_rolepermission_returns_unauthorized_without_valid_token()
    {
        // Arrange
        var rolePermission = new FakeRolePermissionBuilder().Build();
        var updatedRolePermissionDto = new FakeRolePermissionForUpdateDto { }.Generate();

        // Act
        var route = ApiRoutes.RolePermissions.Put(rolePermission.Id);
        var result = await FactoryClient.PutJsonRequestAsync(route, updatedRolePermissionDto);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
            
    [Fact]
    public async Task put_rolepermission_returns_forbidden_without_proper_scope()
    {
        // Arrange
        var rolePermission = new FakeRolePermissionBuilder().Build();
        var updatedRolePermissionDto = new FakeRolePermissionForUpdateDto { }.Generate();
        FactoryClient.AddAuth();

        // Act
        var route = ApiRoutes.RolePermissions.Put(rolePermission.Id);
        var result = await FactoryClient.PutJsonRequestAsync(route, updatedRolePermissionDto);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}