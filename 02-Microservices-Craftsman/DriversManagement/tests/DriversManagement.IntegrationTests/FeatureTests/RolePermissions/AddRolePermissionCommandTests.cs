namespace DriversManagement.IntegrationTests.FeatureTests.RolePermissions;

using DriversManagement.SharedTestHelpers.Fakes.RolePermission;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using DriversManagement.Domain.RolePermissions.Features;

public class AddRolePermissionCommandTests : TestBase
{
    [Fact]
    public async Task can_add_new_rolepermission_to_db()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        var rolePermissionOne = new FakeRolePermissionForCreationDto().Generate();

        // Act
        var command = new AddRolePermission.Command(rolePermissionOne);
        var rolePermissionReturned = await testingServiceScope.SendAsync(command);
        var rolePermissionCreated = await testingServiceScope.ExecuteDbContextAsync(db => db.RolePermissions
            .FirstOrDefaultAsync(r => r.Id == rolePermissionReturned.Id));

        // Assert
        rolePermissionReturned.Permission.Should().Be(rolePermissionOne.Permission);
        rolePermissionReturned.Role.Should().Be(rolePermissionOne.Role);

        rolePermissionCreated?.Permission.Should().Be(rolePermissionOne.Permission);
        rolePermissionCreated?.Role.Value.Should().Be(rolePermissionOne.Role);
    }
}