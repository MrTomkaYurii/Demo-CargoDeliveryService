namespace CargoOrderingService.FunctionalTests.FunctionalTests.RolePermissions;

using CargoOrderingService.SharedTestHelpers.Fakes.RolePermission;
using CargoOrderingService.FunctionalTests.TestUtilities;
using CargoOrderingService.Domain;
using System.Net;
using System.Threading.Tasks;

public class GetRolePermissionTests : TestBase
{
    [Fact]
    public async Task get_rolepermission_returns_success_when_entity_exists_using_valid_auth_credentials()
    {
        // Arrange
        var rolePermission = new FakeRolePermissionBuilder().Build();

        var callingUser = await AddNewSuperAdmin();
        FactoryClient.AddAuth(callingUser.Identifier);
        await InsertAsync(rolePermission);

        // Act
        var route = ApiRoutes.RolePermissions.GetRecord(rolePermission.Id);
        var result = await FactoryClient.GetRequestAsync(route);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
    }
            
    [Fact]
    public async Task get_rolepermission_returns_unauthorized_without_valid_token()
    {
        // Arrange
        var rolePermission = new FakeRolePermissionBuilder().Build();

        // Act
        var route = ApiRoutes.RolePermissions.GetRecord(rolePermission.Id);
        var result = await FactoryClient.GetRequestAsync(route);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
            
    [Fact]
    public async Task get_rolepermission_returns_forbidden_without_proper_scope()
    {
        // Arrange
        var rolePermission = new FakeRolePermissionBuilder().Build();
        FactoryClient.AddAuth();

        // Act
        var route = ApiRoutes.RolePermissions.GetRecord(rolePermission.Id);
        var result = await FactoryClient.GetRequestAsync(route);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}