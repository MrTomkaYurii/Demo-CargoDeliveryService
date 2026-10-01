namespace CargoOrderingService.IntegrationTests.FeatureTests.RolePermissions;

using CargoOrderingService.SharedTestHelpers.Fakes.RolePermission;
using CargoOrderingService.Domain.RolePermissions.Dtos;
using CargoOrderingService.Domain.RolePermissions.Features;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

public class UpdateRolePermissionCommandTests : TestBase
{
    [Fact]
    public async Task can_update_existing_rolepermission_in_db()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        var rolePermissionOne = new FakeRolePermissionBuilder().Build();
        var updatedRolePermissionDto = new FakeRolePermissionForUpdateDto().Generate();
        await testingServiceScope.InsertAsync(rolePermissionOne);

        var rolePermission = await testingServiceScope.ExecuteDbContextAsync(db => db.RolePermissions
            .FirstOrDefaultAsync(r => r.Id == rolePermissionOne.Id));
        var id = rolePermission.Id;

        // Act
        var command = new UpdateRolePermission.Command(id, updatedRolePermissionDto);
        await testingServiceScope.SendAsync(command);
        var updatedRolePermission = await testingServiceScope.ExecuteDbContextAsync(db => db.RolePermissions.FirstOrDefaultAsync(r => r.Id == id));

        // Assert
        updatedRolePermission?.Permission.Should().Be(updatedRolePermissionDto.Permission);
        updatedRolePermission?.Role.Value.Should().Be(updatedRolePermissionDto.Role);
    }
}